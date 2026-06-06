using System.Collections.Generic;
using System.Linq;
using Nucleus.Core.Model;

namespace Nucleus.Core.Command
{
    /// <summary>
    /// A <see cref="ThreatGroup"/> paired with its computed priority <see cref="Score"/> and the
    /// <see cref="ObjectiveKind"/> the commander should pursue against it. The output unit of
    /// <see cref="TargetPrioritizer"/>. Pure data — no Unity, no game refs.
    /// </summary>
    public sealed class ScoredTarget
    {
        public ThreatGroup Group { get; }
        public float Score { get; }
        public ObjectiveKind SuggestedKind { get; }

        public ScoredTarget(ThreatGroup group, float score, ObjectiveKind suggestedKind)
        {
            Group = group;
            Score = score;
            SuggestedKind = suggestedKind;
        }
    }

    /// <summary>
    /// Ranks the commander's <see cref="ThreatBoard"/> into an ordered target list. Each
    /// <see cref="ThreatGroup"/> is scored from its strategic priority, its proximity to the home base
    /// (closer = higher), and a small bump for air-defense/radar pockets (suppressing them unlocks air ops).
    /// <see cref="Doctrine.RiskTolerance"/> shifts the balance: aggressive weights high-value, distant targets
    /// more; cautious weights nearby, lower-risk ones. Deterministic and Unity-free.
    /// </summary>
    public static class TargetPrioritizer
    {
        // Proximity falls off over ~10km; ProximityWeight is its full-strength contribution at zero range.
        private const float ProximityFalloff = 10000f;

        public static IReadOnlyList<ScoredTarget> Rank(
            IReadOnlyList<ThreatGroup> groups, Vec3 homeBase, Doctrine doctrine)
        {
            var ranked = new List<ScoredTarget>();
            if (groups == null) return ranked;

            float risk = doctrine != null ? doctrine.RiskTolerance : 0.5f;
            if (risk < 0f) risk = 0f;
            else if (risk > 1f) risk = 1f;

            float priorityWeight = 1.0f + risk;                 // 1.0 cautious .. 2.0 aggressive
            float proximityWeight = 1.5f - risk;                // 1.5 cautious .. 0.5 aggressive

            foreach (var group in groups)
            {
                if (group == null) continue;

                float strategic = priorityWeight * group.TotalStrategicPriority;

                float distance = homeBase.HorizontalDistanceTo(group.Center);
                float proximity = proximityWeight / (1f + distance / ProximityFalloff);

                // Air defense / radar pockets are worth hitting first — clearing them unlocks air ops.
                float threatBump = 0f;
                if (group.Threat != null)
                {
                    if (group.Threat.HasAirDefense) threatBump += 0.5f;
                    if (group.Threat.HasRadar) threatBump += 0.25f;
                }

                float score = strategic + proximity + threatBump;
                ranked.Add(new ScoredTarget(group, score, SuggestKind(group)));
            }

            return ranked
                .OrderByDescending(t => t.Score)
                .ThenBy(t => t.Group.Center.X)
                .ThenBy(t => t.Group.Count)
                .ToList();
        }

        /// <summary>
        /// Simple rule: a pocket dominated by a holdable ground presence (armor/infantry) is something to
        /// capture and hold; everything else is destroyed.
        /// </summary>
        private static ObjectiveKind SuggestKind(ThreatGroup group)
        {
            switch (group.Dominant)
            {
                case RoleFamily.Armor:
                case RoleFamily.Infantry:
                    return ObjectiveKind.CapturePoint;
                default:
                    return ObjectiveKind.DestroyTarget;
            }
        }
    }
}
