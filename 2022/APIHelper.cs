using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class APIHelper : MonoBehaviour
{
    [ContextMenu("Get")]
    public async void GetSomething()
    {
        var getUrl = "https://api.chucknorris.io/jokes/random";
        var postUrl = "";

        var result = await Get<string>(getUrl);
        result = await Post<string>(postUrl, "Hullo");
    }
    public async Task<ClassType> Post<ClassType>(string connectionString, string body)
    {
        var www = UnityWebRequest.Post(connectionString, body);

        var operation = www.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        var jsonResponse = www.downloadHandler.text;

        if (www.result != UnityWebRequest.Result.Success)
            Debug.LogError("Could not connect to server");

        try
        {
            Debug.Log($"Successfuly uploaded data.");
            return default;
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"{this} could not upload data.");
            return default;
        }

    }

    public async Task<ClassType> Get<ClassType>(string connectionString)
    {
        var www = UnityWebRequest.Get(connectionString);

        www.SetRequestHeader("Content-Type", "application/json");

        var operation = www.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        var jsonResponse = www.downloadHandler.text;

        if (www.result != UnityWebRequest.Result.Success)
            Debug.LogError("Could not connect to server");

        try
        {
            var result = JsonUtility.FromJson<ClassType>(jsonResponse);
            Debug.Log($"Successfuly received JSON: {www.downloadHandler.text}");
            return result;
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"{this} could not parse response {jsonResponse}. {exception.Message}");
            return default;
        }

    }
}
