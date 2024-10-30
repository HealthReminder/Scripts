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
using Assets.Scripts.Types;
using UnityEngine.Networking;
using static FacebookHandler;
public abstract class FacebookQueries
{
    internal bool _isQuerying = false;
    internal bool _isPicDownloaded = false;
    internal string _playerEmail;
    internal Texture2D _profilePicTexture;
    internal Texture2D _fbProfilPicTexture;

    // This method allows us to fetch a profile url via the graph api.
    // note: Once it is fetched, it will automatically be loaded, and will return a UserProfilePicLoadedEvent
    public void QueryProfilePic()
    {
#if UNITY_WEBGL
        FB.API("/me/picture?width=200&height=200&redirect=false", HttpMethod.GET, this.OnFetchProfilePicUrlComplete);
#else
        FB.API("/me/picture?width=100&height=100&redirect=false", HttpMethod.GET, this.OnQueryProfilePicComplete);
#endif

    }
    
    /// <summary>
    /// This will query the players email if they have allowed the permission and have an email associated with their account.
    /// </summary>
    public void QueryPlayerEmail()
    {
        _isQuerying = true;
        FB.API("/me/?fields=email", HttpMethod.GET, OnQueryPlayerEmailComplete);
    }

    /// <summary>
    /// Use this callback to set the player email.
    /// </summary>
    /// <param name="result"></param>
    private void OnQueryPlayerEmailComplete(IGraphResult result)
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

        _isQuerying = false;
    }
    /// Triggered when the graph api returns a result from the FetchProfilePicUrl() call
    private void OnQueryProfilePicComplete(IGraphResult result)
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
}
public abstract class FacebookLogin : FacebookQueries
{
    private Action logoutSuccessCallback = null;
    private Action logoutFailCallback = null;

    private int logoutTimeoutCounter;          // for endless loop prevention upon logging out
    private int logoutTimeoutMax = 500;      // amount of loops before failing when attempting a logout

    /// Returns a bool to indicate whether the player is logged into Facebook
    public bool IsLoggedIn => FB.IsLoggedIn;

    /// Returns a bool to indicate whether the FB object is initialized
    public bool IsInitialized => FB.IsInitialized;
    public void LogoutOfFacebook(Action logoutSuccessCallback = null, Action logoutFailCallback = null)
    {
        Debug.Log(this + ".LogoutOfFacebook(): FB.ISLoggedIn:" + FB.IsLoggedIn.ToString());

        if (FB.IsLoggedIn)
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
    protected virtual IEnumerator WaitForLogout()
    {
        while (FB.IsLoggedIn && this.logoutTimeoutCounter < this.logoutTimeoutMax)
        {
            this.logoutTimeoutCounter++;
            yield return new WaitForSeconds(0.01f);
        }

        if (!FB.IsLoggedIn)
        {
            this.OnLogoutSuccess();
        }
        else
        {
            this.OnLogoutFail();
        }
    }

}
public class FacebookHandler : FacebookLogin
{
    private IEnumerable<string> _permissions;

    /// We will need to add "user_likes" if we want to query whether a user has liked a page.
    private static readonly string[] _facebookBasicPermissions = new string[] { "public_profile", "email" };
    public bool IsQuerying { get => _isQuerying; } 
    public bool IsPicDownloaded { get => _isPicDownloaded; }
    public string PlayerEmail { get => _playerEmail; }

    /// Profile picture set in Back Office
    public Texture2D ProfilePicture => this._profilePicTexture;

    /// Profile picture from Facebook profile
    public Texture2D FBProfilePicture => this._fbProfilPicTexture;

    private static readonly FacebookHandler _instance = new FacebookHandler();
    public static FacebookHandler Instance => _instance;
   
    FacebookHandler()
    {
        if (!FB.IsInitialized)
        {
            InitializeFacebook();
        }
    }

   /// <summary>
   /// This async call and response need to happen before the FB object can be used reliably.
   /// </summary>
    public void InitializeFacebook()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(OnInitComplete);
        } else
        {
            FB.ActivateApp();
            ArcadeAnalyticsManager.Instance.DispatchSDKLoadingEvent("facebook", true);
        }
    }

    public void LoginToFacebook(IEnumerable<string> permissions = null)
    {
        if (!FB.IsLoggedIn || Facebook.Unity.AccessToken.CurrentAccessToken == null)
        {
            if (permissions != null)
            {
                _permissions = permissions;
            }
            else
            {
                _permissions = _facebookBasicPermissions;
            }
            FB.LogInWithReadPermissions(_permissions, OnLoginToFacebookResult);
        } 
        else
        {
            Debug.LogWarning(this + ".LoginToFacebook() - WARNING attempting login while already logged in.");
        }
    }
   
    public void OverrideProfilePicture(Texture2D texture)
    {
        _profilePicTexture = texture;
    }
    
    #region Callbacks 
    /// Triggered when we hear back from an attempt to login to facebook
    private void OnLoginToFacebookResult(ILoginResult result)
    {
        if (FB.IsLoggedIn && Facebook.Unity.AccessToken.CurrentAccessToken != null)
        {
            ArcadeAnalyticsManager.Instance.DispatchFacebookAPIEvent("LogInWithReadPermissions", true);
            QueryPlayerEmail();
            EventDispatcher.TriggerEvent(new FBLoginChangedEvent(FB.IsLoggedIn));

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

    /// Triggered once the FB object is done initializing.  
    /// Throws and event to indicate that the init is complete.
    private void OnInitComplete()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            ArcadeAnalyticsManager.Instance.DispatchSDKLoadingEvent("facebook", true);
        }
        else
        {
            string errorMsg = "FacebookHandler.InitCallback - Facebook failed to initialize.";
            ArcadeAnalyticsManager.Instance.DispatchSDKLoadingEvent("facebook", false, errorMsg);
            ThanosLogger.Error(errorMsg);
        }
        // fire off an event to let objects know we've completed our initialization attempt
        EventDispatcher.TriggerEvent(new FBInitCompleteEvent(FB.IsInitialized));
    }
    #endregion
    
    #region  Events thrown by this class
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