namespace VertigoCase.Core
{
    /// <summary>
    /// The three wheel/zone flavours. Normal = bronze (contains a bomb), Safe = silver (no bomb),
    /// Super = golden (no bomb, richer rewards). A plain enum so the rules stay data-light and the
    /// rest of the code can switch on a clear name instead of magic numbers.
    /// </summary>
    public enum ZoneType
    {
        Normal,
        Safe,
        Super
    }
}
