using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigestionHandler : MonoBehaviour
{
    private OrganismHandler _handler;
    private OrganismStats _stats;

    public float baseMaximumEnergyUnitsInStomachMultiplier;
    public float percentageDigestionRatePerSecond;
    public float consumptionRatePerSecond;
    public float mouthSizeRatio;
    private float _maximumEnergyUnitsInStomach;
    private float _currentEnergyUnitsInStomach;
    private float _digestedEnergy;
    private CircleCollider2D _collider;

    private void Start()
    {
        _collider = GetComponent<CircleCollider2D>();
        _handler = GetComponent<OrganismHandler>();
        _stats = _handler.GetStats();
    }

    private void Update()
    {
        _digestedEnergy = 0.0f;
        _maximumEnergyUnitsInStomach = baseMaximumEnergyUnitsInStomachMultiplier * _handler.actualMaxEnergy * _stats.sizeRatio;
        CheckForFood();
        
        
        if (_currentEnergyUnitsInStomach <= 0) return;
        _digestedEnergy = _currentEnergyUnitsInStomach * percentageDigestionRatePerSecond * Time.deltaTime;
        _currentEnergyUnitsInStomach -= _digestedEnergy;
    }

    private void CheckForFood()
    {
        if (_currentEnergyUnitsInStomach >= _maximumEnergyUnitsInStomach) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + transform.right * _collider.bounds.extents.x,
            _collider.bounds.extents.x * mouthSizeRatio);
        Debug.DrawLine(transform.position, transform.position + transform.right * _collider.bounds.extents.x, Color.cyan);
        
        foreach (var hit in hits)
        {
            if (hit.transform.CompareTag("food"))
            {
                FoodHandler foodHandler = hit.GetComponent<FoodHandler>();
                foodHandler.energyValue -= consumptionRatePerSecond * Time.deltaTime;
                _currentEnergyUnitsInStomach += consumptionRatePerSecond * Time.deltaTime;
            }
        }
    }

    public float GetCurrentDigestionEnergy()
    {
        return _digestedEnergy;
    }

    public float GetCurrentEnergyInStomach()
    {
        return _currentEnergyUnitsInStomach;
    }
}
