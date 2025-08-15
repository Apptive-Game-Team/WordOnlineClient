using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Script.DeckScene
{
    public static class JsonHelper {
        public static T[] FromJson<T>(string json) {
            string wrapped = "{\"Items\":" + json + "}";
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return wrapper.Items;
        }
        [System.Serializable]
        private class Wrapper<T> {
            public T[] Items;
        }
    }
    [System.Serializable]
    public enum Type
    {
        Magic,
        Type
    }

    [System.Serializable]
    public class CardDto {
        public long id;
        public string name;
        public string type;
    }

    [System.Serializable]
    public class DeckResponseDto {
        public long id;
        public string name;
        public CardDto[] cards;
    }
    [Serializable]
    public class DeckRequestDto
    {
        public string   name;
        public long[]   cardIds;
    }
    [System.Serializable]
    public class CardPoolDto {
        public CardDto[] cards;
    }

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

        private DeckResponseDto[] userDecks;
        private CardDto[] ownedCards;

        void Start() {
            StartCoroutine(LoadAll());
        }

        IEnumerator LoadAll() {
        
#if UNITY_WEBGL && !UNITY_EDITOR
        var token = SceneContext.JwtToken; 

        // 2) 요청 생성 (GET)
        string urlCards = SceneContext.ServerUrl + "/api/users/mine/cards";
        using var wwwPool = new UnityWebRequest(urlCards, "GET");
    
        // 3) 헤더 설정
        wwwPool.SetRequestHeader("Authorization", "Bearer " + token);

        // 4) 다운로드 핸들러 할당
        wwwPool.downloadHandler = new DownloadHandlerBuffer();

        // 5) 전송
        yield return wwwPool.SendWebRequest();

        // 6) 결과 체크
        if (wwwPool.result != UnityWebRequest.Result.Success) {
            Debug.LogError(
                $"보유 카드 풀 로드 실패: {wwwPool.responseCode} / {wwwPool.error}\n{wwwPool.downloadHandler.text}"
            );
            yield break;
        }

        // 7) 파싱
        Debug.Log($"보유 카드 리스트: {wwwPool.downloadHandler.text}");
        var poolDto = JsonUtility.FromJson<CardPoolDto>(wwwPool.downloadHandler.text);
        ownedCards = poolDto.cards ?? Array.Empty<CardDto>();
        

        // 2) 요청 생성 (GET)
        string urlDeck = SceneContext.ServerUrl + "/api/users/mine/decks";
        using var wwwDecks = new UnityWebRequest(urlDeck, "GET");
    
        // 3) 헤더 설정
        wwwDecks.SetRequestHeader("Authorization", "Bearer " + token);

        // 4) 다운로드 핸들러 할당
        wwwDecks.downloadHandler = new DownloadHandlerBuffer();

        // 5) 전송
        yield return wwwDecks.SendWebRequest();

        // 6) 결과 체크
        if (wwwDecks.result != UnityWebRequest.Result.Success) {
            Debug.LogError(
                $"유저 덱 리스트 로드 실패: {wwwDecks.responseCode} / {wwwDecks.error}\n{wwwDecks.downloadHandler.text}"
            );
            yield break;
        }

        // 7) 파싱
        Debug.Log($"유저 덱 리스트: {wwwDecks.downloadHandler.text}");
        userDecks = JsonHelper.FromJson<DeckResponseDto>(wwwDecks.downloadHandler.text);
        Debug.Log(userDecks.Length);
        
#endif
        

#if UNITY_EDITOR
            ownedCards = JsonUtility.FromJson<CardPoolDto>("{\"cards\":[{\"id\":1,\"name\":\"Fire\",\"type\":\"Type\"},{\"id\":1,\"name\":\"Fire\",\"type\":\"Type\"},{\"id\":1,\"name\":\"Fire\",\"type\":\"Type\"},{\"id\":2,\"name\":\"Water\",\"type\":\"Type\"},{\"id\":2,\"name\":\"Water\",\"type\":\"Type\"},{\"id\":2,\"name\":\"Water\",\"type\":\"Type\"},{\"id\":3,\"name\":\"Lightning\",\"type\":\"Type\"},{\"id\":3,\"name\":\"Lightning\",\"type\":\"Type\"},{\"id\":3,\"name\":\"Lightning\",\"type\":\"Type\"},{\"id\":4,\"name\":\"Rock\",\"type\":\"Type\"},{\"id\":4,\"name\":\"Rock\",\"type\":\"Type\"},{\"id\":4,\"name\":\"Rock\",\"type\":\"Type\"},{\"id\":5,\"name\":\"Leaf\",\"type\":\"Type\"},{\"id\":5,\"name\":\"Leaf\",\"type\":\"Type\"},{\"id\":5,\"name\":\"Leaf\",\"type\":\"Type\"},{\"id\":6,\"name\":\"Shoot\",\"type\":\"Magic\"},{\"id\":6,\"name\":\"Shoot\",\"type\":\"Magic\"},{\"id\":6,\"name\":\"Shoot\",\"type\":\"Magic\"},{\"id\":7,\"name\":\"Summon\",\"type\":\"Magic\"},{\"id\":7,\"name\":\"Summon\",\"type\":\"Magic\"},{\"id\":7,\"name\":\"Summon\",\"type\":\"Magic\"},{\"id\":8,\"name\":\"Spawn\",\"type\":\"Magic\"},{\"id\":8,\"name\":\"Spawn\",\"type\":\"Magic\"},{\"id\":8,\"name\":\"Spawn\",\"type\":\"Magic\"},{\"id\":9,\"name\":\"Explode\",\"type\":\"Magic\"},{\"id\":9,\"name\":\"Explode\",\"type\":\"Magic\"},{\"id\":9,\"name\":\"Explode\",\"type\":\"Magic\"}]}").cards;
            userDecks = JsonHelper.FromJson<DeckResponseDto>("[{\"id\":3,\"name\":\"기본 덱\",\"cards\":[{\"id\":1,\"name\":\"Fire\",\"type\":\"Type\"},{\"id\":2,\"name\":\"Water\",\"type\":\"Type\"},{\"id\":3,\"name\":\"Lightning\",\"type\":\"Type\"},{\"id\":4,\"name\":\"Rock\",\"type\":\"Type\"},{\"id\":5,\"name\":\"Leaf\",\"type\":\"Type\"},{\"id\":6,\"name\":\"Shoot\",\"type\":\"Magic\"},{\"id\":6,\"name\":\"Shoot\",\"type\":\"Magic\"},{\"id\":7,\"name\":\"Summon\",\"type\":\"Magic\"},{\"id\":8,\"name\":\"Spawn\",\"type\":\"Magic\"},{\"id\":9,\"name\":\"Explode\",\"type\":\"Magic\"}]}]");
#endif
        
            PopulateDeckList();
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

            yield return null;
        }

        void PopulateDeckList() {
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
        
        void OnDeckSelected(DeckResponseDto deck) {
            var summary = deck.cards
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
            
            //DeckContext에 현재 덱 저장
            DeckSceneContext.CurrentDeck = deck;
            submitDeckButton.onClick.AddListener(() => OnDeckSubmit());
        }
        void OnNewDeckSelected() {
            DeckResponseDto deck = new DeckResponseDto(){id = userDecks[0].id, name = "새 덱", cards = Array.Empty<CardDto>()};
            
            //DeckContext에 현재 덱 저장
            DeckSceneContext.CurrentDeck = deck;
            submitDeckButton.onClick.AddListener(() => OnNewDeckSubmit());
        }

        void OnOwnedCardSelected(CardDto card)
        {
            if (DeckSceneContext.CurrentDeck.cards.Length < 10  && 
                DeckSceneContext.CurrentDeck.cards.Count(c => c.id == card.id)
                < ownedCards.Count(c => c.id == card.id))
            {
                var cardList = DeckSceneContext.CurrentDeck.cards.ToList();
                cardList.Add(card);
                DeckSceneContext.CurrentDeck.cards = cardList.ToArray();
            }
            OnDeckSelected(DeckSceneContext.CurrentDeck);
        }

        void OnCardInDeckSelected(CardDto card)
        {
            var cardList = DeckSceneContext.CurrentDeck.cards.ToList();
            var toDelete = cardList.FirstOrDefault(c => c.id == card.id);
            if (toDelete != null) cardList.Remove(toDelete);
            DeckSceneContext.CurrentDeck.cards = cardList.ToArray();
            OnDeckSelected(DeckSceneContext.CurrentDeck);
        }
        
        public void OnDeckSubmit()
        {
            StartCoroutine(PutDeckCoroutine(DeckSceneContext.CurrentDeck));
        }
        public void OnNewDeckSubmit()
        {
            StartCoroutine(PostDeckCoroutine(DeckSceneContext.CurrentDeck));
        }
        
        private IEnumerator PostDeckCoroutine(DeckResponseDto deck)
        {
            //vaild한가?
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

            Debug.Log($"Submit Try - cards : {deck.cards.Length} type : {typeCount} magic : {magicCount}");
            
            
            if (deck.cards.Length != 10 || typeCount < 2 || magicCount < 2)
            {
                Debug.LogError("invalid deck");
                yield break;
            }
            
            var reqDto = new DeckRequestDto {
                name    = deck.name,
                cardIds = deck.cards.Select(c => c.id).ToArray()
            };
            
            string json = JsonUtility.ToJson(reqDto);
            Debug.Log("POST 덱 페이로드: " + json);

            // 2) 요청 생성
            string url = $"{SceneContext.ServerUrl}/api/users/mine/decks/{deck.id}";
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
                SystemMessageUI.Instance.ShowMessage($"덱 수정 실패: {www.responseCode} / {www.error}\n{www.downloadHandler.text}");
                Debug.LogError($"덱 수정 실패: {www.responseCode} / {www.error}\n{www.downloadHandler.text}");
            }
            else
            {
                SystemMessageUI.Instance.ShowMessage($"덱 수정 성공: {www.downloadHandler.text}");
                Debug.Log($"덱 수정 성공: {www.downloadHandler.text}");
            }
        }
        
        private IEnumerator PutDeckCoroutine(DeckResponseDto deck)
        {
            //vaild한가?
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

            Debug.Log($"Submit Try - cards : {deck.cards.Length} type : {typeCount} magic : {magicCount}");
            
            
            if (deck.cards.Length != 10 || typeCount < 2 || magicCount < 2)
            {
                Debug.LogError("invalid deck");
                yield break;
            }
            
            var reqDto = new DeckRequestDto {
                name    = deck.name,
                cardIds = deck.cards.Select(c => c.id).ToArray()
            };
            
            string json = JsonUtility.ToJson(reqDto);
            Debug.Log("PUT 덱 페이로드: " + json);

            // 2) 요청 생성
            string url = $"{SceneContext.ServerUrl}/api/users/mine/decks";
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
                SystemMessageUI.Instance.ShowMessage($"덱 수정 실패: {www.responseCode} / {www.error}\n{www.downloadHandler.text}");
                Debug.LogError($"덱 수정 실패: {www.responseCode} / {www.error}\n{www.downloadHandler.text}");
            }
            else
            {
                SystemMessageUI.Instance.ShowMessage($"덱 수정 성공: {www.downloadHandler.text}");
                Debug.Log($"덱 수정 성공: {www.downloadHandler.text}");
            }
        }
    }
}