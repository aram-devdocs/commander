using CommanderLayer.Core.Command;
using CommanderLayer.Core.Model;
using Xunit;

namespace CommanderLayer.Tests
{
    public class CommandModelTests
    {
        private static Vec3 P => new Vec3(0, 0, 0);
        private static Objective Obj(ObjectiveKind k) => new Objective("o", k, P, ObjectiveSource.Auto);

        [Fact]
        public void Objective_maps_to_order_kind()
        {
            Assert.Equal(OrderKind.Attack, Obj(ObjectiveKind.DestroyTarget).ToOrderKind());
            Assert.Equal(OrderKind.Capture, Obj(ObjectiveKind.CapturePoint).ToOrderKind());
            Assert.Equal(OrderKind.Defend, Obj(ObjectiveKind.DefendArea).ToOrderKind());
            Assert.Equal(OrderKind.Defend, Obj(ObjectiveKind.ControlAirspace).ToOrderKind());
            Assert.Equal(OrderKind.Resupply, Obj(ObjectiveKind.Resupply).ToOrderKind());
            Assert.Equal(OrderKind.Move, Obj(ObjectiveKind.Recon).ToOrderKind());
        }

        [Fact]
        public void Objective_maps_to_domains()
        {
            Assert.Equal(DomainSet.Air, Obj(ObjectiveKind.ControlAirspace).ToDomains());                 // aircraft only
            Assert.Equal(DomainSet.Land | DomainSet.Sea, Obj(ObjectiveKind.CapturePoint).ToDomains());   // surface only
            Assert.Equal(DomainSet.All, Obj(ObjectiveKind.DestroyTarget).ToDomains());
        }

        [Fact]
        public void Operation_defaults_to_auto_and_planning()
        {
            var op = new Operation("op", Obj(ObjectiveKind.DestroyTarget), new[] { "sq1" });
            Assert.Equal(AutonomyLevel.Auto, op.Autonomy);
            Assert.Equal(OperationStatus.Planning, op.Status);
            Assert.Contains("sq1", op.SquadIds);
            Assert.False(op.IsTerminal);
            op.Status = OperationStatus.Complete;
            Assert.True(op.IsTerminal);
        }
    }
}
