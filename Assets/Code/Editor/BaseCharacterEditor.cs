using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BaseCharacter))]
public class BaseCharacterEditor : Editor
{
    private ActionType addedAction = ActionType.Invalid;

    private BaseCharacter targetChar;

    private SerializedProperty settingsListProp;
    private ReorderableList reordList;

    private void OnEnable()
    {
        targetChar = (BaseCharacter)target;

        settingsListProp = serializedObject.FindProperty("settings");
        reordList = new ReorderableList(serializedObject, settingsListProp, true, true, true, true);

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

        EditorGUILayout.LabelField("Edit actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        addedAction = (ActionType)EditorGUILayout.EnumPopup("Action to add ", addedAction);

        if (GUILayout.Button("Add action to the character"))
        {
            settingsListProp.ClearArray();

            int rand = Random.Range(0, 8);

            for (int i = 0; i < rand; i++)
            {
                settingsListProp.InsertArrayElementAtIndex(i);

                SerializedProperty indexProp = settingsListProp.GetArrayElementAtIndex(i);

                if (i == 0)
                    indexProp.managedReferenceValue = new MeleeAttackActionSettings();
                else
                    indexProp.managedReferenceValue = new MovementActionSettings();
            }
        }

        for (int i = 0; i < settingsListProp.arraySize; i++)
        {
            SerializedProperty indexProp = settingsListProp.GetArrayElementAtIndex(i);
            indexProp.serializedObject.ApplyModifiedProperties();
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.EndVertical();
    }

    private void OnDrawReorderListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty parentProp = reordList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty actionTypeParentProp = parentProp.FindPropertyRelative("actionType");

        // substract 1 because the "Invalid" one shifts the value index by 1
        ActionType actionType = (ActionType)(actionTypeParentProp.enumValueIndex - 1);
        string actionName = SplitStringByUpperCases(actionType.ToString());

        Rect r = rect;
        EditorGUI.LabelField(r, actionName);
        //EditorGUI.DropShadowLabel(rect, actionName);

        // this is efectively the same as serializedObject.GetIterator()...
        SerializedProperty iteratorProp = parentProp.serializedObject.GetIterator();

        // get the following property in the array, if any
        int length = reordList.serializedProperty.arraySize;
        SerializedProperty nextProp = (length > 0 && index < length - 1) ? reordList.serializedProperty.GetArrayElementAtIndex(index + 1) : null;

        // so we must start from the top and find the action, because I can't find a way to start
        // doing it from the first array element
        while (iteratorProp.Next(true))
        {
            // if we find this property it means is the first array element we found earlier
            if (EqualContents(parentProp, iteratorProp))
            {
                int i = 1;
                Rect newRect = rect;
                //newRect.y += (i * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                //EditorGUI.PropertyField(newRect, iteratorProp, true);
                //i++;

                while (iteratorProp.Next(true))
                {
                    if (EqualContents(nextProp, iteratorProp))
                        break;

                    newRect = rect;
                    newRect.y += (i * EditorGUIUtility.singleLineHeight + i * EditorGUIUtility.standardVerticalSpacing);

                    EditorGUI.PropertyField(newRect, iteratorProp, true);
                    i++;
                }

                break;
            }
        }

        //EditorGUI.BeginVertical(EditorStyles.helpBox);

        //EditorGUILayout.EndVertical();
    }

    private float OnReorderListElementHeight(int index)
    {
        SerializedProperty parentProp = reordList.serializedProperty.GetArrayElementAtIndex(index);

        int length = reordList.serializedProperty.arraySize;
        SerializedProperty nextProp = (length > 0 && index < length - 1) ? reordList.serializedProperty.GetArrayElementAtIndex(index + 1) : null;

        SerializedProperty iteratorProp = parentProp.serializedObject.GetIterator();

        float height = 0.0f;

        while (iteratorProp.Next(true))
        {
            if (EqualContents(parentProp, iteratorProp))
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                while (iteratorProp.Next(true))
                {
                    if (EqualContents(nextProp, iteratorProp))
                        break;

                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                break;
            }
        }

        //height += 2.0f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

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