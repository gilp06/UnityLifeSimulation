using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OrganismStatView : MonoBehaviour
{
    public GameObject target;
    private OrganismHandler _organismHandler;
    public TMP_Text text;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("organism"))
                {
                    target = hit.transform.gameObject;
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            target = null;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            target = SimulationManager.instance.GetRandomOrganism();
            _organismHandler = target.GetComponent<OrganismHandler>();
        }

        UpdateText();
    }

    private void UpdateText()
    {
        string output = "";
        output += "Statistics:\n";
        output += "Current Population: " + SimulationManager.instance.GetPopulation() +"\n";
        output += "Food Count: " + SimulationMapManager.instance.foodCount + "\n";
        if (target != null)
        {
            output += "Energy: " + Format(_organismHandler.GetEnergy()) + "/" + Format(_organismHandler.actualMaxEnergy) + "\n";
            output += "Generation: " + Format(_organismHandler.GetGenome().generation) + "\n";
            output += "Speed: " + Format(_organismHandler.GetComponent<Rigidbody2D>().velocity.magnitude) + "\n";
            output += "Maximum Speed: " + Format(_organismHandler.actualSpeed) + "\n";
            output += "Size Multiplier: " + Format(_organismHandler.GetStats().sizeRatio)+ "\n";
            output += "Speed Multiplier: " + Format(_organismHandler.GetStats().speedRatio) + "\n";
            output += "Field of View: " + Format(_organismHandler.GetStats().fieldOfView) + "\n";
            output += "View Distance: " + Format(_organismHandler.GetStats().maxViewDistance) + "\n";
            output += "Offspring Energy: " + Format(_organismHandler.GetStats().offspringStartingEnergy) + "\n";
            output += "Offspring Mutations: " + Format(_organismHandler.GetStats().offspringMutationCount) + "\n";
            output += "Offspring Hatch Time: " + Format(_organismHandler.GetStats().offspringHatchTime) + "\n";
            output += "Incubation Time: "+ Format(_organismHandler.reproductionTime) + "/" + Format(_organismHandler.GetStats().offspringIncubateTime) + "\n";
        }

        text.text = output;
    }

    private string Format(float value)
    {
        return $"{value:0.##}";
    }
}
