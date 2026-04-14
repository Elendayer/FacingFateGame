using UnityEngine;
using System.Collections.Generic;

public static class MeshUtility
{
    public static Mesh GenerateHexMesh(List<Vector3Int> centers)
    {
        float radius = 0.5f;
        Mesh mesh = new Mesh();

        //convert arry of centers to worldSpace
        List<Vector3> worldCenters = new List<Vector3>();
        foreach (var center in centers)
        {
            //worldCenters.Add(TilemapUtilityScript.BaseTilemap.CellToWorld(center));
        }


        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (var center in worldCenters)
        {
            int centerIndex = vertices.Count;
            vertices.Add(center);

            // 6 corners (point-top orientation)
            // Start at 90 degrees so the first corner is the "top" point of the hex
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (90f + 60f * i);
                vertices.Add(center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                ));
            }

            // triangles
            for (int i = 0; i < 6; i++)
            {
                triangles.Add(centerIndex);
                triangles.Add(centerIndex + 1 + i);
                triangles.Add(centerIndex + 1 + ((i + 1) % 6));
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        return mesh;
    }
}
