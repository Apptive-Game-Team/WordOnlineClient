using System;
using System.Collections;
using System.Text;
using Script.Data;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;

public class DecorationsApiClient : MonoBehaviour
{
    private string baseUrl = ServerList.MatchingServer.url;

    [Serializable]
    private class DecorationRequest
    {
        public long decorationId;
        public DecorationRequest(long id) { decorationId = id; }
    }

    public IEnumerator GetMyDecorations(
        bool equippedOnly, 
        Action<DecorationsResponse> onSuccess, 
        Action<string> onError)
    {
        var url = $"{baseUrl}/api/users/mine/decorations?equippedOnly={equippedOnly.ToString().ToLower()}";

        using var req = UnityWebRequest.Get(url);
        Server.SetAcceptLanguage(req);
        Server.SetAuthorization(req);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"{req.responseCode} / {req.error}");
            yield break;
        }

        try
        {
            var json = req.downloadHandler.text;
            var resp = JsonUtility.FromJson<DecorationsResponse>(json);
            onSuccess?.Invoke(resp);
        }
        catch (Exception e)
        {
            onError?.Invoke($"JSON parse error: {e}\n{req.downloadHandler.text}");
        }
    }

    public IEnumerator EquipDecoration(
        long decorationId, 
        Action onSuccess, 
        Action<string> onError)
    {
        var url = $"{baseUrl}/api/users/mine/decorations";

        var bodyObj = new DecorationRequest(decorationId);
        var jsonBody = JsonUtility.ToJson(bodyObj);
        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");
        Server.SetAcceptLanguage(req);
        Server.SetAuthorization(req);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"{req.responseCode} / {req.error}\n{req.downloadHandler.text}");
            yield break;
        }

        // 200 또는 204 둘 다 OK
        if (req.responseCode == 200 || req.responseCode == 204)
        {
            onSuccess?.Invoke();
        }
        else
        {
            onError?.Invoke("Unexpected status: " + req.responseCode + "\n" + req.downloadHandler.text);
        }
    }
}
