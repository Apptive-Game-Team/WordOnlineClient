using UnityEngine;

namespace GameScene
{
    /// <summary>
    /// MonoBehaviour that drives FrameClock.Tick() at the server's tick rate.
    /// Attach to any persistent GameObject in the GameScene.
    /// </summary>
    public class FrameClockDriver : MonoBehaviour
    {
        private void Awake()
        {
            // Align Unity's fixed timestep with the server's frame duration
            Time.fixedDeltaTime = GameConfig.FRAME_DURATION;
            FrameClock.Reset();
        }

        private void FixedUpdate()
        {
            FrameClock.Tick();
        }
    }
}
