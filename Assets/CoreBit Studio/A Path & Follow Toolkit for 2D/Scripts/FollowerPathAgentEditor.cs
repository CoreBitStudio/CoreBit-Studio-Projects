using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FollowerPathAgent))]
public class FollowerPathAgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FollowerPathAgent agent = (FollowerPathAgent)target;
        if (GUILayout.Button("Create Grid Manager"))
        {
            ((FollowerPathAgent)target).CreateGridManager();
        }
    }
}
