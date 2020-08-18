using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BaseCharacter : MonoBehaviour
{
    [SerializeReference, HideInInspector] private List<BaseActionSettings> settings;
}