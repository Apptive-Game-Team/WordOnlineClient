using CustomizeScene;
using CustomizeScene.dto;
using Data;
using Global;
using UnityEngine;

namespace GameScene.Player
{
    public class PlayerObjectBuilder : MonoBehaviour
    {
        [SerializeField] private DecorationsApiClient decorationsApiClient;

        [SerializeField] private PlayerObjectView leftPlayerView;
        [SerializeField] private PlayerObjectView rightPlayerView;

        private void Start()
        {
            var matchInfo = SceneContext.MatchInfo;
            if (matchInfo == null) return;

            if (IsPveMatch(matchInfo))
            {
                DisableRightPlayer();
            }

            if (matchInfo.leftUser != null)
            {
                StartCoroutine(
                    decorationsApiClient.GetUsersDecorations(
                        matchInfo.leftUser.id,
                        resp => OnLeftDecorationsLoaded(resp),
                        err => Debug.LogError($"Left decorations error: {err}")
                    )
                );
            }

            if (matchInfo.rightUser == null || !rightPlayerView.gameObject.activeInHierarchy) return;

            StartCoroutine(
                decorationsApiClient.GetUsersDecorations(
                    matchInfo.rightUser.id,
                    resp => OnRightDecorationsLoaded(resp),
                    err => Debug.LogError($"Right decorations error: {err}")
                )
            );
        }

        private void OnLeftDecorationsLoaded(DecorationsResponse resp)
        {
            leftPlayerView.SetDecorations(resp);
        }

        private void OnRightDecorationsLoaded(DecorationsResponse resp)
        {
            rightPlayerView.SetDecorations(resp);
        }

        private static bool IsPveMatch(MatchedInfoDto matchInfo)
        {
            if (!string.IsNullOrEmpty(matchInfo.message) &&
                matchInfo.message.ToLowerInvariant().Contains("pve"))
            {
                return true;
            }

            return matchInfo.rightUser == null || matchInfo.rightUser.id <= 0;
        }

        private static void DisableRightPlayer()
        {
            var rightPlayer = GameObject.Find("RightPlayer");
            if (rightPlayer != null)
            {
                rightPlayer.SetActive(false);
            }
        }
    }
}
