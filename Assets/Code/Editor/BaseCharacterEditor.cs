using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BaseCharacter))]
public class BaseCharacterEditor : Editor
{
    private int MaxNumberOfActions = BaseCharacter.MaxNumberOfActions;

    private BaseCharacter targetChar;

    private SerializedProperty settingsListProp;
    private ReorderableList reordList;

    // needed to save the state in case the user selects / deselects
    private BaseCharacter.PerReorderableListElementState[] perReorderableListElementStates;

    private void OnEnable()
    {
        targetChar = (BaseCharacter)target;
        perReorderableListElementStates = targetChar.PerReorderableListElementStates;

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

        float height = 20.0f;
        Rect foldOutRect = rect;
        foldOutRect.x += 15.0f;
        foldOutRect.height = 20.0f;

        bool clicked = EditorGUI.BeginFoldoutHeaderGroup(foldOutRect, true, new GUIContent(actionName));

        if (clicked)
            perReorderableListElementStates[index].ToggleFolded();

        if (!perReorderableListElementStates[index].Unfolded)
        {
            perReorderableListElementStates[index].SetHeight(height);
            EditorGUI.EndFoldoutHeaderGroup();
            return;
        }

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
                int i = 1;
                Rect newRect = rect;
                newRect.y += (i * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                while (iteratorProp.Next(true))
                {
                    if (EqualContents(nextProp, iteratorProp))
                        break;

                    newRect = rect;
                    newRect.y += (i * EditorGUIUtility.singleLineHeight + i * EditorGUIUtility.standardVerticalSpacing);

                    EditorGUI.PropertyField(newRect, iteratorProp, true);
                    i++;

                    height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                }

                perReorderableListElementStates[index].SetHeight(height);
                break;
            }
        }

        EditorGUI.EndFoldoutHeaderGroup();
    }

    private float OnReorderListElementHeight(int index)
    {
        return perReorderableListElementStates[index].Height;
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