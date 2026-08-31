using System;
using System.Collections.Generic;
using Data;
using Data.Magic;
using GameScene.Card;
using GameScene.PopupBook;
using GameScene.ServedObjectComponent;
using Global;
using Global.Util;
using Unity.Mathematics;
using UnityEngine;

namespace TutorialScene
{
    public class TutorialCardSender : MonoBehaviour, ICardSender
    {
        private static readonly Vector3 CasterPosition = new Vector3(1f, 0f, 5f);

        public event Action<IReadOnlyList<CardType>> MagicUsed;
        public event Action SingleCardUsed;

        private readonly List<string> _currentCardNameList = new List<string>();
        private readonly List<TutorialCardUI> _currentCardList = new List<TutorialCardUI>();

        [SerializeField] GameObject shotPrefab;
        [SerializeField] GameObject mobPrefab;
        
        public bool CanSelectField => _currentCardList.Count >= 1;
        private bool isFieldSelectMode = false;

        public bool IsFieldSelectMode()
        {
            return isFieldSelectMode;
        }

        public void CancelUseCard(TutorialCardUI cardObj)
        {
            if (_currentCardNameList.Contains(cardObj.CardName))
            {
                _currentCardNameList.Remove(cardObj.CardName);
                _currentCardList.Remove(cardObj);
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
            if (TutorialSceneUIController.Instance == null)
            {
                return;
            }

            TutorialCardUI card = TutorialSceneUIController.Instance.GetCardAt(slotIndex);
            if (card != null)
            {
                card.OnCardClicked();
            }
        }

        public void Cancel()
        {
            if (isFieldSelectMode)
            {
                CancelAll();
            }
        }

        public void Confirm()
        {
            if (!CanSelectField)
                return;

            var types = GetCurrentRecipeTypes();

            if (!LocalCombinedMagicData.TryGetByRecipe(types, out _))
            {
                if (_currentCardList.Count == 1)
                {
                    SingleCardUsed?.Invoke();
                }

                WDebug.Log("Cannot resolve the current recipe.");
                TutorialSceneUIController.Instance?.PlayMagicFailEffect();
                ClearCurrentSelection();
                return;
            }

            isFieldSelectMode = true;
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

        public void CancelAll()
        {
            WDebug.Log("CancelAll");
            foreach (var card in _currentCardList)
            {
                card.SetCardActive(false);
            }

            _currentCardList.Clear();
            _currentCardNameList.Clear();
            FindObjectOfType<TutorialCardSender>().SetExpectedMagicUI();
            isFieldSelectMode = false;
        }

        public void TryUseCard(TutorialCardUI cardObj)
        {
            AddCardList(cardObj);
        }

        public void SendInput(Vector3 pos)
        {
            var types = GetCurrentRecipeTypes();
            var input = new CardUseInput(new List<string>(_currentCardNameList), pos);

            MagicUsed?.Invoke(types);

            if (_currentCardNameList.Contains("Spawn"))
            {
                AttachPopupBookPresenter(Instantiate(mobPrefab, pos, quaternion.identity));
            }
            else if (_currentCardNameList.Contains("Shoot"))
            {
                AttachPopupBookPresenter(Instantiate(shotPrefab, CasterPosition, quaternion.identity));
            }
            
            _currentCardNameList.Clear();
            _currentCardList.Clear();
            isFieldSelectMode = false;

            // 카드가 손을 떠났으니 마나 바에 남은 예상 소모량을 지운다.
            TutorialSceneUIController.Instance.SetExpectedManaCost(0);
        }

        private static void AttachPopupBookPresenter(GameObject target)
        {
            ServedObject servedObject = target.GetComponent<ServedObject>();
            if (servedObject == null)
            {
                servedObject = target.AddComponent<ServedObject>();
            }

            PopupBookVisualPresenter.Attach(servedObject);
            servedObject.BindListeners();
        }

        public void TryUseCard(CardUI cardObj)
        {
            throw new NotImplementedException();
        }

        private void AddCardList(TutorialCardUI card)
        {
            WDebug.Log("AddCardList: " + card.CardName);
            _currentCardNameList.Add(card.CardName);
            _currentCardList.Add(card);
        }

        private void ClearCurrentSelection()
        {
            foreach (var card in _currentCardList)
            {
                card.SetCardActive(false);
            }

            _currentCardNameList.Clear();
            _currentCardList.Clear();
            isFieldSelectMode = false;
            SetExpectedMagicUI();
        }

        private List<CardType> GetCurrentRecipeTypes()
        {
            var list = new List<CardType>(_currentCardList.Count);
            foreach (var c in _currentCardList)
            {
                if (CardNameMapper.TryMapToCardType(c.CardName, out var t))
                    list.Add(t);
                else
                    WDebug.LogWarning($"[CardInputSender] Unknown CardName → CardType map: {c.CardName}");
            }

            return list;
        }

        public void SetExpectedMagicUI()
        {
            List<CardType> recipe = GetCurrentRecipeTypes();
            TutorialSceneUIController.Instance.TrySetExpectedMagicUI(recipe);
            TutorialSceneUIController.Instance.SetExpectedManaCost(CardManaCost.SumOf(recipe));
        }
    }
}
