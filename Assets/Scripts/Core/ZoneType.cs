namespace VertigoCase.Core
{
    /// <summary>
    /// Stable identifiers for configured zone policies. Routing frequency, wheel selection, leave
    /// permission and reward scaling live in ZonePolicy assets instead of branching on this enum.
    /// </summary>
    public enum ZoneType
    {
        Normal,
        Safe,
        Super
    }
}
