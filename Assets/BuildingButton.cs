using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
    }

    public void SelectBuilding()
    {
        buttonImage.color = Color.yellow;
    }
}