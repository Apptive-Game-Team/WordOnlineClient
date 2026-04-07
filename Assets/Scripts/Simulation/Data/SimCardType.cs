namespace Simulation.Core
{
    public enum SimCardType
    {
        // Magic action types
        Shoot, Build, Spawn, Explode, Drop,
        // Element types
        Fire, Water, Lightning, Rock, Nature, Wind,
    }

    public static class SimCardTypeExt
    {
        public static bool IsMagic(this SimCardType c) => c <= SimCardType.Drop;
        public static bool IsElement(this SimCardType c) => c >= SimCardType.Fire;
    }
}
