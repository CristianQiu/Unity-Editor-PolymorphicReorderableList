using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BaseCharacter : MonoBehaviour
{
#if UNITY_EDITOR

    public struct PerReorderableListElementState
    {
        private float height;
        private bool unfolded;

        public float Height { get { return height; } }
        public bool Unfolded { get { return unfolded; } }

        public void SetHeight(float height)
        {
            this.height = height;
        }

        public void ToggleFolded()
        {
            unfolded = !unfolded;
        }
    }

    private PerReorderableListElementState[] perReorderableListElementStates = new PerReorderableListElementState[MaxNumberOfActions];
    public PerReorderableListElementState[] PerReorderableListElementStates { get { return perReorderableListElementStates; } }

#endif

    public const int MaxNumberOfActions = 8;

    [SerializeReference, HideInInspector] private List<BaseActionSettings> settings;
}