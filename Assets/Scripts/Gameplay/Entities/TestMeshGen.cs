using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace facingfate
{
    public class TestMeshGen : MonoBehaviour
    {
        public Mesh mesh;
        public List<Vector3Int> vector3Ints = new List<Vector3Int>()
        {
            new Vector3Int(0,0,0),
            new Vector3Int(1,0,0),
            new Vector3Int(2,0,0),
            new Vector3Int(0,0,0),
            new Vector3Int(-1,0,0),
            new Vector3Int(-2,0,0)
        }; 
        
        [ContextMenu("Bind Mesh")]
        public void BindMesh()
        {
           Mesh m =  MeshUtility.GenerateHexMesh(vector3Ints);
           mesh = m;

            VisualEffect vfx = GetComponent<VisualEffect>();
            vfx.SetMesh("_Mesh", m);
            vfx.SetInt("_Size", vector3Ints.Count);

            GetComponent<MeshFilter>().mesh = m;
        }
    }
}