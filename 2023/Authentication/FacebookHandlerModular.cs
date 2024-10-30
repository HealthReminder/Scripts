using Blastworks.SlingoCasino;
using Facebook.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Thanos.Events;
using Thanos.GameHub;
using Thanos.Logging;
using UnityEngine;
using Blastworks.SlingoCasino.Analytics;
using UnityEngine.Networking;
using static FacebookHandler;
public class FacebookLogin
{
    private int logoutTimeoutCounter;               /// For endless loop prevention upon logging out
    private int logoutTimeoutMax = 500;             /// Amount of loops before failing when attempting a logout

    private IEnumerable<string> _permissions;

    private Action logoutSuccessCallback = null;
    private Action logoutFailCallback = null;
    private Action fetchEmailCallback = null;

    public bool IsInitialized => FB.IsInitialized;  /// Indicates if Facebook SDK has been initialized
    public bool IsLoggedIn => FB.IsLoggedIn;        /// Indicates if user is logged into Facebook

    public FacebookLogin(Action _fetchUserEmailCallback)
    {
        fetchEmailCallback = _fetchUserEmailCallback;
        Initialize();
    }

    #region Login
    /// <summary>
    /// This async call and response need to happen before the FB object can be used reliably.
    /// </summary>
    internal void Initialize()
    {
        if (!IsInitialized)
        {
            Debug.Log("[Facebook] Initializing Facebook");
            FB.Init(OnInitComplete);
        }
        else
        {
            Debug.Log("[Facebook] Facebook is already initialized. Activating App.");
            FB.ActivateApp();
            ArcadeAnalyticsManager.Instance.DispatchSDKLoadingEvent("facebook", true);
        }
    }
    internal void LoginToFacebook(IEnumerable<string> permissions)
    {
        if (!IsLoggedIn || Facebook.Unity.AccessToken.CurrentAccessToken == null && permissions != null)
        {
           _permissions = permissions;
            
            FB.LogInWithReadPermissions(_permissions, OnLoginToFacebookResult);
        }
        else
        {
            Debug.LogWarning(this + ".LoginToFacebook() - WARNING attempting login while already logged in or with null permissions.");
        }
    }
    /// Triggered once the FB object is done initializing.  
    /// Throws and event to indicate that the init is complete.
    private void OnInitComplete()
    {
        if (IsInitialized)
        {
            Debug.Log("[Facebook] Facebook was initialized. Activating App.");
            FB.ActivateApp();
            ArcadeAnalyticsManager.Instance.DispatchSDKLoadingEvent("facebook", true);
            if (IsLoggedIn)
            {
                Debug.Log("$ [Facebook] Facebook user is currently logged in." +
                    $"Client Token: {FB.ClientToken}" +
                    $"Client Token: {AccessToken.CurrentAccessToken.ToString()}" +
                    $"User ID: {AccessToken.CurrentAccessToken.UserId}");
            }
            else
            {
                Debug.Log("[Facebook] Facebook user is currently logged off.");
            }
        }
        else
        {
            string errorMsg = "[Facebook] FacebookHandler.InitCallback -  Facebook could not be initialized.";
            ArcadeAnalyticsManager.Instance.DispatchSDKLoadingEvent("facebook", false, errorMsg);
            ThanosLogger.Error(errorMsg);
        }
        // fire off an event to let objects know we've completed our initialization attempt
        EventDispatcher.TriggerEvent(new FBInitCompleteEvent(IsInitialized));
    }
    private void OnLoginToFacebookResult(ILoginResult result)
    {
        if (IsLoggedIn && Facebook.Unity.AccessToken.CurrentAccessToken != null)
        {
            ArcadeAnalyticsManager.Instance.DispatchFacebookAPIEvent("LogInWithReadPermissions", true);
            fetchEmailCallback.Invoke();
            EventDispatcher.TriggerEvent(new FBLoginChangedEvent(IsLoggedIn));

            var fbInfo = new GameService.UserAPI.AuthRequest.FacebookInfo
            {
                AccessToken = result.AccessToken.TokenString,
                ExpiresIn = (long)TimeSpan.FromSeconds(result.AccessToken.ExpirationTime.TotalSeconds()).TotalMinutes,
                SignedRequest = "",
                UserId = result.AccessToken.UserId
            };

            EventDispatcher.TriggerEvent(new FBLoginSuccessEvent(fbInfo));
        }
        else
        {
            ArcadeAnalyticsManager.Instance.DispatchFacebookAPIEvent("LogInWithReadPermissions", false, result.Error);
            ThanosLogger.Warning("FacebookHandler failed login. Error: " + result.Error);
            EventDispatcher.TriggerEvent(new FBLoginFailEvent(result.Cancelled, result.Error));
        }
    }
    
    #endregion

    #region Logout
    internal void LogoutOfFacebook(Action logoutSuccessCallback = null, Action logoutFailCallback = null)
    {
        Debug.Log(this + ".LogoutOfFacebook(): FB.ISLoggedIn:" + IsLoggedIn.ToString());

        if (IsInitialized && IsLoggedIn)
        {
            this.logoutSuccessCallback = logoutSuccessCallback;
            this.logoutFailCallback = logoutFailCallback;

            FB.LogOut();

            this.logoutTimeoutCounter = 0;
            Thanos.Core.Coroutiner.StartCoroutine(this.WaitForLogout());
        }
    }

    /// Triggered upon a successful logout
    private void OnLogoutSuccess()
    {
        EventDispatcher.TriggerEvent(new FBLoginChangedEvent(false));
        if (this.logoutSuccessCallback != null)
        {
            this.logoutSuccessCallback();
        }
    }

    /// Triggered upon a failed logout
    private void OnLogoutFail()
    {
        if (this.logoutFailCallback != null)
        {
            this.logoutFailCallback();
        }
    }
    // This will wait for the FB level to logout (async).
    // It will call corresponding success/fail routines depending on the result.
    // The fail situation happens when we wait for too many loop iterations (this.LOGOUT_LOOP_MAX)
    private IEnumerator WaitForLogout()
    {
        while (IsLoggedIn && this.logoutTimeoutCounter < this.logoutTimeoutMax)
        {
            this.logoutTimeoutCounter++;
            yield return new WaitForSeconds(0.01f);
        }

        if (!IsLoggedIn)
        {
            this.OnLogoutSuccess();
        }
        else
        {
            this.OnLogoutFail();
        }
    }

    #endregion

}
internal class FacebookFetcher
{
    internal bool _isFetching = false;
    internal bool _isPicDownloaded = false;
    internal string _playerEmail;
    internal Texture2D _profilePicTexture;
    internal Texture2D _fbProfilPicTexture;

    #region Fetch Email
    /// <summary>
    /// This will query the players email if they have allowed the permission and have an email associated with their account.
    /// </summary>
    internal void FetchUserEmail()
    {
        if (!FB.IsInitialized || !FB.IsLoggedIn)
            return;

        _isFetching = true;
        FB.API("/me/?fields=email", HttpMethod.GET, OnFetchPlayerEmailComplete);
    }

    /// <summary>
    /// Use this callback to set the player email.
    /// </summary>
    /// <param name="result"></param>
    internal void OnFetchPlayerEmailComplete(IGraphResult result)
    {
        if (string.IsNullOrEmpty(result.Error))
        {
            ArcadeAnalyticsManager.Instance.DispatchFacebookAPIEvent("/me/?fields=email", true);
            foreach (var item in result.ResultDictionary)
            {
                if (item.Key == "email")
                {
                    _playerEmail = item.Value.ToString();
                    break;
                }
            }
        }
        else
        {
            ArcadeAnalyticsManager.Instance.DispatchFacebookAPIEvent("/me/?fields=email", false, result.Error);
            ThanosLogger.Error(this + ".SetPlayerEmail failed due to an error: " + result.Error);
        }

        _isFetching = false;
    }
    #endregion
    
    #region Fetch Profile Picture
    // This method allows us to fetch a profile url via the graph api.
    // note: Once it is fetched, it will automatically be loaded, and will return a UserProfilePicLoadedEvent
    internal void FetchProfilePic()
    {
        if (!FB.IsInitialized || !FB.IsLoggedIn)
            return;

#if UNITY_WEBGL
        FB.API("/me/picture?width=200&height=200&redirect=false", HttpMethod.GET, this.OnFetchProfilePicUrlComplete);
#else
        FB.API("/me/picture?width=100&height=100&redirect=false", HttpMethod.GET, this.OnFetchProfilePicComplete);
#endif

    }
    /// Triggered when the graph api returns a result from the FetchProfilePicUrl() call
    private void OnFetchProfilePicComplete(IGraphResult result)
    {
        string imageUrl = "";
        object pictureData, url;

        if (!string.IsNullOrEmpty(result.Error))
        {
            ArcadeAnalyticsManager.Instance.DispatchFacebookAPIEvent("/me/picture?width=200&height=200&redirect=false", false, result.Error);
        }
        ArcadeAnalyticsManager.Instance.DispatchFacebookAPIEvent("/me/picture?width=200&height=200&redirect=false", true);

        result.ResultDictionary.TryGetValue("data", out pictureData);

        if (pictureData != null)
        {

            ((Dictionary<string, object>)pictureData).TryGetValue("url", out url);
            if (url != null)
            {
                imageUrl = url.ToString();
            }
        }

        // load the profile pic
        this.LoadProfilePicFromUrl(imageUrl);
    }
    /// starts loading the image at the specified url.  An event will be triggered when it's done.
    private void LoadProfilePicFromUrl(string url)
    {
        Thanos.Core.Coroutiner.StartCoroutine(this.LoadProfilePicAsync(url));
    }
    /// does the work of loading the profile pic url, and fires a UserProfilePicLoadedEvent when it is done. 
    private IEnumerator LoadProfilePicAsync(string url)
    {
        if (!string.IsNullOrEmpty(url) && !App.Instance.GetIsEdgeBrowser())
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();
                _profilePicTexture = DownloadHandlerTexture.GetContent(uwr);
                _fbProfilPicTexture = _profilePicTexture;
                EventDispatcher.TriggerEvent(new UserProfilePicLoadedEvent(true));
                _isPicDownloaded = true;
            }
        }
        else
        {
            EventDispatcher.TriggerEvent(new UserProfilePicLoadedEvent(false));
        }
    }
    #endregion

}
public class FacebookHandler
{
    FacebookLogin _login = null;
    FacebookFetcher _fetcher = null;
    private static readonly FacebookHandler _instance = new FacebookHandler();

    public static FacebookHandler Instance => _instance;
    
    private static readonly string[] _facebookBasicPermissions = new string[] { "public_profile", "email" }; /// We will need to add "user_likes" if we want to query whether a user has liked a page.
    public bool IsInitialized => _login == null? false : _login.IsInitialized;
    public bool IsLoggedIn =>    _login == null ? false : _login.IsLoggedIn;

    public bool IsFetching { get =>      _fetcher == null ? false : _fetcher._isFetching; } 
    public bool IsPicDownloaded { get => _fetcher == null ? false : _fetcher._isPicDownloaded; }
    public string PlayerEmail { get =>   _fetcher == null ? "" : _fetcher._playerEmail; }
    public Texture2D ProfilePicture =>   _fetcher == null ? null : _fetcher._profilePicTexture;    /// Profile picture set in Back Office
    public Texture2D FBProfilePicture => _fetcher == null ? null : _fetcher._fbProfilPicTexture;   /// Profile picture from Facebook profile

    FacebookHandler()
    {
        Initialize();
    }
    public void Initialize()
    {
        if (_login == null || _fetcher == null)
        {
            _fetcher = new FacebookFetcher();
            _login = new FacebookLogin(_fetcher.FetchUserEmail);
            _login.Initialize();
        }
    }
    public void LoginToFacebook(IEnumerable<string> permissions)
    {
        _login.LoginToFacebook(permissions);
    }
    public void LogOut(Action logoutSuccessCallback = null, Action logoutFailCallback = null)
    {
        _login.LogoutOfFacebook(logoutSuccessCallback, logoutFailCallback);
    }
    public void FetchProfilePic()
    {
        _fetcher.FetchProfilePic();
    }
    public void FetchUserEmail()
    {
        _fetcher.FetchUserEmail();
    }
    public void OverrideProfilePicture(Texture2D texture)
    {
        _fetcher._profilePicTexture = texture;
    }
    
    #region  Events Thrown
    public class FBLoginChangedEvent : BaseEvent
    {
        public bool loggedIn;

        public FBLoginChangedEvent(bool loggedIn)
        {
            this.type = ArcadeEventType.FB_LOGIN_CHANGED.ToString();
            this.loggedIn = loggedIn;
        }
    }

    public class FBLoginSuccessEvent : BaseEvent
    {
        public GameService.UserAPI.AuthRequest.FacebookInfo fbInfo;

        public FBLoginSuccessEvent(GameService.UserAPI.AuthRequest.FacebookInfo fbInfo)
        {
            this.type = ArcadeEventType.FB_LOGIN_SUCCESS.ToString();
            this.fbInfo = fbInfo;
        }
    }

    public class FBLoginFailEvent : BaseEvent
    {
        public bool cancelled;
        public string errorMsg;

        public FBLoginFailEvent(bool cancelled, string errorMsg)
        {
            this.type = ArcadeEventType.FB_LOGIN_FAIL.ToString();
            this.cancelled = cancelled;
            this.errorMsg = errorMsg;
        }
    }

    public class UserProfilePicLoadedEvent : BaseEvent
    {
        public bool success;
        public UserProfilePicLoadedEvent(bool success)
        {
            this.type = ArcadeEventType.USER_FB_PROFILE_PIC_LOADED.ToString();
            this.success = success;
        }
    }

    public class FBInitCompleteEvent : BaseEvent
    {
        public bool success;
        public FBInitCompleteEvent(bool success)
        {
            this.type = ArcadeEventType.FB_INIT_COMPLETE_ATTEMPT.ToString();
            this.success = success;
        }
    }
    #endregion

}