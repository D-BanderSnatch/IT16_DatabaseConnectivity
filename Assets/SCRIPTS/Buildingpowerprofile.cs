using UnityEngine;


public class BuildingPowerProfile : MonoBehaviour
{
    public enum PowerLevel
    {
        Low,
        Medium,
        High
    }

    [Header("Building Power")]

    [Tooltip("How power-hungry this building is.")]
    [SerializeField] private PowerLevel powerLevel = PowerLevel.Low;

    [Header("Power Per Second For Each Level")]

    [SerializeField] private float lowPower = 5f;
    [SerializeField] private float mediumPower = 10f;
    [SerializeField] private float highPower = 20f;

    [Header("State")]

    [SerializeField] private bool isOn = false;


   
    public float GetPowerPerSecond()
    {
        switch (powerLevel)
        {
            case PowerLevel.Low: return lowPower;
            case PowerLevel.Medium: return mediumPower;
            case PowerLevel.High: return highPower;
            default: return 0f;
        }
    }


    public float GetCurrentConsumption()
    {
        return isOn ? GetPowerPerSecond() : 0f;
    }


    public bool IsOn()
    {
        return isOn;
    }


    public void SetOn(bool value)
    {
        isOn = value;
    }
}