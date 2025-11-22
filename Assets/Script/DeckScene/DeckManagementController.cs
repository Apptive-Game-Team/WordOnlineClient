using System;
using System.Collections;
using System.Linq;
using System.Text;
using Script.Data;
using Script.Data.Deck;
using Script.Global;
using Script.Global.Util;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Script.DeckScene
{

    public class DeckManagementController : MonoBehaviour
    {
        [Header("UI")]
        public Transform deckListContainer;      // 덱 리스트
        public GameObject deckPrefab; // 덱 
        public Transform deckCardsContainer;     // 선택된 덱 카드
        public Transform ownedCardsContainer;    // 보유 카드
        public GameObject cardItemPrefab;        // 카드 UI 프리팹
        public GameObject cardInDeckItemPrefab;        // 덱 카드 UI 프리팹
        public GameObject createDeckPrefab;        // 덱 생성 UI 프리팹
        public Button submitDeckButton;        // 덱 제출 버튼
        public InputField deckNameInputField; // 덱 이름 입력 필드
        
        private static DeckResponseDto[] userDecks;
        private static CardDto[] ownedCards;
        
        public LocalizedString newDeck;
        public LocalizedString deckCreationFailed;
        public LocalizedString deckCreationSuccess;
        public LocalizedString deckUpdateFailed;
        public LocalizedString deckUpdateSuccess;
        public LocalizedString errorCardCount;
        public LocalizedString errorAttributeCount;
        public LocalizedString errorMagicCount;
        
        void Start() {
            LoadingPage.Instance.IsLoading = true;
            if (userDecks != null && userDecks.Length > 0) {
                PopulateDeckList();
                PopulateOwnedCardsList();
            }
            StartCoroutine(LoadAll());
        }

        IEnumerator LoadAll() {
            var token = SceneContext.JwtToken; 

            // 2) 요청 생성 (GET)
            string urlCards = ServerList.MatchingServer.url + "/api/users/mine/cards";
            using var wwwPool = new UnityWebRequest(urlCards, "GET");
            Server.SetAcceptLanguage(wwwPool);
        
            // 3) 헤더 설정
            wwwPool.SetRequestHeader("Authorization", "Bearer " + token);

            // 4) 다운로드 핸들러 할당
            wwwPool.downloadHandler = new DownloadHandlerBuffer();

            // 5) 전송
            yield return wwwPool.SendWebRequest();

            // 6) 결과 체크
            if (wwwPool.result != UnityWebRequest.Result.Success) {
                WDebug.LogError(
                    $"보유 카드 풀 로드 실패: {wwwPool.responseCode} / {wwwPool.error}\n{wwwPool.downloadHandler.text}"
                );
                yield break;
            }

            // 7) 파싱
            WDebug.Log($"보유 카드 리스트: {wwwPool.downloadHandler.text}");
            var poolDto = JsonUtility.FromJson<CardPoolDto>(wwwPool.downloadHandler.text);
            ownedCards = poolDto.cards ?? Array.Empty<CardDto>();
            

            // 2) 요청 생성 (GET)
            string urlDeck = ServerList.MatchingServer.url  + "/api/users/mine/decks";
            using var wwwDecks = new UnityWebRequest(urlDeck, "GET");
        
            // 3) 헤더 설정
            wwwDecks.SetRequestHeader("Authorization", "Bearer " + token);

            // 4) 다운로드 핸들러 할당
            wwwDecks.downloadHandler = new DownloadHandlerBuffer();

            // 5) 전송
            yield return wwwDecks.SendWebRequest();

            // 6) 결과 체크
            if (wwwDecks.result != UnityWebRequest.Result.Success) {
                WDebug.LogError(
                    $"유저 덱 리스트 로드 실패: {wwwDecks.responseCode} / {wwwDecks.error}\n{wwwDecks.downloadHandler.text}"
                );
                yield break;
            }

            // 7) 파싱
            WDebug.Log($"유저 덱 리스트: {wwwDecks.downloadHandler.text}");
            userDecks = JsonHelper.FromJson<DeckResponseDto>(wwwDecks.downloadHandler.text);
            WDebug.Log(userDecks.Length);
        
            PopulateDeckList();
            PopulateOwnedCardsList();
            
            LoadingPage.Instance.IsLoading = false;
        }
        
        void PopulateOwnedCardsList() {
            foreach (Transform child in ownedCardsContainer)
            {
                Destroy(child.gameObject);
            }
            
            var summary = ownedCards
                .GroupBy(c => c.name)                
                .Select(g => new { 
                    Name  = g.Key,                  
                    Count = g.Count()
                })
                .ToList();                          
        
            // 보유 카드 표시
            foreach (var card in summary) {
                var item = Instantiate(cardItemPrefab, ownedCardsContainer);
                var ui = item.GetComponent<CardItemUI>();
                var btn = item.GetComponent<Button>();
                
                btn.onClick.AddListener(() => OnOwnedCardSelected(
                    Array.Find(ownedCards, c=> c.name == card.Name))
                );
                ui.Init(card.Name,card.Count);
            }
        }

        void PopulateDeckList() {
            foreach (Transform child in deckListContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var deck in userDecks) {
                var deckObject = Instantiate(deckPrefab, deckListContainer);
                var btn = deckObject.GetComponent<Button>();
                var deckUI = deckObject.GetComponent<DeckItemUI>();
            
                btn.onClick.AddListener(() => OnDeckSelected(deck));
                deckUI.Init(deck.name);
            }

            GameObject createDeckObject = Instantiate(createDeckPrefab, deckListContainer);
            Button createDeckButton = createDeckObject.GetComponent<Button>();
            createDeckButton.onClick.AddListener(() => OnNewDeckSelected());
        }
        
        private void ReloadDeckList() {
            var summary = DeckSceneContext.CurrentDeck.cards
            .GroupBy(c => c.name)                
            .Select(g => new { 
                Name  = g.Key,                  
                Count = g.Count()               
            })
            .ToList(); 
        
            // 덱 카드 표시
            foreach (Transform t in deckCardsContainer) Destroy(t.gameObject);
            foreach (var cardInDeck in summary) {
                var item = Instantiate(cardInDeckItemPrefab, deckCardsContainer);
                var ui = item.GetComponent<CardItemUI>();
                var btn = item.GetComponent<Button>();
                
                btn.onClick.AddListener(() => OnCardInDeckSelected(
                    Array.Find(ownedCards, c=> c.name == cardInDeck.Name))
                );
                ui.Init(cardInDeck.Name, cardInDeck.Count);
            }   
        }
        
        void OnDeckSelected(DeckResponseDto deck) {
            WDebug.Log($"OnDeckSelected: {deck.name} (ID: {deck.id})");
            
            //DeckContext에 현재 덱 저장
            DeckSceneContext.CurrentDeck = deck;
            deckNameInputField.SetTextWithoutNotify(deck.name);

            ReloadDeckList();

            submitDeckButton.onClick.RemoveAllListeners();
            submitDeckButton.onClick.AddListener(() => OnDeckSubmit());
        }
        async void OnNewDeckSelected() {
            WDebug.Log("OnNewDeckSelected");

            string newDeckString = await newDeck.GetLocalizedStringAsync().Task;
            
            DeckResponseDto deck = new DeckResponseDto(){id = -1, name = newDeckString, cards = Array.Empty<CardDto>()};
            
            foreach (Transform t in deckCardsContainer) Destroy(t.gameObject);
            
            //DeckContext에 현재 덱 저장
            DeckSceneContext.CurrentDeck = deck;
            deckNameInputField.SetTextWithoutNotify(deck.name);
            
            submitDeckButton.onClick.RemoveAllListeners();
            submitDeckButton.onClick.AddListener(() => OnNewDeckSubmit());
        }

        void OnOwnedCardSelected(CardDto card)
        {
            WDebug.Log($"OnOwnedCardSelected: {card.name} (ID: {card.id})");
            if (DeckSceneContext.CurrentDeck.cards.Length < 10  && 
                DeckSceneContext.CurrentDeck.cards.Count(c => c.id == card.id)
                < ownedCards.Count(c => c.id == card.id))
            {
                WDebug.Log($"Adding card to deck: {card.name} (ID: {card.id})");
                var cardList = DeckSceneContext.CurrentDeck.cards.ToList();
                cardList.Add(card);
                DeckSceneContext.CurrentDeck.cards = cardList.ToArray();
            }

            ReloadDeckList();
        }

        void OnCardInDeckSelected(CardDto card)
        {
            var cardList = DeckSceneContext.CurrentDeck.cards.ToList();
            var toDelete = cardList.FirstOrDefault(c => c.id == card.id);
            if (toDelete != null) cardList.Remove(toDelete);
            DeckSceneContext.CurrentDeck.cards = cardList.ToArray();
            OnDeckSelected(DeckSceneContext.CurrentDeck);
        }

        private void OnDeckSubmit()
        {
            WDebug.Log("OnDeckSubmit");
            StartCoroutine(PutDeckCoroutine(DeckSceneContext.CurrentDeck));
        }

        private void OnNewDeckSubmit()
        {
            WDebug.Log("OnNewDeckSubmit");
            StartCoroutine(PostDeckCoroutine(DeckSceneContext.CurrentDeck));
        }
        
        private IEnumerator PostDeckCoroutine(DeckResponseDto deck)
        {
            if (!ValidateDeck(deck))
            {
                yield break;
            }
            
            var reqDto = new DeckRequestDto {
                name    = deckNameInputField.text,
                cardIds = deck.cards.Select(c => c.id).ToArray()
            };
            
            string json = JsonUtility.ToJson(reqDto);
            WDebug.Log("POST 덱 페이로드: " + json);

            // 2) 요청 생성
            string url = $"{ServerList.MatchingServer.url}/api/users/mine/decks";
            using var www = new UnityWebRequest(url, "POST")
            {
                uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            // 3) 헤더 설정
            www.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
            www.SetRequestHeader("Content-Type", "application/json");

            // 4) 전송
            yield return www.SendWebRequest();

            // 5) 결과 확인
            if (www.result != UnityWebRequest.Result.Success)
            {
                SystemMessageUI.Instance.ShowMessage(deckCreationFailed);
                WDebug.LogError($"덱 생성 실패: {www.responseCode} / {www.error}\n{www.downloadHandler.text}");
            }
            else
            {
                SystemMessageUI.Instance.ShowMessage(deckCreationSuccess);
                WDebug.Log($"덱 생성 성공: {www.downloadHandler.text}");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        
        private IEnumerator PutDeckCoroutine(DeckResponseDto deck)
        {
            if (!ValidateDeck(deck))
            {
                yield break;
            }
            
            var reqDto = new DeckRequestDto {
                name    = deckNameInputField.text,
                cardIds = deck.cards.Select(c => c.id).ToArray()
            };
            
            string json = JsonUtility.ToJson(reqDto);
            WDebug.Log("PUT 덱 페이로드: " + json);

            // 2) 요청 생성
            string url = $"{ServerList.MatchingServer.url}/api/users/mine/decks/{deck.id}";
            using var www = new UnityWebRequest(url, "PUT")
            {
                uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            // 3) 헤더 설정
            www.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
            www.SetRequestHeader("Content-Type", "application/json");

            // 4) 전송
            yield return www.SendWebRequest();

            // 5) 결과 확인
            if (www.result != UnityWebRequest.Result.Success)
            {
                SystemMessageUI.Instance.ShowMessage(deckUpdateFailed);
                WDebug.LogError($"덱 수정 실패: {www.responseCode} / {www.error}\n{www.downloadHandler.text}");
            }
            else
            {
                SystemMessageUI.Instance.ShowMessage(deckUpdateSuccess);
                WDebug.Log($"덱 수정 성공: {www.downloadHandler.text}");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private bool ValidateDeck(DeckResponseDto deck)
        {
            int typeCount  = deck.cards
                .Where(c => c.type == "Type")
                .Select(c => c.name)     // 이름 기준으로
                .Distinct()              // 중복 제거
                .Count();
            int magicCount = deck.cards
                .Where(c => c.type == "Magic")
                .Select(c => c.name)
                .Distinct()
                .Count();

            WDebug.Log($"Submit Try - cards : {deck.cards.Length} type : {typeCount} magic : {magicCount}");
            
            
            if (deck.cards.Length != 10)
            {
                SystemMessageUI.Instance.ShowMessage(errorCardCount);
                return false;
            }
            else if (typeCount < 2)
            {
                SystemMessageUI.Instance.ShowMessage(errorAttributeCount);
                return false;
            }
            else if (magicCount < 2)
            {
                SystemMessageUI.Instance.ShowMessage(errorMagicCount);
                return false;
            }
            return true;
        }
    }
}