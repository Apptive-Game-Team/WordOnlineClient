using Data.Profile;
using TMPro;
using UnityEngine;

namespace ProfileScene
{
    public class GameHistoryItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text opponentName;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Color winColor = new Color(0.2f, 0.7f, 0.2f);
        [SerializeField] private Color loseColor = new Color(0.85f, 0.2f, 0.2f);

        public void Render(UserGameHistoryDto gameHistory, string opponentUsername)
        {
            if (gameHistory == null)
            {
                SetText(opponentName, "");
                SetText(resultText, "");
                return;
            }

            SetText(opponentName, opponentUsername);
            SetResult(gameHistory.IsWin);
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void SetResult(bool isWin)
        {
            if (resultText == null)
            {
                return;
            }

            resultText.text = isWin ? "Win" : "Loss";
            resultText.color = isWin ? winColor : loseColor;
        }
    }
}
