using UnityEngine;

namespace Scripts.GameScene.Card
{
    public interface ICardSender
    {
        bool IsFieldSelectMode();

        void SendInput(Vector3 pos);

        void TryUseCard(CardUI cardObj);

        void SetExpectedMagicUI();
    }
}