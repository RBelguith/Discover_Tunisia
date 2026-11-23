using System.Collections.Generic;
using UnityEngine;

namespace DiscoverTunisia.Globe
{
    /// <summary>
    /// Sphère procédurale avec convention equirectangulaire explicite :
    ///   u = (lon + 180) / 360   (u = 0.5 au méridien Greenwich)
    ///   v = (lat + 90) / 180    (v = 1 au pôle Nord, haut de la texture)
    ///   position = R * (cos(lat)sin(lon), sin(lat), cos(lat)cos(lon))
    /// Permet de placer des marqueurs exactement au bon endroit sur la carte.
    /// </summary>
    public static class SphereMesh
    {
        public static Mesh Build(float radius, int lonSegments = 96, int latSegments = 48)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var norms = new List<Vector3>();
            var tris = new List<int>();

            for (int iy = 0; iy <= latSegments; iy++)
            {
                float lat = Mathf.Lerp(-90f, 90f, iy / (float)latSegments);
                float latR = lat * Mathf.Deg2Rad;
                for (int ix = 0; ix <= lonSegments; ix++)
                {
                    float lon = Mathf.Lerp(-180f, 180f, ix / (float)lonSegments);
                    float lonR = lon * Mathf.Deg2Rad;

                    float cl = Mathf.Cos(latR);
                    var pos = new Vector3(cl * Mathf.Sin(lonR), Mathf.Sin(latR), cl * Mathf.Cos(lonR)) * radius;
                    verts.Add(pos);
                    norms.Add(pos.normalized);
                    uvs.Add(new Vector2((lon + 180f) / 360f, (lat + 90f) / 180f));
                }
            }

            int stride = lonSegments + 1;
            for (int iy = 0; iy < latSegments; iy++)
            {
                for (int ix = 0; ix < lonSegments; ix++)
                {
                    int i0 = iy * stride + ix;
                    int i1 = i0 + 1;
                    int i2 = i0 + stride;
                    int i3 = i2 + 1;
                    // sens antihoraire vu de l'extérieur + évite les triangles aux pôles
                    if (iy != 0) { tris.Add(i0); tris.Add(i1); tris.Add(i2); }
                    if (iy != latSegments - 1) { tris.Add(i1); tris.Add(i3); tris.Add(i2); }
                }
            }

            var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(norms);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Position locale d'un point géographique sur la sphère.</summary>
        public static Vector3 PointOnSphere(float radius, float latDeg, float lonDeg)
        {
            float lat = latDeg * Mathf.Deg2Rad;
            float lon = lonDeg * Mathf.Deg2Rad;
            float cl = Mathf.Cos(lat);
            return new Vector3(cl * Mathf.Sin(lon), Mathf.Sin(lat), cl * Mathf.Cos(lon)) * radius;
        }

        /// <summary>Rotation amenant la latitude/longitude donnée face à la caméra (+Z).</summary>
        public static Quaternion OrientationFor(float latDeg, float lonDeg)
        {
            var pitch = Quaternion.Euler(latDeg, 0f, 0f);
            var yaw = Quaternion.Euler(0f, -lonDeg, 0f);
            return pitch * yaw;
        }
    }
}
