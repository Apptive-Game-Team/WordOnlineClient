using Scripts.CustomizeScene;
using Scripts.CustomizeScene.dto;
using Scripts.Global;
using UnityEngine;

namespace Scripts.GameScene.Player
{
    public class PlayerObjectBuilder : MonoBehaviour
    {
        [SerializeField] private DecorationsApiClient decorationsApiClient;
    
        // 왼쪽/오른쪽 플레이어 꾸미기를 실제로 그려줄 애들
        [SerializeField] private PlayerObjectView leftPlayerView;
        [SerializeField] private PlayerObjectView rightPlayerView;

        private void Start()
        {
            var matchInfo = SceneContext.MatchInfo;
            var leftUserId  = matchInfo.leftUser.id;
            var rightUserId = matchInfo.rightUser.id;

            // 왼쪽 플레이어 꾸미기 불러오기
            StartCoroutine(
                decorationsApiClient.GetUsersDecorations(
                    leftUserId,
                    resp => OnLeftDecorationsLoaded(resp),
                    err  => Debug.LogError($"Left decorations error: {err}")
                )
            );

            // 오른쪽 플레이어 꾸미기 불러오기
            StartCoroutine(
                decorationsApiClient.GetUsersDecorations(
                    rightUserId,
                    resp => OnRightDecorationsLoaded(resp),
                    err  => Debug.LogError($"Right decorations error: {err}")
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
    }
}
