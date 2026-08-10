using UnityEngine;

namespace GameScene.Card
{
    /// <summary>
    /// 손패 카드를 숫자 키(1~6)로 고르기 위한 입력 도우미.
    /// 슬롯 번호는 손패 루트의 활성 자식 순서(화면 좌 → 우)를 따른다.
    /// </summary>
    public static class CardHotkey
    {
        /// <summary>손패 최대 장수. 이 수를 넘는 숫자 키는 무시한다.</summary>
        public const int SlotCount = 6;

        /// <summary>
        /// 이번 프레임에 눌린 숫자 키를 0부터 시작하는 손패 슬롯 인덱스로 변환한다.
        /// </summary>
        public static bool TryGetPressedSlotIndex(out int index)
        {
            for (int slot = 1; slot <= SlotCount; slot++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + slot) || Input.GetKeyDown(KeyCode.Keypad0 + slot))
                {
                    index = slot - 1;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        /// <summary>
        /// 손패 루트에서 index번째 카드를 찾는다. 해당 슬롯이 비어 있으면 null.
        /// </summary>
        public static T FindCardAt<T>(Transform handRoot, int index) where T : Component
        {
            if (handRoot == null || index < 0)
            {
                return null;
            }

            int cardIndex = 0;
            foreach (Transform child in handRoot)
            {
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                T card = child.GetComponent<T>();
                if (card == null)
                {
                    continue;
                }

                if (cardIndex == index)
                {
                    return card;
                }

                cardIndex++;
            }

            return null;
        }
    }
}
