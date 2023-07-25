using System;
using System.Collections;
using System.Collections.Generic;
using Genes;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class OrganismHandler : MonoBehaviour
{
    public float rotationSpeed = 90.0f;
    public float strafeCoefficient = 0.4f;
    public float backwardCoefficient = 0.4f;
    public float movementSharpness = 15f;
    public float angularSharpness = 10f;

    public int rayCastCount;
    
    public GameObject eggPrefab;
    private Genome _genome;
    private Rigidbody2D _rb;
    private OrganismStats _stats;
    private DigestionHandler _digestionHandler;
    
    private float _remainingEnergy = 50.0f;
    private float _nearestOrganismDistance;
    private float _nearestFoodDistance;
    private float _angleToFood;
    private float _angleToOrganism;
    private int _nearbyFoodCount;
    private int _nearbyOrganismCount;
    private Color _nearestColor; 
    public float reproductionTime;
    private bool _inEgg;
    private bool _statsSet = false;

    private Vector2 _refVel;
    private float _refAngVel;

    public float actualMaxEnergy;
    [FormerlySerializedAs("actualEnergyConsumption")] public float actualEnergyChange;
    public float actualSpeed;
    public float actualTurnSpeed;

    private float _forwardMove;
    private float _backwardMove;
    private float _leftMove;
    private float _rightMove;
    private float _turnLeft;
    private float _turnRight;
    private float _wantsToLayValue;
    private float _wantsToIncubateValue;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _digestionHandler = GetComponent<DigestionHandler>();
    }

    private void Update()
    {
        if(!_statsSet) return;
        FindNearby();
        _genome.SetInput(0, _nearestOrganismDistance / _stats.maxViewDistance);
        // //Distance to food
        _genome.SetInput(1, _nearestFoodDistance / _stats.maxViewDistance);
        // //Positive Angle to food
        _genome.SetInput(2, (_angleToOrganism/(_stats.fieldOfView/2.0f) * 0.5f + 0.5f));
        // //Negative Angle to nearest organism
        _genome.SetInput(3, (_angleToFood/(_stats.fieldOfView/2.0f) * 0.5f + 0.5f));
        // //Energy remaining
        _genome.SetInput(4, _remainingEnergy / actualMaxEnergy);
        // Nearby Food Count
        _genome.SetInput(5, _nearbyFoodCount / (_nearbyFoodCount + 2.0f));
        //Nearby Organism Count
        _genome.SetInput(6, _nearbyOrganismCount / (_nearbyOrganismCount + 2.0f));
        //Nearest Color
        _genome.SetInput(7, _nearestColor.r);
        _genome.SetInput(8, _nearestColor.g);
        _genome.SetInput(9, _nearestColor.b);
        
        _genome.Activate();

        _forwardMove = _genome.GetOutput(0);
        _backwardMove = _genome.GetOutput(1);
        _rightMove = _genome.GetOutput(2);
        _leftMove = _genome.GetOutput(3);
        _turnLeft = _genome.GetOutput(4);
        _turnRight = _genome.GetOutput(5);
        _wantsToLayValue = _genome.GetOutput(6);
        _wantsToIncubateValue = _genome.GetOutput(7);
        
        if (_wantsToIncubateValue >= 0.5f && _remainingEnergy > _stats.offspringStartingEnergy)
        {
            reproductionTime += Time.deltaTime;
        }
        else
        {
            reproductionTime = 0.0f;
        }
        
        
        if (_remainingEnergy <= 0.0f && _statsSet)
        {
            KillOrganism();
        }

        if (actualMaxEnergy > _stats.offspringStartingEnergy && _remainingEnergy >= _stats.offspringStartingEnergy && _statsSet && reproductionTime > _stats.offspringIncubateTime && _wantsToLayValue >= 0.5f)
        {
            GameObject egg = Instantiate(eggPrefab, transform.position, Quaternion.identity);
            EggHandler eggHandler = egg.GetComponent<EggHandler>();
            eggHandler.SetData(_stats, _genome);
            _remainingEnergy -= _stats.offspringStartingEnergy;
            reproductionTime = 0.0f;
        }

        if (reproductionTime > _stats.offspringIncubateTime)
        {
            reproductionTime = _stats.offspringIncubateTime;
        }
        
        CalculateEnergyChange();
        _remainingEnergy += actualEnergyChange;
        if (_remainingEnergy > actualMaxEnergy)
        {
            _remainingEnergy = actualMaxEnergy;
        }
    }

    private void FixedUpdate()
    {
        if(!_statsSet) return;
        //Distance to nearest organism
        float forwardMovement = (_forwardMove - _backwardMove);
         if (forwardMovement < 0)
         {
             forwardMovement *= backwardCoefficient;
         }
         float horizontalMovement = (_rightMove - _leftMove) * strafeCoefficient;
         Vector2 movement = transform.right * forwardMovement +
                            transform.up * horizontalMovement;

         Vector2 targetVelocity = movement * actualSpeed;
         float targetAngularVelocity = actualTurnSpeed * (_turnLeft - _turnRight);

         _rb.velocity = Vector2.Lerp(_rb.velocity, targetVelocity, 1-Mathf.Exp(-movementSharpness * Time.deltaTime));
         _rb.angularVelocity = Mathf.LerpAngle(_rb.angularVelocity, targetAngularVelocity, 1-Mathf.Exp(-angularSharpness * Time.deltaTime));
         
    }

    private void CalculateEnergyChange()
    {
        float baseEnergyConsumption = -SimulationManager.instance.energyConsumptionPerSecond;
        actualEnergyChange = baseEnergyConsumption * _stats.sizeRatio;
        actualEnergyChange *= (1 + (_rb.velocity.magnitude / actualSpeed) * (_stats.speedRatio));
        actualEnergyChange *= Time.deltaTime;
        actualEnergyChange += _digestionHandler.GetCurrentDigestionEnergy();
    }
    

    private void FindNearby()
    {
        if (!_statsSet) return;
        _nearbyOrganismCount = 0;
        _nearbyFoodCount = 0;
        _nearestOrganismDistance = _stats.maxViewDistance + 1;
        _nearestFoodDistance = _stats.maxViewDistance + 1;
        var hits = Physics2D.OverlapCircleAll(transform.position, _stats.maxViewDistance);
            foreach (var hit in hits)
            {
                if(hit.transform == transform) continue;

                float angle = Vector2.Angle(hit.transform.position-transform.position, transform.right);

                if (Mathf.Abs(angle) > _stats.fieldOfView / 2.0f) continue;
                
                Debug.DrawLine(transform.position, hit.transform.position);
                
                Transform hitTransform = hit.transform;
                if (hitTransform.CompareTag("organism"))
                {
                    var distance = (hit.transform.position - transform.position).magnitude;
                    Debug.DrawLine(transform.position, hit.transform.position);
                    _nearbyOrganismCount++;
                    if (_nearestOrganismDistance > distance)
                    {
                        OrganismStats stats = hitTransform.GetComponent<OrganismHandler>().GetStats();
                        _nearestColor = new Color(stats.red, stats.green, stats.blue);
                        _nearestOrganismDistance = distance;
                        _angleToOrganism = 0.0f;
                    }
                }
                if (hitTransform.CompareTag("food"))
                {
                    Debug.DrawLine(transform.position, hit.transform.position);
                    var distance = (hit.transform.position - transform.position).magnitude;
                    _nearbyFoodCount++;
                    if (_nearestFoodDistance > distance)
                    {
                        _nearestFoodDistance = distance;
                        _angleToFood = 0.0f;
                    }
                }
            }


            if (_nearestOrganismDistance > _stats.maxViewDistance)
        {
            _nearestOrganismDistance = 0.0f;
            _angleToOrganism = 0.0f;
        }

        if (_nearestFoodDistance > _stats.maxViewDistance)
        {
            _nearestFoodDistance = 0.0f;
            _angleToFood = 0.0f;
        }
        
    }

    void KillOrganism()
    {
        SimulationManager.instance.RemoveOrganismFromList(gameObject);
        Destroy(gameObject);
    }

    public void SetGenome(Genome genome)
    {
        _genome = genome;
    }

    public void SetBirthStats(OrganismStats stats, float startingEnergy)
    {
        _stats = stats;
        actualSpeed = SimulationManager.instance.baseSpeed * stats.speedRatio * (2 - stats.sizeRatio);
        actualTurnSpeed = rotationSpeed * stats.speedRatio * (2 - stats.sizeRatio);
        actualMaxEnergy = SimulationManager.instance.maxEnergy * stats.sizeRatio;
        _remainingEnergy = startingEnergy;
        transform.localScale = new Vector3(SimulationManager.instance.size, SimulationManager.instance.size, 1) * stats.sizeRatio;
        GetComponent<SpriteRenderer>().color = new Color(stats.red / 255.0f, stats.green / 255.0f , stats.blue, 255.0f);
        _statsSet = true;
    }

    private float GetSpeed(float signal)
    {
        return signal;
    }

    public Genome GetGenome()
    {
        return _genome;
    }

    public OrganismStats GetStats()
    {
        return _stats;
    }

    public float GetEnergy()
    {
        return _remainingEnergy;
    }

    public void AddEnergy(float amount)
    {
        _remainingEnergy += amount;
        if (_remainingEnergy > actualMaxEnergy)
        {
            _remainingEnergy = actualMaxEnergy;
        }
    }

    
    
}
