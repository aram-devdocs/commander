using System.Collections.Generic;
using System.Linq;
using CommanderLayer.Core.Command;
using CommanderLayer.Core.Model;
using Xunit;

namespace CommanderLayer.Tests
{
    public class ProposalTests
    {
        private static Vec3 P(float x, float z) => new Vec3(x, 0, z);

        private static EnemyView E(string id, Vec3 pos, Role role = Role.Armor, float priority = 1f)
        {
            bool isAd = role == Role.GroundAirDefense || role == Role.AirDefenseShip;
            var cap = new UnitCapability(role, true, isAd, false, false, isAd);
            return new EnemyView(id, pos, UnitClass.GroundVehicle, cap, true, priority, 0);
        }

        private static ThreatGroup Group(params EnemyView[] members) => new ThreatGroup(members.ToList());

        private static ScoredTarget Scored(ThreatGroup group, float score, ObjectiveKind kind) =>
            new ScoredTarget(group, score, kind);

        [Fact]
        public void FromTargets_makes_one_open_operation_proposal_per_target_in_order()
        {
            var a = Scored(Group(E("a", P(1000, 0))), 9f, ObjectiveKind.DestroyTarget);
            var b = Scored(Group(E("b", P(2000, 0))), 5f, ObjectiveKind.DestroyTarget);
            var c = Scored(Group(E("c", P(3000, 0))), 1f, ObjectiveKind.DestroyTarget);

            var proposals = ProposalBuilder.FromTargets(new[] { a, b, c }, 10);

            Assert.Equal(3, proposals.Count);
            Assert.All(proposals, p => Assert.Equal(ProposalKind.OpenOperation, p.Kind));
            Assert.Equal(new[] { 9f, 5f, 1f }, proposals.Select(p => p.Priority).ToArray());
        }

        [Fact]
        public void Summary_mentions_the_count_and_suggested_kind()
        {
            var target = Scored(
                Group(E("a", P(1000, 0)), E("b", P(1100, 0)), E("c", P(1200, 0))),
                7f, ObjectiveKind.DestroyTarget);

            var proposal = ProposalBuilder.FromTargets(new[] { target }, 1).Single();

            Assert.Contains("3 contacts", proposal.Summary);
            Assert.Contains("DestroyTarget", proposal.Summary);
        }

        [Fact]
        public void Summary_uses_singular_for_a_single_contact()
        {
            var target = Scored(Group(E("a", P(1000, 0))), 1f, ObjectiveKind.CapturePoint);

            var proposal = ProposalBuilder.FromTargets(new[] { target }, 1).Single();

            Assert.Contains("1 contact", proposal.Summary);
            Assert.Contains("CapturePoint", proposal.Summary);
        }

        [Fact]
        public void Priority_carries_the_source_score()
        {
            var target = Scored(Group(E("a", P(1000, 0))), 42.5f, ObjectiveKind.DestroyTarget);

            var proposal = ProposalBuilder.FromTargets(new[] { target }, 1).Single();

            Assert.Equal(42.5f, proposal.Priority);
        }

        [Fact]
        public void RefId_is_derived_from_the_group_center()
        {
            var group = Group(E("a", P(8000, 0)), E("b", P(8000, 4000)));
            var proposal = ProposalBuilder.FromTargets(new[] { Scored(group, 1f, ObjectiveKind.DestroyTarget) }, 1).Single();

            Assert.Equal($"target:{group.Center.X:0},{group.Center.Z:0}", proposal.RefId);
        }

        [Fact]
        public void Max_caps_the_number_of_proposals()
        {
            var targets = Enumerable.Range(0, 5)
                .Select(i => Scored(Group(E("e" + i, P(1000 * i, 0))), 5 - i, ObjectiveKind.DestroyTarget))
                .ToArray();

            var proposals = ProposalBuilder.FromTargets(targets, 2);

            Assert.Equal(2, proposals.Count);
            Assert.Equal(new[] { 5f, 4f }, proposals.Select(p => p.Priority).ToArray());
        }

        [Fact]
        public void Empty_input_yields_no_proposals()
        {
            Assert.Empty(ProposalBuilder.FromTargets(new List<ScoredTarget>(), 5));
        }

        [Fact]
        public void Null_input_yields_no_proposals()
        {
            Assert.Empty(ProposalBuilder.FromTargets(null, 5));
        }

        [Fact]
        public void Null_entries_are_skipped()
        {
            var good = Scored(Group(E("a", P(1000, 0))), 3f, ObjectiveKind.DestroyTarget);
            var proposals = ProposalBuilder.FromTargets(new[] { null, good, null }, 5);

            Assert.Single(proposals);
            Assert.Equal(3f, proposals[0].Priority);
        }
    }
}
