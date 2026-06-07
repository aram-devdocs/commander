using System.Collections.Generic;
using Nucleus.Core.Model;
using Xunit;

namespace Nucleus.Tests
{
    public class ModelTests
    {
        [Fact]
        public void HorizontalDistance_IgnoresAltitude()
        {
            var a = new Vec3(0, 0, 0);
            var b = new Vec3(3, 9999, 4); // 3-4-5 triangle on the x,z plane; large y ignored
            Assert.Equal(5f, a.HorizontalDistanceTo(b), 3);
        }

        [Fact]
        public void DistanceTo_IsFull3D()
        {
            var a = new Vec3(0, 0, 0);
            var b = new Vec3(2, 3, 6); // 2-3-6 -> 7
            Assert.Equal(7f, a.DistanceTo(b), 3);
        }

        private static UnitView U(Vec3 pos) =>
            new UnitView("u", "u", pos, UnitClass.GroundVehicle, false, true,
                new UnitCapability(Role.Armor, true, false, false, false, false), 1f, 0f, 1);

        [Fact]
        public void Centroid_AveragesAllAxes()
        {
            var roster = new List<UnitView> { U(new Vec3(0, 0, 0)), U(new Vec3(4, 6, 8)), U(new Vec3(2, 0, 4)) };
            var c = RosterGeometry.Centroid(roster);
            Assert.Equal(2f, c.X, 3);
            Assert.Equal(2f, c.Y, 3);
            Assert.Equal(4f, c.Z, 3);
        }

        [Fact]
        public void Centroid_EmptyOrNull_IsOrigin()
        {
            Assert.Equal(0f, RosterGeometry.Centroid(new List<UnitView>()).DistanceTo(new Vec3(0, 0, 0)), 3);
            Assert.Equal(0f, RosterGeometry.Centroid(null).DistanceTo(new Vec3(0, 0, 0)), 3);
        }
    }
}
