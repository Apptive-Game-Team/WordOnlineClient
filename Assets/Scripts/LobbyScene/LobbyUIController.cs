using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;
using Data.Deck;
using Data.Magic;
using DeckScene;
using GameScene.Card;
using Global;
using Global.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LobbyScene
{
    public class LobbyUIController : MonoBehaviour
    {
        [SerializeField] LobbyUserNameUI lobbyUserNameUI;
        [SerializeField] private TMP_Dropdown deckDropdown;
        [SerializeField] private UnityEngine.UI.Button arrowButton;
        [SerializeField] private GameObject rewardUiPrefab;
        [SerializeField] private CardImageMapper cardImageMapper;
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
            bool isHealthy = false;
            yield return DeployStatusChecker.CheckDeployStatus(result => isHealthy = result);

            if (!isHealthy)
            {
                LoadingPage.Instance.IsLoading = false;
                SceneManager.LoadScene("LoginScene");
                yield break;
            }

            if (SceneContext.User == null)
            {
                yield return UserInfoGetter.GetUserInfo();
            }
        
            lobbyUserNameUI.SetUserName(SceneContext.User.name);
            yield return QuestRewardTracker.CheckAndShowRewards(rewardUiPrefab, cardImageMapper);
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
        private const string RewardTypeCard = "CARD";
        private const string RewardTypeDecoration = "DECORATION";
        private const string RewardTypeMagic = "MAGIC";

        // Matching server CardType order: 1=Shoot,2=Build,3=Spawn,4=Explode,5=Drop,6=Fire...
        private static readonly Dictionary<long, string> ServerCardIdToName = new()
        {
            { 1, "Shoot" },
            { 2, "Build" },
            { 3, "Spawn" },
            { 4, "Explode" },
            { 5, "Drop" },
            { 6, "Fire" },
            { 7, "Water" },
            { 8, "Lightning" },
            { 9, "Rock" },
            { 10, "Nature" },
            { 11, "Wind" }
        };

#if UNITY_EDITOR
        private const string RewardUiPrefabEditorPath = "Assets/Prefabs/UI/RewardUI.prefab";
        private const string CardImageMapperEditorPath = "Assets/Art/Images/UI/Card/CardImageMapper.asset";
        private const string DecorationDbEditorPath = "Assets/Scripts/CustomizeScene/New Decoration Database.asset";
#endif

        private sealed class RewardVisual
        {
            public string RewardType { get; }
            public long RewardId { get; }
            public int Amount { get; }
            public Sprite Sprite { get; }

            public RewardVisual(string rewardType, long rewardId, int amount, Sprite sprite)
            {
                RewardType = rewardType;
                RewardId = rewardId;
                Amount = amount;
                Sprite = sprite;
            }
        }

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

        public static IEnumerator CheckAndShowRewards(
            GameObject rewardUiPrefab,
            CardImageMapper cardImageMapper)
        {
            QuestRewardDto[] rewards = Array.Empty<QuestRewardDto>();
            yield return CheckRewards(result => rewards = result ?? Array.Empty<QuestRewardDto>());

            if (rewards.Length == 0)
            {
                yield break;
            }

            if (!TryShowRewardUI(rewards, rewardUiPrefab, cardImageMapper))
            {
                ShowRewardMessage(rewards);
            }
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

        private static bool TryShowRewardUI(
            QuestRewardDto[] rewards,
            GameObject rewardUiPrefab,
            CardImageMapper cardImageMapper)
        {
            var resolvedCardImageMapper = ResolveCardImageMapper(cardImageMapper);

            var visuals = new List<RewardVisual>();
            foreach (var reward in rewards)
            {
                var rewardType = GetRewardType(reward).ToUpperInvariant();
                var rewardId = GetRewardId(reward);
                var amount = Mathf.Max(1, GetAmount(reward));

                if (TryResolveSprite(rewardType, rewardId, resolvedCardImageMapper, out var sprite))
                {
                    visuals.Add(new RewardVisual(rewardType, rewardId, amount, sprite));
                }
            }

            if (visuals.Count == 0)
            {
                return false;
            }

            var rewardUiInstance = ResolveRewardUIInstance(rewardUiPrefab);
            if (rewardUiInstance == null)
            {
                WDebug.LogWarning("[CheckQuestRewards] RewardUI could not be resolved.");
                return false;
            }

            rewardUiInstance.transform.localScale = Vector3.one;
            rewardUiInstance.SetActive(true);
            PopulateRewardUI(rewardUiInstance, visuals);
            return true;
        }

        private static bool TryResolveSprite(
            string rewardType,
            long rewardId,
            CardImageMapper cardImageMapper,
            out Sprite sprite)
        {
            sprite = null;

            switch (rewardType)
            {
                case RewardTypeCard:
                    return TryResolveCardSprite(rewardId, cardImageMapper, out sprite);
                case RewardTypeMagic:
                    return TryResolveMagicSprite(rewardId, out sprite);
                default:
                    return false;
            }
        }

        private static bool TryResolveCardSprite(long rewardId, CardImageMapper cardImageMapper, out Sprite sprite)
        {
            sprite = null;
            if (cardImageMapper == null)
            {
                return false;
            }

            if (ServerCardIdToName.TryGetValue(rewardId, out var cardName))
            {
                sprite = cardImageMapper.GetCardImage(cardName);
                if (sprite != null)
                {
                    return true;
                }
            }

            if (Enum.IsDefined(typeof(CardType), (int)rewardId))
            {
                var cardType = (CardType)(int)rewardId;
                if (cardType != CardType.Dummy)
                {
                    sprite = cardImageMapper.GetCardImage(cardType);
                }
            }

            return sprite != null;
        }

        private static bool TryResolveMagicSprite(long rewardId, out Sprite sprite)
        {
            sprite = LocalCombinedMagicData.dataList
                .FirstOrDefault(magicData => magicData.id == rewardId)
                ?.GetSprite();
            return sprite != null;
        }

        private static CardImageMapper ResolveCardImageMapper(CardImageMapper cardImageMapper)
        {
            if (cardImageMapper != null)
            {
                return cardImageMapper;
            }

            cardImageMapper = Resources.FindObjectsOfTypeAll<CardImageMapper>().FirstOrDefault();
            if (cardImageMapper != null)
            {
                return cardImageMapper;
            }

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<CardImageMapper>(CardImageMapperEditorPath);
#else
            return null;
#endif
        }

        private static GameObject ResolveRewardUIInstance(GameObject rewardUiPrefab)
        {
            if (rewardUiPrefab != null)
            {
                if (rewardUiPrefab.scene.IsValid())
                {
                    return rewardUiPrefab;
                }

                return UnityEngine.Object.Instantiate(rewardUiPrefab);
            }

            var sceneRewardUi = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(gameObject => gameObject.name == "RewardUI" && gameObject.scene.IsValid());
            if (sceneRewardUi != null)
            {
                return sceneRewardUi;
            }

#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(RewardUiPrefabEditorPath);
            if (prefab != null)
            {
                return UnityEngine.Object.Instantiate(prefab);
            }
#endif

            return null;
        }

        private static void PopulateRewardUI(GameObject rewardUI, IReadOnlyList<RewardVisual> visuals)
        {
            var panel = rewardUI.transform.Find("Panal");
            if (panel == null)
            {
                panel = rewardUI.transform;
            }

            var contentRoot = EnsureContentRoot(panel);
            ClearContent(contentRoot);

            for (var i = 0; i < visuals.Count; i++)
            {
                CreateRewardItem(contentRoot, visuals[i], i);
            }
        }

        private static RectTransform EnsureContentRoot(Transform panel)
        {
            var contentRoot = panel.Find("RewardContent") as RectTransform;
            if (contentRoot != null)
            {
                return contentRoot;
            }

            var contentObject = new GameObject("RewardContent", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            contentRoot = contentObject.GetComponent<RectTransform>();
            contentRoot.SetParent(panel, false);
            contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
            contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
            contentRoot.pivot = new Vector2(0.5f, 0.5f);
            contentRoot.anchoredPosition = new Vector2(0f, -30f);
            contentRoot.sizeDelta = new Vector2(430f, 230f);

            var layout = contentObject.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 14f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(6, 6, 6, 6);

            return contentRoot;
        }

        private static void ClearContent(Transform contentRoot)
        {
            for (var childIndex = contentRoot.childCount - 1; childIndex >= 0; childIndex--)
            {
                UnityEngine.Object.Destroy(contentRoot.GetChild(childIndex).gameObject);
            }
        }

        private static void CreateRewardItem(Transform contentRoot, RewardVisual visual, int index)
        {
            var itemObject = new GameObject(
                $"{visual.RewardType}_{visual.RewardId}_{index}",
                typeof(RectTransform),
                typeof(LayoutElement));

            var itemRect = itemObject.GetComponent<RectTransform>();
            itemRect.SetParent(contentRoot, false);
            itemRect.sizeDelta = new Vector2(120f, 170f);

            var itemLayout = itemObject.GetComponent<LayoutElement>();
            itemLayout.preferredWidth = 120f;
            itemLayout.preferredHeight = 170f;

            var imageObject = new GameObject("Image", typeof(RectTransform), typeof(Image));
            var imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.SetParent(itemRect, false);
            imageRect.anchorMin = new Vector2(0.5f, 1f);
            imageRect.anchorMax = new Vector2(0.5f, 1f);
            imageRect.pivot = new Vector2(0.5f, 1f);
            imageRect.anchoredPosition = new Vector2(0f, -8f);
            imageRect.sizeDelta = new Vector2(104f, 104f);

            var image = imageObject.GetComponent<Image>();
            image.sprite = visual.Sprite;
            image.preserveAspect = true;

            var amountObject = new GameObject("Amount", typeof(RectTransform), typeof(TextMeshProUGUI));
            var amountRect = amountObject.GetComponent<RectTransform>();
            amountRect.SetParent(itemRect, false);
            amountRect.anchorMin = new Vector2(0f, 0f);
            amountRect.anchorMax = new Vector2(1f, 0f);
            amountRect.pivot = new Vector2(0.5f, 0f);
            amountRect.anchoredPosition = new Vector2(0f, 8f);
            amountRect.sizeDelta = new Vector2(0f, 42f);

            var amountText = amountObject.GetComponent<TextMeshProUGUI>();
            amountText.text = $"x{visual.Amount}";
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.fontSize = 30f;
            amountText.color = Color.black;
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
