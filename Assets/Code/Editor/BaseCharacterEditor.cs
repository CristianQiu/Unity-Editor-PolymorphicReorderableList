using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BaseCharacter))]
public class BaseCharacterEditor : Editor
{
    #region Private Attributes

    private const float AdditionalSpaceMultiplier = 1.0f;
    private static readonly Color ProSkinTextColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
    private static readonly Color PersonalSkinTextColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

    private BaseCharacter targetChar;

    private ReorderableList reordList;

    private GUIStyle headersStyle;

    #endregion

    #region Editor Methods

    private void OnEnable()
    {
        targetChar = (BaseCharacter)target;

        reordList = new ReorderableList(serializedObject, serializedObject.FindProperty("settings"), true, true, true, true);

        headersStyle = new GUIStyle();
        headersStyle.alignment = TextAnchor.MiddleLeft;
        headersStyle.normal.textColor = EditorGUIUtility.isProSkin ? ProSkinTextColor : PersonalSkinTextColor;
        headersStyle.fontStyle = FontStyle.Bold;

        reordList.drawHeaderCallback += OnDrawReorderListHeader;
        reordList.drawElementCallback += OnDrawReorderListElement;
        reordList.elementHeightCallback += OnReorderListElementHeight;
        reordList.onAddDropdownCallback += OnReorderListAddDropdown;
    }

    private void OnDisable()
    {
        reordList.drawElementCallback -= OnDrawReorderListElement;
        reordList.elementHeightCallback -= OnReorderListElementHeight;
        reordList.drawHeaderCallback -= OnDrawReorderListHeader;
        reordList.onAddDropdownCallback -= OnReorderListAddDropdown;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        reordList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region ReorderableList Callbacks

    private void OnDrawReorderListHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Actions settings");
    }

    private void OnDrawReorderListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (reordList.serializedProperty.arraySize <= 0)
            return;

        SerializedProperty iteratorProp = reordList.serializedProperty.GetArrayElementAtIndex(index);

        SerializedProperty actionTypeParentProp = iteratorProp.FindPropertyRelative("actionType");
        string actionName = actionTypeParentProp.enumDisplayNames[actionTypeParentProp.enumValueIndex];

        Rect labelRect = rect;
        labelRect.height = 20.0f;

        EditorGUI.LabelField(labelRect, actionName, headersStyle);

        EditorGUI.indentLevel++;

        // get the following property in the array, if any
        int length = reordList.serializedProperty.arraySize;
        SerializedProperty nextProp = (length > 0 && index < length - 1) ? reordList.serializedProperty.GetArrayElementAtIndex(index + 1) : null;

        int i = 0;
        while (iteratorProp.NextVisible(true))
        {
            // go until the next property in the array
            if (EqualContents(nextProp, iteratorProp))
                break;

            float multiplier = i == 0 ? AdditionalSpaceMultiplier : 1.0f;
            rect.y += GetDefaultSpaceBetweenElements() * multiplier;
            EditorGUI.PropertyField(rect, iteratorProp, true);
            i++;
        }

        EditorGUI.indentLevel--;
    }

    private float OnReorderListElementHeight(int index)
    {
        if (reordList.serializedProperty.arraySize <= 0)
            return 0.0f;

        int length = reordList.serializedProperty.arraySize;

        SerializedProperty nextProp = (length > 0 && index < length - 1) ? reordList.serializedProperty.GetArrayElementAtIndex(index + 1) : null;
        SerializedProperty iteratorProp = reordList.serializedProperty.GetArrayElementAtIndex(index);

        float height = GetDefaultSpaceBetweenElements();

        int i = 0;
        while (iteratorProp.NextVisible(true))
        {
            if (EqualContents(nextProp, iteratorProp))
                break;

            float multiplier = i == 0 ? AdditionalSpaceMultiplier : 1.0f;
            height += GetDefaultSpaceBetweenElements() * multiplier;
            i++;
        }

        return height;
    }

    private void OnReorderListAddDropdown(Rect buttonRect, ReorderableList list)
    {
        GenericMenu menu = new GenericMenu();

        for (int i = 0; i < (int)ActionType.Count; i++)
        {
            string actionName = ((ActionType)i).ToString();
            actionName = SplitStringByUpperCases(actionName);

            menu.AddItem(new GUIContent(actionName), false, OnAddItemFromDropdown, (object)((ActionType)i));
        }

        menu.ShowAsContext();
    }

    private void OnAddItemFromDropdown(object o)
    {
        ActionType action = (ActionType)o;

        int last = reordList.serializedProperty.arraySize;
        reordList.serializedProperty.InsertArrayElementAtIndex(last);

        SerializedProperty lastProp = reordList.serializedProperty.GetArrayElementAtIndex(last);

        switch (action)
        {
            case ActionType.Movement:
                lastProp.managedReferenceValue = new MovementActionSettings();
                break;

            case ActionType.MeleeAttack:
                lastProp.managedReferenceValue = new MeleeAttackActionSettings();
                break;

            case ActionType.RangedAttack:
                lastProp.managedReferenceValue = new RangedAttackActionSettings();
                break;

            case ActionType.Heal:
                lastProp.managedReferenceValue = new HealActionSettings();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region Helper Methods

    private float GetDefaultSpaceBetweenElements()
    {
        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
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

    #endregion
}