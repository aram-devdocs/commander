using System.Collections.Generic;
using System.Linq;
using CommanderLayer.Core.Command;
using CommanderLayer.Core.Model;
using Xunit;

namespace CommanderLayer.Tests
{
    public class BrainTests
    {
        private static Vec3 P(float x, float z) => new Vec3(x, 0, z);

        private static EnemyView E(string id, Vec3 pos, float priority = 1f) =>
            new EnemyView(id, pos, UnitClass.GroundVehicle, default, true, priority, 0);

        private static Squad Sq(string id, RoleFamily fam, int strength, string assignedOp = null)
        {
            var s = new Squad(id, id, fam, SquadOrigin.Auto, Enumerable.Range(0, strength).Select(i => id + "u" + i));
            s.AssignedOperationId = assignedOp;
            return s;
        }

        private static BrainConfig Cfg() => new BrainConfig { ClusterRadius = 3000f, CoverageRadius = 4000f, MaxSquadsPerOperation = 2 };

        [Fact]
        public void GenerateObjectives_clusters_nearby_enemies_into_one()
        {
            var known = new List<EnemyView> { E("e1", P(0, 0)), E("e2", P(500, 0)), E("e3", P(50000, 0)) };
            var objs = CommanderBrain.GenerateObjectives(known, new List<Objective>(), Cfg());
            Assert.Equal(2, objs.Count);                              // {e1,e2} cluster + e3
            Assert.All(objs, o => Assert.Equal(ObjectiveKind.DestroyTarget, o.Kind));
        }

        [Fact]
        public void GenerateObjectives_skips_areas_already_covered()
        {
            var known = new List<EnemyView> { E("e1", P(0, 0)) };
            var existing = new List<Objective> { new Objective("x", ObjectiveKind.DestroyTarget, P(1000, 0), ObjectiveSource.Player) };
            Assert.Empty(CommanderBrain.GenerateObjectives(known, existing, Cfg())); // within CoverageRadius
        }

        [Fact]
        public void MatchSquads_picks_suitable_unassigned_strongest_first_capped()
        {
            var obj = new Objective("o", ObjectiveKind.DestroyTarget, P(0, 0), ObjectiveSource.Auto);
            var squads = new List<Squad>
            {
                Sq("armorBig", RoleFamily.Armor, 4),
                Sq("armorSmall", RoleFamily.Armor, 1),
                Sq("art", RoleFamily.Artillery, 2),
                Sq("supply", RoleFamily.Supply, 3),          // wrong family -> excluded
                Sq("busy", RoleFamily.Armor, 5, assignedOp: "other"), // already assigned -> excluded
            };
            var chosen = CommanderBrain.MatchSquads(obj, squads, Cfg());
            Assert.Equal(new[] { "armorBig", "art" }, chosen.ToArray()); // strongest suitable, capped at 2
        }
    }
}
