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
    public float mouthLength;
    private float _maximumEnergyUnitsInStomach;
    private float _currentEnergyUnitsInStomach;
    private float _digestedEnergy;
    private PolygonCollider2D _collider;

    private void Start()
    {
        _collider = GetComponent<PolygonCollider2D>();
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
        mouthLength = transform.localScale.x / 32.0f;
        Vector2 reference = _collider.points[0];
        Vector3 extents = new Vector3(mouthLength, reference.y * transform.localScale.y, 1.0f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            transform.position + transform.right * (_collider.points[0].x * transform.localScale.x + mouthLength), extents,
            transform.eulerAngles.z);
        Debug.DrawLine(transform.position + transform.up * (reference.y * transform.localScale.y),
            transform.position + transform.up * (reference.y * transform.localScale.y) + transform.right * (_collider.points[0].x * transform.localScale.x + mouthLength), Color.red);
        Debug.DrawLine(transform.position - transform.up * (reference.y * transform.localScale.y),
            transform.position - transform.up * (reference.y * transform.localScale.y) + transform.right * (_collider.points[0].x * transform.localScale.x + mouthLength), Color.red);

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
