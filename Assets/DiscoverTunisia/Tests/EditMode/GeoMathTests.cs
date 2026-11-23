using DiscoverTunisia.World;
using NUnit.Framework;
using UnityEngine;

namespace DiscoverTunisia.Tests
{
    public class GeoMathTests
    {
        private static readonly object[] Cases =
        {
            new object[] { 36.45f, 10.73f, new Vector3(0f, 0f, -1f) },
            new object[] { 37.28f, 9.87f, new Vector3(-0.36f, 0.02f, -0.93f) },
            new object[] { 35.68f, 10.10f, new Vector3(-0.5f, 0.3f, -0.81f) },
            new object[] { 0f, -120f, new Vector3(0.2f, -0.1f, -0.97f) },
        };

        [TestCaseSource(nameof(Cases))]
        public void AnglesToward_TurnsThePointOntoTheDirection(float lat, float lon, Vector3 direction)
        {
            direction.Normalize();
            var angles = GeoMath.AnglesToward(lat, lon, direction);
            var rotation = Quaternion.Euler(angles.x, 0f, 0f) * Quaternion.Euler(0f, angles.y, 0f);
            var turned = rotation * GeoMath.PointOnSphere(1f, lat, lon);
            Assert.That(Vector3.Distance(turned, direction), Is.LessThan(1e-3f), $"landed on {turned}");
        }

        [Test]
        public void PointOnSphere_PutsLongitudeZeroOnPlusZ()
        {
            Assert.That(Vector3.Distance(GeoMath.PointOnSphere(2f, 0f, 0f), new Vector3(0f, 0f, 2f)), Is.LessThan(1e-5f));
            Assert.That(Vector3.Distance(GeoMath.PointOnSphere(1f, 90f, 0f), Vector3.up), Is.LessThan(1e-5f));
            Assert.That(Vector3.Distance(GeoMath.PointOnSphere(1f, 0f, 90f), Vector3.left), Is.LessThan(1e-5f), "east must sit on -X so the map is not mirrored");
        }
    }
}
