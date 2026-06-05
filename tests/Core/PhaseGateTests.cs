using System;
using System.Collections.Generic;
using CommanderLayer.Core.Command;
using CommanderLayer.Core.Model;
using Xunit;

namespace CommanderLayer.Tests
{
    public class PhaseGateTests
    {
        private static Vec3 P => new Vec3(0, 0, 0);
        private static EnemyView Air() => new EnemyView("a", P, UnitClass.Aircraft,
            new UnitCapability(Role.Fighter, false, true, false, false, false), true, 1f, 0);
        private static EnemyView Sam() => new EnemyView("s", P, UnitClass.GroundVehicle,
            new UnitCapability(Role.GroundAirDefense, false, true, false, false, true), true, 1f, 0);
        private static EnemyView Armor() => new EnemyView("m", P, UnitClass.GroundVehicle,
            new UnitCapability(Role.Armor, true, false, false, false, false), true, 1f, 0);
        private static ThreatPicture TP(params EnemyView[] es) => new ThreatPicture(new List<EnemyView>(es));

        [Fact]
        public void Doctrine_thresholds_are_monotonic_in_risk()
        {
            var cautious = new Doctrine { RiskTolerance = 0f };
            var aggressive = new Doctrine { RiskTolerance = 1f };
            Assert.True(cautious.AirSuperiorityRatio > aggressive.AirSuperiorityRatio); // wants more overmatch
            Assert.True(cautious.MaxResidualAirDefense < aggressive.MaxResidualAirDefense);
            Assert.True(cautious.SoftenThreshold > aggressive.SoftenThreshold);
            Assert.True(Math.Abs(cautious.AirSuperiorityRatio - 2.0f) < 0.001f);
            Assert.Equal(0, cautious.MaxResidualAirDefense);
        }

        [Fact]
        public void AirSuperiority_gate_needs_enough_fighters()
        {
            var doc = new Doctrine { RiskTolerance = 0.5f }; // ratio 1.5
            Assert.True(PhaseGates.Satisfied(CombatPhase.AirSuperiority, TP(), TP(), new ForceState(0), doc));      // no enemy air
            Assert.False(PhaseGates.Satisfied(CombatPhase.AirSuperiority, TP(Air(), Air()), TP(), new ForceState(2), doc)); // need ceil(2*1.5)=3
            Assert.True(PhaseGates.Satisfied(CombatPhase.AirSuperiority, TP(Air(), Air()), TP(), new ForceState(3), doc));
        }

        [Fact]
        public void Sead_gate_tolerates_residual_per_doctrine()
        {
            var cautious = new Doctrine { RiskTolerance = 0f };   // MaxResidualAirDefense 0
            var aggressive = new Doctrine { RiskTolerance = 1f }; // MaxResidualAirDefense 2
            Assert.False(PhaseGates.Satisfied(CombatPhase.Sead, TP(Sam()), TP(), new ForceState(0), cautious));
            Assert.True(PhaseGates.Satisfied(CombatPhase.Sead, TP(Sam()), TP(), new ForceState(0), aggressive));
            Assert.True(PhaseGates.Satisfied(CombatPhase.Sead, TP(), TP(), new ForceState(0), cautious));
        }

        [Fact]
        public void Strike_gate_requires_softening_fraction()
        {
            var doc = new Doctrine { RiskTolerance = 0.5f };               // SoftenThreshold 0.5
            var initial = TP(Armor(), Armor(), Sam(), Sam());             // init armor+AD = 4
            Assert.False(PhaseGates.Satisfied(CombatPhase.Strike, TP(Armor(), Armor(), Sam()), initial, new ForceState(0), doc)); // 1/4
            Assert.True(PhaseGates.Satisfied(CombatPhase.Strike, TP(Armor()), initial, new ForceState(0), doc));                  // 3/4
        }

        [Fact]
        public void ActivePhase_is_the_first_unsatisfied_gate()
        {
            var doc = new Doctrine { RiskTolerance = 0f };
            var withAir = TP(Air(), Sam());
            Assert.Equal(CombatPhase.AirSuperiority, PhaseGates.ActivePhase(withAir, withAir, new ForceState(0), doc));
            var noAir = TP(Sam());
            Assert.Equal(CombatPhase.Sead, PhaseGates.ActivePhase(noAir, noAir, new ForceState(0), doc)); // air clear, SAMs remain
        }
    }
}
