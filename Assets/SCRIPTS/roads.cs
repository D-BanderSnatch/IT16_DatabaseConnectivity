using UnityEngine;
using UnityEngine.UI;

public class Road : MonoBehaviour
{
    private Image roadImage;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow;

    void Start()
    {
        roadImage = GetComponent<Image>();
        roadImage.color = normalColor;
    }

    public void SetYellow()
    {
        roadImage.color = selectedColor;
    }

    public void SetWhite()
    {
        roadImage.color = normalColor;
    }
}