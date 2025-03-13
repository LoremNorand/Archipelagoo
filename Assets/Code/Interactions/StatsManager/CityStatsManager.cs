using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityStatsManager : MonoBehaviour
{
    public static CityStatsManager Instance { get; private set; }
    
    [SerializeField] private List<CityStat> stats;

    private Dictionary<string, CityStat> statsDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        
        InitStats();
    }
    
    private void InitStats()
    {
        statsDictionary = new Dictionary<string, CityStat>();
        foreach (var stat in stats)
        {
            statsDictionary[stat.statName] = stat;
        }
    }

    public void ModifyStat(string name, float amount)
    {
        if (statsDictionary.TryGetValue(name, out CityStat stat))
        {
            stat.Modify(amount);
        }
        else
        {
            Debug.LogWarning($"Stat {name} not found!");
        }
    }

    public void SafeModifyStat(string name, float amount)
    {
		if(statsDictionary.TryGetValue(name, out CityStat stat))
		{
			stat.SafeModify(amount);
		}
		else
		{
			Debug.LogWarning($"Stat {name} not found!");
		}
	}
    
    public void SetStat(string statName, float value)
    {
        if (statsDictionary.TryGetValue(statName, out CityStat stat))
        {
            stat.SetValue(value);
        }
        else
        {
            Debug.LogWarning($"Stat {statName} not found!");
        }
    }

    public float GetStat(string statName)
    {
        return statsDictionary.TryGetValue(statName, out CityStat stat) ? stat.currentValue : 0f;
    }
}
