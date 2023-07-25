using System;
using System.Collections;
using System.Collections.Generic;
using Genes;
using KaimiraGames;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using Genome = Genes.Genome;
using Random = UnityEngine.Random;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager instance;
    
    [Header("Genome Mutation Weights")] 
    [SerializeField] private int addConnectionWeight;
    [SerializeField] private int removeConnectionWeight;
    [SerializeField] private int addNodeWeight;
    [SerializeField] private int changeWeightingWeight;
    [SerializeField] private int changeNodeOperatorWeight; 
    [SerializeField] private int noneMutationWeight;

    [Header("Base Organism Statistics")] 
    public float baseSpeed;
    public float maxEnergy;
    public float energyConsumptionPerSecond;
    public float offspringStartingEnergy;
    public float offspringIncubateTime;
    public float offspringHatchTime;
    public float size;
    public Color color;
    public float fieldOfView;
    public float maxViewDistance;
    public float mutationVariance; 
    public float averageStatMutationChance;
    public float averageGenomeMutationChance;
    

    [Header("Organism Settings")]
    [SerializeField] private GameObject organismPrefab;
    public int inputCount = 5;
    public int outputCount = 6;

    

    [Header("Food Settings")] 
    public float baseFoodEnergy;
    public float foodEnergyRange;
    
    private List<ConnectableOperator> _connectableOperators;
    private WeightedList<GenomeMutationAction> _randomGenomeMutationActions;
    private List<OrganismStatsMutationAction> _statsMutationActions; 
    public OrganismStats defaultOrganismStats;
    private List<GameObject> _organisms;
    private void Awake()
    {
        _organisms = new List<GameObject>();
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
        InitializeRandomGenomeMutationActions();
        InitializeRandomStatMutationActions();
        InitializeDefaultOrganismStats();
        InitializeConnectableOperators();
    }

    private void InitializeRandomStatMutationActions()
    {
        _statsMutationActions = new List<OrganismStatsMutationAction>()
        {
            OrganismStatsMutationAction.MutateFieldOfView,
            OrganismStatsMutationAction.MutateMaxViewDistance,
            OrganismStatsMutationAction.MutateSpeedRatio,
            OrganismStatsMutationAction.MutateSizeRatio,
            OrganismStatsMutationAction.MutateAverageStatMutationChance,
            OrganismStatsMutationAction.MutateAverageGenomeMutationChance,
            OrganismStatsMutationAction.MutateMutationVariance,
            OrganismStatsMutationAction.MutateOffspringStartingEnergy,
            OrganismStatsMutationAction.MutateOffspringIncubateTime,
            OrganismStatsMutationAction.MutateOffspringHatchTime,
            OrganismStatsMutationAction.MutateColor,
        };
    }

    private void InitializeConnectableOperators()
    {
        _connectableOperators = new List<ConnectableOperator>
        {
            ConnectableOperator.Sigmoid,
            ConnectableOperator.Abs,
            ConnectableOperator.Clamped,
            ConnectableOperator.Cube,
            ConnectableOperator.Exp,
            ConnectableOperator.Gauss,
            ConnectableOperator.Hat,
            ConnectableOperator.Sin,
            ConnectableOperator.Square,
            ConnectableOperator.Tanh,
            ConnectableOperator.Identity,
        };
    }

    private void InitializeDefaultOrganismStats()
    {
        defaultOrganismStats = new OrganismStats
        (
            fieldOfView,
            maxViewDistance,
            1.0f,
            1.0f,
            averageStatMutationChance,
            averageGenomeMutationChance,
            mutationVariance,
            offspringStartingEnergy,
            offspringIncubateTime,
            offspringHatchTime,
            color.r * 255f,
            color.g * 255f,
            color.b * 255f
        );
    }

    private void InitializeRandomGenomeMutationActions()
    {
        _randomGenomeMutationActions = new WeightedList<GenomeMutationAction>
        {
            { GenomeMutationAction.AddConnection, addConnectionWeight },
            { GenomeMutationAction.RemoveConnection, removeConnectionWeight },
            { GenomeMutationAction.AddNode, addNodeWeight },
            { GenomeMutationAction.ChangeWeighting, changeWeightingWeight },
            { GenomeMutationAction.ChangeNodeOperator, changeNodeOperatorWeight},
            { GenomeMutationAction.None, noneMutationWeight },
        };
    }

    

    public void SpawnOrganism(Vector2 location, OrganismStats parentStats, Genome parentGenome = null)
    {
        GameObject newOrganism = Instantiate(organismPrefab, location, Quaternion.Euler(0,0,Random.Range(0f,360f)));
        OrganismHandler organismHandler = newOrganism.GetComponent<OrganismHandler>();
        
        //Mutate Genome
        var childGenome = parentGenome == null ? new Genome(inputCount, outputCount) : new Genome(parentGenome);
        int genomeMutationCount = CalculateNumberOfMutations(parentStats.averageGenomeMutationChance);
        MutateGenome(childGenome, genomeMutationCount);
        organismHandler.SetGenome(childGenome);
        
        //Mutate Stats
        OrganismStats childStats = new OrganismStats(parentStats);
        int statMutationCount = CalculateNumberOfMutations(parentStats.averageStatMutationChance);
        MutateStats(childStats, statMutationCount, parentStats.mutationVariance);
        organismHandler.SetBirthStats(childStats, parentStats.offspringStartingEnergy);
        
        
        _organisms.Add(newOrganism);
    }

    private void MutateStats(OrganismStats childStats, int mutationCount, float variance)
    {
        for (int i = 0; i < mutationCount; i++)
        {
            OrganismStatsMutationAction action = _statsMutationActions[Random.Range(0, _statsMutationActions.Count)];
            switch (action)
            {
                case OrganismStatsMutationAction.MutateFieldOfView:
                    childStats.fieldOfView = MutateGeneValue(childStats.fieldOfView, variance);
                    childStats.fieldOfView = Mathf.Clamp(childStats.fieldOfView, 0.0f, 360.0f);
                    break;
                case OrganismStatsMutationAction.MutateMaxViewDistance:
                    childStats.maxViewDistance = MutateGeneValue(childStats.maxViewDistance, variance);
                    childStats.maxViewDistance = Mathf.Clamp(childStats.maxViewDistance, 0.0001f, 200.00f);
                    break;
                case OrganismStatsMutationAction.MutateSpeedRatio:
                    childStats.speedRatio = MutateGeneValue(childStats.speedRatio, variance);
                    childStats.speedRatio = Mathf.Clamp(childStats.speedRatio, 0.0001f, 10.00f);
                    break;
                case OrganismStatsMutationAction.MutateSizeRatio:
                    childStats.sizeRatio = MutateGeneValue(childStats.sizeRatio, variance);
                    childStats.sizeRatio = Mathf.Clamp(childStats.sizeRatio, 0.0001f, 10.00f);
                    break;
                case OrganismStatsMutationAction.MutateAverageStatMutationChance:
                    childStats.averageStatMutationChance = MutateGeneValue(childStats.averageStatMutationChance, variance);
                    break;
                case OrganismStatsMutationAction.MutateAverageGenomeMutationChance:
                    childStats.averageGenomeMutationChance = MutateGeneValue(childStats.averageGenomeMutationChance, variance);
                    break;
                case OrganismStatsMutationAction.MutateMutationVariance:
                    childStats.mutationVariance = MutateGeneValue(childStats.mutationVariance, variance);
                    break;
                case OrganismStatsMutationAction.MutateOffspringStartingEnergy:
                    childStats.offspringStartingEnergy = MutateGeneValue(childStats.offspringStartingEnergy, variance);
                    break;
                case OrganismStatsMutationAction.MutateOffspringIncubateTime:
                    childStats.offspringIncubateTime = MutateGeneValue(childStats.offspringIncubateTime, variance);
                    break;
                case OrganismStatsMutationAction.MutateOffspringHatchTime:
                    childStats.offspringHatchTime = MutateGeneValue(childStats.offspringHatchTime, variance);
                    break;
                case OrganismStatsMutationAction.MutateColor:
                    childStats.red = MutateGeneValue(childStats.red, variance);
                    childStats.red = Mathf.Clamp(childStats.red, 0f, 255f);

                    childStats.green = MutateGeneValue(childStats.green, variance);
                    childStats.green = Mathf.Clamp(childStats.green, 0f, 255f);
                    
                    childStats.blue = MutateGeneValue(childStats.blue, variance);
                    childStats.blue = Mathf.Clamp(childStats.blue, 0f, 255f);
                    break;
            }
        }
    }

    private void MutateGenome(Genome genome, int mutationCount)
    {
        for (int i = 0; i < mutationCount; i++)
        {
            GenomeMutationAction action = _randomGenomeMutationActions.Next();
            switch (action)
            {
                case GenomeMutationAction.AddConnection:
                    genome.MutateAddConnection();
                    break;
                case GenomeMutationAction.RemoveConnection:
                    genome.MutateRemoveConnection();
                    break;
                case GenomeMutationAction.AddNode:
                    genome.MutateAddNode();
                    break;
                case GenomeMutationAction.ChangeWeighting:
                    genome.MutateChangeWeighting();
                    break;
                case GenomeMutationAction.ChangeNodeOperator:
                    genome.MutateChangeNodeOperator();
                    break;
            }
        }
    }

    public GameObject GetRandomOrganism()
    {
        return _organisms[Random.Range(0, _organisms.Count)];
    }

    public ConnectableOperator GetRandomConnectableOperator()
    {
        return _connectableOperators[Random.Range(0, _connectableOperators.Count)];
    }

    public void RemoveOrganismFromList(GameObject organism)
    {
        _organisms.Remove(organism);
    }
    
    public int GetPopulation()
    {
        return _organisms.Count;
    }
    

    private float MutateGeneValue(float value, float variance)
    {
        float baseValue = value;
        float u = DistributionMath.NextGaussian(0.0f, 1.0f, -1.0f, 1.0f);
        float v = Mathf.Pow(1 + variance, u);
        return baseValue * v;
    }

    private int CalculateNumberOfMutations(float lambda)
    {
        return DistributionMath.NextPoisson(lambda);
    }
}
