using UnityEngine;

namespace GameScene
{
    public abstract class GameConfig
    {
        public const float FRAME_DURATION = 0.05f;

        // 아래 필드 치수는 서버 상수의 복제본이다.
        // 원본: game 모듈 com.wordonline.server.game.config.GameConfig 의 WIDTH / HEIGHT.
        // 클라이언트가 보내는 시전 좌표는 변환 없이 서버 좌표로 쓰이므로,
        // 서버 원본이 바뀌었는데 여기를 같이 고치지 않으면 시전 위치가 그대로 어긋난다.
        public const float FIELD_WIDTH = 18f;
        public const float FIELD_HEIGHT = 10f;

        /// <summary>
        /// 필드 중앙. 서버 GameConfig의 X_MID(9), Y_MID(5)와 같은 지점이며
        /// LEFT_PLAYER_POSITION(1,0,5)과 RIGHT_PLAYER_POSITION(17,0,5)에서 등거리다.
        /// 위치를 특정하지 않고 시전해야 할 때의 기본 좌표로 쓴다.
        /// </summary>
        public static readonly Vector3 FIELD_CENTER = new Vector3(FIELD_WIDTH / 2f, 0f, FIELD_HEIGHT / 2f);
    }
}
