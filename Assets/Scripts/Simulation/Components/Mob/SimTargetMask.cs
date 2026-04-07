namespace Simulation.Core
{
    public static class SimTargetMask
    {
        public const int GROUND = 1;
        public const int AIR = 2;
        public const int ANY = GROUND | AIR;

        public static int Of(SimGameObject target)
        {
            return target.Position.Z >= SimGameConfig.AERIAL_STANDARD_HEIGHT ? AIR : GROUND;
        }
    }
}
