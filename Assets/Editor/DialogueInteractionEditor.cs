using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueInteraction))]
public class DialogueInteractionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DialogueInteraction diag = (DialogueInteraction)target;

        diag.Dialogue = (Dialogue)EditorGUILayout.ObjectField("Dialogue", diag.Dialogue, typeof(Dialogue), false);
        diag.GivesItem = EditorGUILayout.ToggleLeft("Acquire Item After", diag.GivesItem);

        if(diag.GivesItem)
        {
            EditorGUILayout.BeginVertical();
            diag.InventoryRef = (Inventory)EditorGUILayout.ObjectField("Inventory", diag.InventoryRef, typeof(Inventory), true);
            diag.Item = (ItemData)EditorGUILayout.ObjectField("Item", diag.Item, typeof(ItemData), false);
            EditorGUILayout.EndVertical();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(diag);
        }
    }
}