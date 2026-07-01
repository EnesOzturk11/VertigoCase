namespace VertigoCase.Core
{
    /// <summary>Bronze zone: carries a bomb, base reward scaling.</summary>
    public class NormalZoneStrategy : IZoneStrategy
    {
        public bool HasBomb => true;
        public int ScaleReward(int baseAmount, int zone) => baseAmount * zone;
    }

    /// <summary>Silver (safe) zone: no bomb, richer reward.</summary>
    public class SafeZoneStrategy : IZoneStrategy
    {
        public bool HasBomb => false;
        public int ScaleReward(int baseAmount, int zone) => baseAmount * zone * 2;
    }

    /// <summary>Golden (super) zone: no bomb, richest reward.</summary>
    public class SuperZoneStrategy : IZoneStrategy
    {
        public bool HasBomb => false;
        public int ScaleReward(int baseAmount, int zone) => baseAmount * zone * 5;
    }

    /// <summary>
    /// Maps a <see cref="ZoneType"/> to its strategy. Adding a new zone type means adding a new class
    /// and one case here; the rest of the game keeps depending on <see cref="IZoneStrategy"/> (Open/Closed).
    /// </summary>
    public static class ZoneStrategyFactory
    {
        public static IZoneStrategy For(ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Super: return new SuperZoneStrategy();
                case ZoneType.Safe:  return new SafeZoneStrategy();
                default:             return new NormalZoneStrategy();
            }
        }
    }
}