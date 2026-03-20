using System;
using System.Collections;
using System.Linq;
using System.Text;
using Data;
using Data.Deck;
using DeckScene;
using Global;
using Global.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace LobbyScene
{
    public class LobbyUIController : MonoBehaviour
    {
        [SerializeField] LobbyUserNameUI lobbyUserNameUI;
        [SerializeField] private TMP_Dropdown deckDropdown;
        [SerializeField] private UnityEngine.UI.Button arrowButton;
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
            yield return QuestRewardTracker.CheckAndShowRewards();
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
            Server.SetAuthorization(www);
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                SystemMessageUI.Instance.ShowMessage(deckLoadFailed);
                WDebug.LogError($"덱 리스트 로드 실패: {www.error}");
                LoadingPage.Instance.IsLoading = false;
                SceneManager.LoadScene("LoginScene");
                yield break;
            }

            // JsonHelper 는 이전에 정의한 generic 래퍼 유틸리티
            userDecks = JsonHelper.FromJson<DeckResponseDto>(www.downloadHandler.text);
        
            if (userDecks == null || userDecks.Length == 0)
            {
                SystemMessageUI.Instance.ShowMessage(noDecksAvailable);
                WDebug.LogWarning("덱이 하나도 없습니다.");
                SceneManager.LoadScene("LoginScene");
                yield break;
            }
        
            PopulateDropdown();
        
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

    public static class QuestRewardTracker
    {
        [Serializable]
        public class QuestRewardDto
        {
            public string rewardType;
            public long rewardId;
            public int amount;
            public long questId;

            // Compatibility fields for temporary backend naming differences.
            public string type;
            public long id;
            public int value;
        }

        [Serializable]
        private class QuestRewardResponseDto
        {
            public QuestRewardDto[] rewards;
        }

        public static IEnumerator CheckAndShowRewards()
        {
            QuestRewardDto[] rewards = Array.Empty<QuestRewardDto>();
            yield return CheckRewards(result => rewards = result ?? Array.Empty<QuestRewardDto>());

            if (rewards.Length == 0)
            {
                yield break;
            }

            ShowRewardMessage(rewards);
        }

        private static IEnumerator CheckRewards(Action<QuestRewardDto[]> onSuccess)
        {
            var url = $"{ServerList.MatchingServer.url}/api/users/mine/quests/check";

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>());
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 404 || request.responseCode == 405)
                {
                    WDebug.Log("[CheckQuestRewards] endpoint is not ready yet. Skip reward visualization.");
                }
                else
                {
                    WDebug.LogError($"[CheckQuestRewards] fail: {request.responseCode} / {request.error}");
                }
                onSuccess?.Invoke(Array.Empty<QuestRewardDto>());
                yield break;
            }

            if (request.responseCode == 204 || string.IsNullOrWhiteSpace(request.downloadHandler.text))
            {
                onSuccess?.Invoke(Array.Empty<QuestRewardDto>());
                yield break;
            }

            try
            {
                onSuccess?.Invoke(ParseRewards(request.downloadHandler.text));
            }
            catch (Exception e)
            {
                WDebug.LogError($"[CheckQuestRewards] parse error: {e}\n{request.downloadHandler.text}");
                onSuccess?.Invoke(Array.Empty<QuestRewardDto>());
            }
        }

        private static QuestRewardDto[] ParseRewards(string json)
        {
            var trimmed = json.TrimStart();
            if (trimmed.StartsWith("["))
            {
                return JsonHelper.FromJson<QuestRewardDto>(json) ?? Array.Empty<QuestRewardDto>();
            }

            var response = JsonUtility.FromJson<QuestRewardResponseDto>(json);
            return response?.rewards ?? Array.Empty<QuestRewardDto>();
        }

        private static void ShowRewardMessage(QuestRewardDto[] rewards)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Quest rewards received:");

            foreach (var reward in rewards)
            {
                var rewardType = GetRewardType(reward);
                var rewardId = GetRewardId(reward);
                var amount = GetAmount(reward);
                builder.Append("- ").Append(rewardType).Append(" #").Append(rewardId).Append(" x").Append(amount);
                if (reward.questId > 0)
                {
                    builder.Append(" (quest ").Append(reward.questId).Append(")");
                }
                builder.AppendLine();
            }

            if (SystemMessageUI.Instance != null)
            {
                SystemMessageUI.Instance.ShowMessage(builder.ToString().TrimEnd());
                return;
            }

            WDebug.Log(builder.ToString());
        }

        private static string GetRewardType(QuestRewardDto reward)
        {
            if (!string.IsNullOrEmpty(reward.rewardType))
            {
                return reward.rewardType;
            }

            if (!string.IsNullOrEmpty(reward.type))
            {
                return reward.type;
            }

            return "UNKNOWN";
        }

        private static long GetRewardId(QuestRewardDto reward)
        {
            if (reward.rewardId > 0)
            {
                return reward.rewardId;
            }

            return reward.id;
        }

        private static int GetAmount(QuestRewardDto reward)
        {
            if (reward.amount > 0)
            {
                return reward.amount;
            }

            if (reward.value > 0)
            {
                return reward.value;
            }

            return 1;
        }
    }
}
