using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueLine))]
public class DialogueLineDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw foldout
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label,
            true
        );

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = position.y + EditorGUIUtility.singleLineHeight + 2;

            var messageProp = property.FindPropertyRelative("Message");
            var portraitProp = property.FindPropertyRelative("Portrait");
            var isBranchingProp = property.FindPropertyRelative("IsBranching");
            var choicesProp = property.FindPropertyRelative("Choices");

            // Draw Message
            var messageHeight = EditorGUI.GetPropertyHeight(messageProp, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, messageHeight), messageProp, true);
            y += messageHeight + 4;

            // Draw Portrait
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), portraitProp);
            y += EditorGUIUtility.singleLineHeight + 4;

            // Draw IsBranching
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), isBranchingProp);
            y += EditorGUIUtility.singleLineHeight + 4;

            // Draw Choices only if branching
            if (isBranchingProp.boolValue)
            {
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(choicesProp, true)), choicesProp, true);
                y += EditorGUI.GetPropertyHeight(choicesProp, true) + 4;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight + 2;

        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Message"), true) + 4;
        height += EditorGUIUtility.singleLineHeight + 4; // Portrait
        height += EditorGUIUtility.singleLineHeight + 4; // IsBranching

        if (property.FindPropertyRelative("IsBranching").boolValue)
        {
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Choices"), true) + 4;
        }

        return height;
    }
}