using UnityEngine;
using UnityEngine.UI;

public class EnergyManager : MonoBehaviour
{
    [Header("Energy Storage")]

    [Tooltip("Maximum amount of energy the storage can hold.")]
    public float maxEnergy = 1000f;

    [Tooltip("Current amount of energy.")]
    public float currentEnergy = 1000f;


    [Header("Buildings")]

    [Tooltip("All buildings that can consume energy.")]
    public BuildingRoadSelector[] buildings;


    [Header("Road Colors")]

    public Color roadOnColor = Color.yellow;
    public Color roadOffColor = Color.white;


    [Header("Progress (Firebase)")]

    [Tooltip("Drag the GameProgress object here so energy is saved as the score.")]
    [SerializeField] private GameProgress progress;


    void Start()
    {
        // Make sure current energy doesn't exceed maximum
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        // Update road colors when the game starts
        UpdateRoads();
    }


    void Update()
    {
        // Calculate total energy consumption
        float totalConsumption = GetTotalEnergyConsumption();

        // Deduct energy over time
        currentEnergy -= totalConsumption * Time.deltaTime;

        // Prevent energy from going below zero
        currentEnergy = Mathf.Max(currentEnergy, 0f);


        // Report the current energy to GameProgress so it can be
        // shown and saved as the score. This only updates memory;
        // the database write is throttled inside GameProgress.
        if (progress != null)
        {
            progress.SetScore(Mathf.RoundToInt(currentEnergy));
        }


        // If energy reaches zero,
        // automatically turn all buildings OFF
        if (currentEnergy <= 0f && totalConsumption > 0f)
        {
            TurnOffAllBuildings();
        }
    }


    // Calculates the combined consumption
    // of every building that is currently ON
    public float GetTotalEnergyConsumption()
    {
        float totalConsumption = 0f;

        if (buildings == null)
        {
            return 0f;
        }

        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] == null)
            {
                continue;
            }

            totalConsumption += buildings[i].GetEnergyConsumption();
        }

        return totalConsumption;
    }


    // Checks whether there is enough energy
    // to turn on another building
    public bool CanTurnOnBuilding(BuildingRoadSelector building)
    {
        if (building == null)
        {
            return false;
        }

        // Current consumption from buildings already ON
        float currentConsumption = GetTotalEnergyConsumption();

        // Consumption after turning on this building
        float newConsumption =
            currentConsumption +
            building.energyConsumptionPerSecond;

        // If there is no energy left
        if (currentEnergy <= 0f)
        {
            return false;
        }

        // Allow the building to turn on
        return true;
    }


    // Turns every building OFF
    public void TurnOffAllBuildings()
    {
        if (buildings == null)
        {
            return;
        }

        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] == null)
            {
                continue;
            }

            buildings[i].TurnOffBuilding();
        }

        UpdateRoads();

        Debug.Log("Energy storage is empty. All buildings turned OFF.");
    }


    // Updates all road colors.
    //
    // This handles shared roads correctly.
    //
    // Example:
    //
    // Building 1 → Road A, Road B
    // Building 2 → Road B, Road C
    //
    // If Building 1 is OFF but Building 2 is ON,
    // Road B will remain yellow.
    public void UpdateRoads()
    {
        if (buildings == null)
        {
            return;
        }

        // First turn every road we know about white
        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] == null)
            {
                continue;
            }

            buildings[i].SetRoadsWhite();
        }


        // Then turn roads yellow if at least
        // one connected building is ON
        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] == null)
            {
                continue;
            }

            if (!buildings[i].IsActive())
            {
                continue;
            }

            GameObject[] roads = buildings[i].connectedRoads;

            if (roads == null)
            {
                continue;
            }

            for (int j = 0; j < roads.Length; j++)
            {
                if (roads[j] == null)
                {
                    continue;
                }

                Image roadImage = roads[j].GetComponent<Image>();

                if (roadImage != null)
                {
                    roadImage.color = roadOnColor;
                }
            }
        }
    }
}