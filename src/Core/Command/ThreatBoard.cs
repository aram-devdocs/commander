using System.Collections.Generic;
using System.Linq;
using CommanderLayer.Core.Model;

namespace CommanderLayer.Core.Command
{
    /// <summary>
    /// A spatial cluster of known enemies — the unit the planner reasons about instead of individual
    /// contacts. Carries the group's footprint (centre + radius), an aggregate <see cref="ThreatPicture"/>,
    /// the summed strategic priority and the dominant role family, so an objective can be tasked against a
    /// whole pocket of resistance at once.
    /// </summary>
    public sealed class ThreatGroup
    {
        /// <summary>Centroid of the group's enemies.</summary>
        public Vec3 Center { get; }

        /// <summary>Largest distance from <see cref="Center"/> to any member — the group's footprint.</summary>
        public float Radius { get; }

        /// <summary>Aggregate threat built from the group's enemies (air-defense/armor/air/radar flags + counts).</summary>
        public ThreatPicture Threat { get; }

        /// <summary>Sum of the members' <see cref="EnemyView.StrategicPriority"/> — how badly this pocket matters.</summary>
        public float TotalStrategicPriority { get; }

        /// <summary>The most common <see cref="RoleFamily"/> among members (via <see cref="Families.Of"/>).</summary>
        public RoleFamily Dominant { get; }

        /// <summary>Number of enemies in the group.</summary>
        public int Count { get; }

        public ThreatGroup(IReadOnlyList<EnemyView> members)
        {
            Count = members.Count;

            float sx = 0f, sy = 0f, sz = 0f, priority = 0f;
            foreach (var e in members)
            {
                sx += e.Position.X;
                sy += e.Position.Y;
                sz += e.Position.Z;
                priority += e.StrategicPriority;
            }
            float inv = Count > 0 ? 1f / Count : 0f;
            Center = new Vec3(sx * inv, sy * inv, sz * inv);
            TotalStrategicPriority = priority;

            float radius = 0f;
            foreach (var e in members)
            {
                float d = Center.HorizontalDistanceTo(e.Position);
                if (d > radius) radius = d;
            }
            Radius = radius;

            Threat = new ThreatPicture(members.ToList());

            Dominant = members
                .GroupBy(e => Families.Of(e.Cap.Role))
                .OrderByDescending(g => g.Count())
                .ThenBy(g => (int)g.Key)
                .Select(g => g.Key)
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Builds the commander's threat board: greedy proximity clustering of known enemies into
    /// <see cref="ThreatGroup"/>s. Highest-priority contacts seed groups first (deterministic, Id as
    /// tie-break) so the board is stable across ticks. Pure + Unity-free.
    /// </summary>
    public static class ThreatBoard
    {
        public static IReadOnlyList<ThreatGroup> Build(IReadOnlyList<EnemyView> known, float clusterRadius)
        {
            var groups = new List<ThreatGroup>();
            if (known == null) return groups;

            var remaining = known
                .Where(e => e != null)
                .OrderByDescending(e => e.StrategicPriority)
                .ThenBy(e => e.Id)
                .ToList();

            var taken = new bool[remaining.Count];
            for (int i = 0; i < remaining.Count; i++)
            {
                if (taken[i]) continue;
                var seed = remaining[i];
                taken[i] = true;

                var members = new List<EnemyView> { seed };
                for (int j = i + 1; j < remaining.Count; j++)
                {
                    if (taken[j]) continue;
                    if (seed.Position.HorizontalDistanceTo(remaining[j].Position) <= clusterRadius)
                    {
                        taken[j] = true;
                        members.Add(remaining[j]);
                    }
                }

                groups.Add(new ThreatGroup(members));
            }

            return groups;
        }
    }
}
