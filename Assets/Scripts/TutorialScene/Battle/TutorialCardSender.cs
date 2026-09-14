using System;
using System.Collections.Generic;
using Data;
using Data.Magic;
using GameScene.Card;
using GameScene.PopupBook;
using GameScene.ServedObjectComponent;
using Global;
using Unity.Mathematics;
using UnityEngine;

namespace TutorialScene
{
    public class TutorialCardSender : MonoBehaviour, ICardSender
    {
        private static readonly Vector3 CasterPosition = new Vector3(1f, 0f, 5f);

        public event Action<IReadOnlyList<CombinedMagicData>> MagicUsed;
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

            // 카드 한 장이 마법 하나이므로 조합을 맞춰볼 것이 없다.
            if (GetCurrentMagic() == null)
            {
                if (_currentCardList.Count == 1)
                {
                    SingleCardUsed?.Invoke();
                }

                WDebug.Log("Selected card has no magic data yet.");
                TutorialSceneUIController.Instance?.PlayMagicFailEffect();
                ClearCurrentSelection();
                return;
            }

            isFieldSelectMode = true;
        }

        public string GetMagicName()
        {
            return _currentCardNameList.Count > 0 ? _currentCardNameList[0] : null;
        }

        /// <summary>지금 고른 카드의 마법. 한 번에 한 장만 고르므로 목록의 첫 장이다.</summary>
        public CombinedMagicData GetCurrentMagic()
        {
            return _currentCardList.Count > 0 ? _currentCardList[0].Magic : null;
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
            var magics = GetCurrentMagics();

            MagicUsed?.Invoke(magics);

            // TODO(#579): 튜토리얼은 아직 Spawn/Shoot 카드 이름으로 연출을 고른다.
            // 그 카드가 없어졌으므로 마법의 조준 모양으로 갈라 둔다. 튜토리얼 대본을 새 마법으로
            // 다시 쓸 때 이 분기 전체를 다시 설계한다.
            CombinedMagicData magic = magics.Count > 0 ? magics[0] : null;
            if (magic != null && GameScene.MagicIndicatorResolver.IsLaneAim(magic))
            {
                AttachPopupBookPresenter(Instantiate(shotPrefab, CasterPosition, quaternion.identity));
            }
            else if (magic != null)
            {
                AttachPopupBookPresenter(Instantiate(mobPrefab, pos, quaternion.identity));
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

        private List<CombinedMagicData> GetCurrentMagics()
        {
            var list = new List<CombinedMagicData>(_currentCardList.Count);
            foreach (var c in _currentCardList)
            {
                if (c.Magic != null)
                    list.Add(c.Magic);
                else
                    WDebug.LogWarning($"[TutorialCardSender] Unknown magic name: {c.CardName}");
            }

            return list;
        }

        public void SetExpectedMagicUI()
        {
            CombinedMagicData magic = GetCurrentMagic();
            TutorialSceneUIController.Instance.TrySetExpectedMagicUI(magic);
            TutorialSceneUIController.Instance.SetExpectedManaCost(CardManaCost.Of(magic));
        }
    }
}
