using TMPro;
using UnityEngine;

namespace LobbyScene.Matching
{
    /// <summary>
    /// Counts up from the moment the matching page opened. The server ticket carries
    /// no enqueue timestamp, so a ticket recovered after a reload restarts the clock.
    /// </summary>
    public sealed class MatchingElapsedTimeUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private float openedAt;

        private void OnEnable()
        {
            openedAt = Time.realtimeSinceStartup;
            Render(0f);
        }

        private void Update()
        {
            Render(Time.realtimeSinceStartup - openedAt);
        }

        private void Render(float elapsedSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
            text.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}
