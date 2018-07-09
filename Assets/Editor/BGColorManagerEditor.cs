using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BGColorManager))]
public class BGColorManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
