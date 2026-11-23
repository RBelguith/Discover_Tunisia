using UnityEngine;

namespace DiscoverTunisia.World
{
    /// <summary>
    /// Latitude/longitude on the globe mesh: lon 0 on +Z, lat 90 on +Y, east toward -X so the map reads unmirrored from outside.
    /// </summary>
    public static class GeoMath
    {
        public static Vector3 PointOnSphere(float radius, float latitude, float longitude)
        {
            float lat = latitude * Mathf.Deg2Rad;
            float lon = longitude * Mathf.Deg2Rad;
            float c = Mathf.Cos(lat);
            return new Vector3(-c * Mathf.Sin(lon), Mathf.Sin(lat), c * Mathf.Cos(lon)) * radius;
        }

        /// <summary>
        /// Pitch (x) and yaw (y), applied as Euler(pitch, 0, 0) * Euler(0, yaw, 0), that turn a geographic point toward a unit direction.
        /// </summary>
        public static Vector2 AnglesToward(float latitude, float longitude, Vector3 direction)
        {
            float lat = latitude * Mathf.Deg2Rad;
            float cosLat = Mathf.Max(Mathf.Cos(lat), 1e-4f);
            float a = Mathf.Asin(Mathf.Clamp(direction.x / cosLat, -1f, 1f));
            float turned = direction.z < 0f ? Mathf.PI - a : a;
            float yaw = turned * Mathf.Rad2Deg + longitude;
            float pitch = (Mathf.Atan2(direction.z, direction.y) - Mathf.Atan2(cosLat * Mathf.Cos(turned), Mathf.Sin(lat))) * Mathf.Rad2Deg;
            return new Vector2(Mathf.DeltaAngle(0f, pitch), yaw);
        }
    }
}
