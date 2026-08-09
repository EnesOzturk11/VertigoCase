using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    /// <summary>
    /// Data-driven catalog for zone routing, wheels and reward strategies. Consumers query the
    /// catalog through narrow interfaces; adding a policy asset does not modify their code.
    /// </summary>
    [CreateAssetMenu(fileName = "ZoneCatalog", menuName = "Vertigo/Zone Catalog")]
    public sealed class ZoneCatalog :
        ScriptableObject,
        IZoneStrategyResolver,
        IZoneTypeResolver,
        IZoneWheelResolver
    {
        [SerializeField] private List<ZonePolicy> policies = new List<ZonePolicy>();

        public IZoneStrategy Resolve(ZoneType type) => PolicyFor(type);

        public ZoneType ResolveType(int zone) => PolicyForZone(zone).Type;

        public WheelData WheelFor(ZoneType type) => PolicyFor(type).Wheel;

        public void ValidateConfiguration()
        {
            if (policies == null || policies.Count == 0)
                throw new InvalidOperationException("ZoneCatalog must contain at least one policy.");

            var types = new HashSet<ZoneType>();
            var intervals = new HashSet<int>();
            bool hasFallback = false;

            for (int i = 0; i < policies.Count; i++)
            {
                ZonePolicy policy = policies[i];
                if (policy == null)
                    throw new InvalidOperationException($"Zone policy at index {i} is missing.");
                if (policy.Wheel == null)
                    throw new InvalidOperationException($"{policy.name} does not reference a wheel.");
                if (!Enum.IsDefined(typeof(ZoneType), policy.Type))
                    throw new InvalidOperationException($"{policy.name} has an unknown zone type.");
                WheelConfigurationValidator.Validate(
                    policy.Wheel,
                    policy.RequiredBombCount,
                    policy.name);
                if (policy.OccurrenceInterval < GameConstants.Zones.MinimumOccurrenceInterval)
                    throw new InvalidOperationException($"{policy.name} has an invalid occurrence interval.");
                if (!types.Add(policy.Type))
                    throw new InvalidOperationException($"Multiple policies use zone type {policy.Type}.");
                if (!intervals.Add(policy.OccurrenceInterval))
                    throw new InvalidOperationException(
                        $"Multiple policies use occurrence interval {policy.OccurrenceInterval}.");

                hasFallback |=
                    policy.OccurrenceInterval == GameConstants.Zones.DefaultOccurrenceInterval;
            }

            if (!hasFallback)
                throw new InvalidOperationException(
                    $"ZoneCatalog requires a policy with interval {GameConstants.Zones.DefaultOccurrenceInterval}.");
        }

        private ZonePolicy PolicyFor(ZoneType type)
        {
            for (int i = 0; i < policies.Count; i++)
            {
                ZonePolicy policy = policies[i];
                if (policy != null && policy.Type == type)
                    return policy;
            }

            throw new KeyNotFoundException($"No zone policy is configured for {type}.");
        }

        private ZonePolicy PolicyForZone(int zone)
        {
            if (zone < GameConstants.Zones.FirstZone)
                throw new ArgumentOutOfRangeException(nameof(zone), "Zone must be greater than zero.");

            ZonePolicy bestMatch = null;

            for (int i = 0; i < policies.Count; i++)
            {
                ZonePolicy policy = policies[i];
                if (policy == null || !policy.AppliesTo(zone))
                    continue;

                if (bestMatch == null ||
                    policy.OccurrenceInterval > bestMatch.OccurrenceInterval)
                {
                    bestMatch = policy;
                }
            }

            if (bestMatch != null)
                return bestMatch;

            throw new InvalidOperationException($"No zone policy applies to zone {zone}.");
        }
    }
}
