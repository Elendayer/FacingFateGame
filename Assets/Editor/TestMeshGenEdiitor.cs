using facingfate;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestMeshGen))]
public class MeshBinderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TestMeshGen binder = (TestMeshGen)target;

        if (GUILayout.Button("Bind Mesh"))
        {
            binder.BindMesh();
        }
    }
}