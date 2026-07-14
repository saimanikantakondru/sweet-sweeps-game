using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Player
{
    public class RespawnService
    {
        private readonly IFrameService _iFrameService;
        private readonly LayerMask _groundMask;
        private readonly RaycastHit2D[] _columnHits = new RaycastHit2D[8];

        private const float GroundClearance = 1f;
        private const float ScanRadius = 0.3f;
        private const float HorizontalStep = 1f;
        private const float MaxHorizontalDistance = 60f;
        private const float VerticalScanUp = 40f;
        private const float VerticalScanDown = 40f;

        public RespawnService(
            IFrameService iFrameService,
            LayerMask groundMask)
        {
            _iFrameService = iFrameService;
            _groundMask = groundMask;
        }

        public void RespawnAt(Transform player, Vector3 rawPosition, Vector3 fallbackPosition)
        {
            Vector3 safePos = FindSafePosition(rawPosition, fallbackPosition);
            player.position = safePos;

            _iFrameService.TriggerInvincibility(true);

            Debug.Log($"[RespawnService] Respawned at {safePos} " +
                      $"(requested {rawPosition}) iframes=Active");
        }

        private Vector3 FindSafePosition(Vector3 origin, Vector3 fallbackPosition)
        {
            if (TryFindGroundColumn(origin, 0f, out var direct))
                return direct;

            for (float offset = HorizontalStep; offset <= MaxHorizontalDistance; offset += HorizontalStep)
            {
                if (TryFindGroundColumn(origin, offset, out var right))
                {
                    Debug.Log($"[RespawnService] Safe position found {offset:F1} to the right: {right}");
                    return right;
                }

                if (TryFindGroundColumn(origin, -offset, out var left))
                {
                    Debug.Log($"[RespawnService] Safe position found {offset:F1} to the left: {left}");
                    return left;
                }
            }

            Debug.LogWarning("[RespawnService] No safe ground found — using initial spawn point.");
            return fallbackPosition;
        }

        private bool TryFindGroundColumn(Vector3 origin, float offsetX, out Vector3 safePosition)
        {
            safePosition = default;

            var rayStart = new Vector2(origin.x + offsetX, origin.y + VerticalScanUp);
            float rayLength = VerticalScanUp + VerticalScanDown;

            int hitCount = Physics2D.RaycastNonAlloc(
                rayStart, Vector2.down, _columnHits, rayLength, _groundMask);

            bool found = false;
            float bestVerticalDelta = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Vector3 candidate = new Vector3(
                    _columnHits[i].point.x,
                    _columnHits[i].point.y + GroundClearance,
                    origin.z);

                if (Physics2D.OverlapCircle(candidate, ScanRadius, _groundMask))
                    continue;

                float verticalDelta = Mathf.Abs(candidate.y - origin.y);
                if (verticalDelta >= bestVerticalDelta)
                    continue;

                bestVerticalDelta = verticalDelta;
                safePosition = candidate;
                found = true;
            }

            return found;
        }
    }
}