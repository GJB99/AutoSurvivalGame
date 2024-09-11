using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Smelter : MonoBehaviour
{
    public float smeltingTime = 5f;
    private Dictionary<string, int> inputResources = new Dictionary<string, int>();
    private Dictionary<string, int> outputResources = new Dictionary<string, int>();
    private int fuelAmount = 0;
    private const int MAX_STACK = 100;

    public void AddResource(string resourceName, int amount)
    {
        if (inputResources.ContainsKey(resourceName))
        {
            inputResources[resourceName] = Mathf.Min(inputResources[resourceName] + amount, MAX_STACK);
        }
        else
        {
            inputResources[resourceName] = Mathf.Min(amount, MAX_STACK);
        }
    }

    public void AddFuel(string fuelType, int amount)
    {
        fuelAmount = Mathf.Min(fuelAmount + amount, MAX_STACK);
    }

    public void StartSmelting()
    {
        if (fuelAmount > 0)
        {
            foreach (var resource in inputResources)
            {
                if (resource.Value > 0)
                {
                    StartCoroutine(SmeltResource(resource.Key));
                }
            }
        }
    }

    IEnumerator SmeltResource(string inputResource)
    {
        while (inputResources[inputResource] > 0 && fuelAmount > 0)
        {
            inputResources[inputResource]--;
            fuelAmount--;
            yield return new WaitForSeconds(smeltingTime);
            string outputResource = inputResource + " Ingot";
            if (outputResources.ContainsKey(outputResource))
            {
                outputResources[outputResource] = Mathf.Min(outputResources[outputResource] + 1, MAX_STACK);
            }
            else
            {
                outputResources[outputResource] = 1;
            }
        }
    }

    public string GetSmelterInfo()
    {
        string info = "Smelter:\n";
        info += "Input:\n";
        foreach (var resource in inputResources)
        {
            info += $"{resource.Key}: {resource.Value}\n";
        }
        info += $"Fuel: {fuelAmount}\n";
        info += "Output:\n";
        foreach (var resource in outputResources)
        {
            info += $"{resource.Key}: {resource.Value}\n";
        }
        return info;
    }
}