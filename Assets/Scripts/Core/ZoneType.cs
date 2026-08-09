namespace VertigoCase.Core
{
    /// <summary>
    /// Stable identifiers for configured zone policies. Routing frequency, wheel selection and
    /// leave permission live in ZonePolicy assets; UI styling lives in its own theme asset.
    /// </summary>
    public enum ZoneType
    {
        Normal,
        Safe,
        Super
    }
}
