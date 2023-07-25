using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class FoodHandler : MonoBehaviour
{
    public float energyValue;

    private void Start()
    {
        energyValue = SimulationManager.instance.baseFoodEnergy;
        energyValue += Random.Range(0.0f,
            SimulationManager.instance.foodEnergyRange);
        transform.localScale *= energyValue / SimulationManager.instance.baseFoodEnergy;
    }

    private void Update()
    {
        if (!(energyValue <= 0.0f))
        {
            transform.localScale = new Vector3(1, 1, 1) * (energyValue / SimulationManager.instance.baseFoodEnergy);
            transform.localScale = Vector3.Max(new Vector3(1.0f, 1.0f, 1.0f), transform.localScale);
        }
        else
        {
            SimulationMapManager.instance.foodCount--;
            Destroy(gameObject);
        }
        
    }
}
