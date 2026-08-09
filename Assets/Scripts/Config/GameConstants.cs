namespace VertigoCase.Config
{
    /// <summary>
    /// Shared mathematical invariants and editor defaults. Runtime-tunable content such as zone
    /// intervals and policy multipliers remains in ScriptableObject assets.
    /// </summary>
    public static class GameConstants
    {
        public static class ExecutionOrder
        {
            public const int GameInstaller = -1000;
        }

        public static class Wheel
        {
            public const float FullRotationDegrees = 360f;
            public const float DefaultSpinDurationSeconds = 4f;
            public const int DefaultExtraSpins = 5;
            public const float DefaultAngleOffsetDegrees = 0f;
            public const float DefaultSliceRadius = 250f;
            public const int MinimumSliceWeight = 1;
            public const int DefaultSliceMultiplier = 1;
            public const int DefaultSliceWeight = MinimumSliceWeight;
            public const int CompactThousands = 1000;
            public const int CompactMillions = 1000000;
            public const string RewardLabelPrefix = "x";
            public const string ThousandsSuffix = "K";
            public const string MillionsSuffix = "M";
            public const float SliceIconBoxWidth = 116f;
            public const float SliceIconBoxHeight = 80f;
            public const float SliceLabelWidth = 120f;
            public const float SliceLabelHeight = 44f;
            public const float SliceLabelFontSize = 32f;
            public const float SliceLabelMinFontSize = 24f;
            public const float SliceLabelMaxFontSize = 36f;
            public const float SliceLabelVisualHeight = 32f;
            public const float SliceContentGap = 2f;
            public const float SliceContentOffsetY = 0f;
            public const float SliceVisibleIconWidth = 104f;
            public const float SliceVisibleIconHeight = 66f;
            public const float SliceMinIconScale = 0.75f;
            public const float SliceMaxIconScale = 1.6f;
            public const float TitleLabelFontSize = 44f;
            public const float LabelMinFontSize = 30f;
            public const float LabelMaxFontSize = 46f;
        }

        public static class Rewards
        {
            public const int DefaultBaseAmount = 100;
        }

        public static class Zones
        {
            public const int FirstZone = 1;
            public const int MinimumOccurrenceInterval = 1;
            public const int DefaultOccurrenceInterval = MinimumOccurrenceInterval;
        }

        public static class ZoneBar
        {
            public const int VisibleZoneCount = 13;
            public const float CellWidth = 84f;
            public const float CellSpacing = 4f;
            public const float BarHeight = 90f;
            public const float CurrentMarkerHeight = 120f;
            public const float TransitionSeconds = 0.35f;
            public const float CurrentMarkerWidthPadding = 12f;
            public const float MarkerPulseScale = 0.1f;
            public const float MarkerPulseSeconds = 0.2f;
            public const int MarkerPulseVibrato = 5;
            public const float MarkerPulseElasticity = 0.45f;
        }

        public static class Inventory
        {
            public const float RowAnimationSeconds = 0.2f;
            public const int RewardFlyIconCount = 5;
            public const float RewardFlyIconSize = 58f;
            public const float RewardFlyScatterRadius = 64f;
            public const float RewardFlyScatterSeconds = 0.18f;
            public const float RewardFlyTravelSeconds = 0.55f;
            public const float RewardFlyStaggerSeconds = 0.055f;
            public const float RewardFlyFadeSeconds = 0.12f;
        }
    }
}
