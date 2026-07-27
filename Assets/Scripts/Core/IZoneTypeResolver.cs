namespace VertigoCase.Core
{
    /// <summary>Resolves the policy type that applies to a numbered zone.</summary>
    public interface IZoneTypeResolver
    {
        ZoneType ResolveType(int zone);
    }
}
