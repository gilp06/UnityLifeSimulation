using System;
using System.Collections;
using System.Collections.Generic;
using Genes;
using UnityEngine;

public class EggHandler : MonoBehaviour
{
    private Genome _genome;
    private OrganismStats _stats;
    private float _eggTime = 0.0f;
    private bool _statsSet = false;
    private void Start()
    {
        
    }

    private void Update()
    {
        if (!_statsSet) return;
        _eggTime += Time.deltaTime;

        if (_eggTime >= _stats.offspringHatchTime)
        {
            SimulationManager.instance.SpawnOrganism(transform.position, _stats, _genome);
            Destroy(gameObject);
        }
    }

    public void SetData(OrganismStats stats, Genome genome)
    {
        _genome = genome;
        _stats = stats;
        _eggTime = 0.0f;
        _statsSet = true;
    }
}
