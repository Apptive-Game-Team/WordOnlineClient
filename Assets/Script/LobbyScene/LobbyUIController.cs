using System.Collections;
using System.Linq;
using Script.Data;
using Script.Data.Deck;
using Script.DeckScene;
using Script.Global;
using Script.Global.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] LobbyUserNameUI lobbyUserNameUI;
    [SerializeField] private TMP_Dropdown deckDropdown;
    [SerializeField] private Button arrowButton;
    private static DeckResponseDto[] userDecks;

    public LocalizedString deckLoadFailed;
    public LocalizedString noDecksAvailable;
    public LocalizedString deckSelectionFailed;
    public LocalizedString deckSelectionSuccess;
    
    private void Start()
    {
        WDebug.Log("LobbyUIController Start");
        LoadingPage.Instance.IsLoading = true;
        deckDropdown.onValueChanged.AddListener(OnDropdownChanged);
        StartCoroutine(LoadUserInfo());
    }
    
    private IEnumerator LoadUserInfo()
    {
        if (SceneContext.User == null)
        {
            yield return UserInfoGetter.GetUserInfo();
        }
        
        lobbyUserNameUI.SetUserName(SceneContext.User.name);
        yield return FetchDecks();
    }

    public IEnumerator FetchDecks()
    {
        if (userDecks != null && userDecks.Length > 0)
        {
            PopulateDropdown();
        }
        
        string url = $"{ServerList.MatchingServer.url}/api/users/mine/decks";
        using var www = UnityWebRequest.Get(url);
        Server.SetAcceptLanguage(www);
        www.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SystemMessageUI.Instance.ShowMessage(deckLoadFailed);
            WDebug.LogError($"덱 리스트 로드 실패: {www.error}");
            yield break;
        }

        // JsonHelper 는 이전에 정의한 generic 래퍼 유틸리티
        userDecks = JsonHelper.FromJson<DeckResponseDto>(www.downloadHandler.text);
        
        if (userDecks == null || userDecks.Length == 0)
        {
            SystemMessageUI.Instance.ShowMessage(noDecksAvailable);
            WDebug.LogWarning("덱이 하나도 없습니다.");
            yield break;
        }
        
        PopulateDropdown();
        //sessiontracking 테스트
        StartCoroutine(StatusTracker.GetUserStatus());
    }

    // 2) 드랍다운 옵션 갱신
    private void PopulateDropdown()
    {
        // 옵션 이름만 뽑아서 리스트로
        var names = userDecks.Select(d => d.name).ToList();

        // 드랍다운 옵션 클리어 후 추가
        deckDropdown.ClearOptions();
        deckDropdown.AddOptions(names);
        
        // 현재 선택된 덱 인덱스 찾아 세팅
        int idx = userDecks
            .Select(d => d.id)
            .ToList()
            .IndexOf(SceneContext.User.selectedDeckId);
        idx = Mathf.Clamp(idx, 0, names.Count - 1);

        deckDropdown.value = idx;
        deckDropdown.RefreshShownValue();
        
        UpdateCaption(names[idx]);
        
        LoadingPage.Instance.IsLoading = false;
    }

    // 3) 드랍다운에서 선택 바뀌었을 때
    public void OnDropdownChanged(int newIndex)
    {
        
        var selected = userDecks[newIndex];
        DeckSceneContext.CurrentDeck = selected;     // 컨텍스트 갱신
        WDebug.Log($"index: {newIndex} 선택된 덱: {selected.name} (ID: {selected.id})");
        UpdateCaption(selected.name);                // 상단 텍스트 갱신
        StartCoroutine(SelectDeckCoroutine(DeckSceneContext.CurrentDeck.id));
    }
    private IEnumerator SelectDeckCoroutine(long deckId)
    {
        string url = $"{ServerList.MatchingServer.url}/api/users/mine/decks/{deckId}";
        using var www = UnityWebRequest.Post(url, new WWWForm());
        
        Server.SetAuthorization(www);
        Server.SetAcceptLanguage(www);
        
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SystemMessageUI.Instance.ShowMessage(deckSelectionFailed);
            WDebug.LogError($"덱 선택 실패: {www.responseCode} / {www.error}");
        }
        else
        {
            SystemMessageUI.Instance.ShowMessage(deckSelectionSuccess);
            WDebug.Log("덱 선택 성공: " + www.downloadHandler.text);
        }
    }
    private void UpdateCaption(string deckName)
    {
        if (deckDropdown.captionText != null)
            deckDropdown.captionText.text = deckName;
    }
}
