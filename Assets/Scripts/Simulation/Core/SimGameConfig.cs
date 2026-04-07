namespace Simulation.Core
{
    public static class SimGameConfig
    {
        public const int FPS = 20;
        public static readonly Fix64 DeltaTime = Fix64.One / Fix64.FromInt(FPS); // 0.05s fixed timestep

        // Map bounds
        public const int X_BOUND = 15;
        public const int Y_BOUND = 10;
        public const int X_MID = 9;
        public const int Y_MID = 5;
        public const int WIDTH = 18;
        public const int HEIGHT = 10;

        // Physics
        public static readonly Fix64 GRAVITY_ACCEL = Fix64.FromInt(4);
        public static readonly Fix64 AERIAL_STANDARD_HEIGHT = Fix64.FromInt(2);
        public static readonly Fix64 AERIAL_MOB_INIT_HEIGHT = Fix64.FromInt(3);
        public static readonly Fix64 DROP_MAGIC_INITIAL_HEIGHT = Fix64.FromInt(10);
        public static readonly Fix64 FALL_THRESHOLD_VELOCITY = Fix64.FromInt(4);
        public static readonly Fix64 FALL_THRESHOLD = Fix64.FromDouble(0.01);
        public static readonly Fix64 DEFAULT_FLOATING_VELOCITY = Fix64.FromInt(1);

        // Player positions
        public static readonly SimVector3 LEFT_PLAYER_POSITION = new(1, 5, 0);
        public static readonly SimVector3 RIGHT_PLAYER_POSITION = new(17, 5, 0);

        // Center obstacle
        public static readonly SimVector2 OBSTACLE_CENTER = new(9, 5);
        public static readonly Fix64 OBSTACLE_RADIUS = Fix64.FromDouble(1.5);

        public static SimVector3 GetPlayerPosition(Master master)
        {
            return master switch
            {
                Master.LeftPlayer => LEFT_PLAYER_POSITION,
                Master.RightPlayer => RIGHT_PLAYER_POSITION,
                _ => SimVector3.Zero
            };
        }

        public static bool IsOutOfBounds(SimVector3 pos)
        {
            return pos.X < Fix64.Zero || pos.X > Fix64.FromInt(WIDTH)
                || pos.Y < Fix64.Zero || pos.Y > Fix64.FromInt(HEIGHT);
        }
    }
}
