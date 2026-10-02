using UnityEngine;
using UnityEngine.UI;

public class BuildingRoadSelector : MonoBehaviour
{
    [Header("Building Settings")]

    [Tooltip("Roads connected to this building.")]
    public GameObject[] connectedRoads;

    [Tooltip("How much energy this building consumes every second.")]
    public float energyConsumptionPerSecond = 2f;

    // Is this building currently turned ON?
    private bool isActive = false;

    // Reference to the Energy Manager
    private EnergyManager energyManager;


    void Start()
    {
        // Find the EnergyManager in the scene
        energyManager = FindFirstObjectByType<EnergyManager>();

        // Make sure the building starts OFF
        isActive = false;

        // Make sure its roads start white
        SetRoadsWhite();
    }


    // Called by the Building Button
    public void SelectBuilding()
    {
        // If building is currently OFF
        if (!isActive)
        {
            // Check if there is enough energy available
            if (energyManager != null && !energyManager.CanTurnOnBuilding(this))
            {
                Debug.Log("Not enough energy to turn on " + gameObject.name);
                return;
            }

            // Turn building ON
            isActive = true;

            Debug.Log(gameObject.name + " turned ON");
        }
        else
        {
            // Turn building OFF
            isActive = false;

            Debug.Log(gameObject.name + " turned OFF");
        }

        // Update all roads
        UpdateRoadColors();

        // Tell EnergyManager that something changed
        if (energyManager != null)
        {
            energyManager.UpdateRoads();
        }
    }


    // Returns whether this building is currently ON
    public bool IsActive()
    {
        return isActive;
    }


    // Returns how much energy this building consumes
    public float GetEnergyConsumption()
    {
        if (isActive)
        {
            return energyConsumptionPerSecond;
        }

        return 0f;
    }


    // Used by EnergyManager when energy reaches zero
    public void TurnOffBuilding()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;

        UpdateRoadColors();

        Debug.Log(gameObject.name + " automatically turned OFF.");
    }


    // Updates the roads connected to this building
    public void UpdateRoadColors()
    {
        if (connectedRoads == null)
        {
            return;
        }

        for (int i = 0; i < connectedRoads.Length; i++)
        {
            if (connectedRoads[i] == null)
            {
                continue;
            }

            Image roadImage = connectedRoads[i].GetComponent<Image>();

            if (roadImage != null)
            {
                // Road will be updated by EnergyManager
                // so shared roads work correctly.
                continue;
            }
        }
    }


    // Turns all roads connected to this building white
    public void SetRoadsWhite()
    {
        if (connectedRoads == null)
        {
            return;
        }

        for (int i = 0; i < connectedRoads.Length; i++)
        {
            if (connectedRoads[i] == null)
            {
                continue;
            }

            Image roadImage = connectedRoads[i].GetComponent<Image>();

            if (roadImage != null)
            {
                roadImage.color = Color.white;
            }
        }
    }


    // Turns all roads connected to this building yellow
    public void SetRoadsYellow()
    {
        if (connectedRoads == null)
        {
            return;
        }

        for (int i = 0; i < connectedRoads.Length; i++)
        {
            if (connectedRoads[i] == null)
            {
                continue;
            }

            Image roadImage = connectedRoads[i].GetComponent<Image>();

            if (roadImage != null)
            {
                roadImage.color = Color.yellow;
            }
        }
    }
}
