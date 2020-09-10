using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class BaseCharacter : MonoBehaviour
{
    [SerializeReference] private List<BaseActionSettings> settings = null;
}