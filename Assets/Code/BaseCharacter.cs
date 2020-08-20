using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BaseCharacter : MonoBehaviour
{
    [SerializeReference] private List<BaseActionSettings> settings;
}