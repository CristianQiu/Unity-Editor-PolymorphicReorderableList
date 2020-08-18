using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BaseCharacter))]
public class BaseCharacterEditor : Editor
{
    private int MaxNumberOfActions = 8;
    private const float AdditionalSpaceMultiplier = 1.25f;

    private BaseCharacter targetChar;

    private SerializedProperty settingsListProp;
    private ReorderableList reordList;

    private GUIStyle headersStyle;

    private void OnEnable()
    {
        targetChar = (BaseCharacter)target;

        settingsListProp = serializedObject.FindProperty("settings");
        reordList = new ReorderableList(serializedObject, settingsListProp, true, true, true, true);

        headersStyle = new GUIStyle();
        headersStyle.alignment = TextAnchor.MiddleLeft;
        headersStyle.fontSize = 13;
        headersStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 1.0f) : new Color(0.2f, 0.2f, 0.2f, 1.0f);
        headersStyle.fontStyle = FontStyle.Bold;

        reordList.drawElementCallback += OnDrawReorderListElement;
        reordList.elementHeightCallback += OnReorderListElementHeight;
    }

    private void OnDisable()
    {
        reordList.drawElementCallback -= OnDrawReorderListElement;
        reordList.elementHeightCallback -= OnReorderListElementHeight;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        reordList.DoLayoutList();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add some actions to the character"))
        {
            settingsListProp.ClearArray();

            int rand = Random.Range((int)(MaxNumberOfActions / 2), MaxNumberOfActions);

            for (int i = 0; i < rand; i++)
            {
                settingsListProp.InsertArrayElementAtIndex(i);
                SerializedProperty indexProp = settingsListProp.GetArrayElementAtIndex(i);

                if (i % 4 == 0)
                    indexProp.managedReferenceValue = new MeleeAttackActionSettings();
                else if (i % 4 == 1)
                    indexProp.managedReferenceValue = new MovementActionSettings();
                else if (i % 4 == 2)
                    indexProp.managedReferenceValue = new RangedAttackActionSettings();
                else
                    indexProp.managedReferenceValue = new HealActionSettings();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnDrawReorderListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty parentProp = reordList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty actionTypeParentProp = parentProp.FindPropertyRelative("actionType");

        // substract 1 because the "Invalid" one shifts the value index by 1
        ActionType actionType = (ActionType)(actionTypeParentProp.enumValueIndex - 1);
        string actionName = SplitStringByUpperCases(actionType.ToString());

        Rect labelRect = rect;
        labelRect.height = 20.0f;

        EditorGUI.LabelField(labelRect, new GUIContent(actionName), headersStyle);

        EditorGUI.indentLevel++;

        // get the following property in the array, if any
        int length = reordList.serializedProperty.arraySize;
        SerializedProperty nextProp = (length > 0 && index < length - 1) ? reordList.serializedProperty.GetArrayElementAtIndex(index + 1) : null;

        // this is efectively the same as serializedObject.GetIterator()...
        SerializedProperty iteratorProp = parentProp.serializedObject.GetIterator();

        // so start from the top and find the action, because I can't find a way to start doing it
        // from the first array element
        while (iteratorProp.Next(true))
        {
            // if we find this property it means it is the first array element we found earlier
            if (EqualContents(parentProp, iteratorProp))
            {
                int i = 0;
                while (iteratorProp.Next(true))
                {
                    if (EqualContents(nextProp, iteratorProp))
                        break;

                    if (EqualContents(actionTypeParentProp, iteratorProp))
                        continue;

                    float multiplier = i == 0 ? AdditionalSpaceMultiplier : 1.0f;
                    rect.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * multiplier;
                    EditorGUI.PropertyField(rect, iteratorProp, true);
                    i++;
                }

                break;
            }
        }

        EditorGUI.indentLevel--;
    }

    private float OnReorderListElementHeight(int index)
    {
        int length = reordList.serializedProperty.arraySize;
        SerializedProperty nextProp = (length > 0 && index < length - 1) ? reordList.serializedProperty.GetArrayElementAtIndex(index + 1) : null;

        SerializedProperty parentProp = reordList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty actionTypeParentProp = parentProp.FindPropertyRelative("actionType");

        SerializedProperty iteratorProp = parentProp.serializedObject.GetIterator();

        float height = 0.0f;

        while (iteratorProp.Next(true))
        {
            if (EqualContents(parentProp, iteratorProp))
            {
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                int i = 0;
                while (iteratorProp.Next(true))
                {
                    if (EqualContents(nextProp, iteratorProp))
                    {
                        height += (EditorGUIUtility.standardVerticalSpacing) * AdditionalSpaceMultiplier;
                        break;
                    }

                    if (EqualContents(actionTypeParentProp, iteratorProp))
                        continue;

                    float multiplier = i == 0 ? AdditionalSpaceMultiplier : 1.0f;
                    height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * multiplier;
                    i++;
                }

                break;
            }
        }

        return height;
    }

    private bool EqualContents(SerializedProperty a, SerializedProperty b)
    {
        return SerializedProperty.EqualContents(a, b);
    }

    private string SplitStringByUpperCases(string toSplitString)
    {
        for (int i = 0; i < toSplitString.Length; i++)
        {
            char currChar = toSplitString[i];

            if (char.IsUpper(currChar))
            {
                toSplitString = toSplitString.Insert(i, " ");
                i++;
            }
        }

        return toSplitString;
    }
}