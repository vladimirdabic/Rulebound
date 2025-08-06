using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

[CustomEditor(typeof(WaypointHandler))]
public class WaypointEditor : Editor
{
    private void OnSceneGUI()
    {
        WaypointHandler handler = (WaypointHandler)target;

        if (handler.JumpType != WaypointHandler.JType.EXACT) return;

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(handler.TeleportTo, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(handler, "Move Waypoint");
            handler.TeleportTo = newPosition;
            EditorUtility.SetDirty(handler);
        }
    }

    public override void OnInspectorGUI()
    {
        WaypointHandler handler = (WaypointHandler)target;

        handler.CameraBounds = (BoxCollider2D)EditorGUILayout.ObjectField("Next Camera Bounds", handler.CameraBounds, typeof(BoxCollider2D), true);
        handler.Confiner = (CinemachineConfiner2D)EditorGUILayout.ObjectField("Cinemachine Confiner", handler.Confiner, typeof(CinemachineConfiner2D), true);
        handler.JumpType = (WaypointHandler.JType)EditorGUILayout.EnumPopup("Jump Type", handler.JumpType);

        EditorGUILayout.BeginHorizontal();
        switch(handler.JumpType)
        {
            case WaypointHandler.JType.EXACT:
                handler.TeleportTo = EditorGUILayout.Vector2Field("Location", handler.TeleportTo);
                break;

            case WaypointHandler.JType.RELATIVE:
                handler.TeleportDirection = (WaypointHandler.Direction)EditorGUILayout.EnumPopup("Direction", handler.TeleportDirection);
                handler.TeleportOffset = EditorGUILayout.FloatField("Offset", handler.TeleportOffset);
                break;
        }

        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(handler);
        }
    }
}
