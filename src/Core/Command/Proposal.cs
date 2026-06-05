using System.Collections.Generic;

namespace CommanderLayer.Core.Command
{
    /// <summary>What a <see cref="Proposal"/> is asking the player to authorise.</summary>
    public enum ProposalKind { OpenOperation, Reinforce, Recon }

    /// <summary>
    /// A single suggested action the AI surfaces under <see cref="AutonomyLevel.Assisted"/>: the AI proposes,
    /// the player confirms. Pure data — no Unity, no game refs. <see cref="Summary"/> is human-readable;
    /// <see cref="RefId"/> is a stable id the caller acts on; <see cref="Priority"/> carries the source score
    /// so the UI can order/highlight proposals.
    /// </summary>
    public sealed class Proposal
    {
        public ProposalKind Kind { get; }
        public string Summary { get; }
        public string RefId { get; }
        public float Priority { get; }

        public Proposal(ProposalKind kind, string summary, string refId, float priority)
        {
            Kind = kind;
            Summary = summary;
            RefId = refId;
            Priority = priority;
        }
    }

    /// <summary>
    /// Turns the planner's outputs into player-facing <see cref="Proposal"/>s for Assisted autonomy.
    /// Deterministic and Unity-free: same ranked input always yields the same proposals in the same order.
    /// </summary>
    public static class ProposalBuilder
    {
        /// <summary>
        /// Maps the top <paramref name="max"/> ranked targets to <see cref="ProposalKind.OpenOperation"/>
        /// proposals, preserving rank order and skipping nulls.
        /// </summary>
        public static IReadOnlyList<Proposal> FromTargets(IReadOnlyList<ScoredTarget> rankedTargets, int max)
        {
            var proposals = new List<Proposal>();
            if (rankedTargets == null || max <= 0) return proposals;

            foreach (var target in rankedTargets)
            {
                if (target == null || target.Group == null) continue;

                proposals.Add(new Proposal(
                    ProposalKind.OpenOperation,
                    Describe(target),
                    RefId(target.Group),
                    target.Score));

                if (proposals.Count >= max) break;
            }

            return proposals;
        }

        private static string Describe(ScoredTarget target)
        {
            var group = target.Group;
            int count = group.Count;
            string contacts = count == 1 ? "1 contact" : count + " contacts";
            return $"{target.SuggestedKind} {group.Dominant} ({contacts})";
        }

        /// <summary>Stable id from the group centre so the same pocket maps to the same ref across ticks.</summary>
        private static string RefId(ThreatGroup group) => $"target:{group.Center.X:0},{group.Center.Z:0}";
    }
}
