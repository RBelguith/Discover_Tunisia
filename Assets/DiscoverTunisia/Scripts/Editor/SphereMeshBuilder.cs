using System.Collections.Generic;
using DiscoverTunisia.World;
using UnityEngine;
using UnityEngine.Rendering;

namespace DiscoverTunisia.EditorTools
{
    /// <summary>UV sphere for the equirectangular earth texture, using the same point mapping as the markers.</summary>
    public static class SphereMeshBuilder
    {
        public static Mesh Build(float radius, int lonSegments = 96, int latSegments = 48)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            for (int iy = 0; iy <= latSegments; iy++)
            {
                float lat = Mathf.Lerp(-90f, 90f, iy / (float)latSegments);
                for (int ix = 0; ix <= lonSegments; ix++)
                {
                    float lon = Mathf.Lerp(-180f, 180f, ix / (float)lonSegments);
                    var p = GeoMath.PointOnSphere(radius, lat, lon);
                    vertices.Add(p);
                    normals.Add(p.normalized);
                    uvs.Add(new Vector2((lon + 180f) / 360f, (lat + 90f) / 180f));
                }
            }

            int stride = lonSegments + 1;
            var triangles = new List<int>();
            for (int iy = 0; iy < latSegments; iy++)
            {
                for (int ix = 0; ix < lonSegments; ix++)
                {
                    int a = iy * stride + ix, b = a + 1, c = a + stride, d = c + 1;
                    if (iy != 0) AddFacingOut(triangles, vertices, a, b, c);
                    if (iy != latSegments - 1) AddFacingOut(triangles, vertices, b, d, c);
                }
            }

            var mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>Unity front faces wind clockwise seen from outside, so flip any triangle whose normal points inward.</summary>
        private static void AddFacingOut(List<int> tris, List<Vector3> v, int i0, int i1, int i2)
        {
            var normal = Vector3.Cross(v[i1] - v[i0], v[i2] - v[i0]);
            var center = (v[i0] + v[i1] + v[i2]) / 3f;
            if (Vector3.Dot(normal, center) >= 0f) { tris.Add(i0); tris.Add(i1); tris.Add(i2); }
            else { tris.Add(i0); tris.Add(i2); tris.Add(i1); }
        }
    }
}
