using System;
using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Connects zone progression to the progress-bar presentation. Layout construction, rendering,
    /// style lookup and animation are delegated to focused collaborators.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZoneProgressBarView : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private MonoBehaviour game;
        [SerializeField] private ZoneProgressBarTheme theme;
        [SerializeField] private ZoneProgressBarWidget widgetPrefab;

        [Header("Layout")]
        [SerializeField, Min(5)] private int visibleZoneCount = GameConstants.ZoneBar.VisibleZoneCount;
        [SerializeField, Min(1f)] private float cellWidth = GameConstants.ZoneBar.CellWidth;
        [SerializeField, Min(0f)] private float cellSpacing = GameConstants.ZoneBar.CellSpacing;
        [SerializeField, Min(1f)] private float barHeight = GameConstants.ZoneBar.BarHeight;
        [SerializeField, Min(1f)] private float currentMarkerHeight =
            GameConstants.ZoneBar.CurrentMarkerHeight;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float transitionSeconds =
            GameConstants.ZoneBar.TransitionSeconds;

        private IZonePort gamePort;
        private ZoneProgressBarLayout layout;
        private ZoneProgressBarRenderer barRenderer;
        private ZoneProgressBarAnimator animator;
        private int currentZone;
        private int firstVisibleZone = GameConstants.Zones.FirstZone;
        private bool hasZone;

        private void Awake()
        {
            gamePort = game as IZonePort ??
                       throw new InvalidOperationException(
                           "ZoneProgressBarView requires a component implementing IZonePort.");

            ValidateReferences();
            theme.ValidateConfiguration();

            layout = new ZoneProgressBarLayout(
                visibleZoneCount,
                cellWidth,
                cellSpacing,
                barHeight,
                currentMarkerHeight);

            ZoneProgressBarWidget widget = Instantiate(widgetPrefab, transform, false);
            ZoneProgressBarElements elements = widget.Initialize(layout);

            barRenderer = new ZoneProgressBarRenderer(elements, layout, theme);
            animator = new ZoneProgressBarAnimator(elements);
        }

        private void OnEnable() => gamePort.OnZoneChanged += HandleZoneChanged;

        private void OnDisable()
        {
            if (gamePort != null)
                gamePort.OnZoneChanged -= HandleZoneChanged;

            animator?.Stop(false);
        }

        private void HandleZoneChanged(int zone, ZoneType _)
        {
            if (zone < GameConstants.Zones.FirstZone)
                throw new ArgumentOutOfRangeException(nameof(zone), zone, "Zone must be positive.");

            animator.Stop(true);

            if (!hasZone)
            {
                hasZone = true;
                ApplyZoneImmediately(zone);
                return;
            }

            int nextFirstVisibleZone = layout.FirstVisibleZone(zone);
            bool isSequentialAdvance = zone == currentZone + 1;
            bool shouldSlide = isSequentialAdvance &&
                               nextFirstVisibleZone == firstVisibleZone + 1;

            if (shouldSlide)
            {
                animator.SlideStrip(
                    -layout.CellStride,
                    transitionSeconds,
                    () =>
                    {
                        ApplyState(zone, nextFirstVisibleZone, false);
                        animator.PulseMarker();
                    });
                return;
            }

            ApplyState(zone, nextFirstVisibleZone, isSequentialAdvance);
        }

        private void ApplyZoneImmediately(int zone)
        {
            ApplyState(zone, layout.FirstVisibleZone(zone), false);
        }

        private void ApplyState(int zone, int firstZone, bool animateMarker)
        {
            currentZone = zone;
            firstVisibleZone = firstZone;

            Color markerColor = barRenderer.Render(
                currentZone,
                firstVisibleZone,
                gamePort.TypeOf);

            float markerX = layout.CellPosition(currentZone - firstVisibleZone);
            animator.MoveMarker(
                markerX,
                markerColor,
                transitionSeconds,
                animateMarker,
                animateMarker ? animator.PulseMarker : (Action)null);
        }

        private void ValidateReferences()
        {
            if (theme == null)
                throw new InvalidOperationException("Zone progress bar theme is missing.");
            if (widgetPrefab == null)
                throw new InvalidOperationException("Zone progress bar widget prefab is missing.");
        }
    }
}
