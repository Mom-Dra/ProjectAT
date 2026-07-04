using System.Collections.Generic;
using System.Diagnostics;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public enum ModifierType { Flat, PercentAdd }

public class StatModifier
{
    public float Value;
    public ModifierType Type;
    public object Source;

    public StatModifier(float value, ModifierType type, object source)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}

public class Stat
{
    public float BaseValue { get; private set; }
    public float MinValue { get; private set; }
    public float MaxValue { get; private set; }

    private List<StatModifier> modifiers = new List<StatModifier>();
    private float value;
    private bool isDirty = true;

    public float Value
    {
        get
        {
            if (isDirty)
            {
                value = CalculateFinalValue();
                isDirty = false;
            }

            return value;
        }
    }

    public Stat(float baseValue, float minValue = float.NegativeInfinity, float maxValue = float.PositiveInfinity)
    {
        BaseValue = baseValue;
        MinValue = minValue;
        MaxValue = Mathf.Max(minValue, maxValue);
    }

    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
        isDirty = true;
    }

    public void RemoveAllModifiersFromSource(object source)
    {
        modifiers.RemoveAll(m => m.Source == source);
        isDirty = true;
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0f;

        foreach (StatModifier modifier in modifiers)
        {
            switch (modifier.Type)
            {
                case ModifierType.Flat:
                    finalValue += modifier.Value;
                    break;

                case ModifierType.PercentAdd:
                    sumPercentAdd += modifier.Value;
                    break;
            }
        }

        float calculatedValue = finalValue * (1 + sumPercentAdd);
        return Mathf.Clamp(calculatedValue, MinValue, MaxValue);
    }
}
