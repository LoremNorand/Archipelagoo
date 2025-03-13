using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStat", menuName = "City Builder/City Stat")]
public class CityStat : ScriptableObject
{
    public string statName;
    public float baseValue;
    public float currentValue;

    private void OnEnable()
    {
        currentValue = baseValue;
    }
    
    public void Modify(float amount)
    {
        currentValue += amount;
    }

    public void SetValue(float newValue)
    {
        currentValue = newValue;
    }
}
