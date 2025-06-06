using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public Image crosshairImage; // UI Image для точки прицела
    public Color inactiveColor = Color.white; // Цвет, когда не наведено
    public Color activeColor = Color.green; // Цвет, когда наведено

    void Start()
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = inactiveColor;
        }
    }

    public void SetActive(bool isActive)
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = isActive ? activeColor : inactiveColor;
        }
    }
}