using System;
using UnityEngine;

public enum ActionType
{
    Invalid = -1,

    Movement,
    MeleeAttack,
    RangedAttack,
    Heal,

    Count
}

[Serializable]
public abstract class BaseActionSettings
{
    [SerializeField, HideInInspector] protected ActionType actionType = ActionType.Invalid;

    protected BaseActionSettings()
    {
    }
}

[Serializable]
public class MovementActionSettings : BaseActionSettings
{
    [SerializeField, Range(0.0f, 5.0f)] private float maxSpeed = 4.75f;
    [SerializeField] private float acceleration = 16.0f;
    [SerializeField] private float decelerationBoostWithRespectToAcceleration = 3.0f;
    [SerializeField] private float turnToAnyAxisSmoothness = 0.33f;

    public MovementActionSettings() : base()
    {
        actionType = ActionType.Movement;
    }
}

[Serializable]
public class HealActionSettings : BaseActionSettings
{
    [SerializeField] private int healQuantity = 10;
    [SerializeField] private bool removesDiseases = true;

    public HealActionSettings() : base()
    {
        actionType = ActionType.Heal;
    }
}

[Serializable]
public class MeleeAttackActionSettings : BaseActionSettings
{
    [SerializeField] private int damage = 10;

    public MeleeAttackActionSettings() : base()
    {
        actionType = ActionType.MeleeAttack;
    }
}

[Serializable]
public class RangedAttackActionSettings : MeleeAttackActionSettings
{
    [SerializeField] private float range = 10.0f;

    public RangedAttackActionSettings() : base()
    {
        actionType = ActionType.RangedAttack;
    }
}