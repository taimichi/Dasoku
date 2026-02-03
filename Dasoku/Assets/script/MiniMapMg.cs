using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapMg : MonoBehaviour
{
    [Tooltip("左下")]
    public Vector2 worldMin;
    [Tooltip("右上")]
    public Vector2 worldMax;
    public RectTransform miniMapRect;

    public Vector2 WorldToMiniMapPosition(Vector3 worldPos)
    {
        float u = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
        float v = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.y); // ←ここ重要（y）

        Vector2 size = miniMapRect.sizeDelta;

        return new Vector2(
            (u - 0.5f) * size.x,
            (v - 0.5f) * size.y
        );
    }
}
