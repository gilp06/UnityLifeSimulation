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
    public int offspringMutationCount;

    [Header("Organism Statistic Mutation Weights")]
    [SerializeField] private int fieldOfViewWeight;
    [SerializeField] private int maxViewDistanceWeight;
    [SerializeField] private int speedRatioWeight;
    [SerializeField] private int sizeRatioWeight;
    [SerializeField] private int offspringMutationCountWeight;
    [SerializeField] private int offspringStartingEnergyWeight;
    [SerializeField] private int offspringIncubateTimeWeight;
    [SerializeField] private int offspringHatchTimeWeight;
    [SerializeField] private int colorWeight;

    [Header("Organism Statistic Mutation Ranges")]
    [SerializeField] private float fieldOfViewRange;
    [SerializeField] private float maxViewDistanceRange;
    [SerializeField] private float speedRatioRange;
    [SerializeField] private float sizeRatioRange;
    [SerializeField] private int offspringMutationCountRange;
    [SerializeField] private float offspringStartingEnergyRange;
    [SerializeField] private float offspringIncubateTimeRange;
    [SerializeField] private float offspringHatchTimeRange;
    [SerializeField] private float colorRange;

        [Header("Organism Settings")]
    [SerializeField] private GameObject organismPrefab;
    public int inputCount = 5;
    public int outputCount = 6;

    

    [Header("Food Settings")] 
    public float baseFoodEnergy;
    public float foodEnergyRange;
    
    private List<ConnectableOperator> _connectableOperators;
    private WeightedList<GenomeMutationAction> _randomGenomeMutationActions;
    private WeightedList<OrganismStatsMutationAction> _randomOrganismStatMutationActions;
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
        _randomOrganismStatMutationActions = new WeightedList<OrganismStatsMutationAction>()
        {
            { OrganismStatsMutationAction.MutateFieldOfView, fieldOfViewWeight },
            { OrganismStatsMutationAction.MutateMaxViewDistance, maxViewDistanceWeight},
            { OrganismStatsMutationAction.MutateSpeedRatio, speedRatioWeight},
            { OrganismStatsMutationAction.MutateSizeRatio, sizeRatioWeight},
            { OrganismStatsMutationAction.MutateOffspringMutationCount , offspringMutationCountWeight},
            { OrganismStatsMutationAction.MutateOffspringStartingEnergy , offspringStartingEnergyWeight},
            { OrganismStatsMutationAction.MutateOffspringIncubateTime, offspringIncubateTimeWeight},
            { OrganismStatsMutationAction.MutateOffspringHatchTime , offspringHatchTimeWeight},
            { OrganismStatsMutationAction.MutateColor, colorWeight},
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
            offspringMutationCount,
            offspringStartingEnergy,
            offspringIncubateTime,
            offspringHatchTime,
            color.r,
            color.g,
            color.b
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
        var childGenome = parentGenome == null ? new Genome(inputCount, outputCount) : new Genome(parentGenome);
        
        MutateGenome(childGenome, parentStats.offspringMutationCount);
        organismHandler.SetGenome(childGenome);

        OrganismStats childStats = new OrganismStats(parentStats);
        MutateStats(childStats, parentStats.offspringMutationCount);
        organismHandler.SetBirthStats(childStats, parentStats.offspringStartingEnergy);
        _organisms.Add(newOrganism);
    }

    private void MutateStats(OrganismStats childStats, int mutationCount)
    {
        for (int i = 0; i < Random.Range(1,mutationCount+1); i++)
        {
            OrganismStatsMutationAction action = _randomOrganismStatMutationActions.Next();
            switch (action)
            {
                case OrganismStatsMutationAction.MutateFieldOfView:
                    childStats.fieldOfView += Random.Range(-fieldOfViewRange, fieldOfViewRange);
                    childStats.fieldOfView = Mathf.Clamp(childStats.fieldOfView, 0.0f, 180.0f);
                    break;
                case OrganismStatsMutationAction.MutateMaxViewDistance:
                    childStats.maxViewDistance += Random.Range(-maxViewDistanceRange, maxViewDistanceRange);
                    childStats.maxViewDistance = Mathf.Max(childStats.maxViewDistance, 0.0f);
                    break;
                case OrganismStatsMutationAction.MutateSpeedRatio:
                    childStats.speedRatio += Random.Range(-speedRatioRange, speedRatioRange);
                    childStats.speedRatio = Mathf.Max(childStats.speedRatio, 0.0f);
                    break;
                case OrganismStatsMutationAction.MutateSizeRatio:
                    childStats.sizeRatio += Random.Range(-sizeRatioRange, sizeRatioRange);
                    childStats.sizeRatio = Mathf.Max(childStats.sizeRatio, 0.01f);
                    break;
                case OrganismStatsMutationAction.MutateOffspringMutationCount:
                    childStats.offspringMutationCount += Random.Range(-(offspringMutationCountRange),
                        offspringMutationCountRange + 1);
                    childStats.offspringMutationCount = Mathf.Max(1, childStats.offspringMutationCount);
                    break;
                case OrganismStatsMutationAction.MutateOffspringStartingEnergy:
                    childStats.offspringStartingEnergy +=
                        Random.Range(-offspringStartingEnergyRange, offspringStartingEnergyRange);
                    childStats.offspringStartingEnergy = Mathf.Max(0.1f, childStats.offspringStartingEnergy);
                    break;
                case OrganismStatsMutationAction.MutateOffspringIncubateTime:
                    childStats.offspringIncubateTime +=
                        Random.Range(-offspringIncubateTimeRange, offspringIncubateTimeRange);
                    childStats.offspringIncubateTime = Mathf.Max(0.1f, childStats.offspringIncubateTime);
                    break;
                case OrganismStatsMutationAction.MutateOffspringHatchTime:
                    childStats.offspringHatchTime +=
                        Random.Range(-offspringHatchTimeRange, offspringHatchTimeRange);
                    childStats.offspringHatchTime = Mathf.Max(0.1f, childStats.offspringHatchTime);
                    break;
                case OrganismStatsMutationAction.MutateColor:
                    childStats.red += Random.Range(-colorRange, colorRange);
                    childStats.red = Mathf.Clamp(childStats.red, 0.0f, 1.0f);
                    
                    childStats.blue += Random.Range(-colorRange, colorRange);
                    childStats.blue = Mathf.Clamp(childStats.blue, 0.0f, 1.0f);
                    
                    childStats.green += Random.Range(-colorRange, colorRange);
                    childStats.green = Mathf.Clamp(childStats.green, 0.0f, 1.0f);
                    break;
               

            }
        }
    }

    private void MutateGenome(Genome genome, int mutationCount)
    {
        for (int i = 0; i < Random.Range(1,mutationCount+1); i++)
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

    private void Update()
    {
        
    }
}
