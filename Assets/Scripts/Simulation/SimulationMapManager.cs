using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

enum SpawnType
{
    Food,
    Organism,
}

public class SimulationMapManager : MonoBehaviour
{
    public static SimulationMapManager instance = null;
    
    [Header("Initial Spawn Settings")]
    [SerializeField] private int startingOrganismCount;
    [SerializeField] private int startingFoodCount;
    [SerializeField] private float foodCutoff;
    [SerializeField] private float organismCutoff;
    [Header("Map Settings")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    [SerializeField] private float perlinScale;

    [Header("Food Spawn Settings")]
    [SerializeField] private float foodSpawnInterval;
    [SerializeField] private int idealCountPerInterval;
    [SerializeField] private int idealOrganismCount;
    [SerializeField] private int maxFoodCount;
    private float _foodTime;
    public int foodCount;
    public int seed = 0;
    [SerializeField] private GameObject foodPrefab;
    private float[,] _map;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (SimulationManager.instance.GetPopulation() == 0)
        {
            for (int i = 0; i < startingOrganismCount; i++)
            {
                SpawnInMap(SpawnType.Organism);
            }
        }

        _foodTime += Time.deltaTime;
        if (_foodTime > foodSpawnInterval && foodCount < maxFoodCount)
        {
            int count = (idealCountPerInterval * idealOrganismCount / SimulationManager.instance.GetPopulation());
            for (int i = 0; i < count; i++)
            {
                SpawnInMap(SpawnType.Food);
            }
            
            _foodTime = 0.0f;
        }
    }

    public void Start()
    {
        if (foodCutoff is > 1.0f or < 0.0f)
        {
            Debug.LogWarning("Food Cutoff must be between 0 and 1!");
        }
        
        if (organismCutoff is > 1.0f or < 0.0f)
        {
            Debug.LogWarning("Organism Cutoff must be between 0 and 1!");
        }


        
        seed = Random.Range(0, 10000);
        _map = GenerateMap(mapWidth, mapHeight);
        for (int i = 0; i < startingFoodCount; i++)
        {
            SpawnInMap(SpawnType.Food);
        }
        for (int i = 0; i < startingOrganismCount; i++)
        {
            SpawnInMap(SpawnType.Organism);
        }
        Debug.Log("Finished Spawning Everything");
    }

    private void SpawnInMap(SpawnType type)
    {
        if (foodCutoff is > 1.0f or < 0.0f)
        {
            return;
        }
        
        if (organismCutoff is > 1.0f or < 0.0f)
        {
            return;
        }
        
        float cutoff = foodCutoff;
        
        switch (type)
        {
            case SpawnType.Food:
                cutoff = foodCutoff;
                break;
            case SpawnType.Organism:
                cutoff = organismCutoff;
                break;
        }

        int x = Random.Range(0, mapWidth);
        int y = Random.Range(0, mapHeight);
        float random = 0;
        switch (type)
        {
            case SpawnType.Food:
                random = Random.Range(cutoff, 1.0f);
                break;
            case SpawnType.Organism:
                random = cutoff;
                break;
        }
        
        float value = _map[x, y];
        while (random > Mathf.Pow(value, 3.0f))
        {
            x = Random.Range(0, mapWidth);
            y = Random.Range(0, mapHeight);
            random = Random.Range(0.0f, 1.0f);
            value = _map[x, y];
        }

        switch (type)
        {
            case SpawnType.Food:
                Instantiate(foodPrefab, new Vector2(x - mapWidth / 2, y - mapHeight / 2), Quaternion.identity);
                foodCount++;
                break;
            case SpawnType.Organism:
                SimulationManager.instance.SpawnOrganism(new Vector2(x - mapWidth / 2, y - mapHeight / 2), SimulationManager.instance.defaultOrganismStats);
                break;
        }
        
            //     .GetComponent<SpriteRenderer>().color =
            // new Color(value, value,value)
            ;
        
    }

    private float[,] GenerateMap(int width, int height)
    {
        float[,] map = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xVal = (float)x / width + seed;
                float yVal = (float)y / height + seed;

                map[x, y] = Mathf.PerlinNoise(xVal * perlinScale, yVal * perlinScale);
            }
        }

        return map;
    }
}
