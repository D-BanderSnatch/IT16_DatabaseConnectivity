using UnityEngine;
using TMPro;

public class EnergyUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI energyText;

    [Header("Energy Manager")]
    public EnergyManager energyManager;


    void Update()
    {
        if (energyText == null || energyManager == null)
        {
            return;
        }

        // Round the energy for display
        int current = Mathf.CeilToInt(energyManager.currentEnergy);
        int max = Mathf.CeilToInt(energyManager.maxEnergy);

        // Display energy
        energyText.text = "⚡ Energy: " + current + " / " + max;
    }
}
