using UnityEngine;

namespace GameScene
{
    /// <summary>
    /// Tracks the client's local frame counter and keeps it synchronized with
    /// the server's frame number.
    ///
    /// Call FrameClock.Tick() once per fixed-update tick (every FRAME_DURATION seconds).
    /// Call FrameClock.SyncTo(serverFrame) whenever a server broadcast is received.
    /// </summary>
    public static class FrameClock
    {
        /// <summary>Current client-side frame number.</summary>
        public static int LocalFrame { get; private set; } = 0;

        private static int _pausedFrames = 0;

        /// <summary>
        /// Advance the clock by one tick.
        /// Called from a MonoBehaviour.FixedUpdate() at the game's tick rate.
        /// </summary>
        public static void Tick()
        {
            if (_pausedFrames > 0)
            {
                _pausedFrames--;
                return;
            }
            LocalFrame++;
        }

        /// <summary>
        /// Synchronize the local frame counter to the server's confirmed frame.
        /// Tolerates ±2 frames of drift without correction.
        /// </summary>
        public static void SyncTo(int serverFrame)
        {
            int drift = LocalFrame - serverFrame;

            if (drift > 2)
            {
                // Client is running ahead — slow down for 'drift' ticks
                _pausedFrames += drift;
                Debug.LogWarning($"[FrameClock] Client ahead by {drift} frames. Pausing.");
            }
            else if (drift < -2)
            {
                // Client is behind — snap forward immediately
                Debug.LogWarning($"[FrameClock] Client behind by {-drift} frames. Snapping.");
                LocalFrame = serverFrame;
                _pausedFrames = 0;
            }
        }

        /// <summary>Reset to frame 0 (call at match start).</summary>
        public static void Reset()
        {
            LocalFrame = 0;
            _pausedFrames = 0;
        }
    }
}
