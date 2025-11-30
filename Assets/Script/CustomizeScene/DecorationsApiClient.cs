using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DecorationsApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "https://your.server.com";
    
    // TODO: 토큰 있는 경우 여기서 넣기
    private void AddCommonHeaders(UnityWebRequest req)
    {
        req.SetRequestHeader("Content-Type", "application/json");
        // req.SetRequestHeader("Authorization", "Bearer " + token);
    }

    [Serializable]
    private class DecorationRequest
    {
        public long decorationId;
        public DecorationRequest(long id) { decorationId = id; }
    }

    public IEnumerator GetMyDecorations(bool equippedOnly, Action<DecorationsResponse> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/users/mine/decorations?equippedOnly={equippedOnly.ToString().ToLower()}";
        using var req = UnityWebRequest.Get(url);
        AddCommonHeaders(req);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(req.error);
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
            onError?.Invoke(e.Message);
        }
    }

    public IEnumerator EquipDecoration(long decorationId, Action onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/users/mine/decorations";

        var bodyObj = new DecorationRequest(decorationId);
        string jsonBody = JsonUtility.ToJson(bodyObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        AddCommonHeaders(req);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(req.error + " / " + req.downloadHandler.text);
            yield break;
        }

        // 200 또는 204 둘 다 OK
        if (req.responseCode == 200 || req.responseCode == 204)
        {
            onSuccess?.Invoke();
        }
        else
        {
            onError?.Invoke("Unexpected status: " + req.responseCode);
        }
    }
}
