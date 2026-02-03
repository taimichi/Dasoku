using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapIcon : MonoBehaviour
{
    public Transform target;
    public MiniMapMg miniMap;

    RectTransform rect;

    PlayerData plData;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        plData = PlayerData.Instance;
    }

    void Update()
    {
        if (target == null)
        {
            target = plData.nowBody.transform;
        }

        if (target != plData.nowBody.transform)
        {
            target = plData.nowBody.transform;
        }

        rect.anchoredPosition =
            miniMap.WorldToMiniMapPosition(target.position);
    }
}
