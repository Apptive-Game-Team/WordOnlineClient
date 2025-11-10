using UnityEngine;

namespace Script.GameScene.Card
{
    public interface ICardSender
    {
        bool IsFieldSelectMode();

        void SendInput(Vector3 pos);

        void TryUseCard(CardUI cardObj);

        void SetExpectedMagicUI();
    }
}