using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AspectRatio : MonoBehaviour
{
    private float screenHeight;
    private RectTransform rectTransform;
    void Start()
    {
        screenHeight = Screen.height;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, screenHeight);
    }

    // Update is called once per frame
    void Update()
    {
        if (screenHeight != Screen.height)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, screenHeight);
            screenHeight = Screen.height;
        }
    }
}
