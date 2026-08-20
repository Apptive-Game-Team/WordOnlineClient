using System.Collections.Generic;
using Data;
using Data.Magic;
using GameScene.Dto;
using GameScene.ServedObjectComponent;
using Global;
using Global.Util;
using UnityEngine;
using Global.Serialization;

namespace GameScene.Card
{
    public class CardInputSender : LocalSingletonObject<CardInputSender>, ICardSender
    {
        private class PendingInputRequest
        {
            public readonly List<CardUI> cards;

            public PendingInputRequest(List<CardUI> cards)
            {
                this.cards = cards;
            }
        }

        private const string Success = "SUCCESS";
        private const string FailInvalidMagic = "FAIL_INVALID_MAGIC";

        // 훈수 시스템이 구독한다. 정적 이벤트이므로 구독자는 씬을 떠날 때 반드시 해제해야 한다.
        public static event System.Action OnCardUsed;
        public static event System.Action OnMagicFailed;
        public static event System.Action OnMagicSucceeded;

        private readonly Dictionary<int, PendingInputRequest> inputRequestDict = new Dictionary<int, PendingInputRequest>();
        private readonly List<string> _currentCardNameList = new List<string>();
        private readonly List<CardUI> _currentCardList = new List<CardUI>();

        // FieldSelector가 매 프레임 호출하는 조회 전용 경로에서 쓰는 재사용 버퍼.
        // 호출자가 리스트를 보관하지 않는 경우에만 쓴다.
        private readonly List<CardType> _recipeQueryBuffer = new List<CardType>();
    
        public bool CanSelectField => _currentCardList.Count >= 1;
        private bool isFieldSelectMode = false;
        private bool isWaitingInputResponse = false;
        
        private CombinedMagicResolver combinedMagicResolver;

        protected override void Awake()
        {
            base.Awake();
            combinedMagicResolver = FindObjectOfType<CombinedMagicResolver>();
        }

        public bool IsFieldSelectMode()
        {
            return isFieldSelectMode;    
        }

        public bool IsWaitingInputResponse()
        {
            return isWaitingInputResponse;
        }

        public void CancelUseCard(CardUI cardObj)
        {
            if (isWaitingInputResponse)
            {
                return;
            }

            WDebug.Log($"CancelUseCard: {cardObj.CardName}");
            if (_currentCardNameList.Contains(cardObj.CardName))
            {
                _currentCardNameList.Remove(cardObj.CardName);
                _currentCardList.Remove(cardObj);
                SendCardSelectionInput(new CardUnselectRequestDto(cardObj.CardType));
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Cancel();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Confirm();
            }

            if (CardHotkey.TryGetPressedSlotIndex(out int slotIndex))
            {
                ToggleCardBySlot(slotIndex);
            }
        }

        private void ToggleCardBySlot(int slotIndex)
        {
            if (isWaitingInputResponse || GameSceneUIController.Instance == null)
            {
                return;
            }

            CardUI card = GameSceneUIController.Instance.GetCardAt(slotIndex);
            if (card != null)
            {
                card.OnCardClicked();
            }
        }

        public void Cancel()
        {
            if (isWaitingInputResponse)
            {
                return;
            }

            if (isFieldSelectMode)
            {
                CancelAll();
            }
        }
    
        public void Confirm()
        {
            if (isWaitingInputResponse)
            {
                return;
            }

            if (CanSelectField)
            {
                if (!combinedMagicResolver.CanResolve(GetCurrentRecipeTypes()))
                {
                    WDebug.Log("Cannot resolve the current recipe.");
                    PlayerFeedbackController.Instance.UseMagicFeedback();
                    OnMagicFailed?.Invoke();
                    SendInput(Vector2.zero);
                    return;
                }
            
                isFieldSelectMode = true;
            }
        }
    

        public string GetMagicName()
        {
            string result =
                _currentCardNameList.Find(c => c.Contains("Build")) ??
                _currentCardNameList.Find(c => c.Contains("Spawn")) ??
                _currentCardNameList.Find(c => c.Contains("Explode")) ??
                _currentCardNameList.Find(c => c.Contains("Drop")) ??
                _currentCardNameList.Find(c => c.Contains("Shoot"));
            return result;
        }

        public bool TryGetCurrentMagicData(out CombinedMagicData data)
        {
            if (combinedMagicResolver == null)
            {
                data = null;
                return false;
            }

            // TryResolve는 recipe를 읽기만 하므로 프레임마다 새 리스트를 만들 필요가 없다.
            return combinedMagicResolver.TryResolve(FillRecipeTypes(_recipeQueryBuffer), out data);
        }
    
        private void CancelAll()
        {
            WDebug.Log("CancelAll");
            foreach (var card in _currentCardList)
            {
                card.SetCardActive(false);
                SendCardSelectionInput(new CardUnselectRequestDto(card.CardType));
            }
            _currentCardList.Clear();
            _currentCardNameList.Clear();
            PlayerFeedbackController.Instance.UseMagicFeedback();
            SetExpectedMagicUI();
            isFieldSelectMode = false;
        }

        public void TryUseCard(CardUI cardObj)
        {
            if (isWaitingInputResponse)
            {
                return;
            }

            AddCardList(cardObj);
            OnCardUsed?.Invoke();
        }

        public void SendInput(Vector3 pos) //whenFieldSelect
        {
            if (isWaitingInputResponse)
            {
                return;
            }

            var input = new CardUseInput(new List<string>(_currentCardNameList), pos);
            string json = JsonCodec.Serialize(input);
        
            string destination = $"/app/game/input/{SceneContext.MatchInfo.sessionId}/{SceneContext.UserID}";
            StompConnector.Instance.SendMessageToServer(destination, json);
            inputRequestDict[input.id] = new PendingInputRequest(new List<CardUI>(_currentCardList));
            isWaitingInputResponse = true;
            _currentCardNameList.Clear();
            _currentCardList.Clear();
            isFieldSelectMode = false;

            StopCoroutine(nameof(WaitInputResponseTimeout));
            StartCoroutine(nameof(WaitInputResponseTimeout));
        }

        public bool TrySendInput(Vector3 pos)
        {
            if (!isFieldSelectMode || isWaitingInputResponse)
            {
                return false;
            }

            SendInput(pos);
            return isWaitingInputResponse;
        }

        private System.Collections.IEnumerator WaitInputResponseTimeout()
        {
            yield return new WaitForSeconds(3f);
            if (isWaitingInputResponse)
            {
                WDebug.LogWarning("[CardInputSender] Input response timeout. Releasing lock.");
                isWaitingInputResponse = false;
                
                // 타임아웃 시 남아있는 PendingRequest 처리 (카드를 다시 복구하거나 파괴)
                foreach (var kvp in inputRequestDict)
                {
                    RestorePendingSelection(kvp.Value.cards);
                }
                inputRequestDict.Clear();
            }
        }

        public void HandleInputResponse(MagicValidInfo magicValid)
        {
            StopCoroutine(nameof(WaitInputResponseTimeout));

            if (!inputRequestDict.TryGetValue(magicValid.id, out PendingInputRequest pendingInputRequest))
            {
                WDebug.LogWarning($"[Magic Valid] Pending input request not found. id: {magicValid.id}");
                isWaitingInputResponse = false;
                return;
            }

            if (ShouldConsumeCards(magicValid))
            {
                foreach (CardUI cardUI in pendingInputRequest.cards)
                {
                    if (cardUI != null)
                    {
                        cardUI.Destroy();
                    }
                }

                isWaitingInputResponse = false;

                // FAIL_INVALID_MAGIC도 카드를 소비하므로 이 분기에 들어온다. 카드가 사라졌다고
                // 시전에 성공한 것은 아니라서 결과 코드로 다시 갈라야 한다.
                if (IsSuccess(magicValid))
                {
                    OnMagicSucceeded?.Invoke();
                }
                else
                {
                    OnMagicFailed?.Invoke();
                }
            }
            else
            {
                RestorePendingSelection(pendingInputRequest.cards);
                SystemMessageUI.Instance.ShowMessage(magicValid.message);
                WDebug.Log("[Magic Valid]" + magicValid.message);
                OnMagicFailed?.Invoke();
            }

            inputRequestDict.Remove(magicValid.id);
        }

        private void RestorePendingSelection(List<CardUI> cards)
        {
            isWaitingInputResponse = false;

            foreach (CardUI currentCard in _currentCardList)
            {
                if (currentCard != null)
                {
                    currentCard.SetCardActive(false);
                }
            }

            _currentCardNameList.Clear();
            _currentCardList.Clear();

            foreach (CardUI card in cards)
            {
                if (card == null)
                {
                    continue;
                }

                card.SetCardActive(true);
                _currentCardNameList.Add(card.CardName);
                _currentCardList.Add(card);
            }

            isFieldSelectMode = CanSelectField;
            SetExpectedMagicUI();
        }

        private static bool IsSuccess(MagicValidInfo magicValid)
        {
            if (string.IsNullOrEmpty(magicValid.resultCode))
            {
                return magicValid.valid;
            }

            return string.Equals(magicValid.resultCode, Success, System.StringComparison.Ordinal);
        }

        private static bool ShouldConsumeCards(MagicValidInfo magicValid)
        {
            if (string.IsNullOrEmpty(magicValid.resultCode))
            {
                return magicValid.valid;
            }

            return string.Equals(magicValid.resultCode, Success, System.StringComparison.Ordinal)
                   || string.Equals(magicValid.resultCode, FailInvalidMagic, System.StringComparison.Ordinal);
        }
    
        private void AddCardList(CardUI card)
        {
            WDebug.Log("AddCardList: " + card.CardName);
            _currentCardNameList.Add(card.CardName);
            _currentCardList.Add(card);
            SendCardSelectionInput(new CardSelectRequestDto(card.CardType));
        }

        private static void SendCardSelectionInput(object input)
        {
            string json = JsonCodec.Serialize(input);
            string destination = $"/app/game/input/{SceneContext.MatchInfo.sessionId}/{SceneContext.UserID}";
            StompConnector.Instance.SendMessageToServer(destination, json);
        }

        private List<CardType> GetCurrentRecipeTypes()
        {
            return FillRecipeTypes(new List<CardType>(_currentCardList.Count));
        }

        private List<CardType> FillRecipeTypes(List<CardType> buffer)
        {
            buffer.Clear();
            foreach (var c in _currentCardList)
            {
                if (CardNameMapper.TryMapToCardType(c.CardName, out var t))
                    buffer.Add(t);
                else
                    WDebug.LogWarning($"[CardInputSender] Unknown CardName → CardType map: {c.CardName}");
            }
            return buffer;
        }
        public void SetExpectedMagicUI()
        {
            GameSceneUIController.Instance.TrySetExpectedMagicUI(GetCurrentRecipeTypes());
        }
    }
}
