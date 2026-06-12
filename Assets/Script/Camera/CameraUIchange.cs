using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class IndependentHoverChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("このボタンに乗った時に色を変えたい画像（複数可）")]
    [SerializeField] private List<Image> targetImages = new List<Image>();

    [Header("ホバー時の色")]
    [SerializeField] private Color hoverColor = Color.red;

    // それぞれの画像の元の色を記憶
    private List<Color> defaultColors = new List<Color>();

    private void Start()
    {
        foreach (var img in targetImages)
        {
            if (img != null)
            {
                defaultColors.Add(img.color);
            }
            else
            {
                defaultColors.Add(Color.white);
            }
        }
    }

    // このボタンにマウスカーソルが重なった時
    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var img in targetImages)
        {
            if (img != null)
            {
                img.color = hoverColor;
            }
        }
    }

    // このボタンからマウスカーソルが離れた時
    public void OnPointerExit(PointerEventData eventData)
    {
        for (int i = 0; i < targetImages.Count; i++)
        {
            if (targetImages[i] != null && i < defaultColors.Count)
            {
                targetImages[i].color = defaultColors[i]; 
            }
        }
    }
}
