using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapFog : MonoBehaviour
{
    public MiniMapMg miniMap;
    public Transform player;
    public int textureSize = 512;
    public float revealRadius = 10f;

    Texture2D fogTex;
    RawImage img;

    PlayerData plData;

    void Start()
    {
        img = GetComponent<RawImage>();

        fogTex = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);

        Color[] cols = new Color[textureSize * textureSize];
        for (int i = 0; i < cols.Length; i++)
            cols[i] = Color.black;

        fogTex.SetPixels(cols);
        fogTex.Apply();

        img.texture = fogTex;

        plData = PlayerData.Instance;

    }

    void Update()
    {
        if (player == null)
        {
            player = plData.nowBody.transform;
        }

        //ƒvƒŒƒCƒ„[‚ÌTransform‚ª•Ï‚í‚Á‚½Žž
        if (player != plData.nowBody.transform)
        {
            player = plData.nowBody.transform;
        }

        Reveal(player.position);
    }

    void Reveal(Vector3 worldPos)
    {
        float u = Mathf.InverseLerp(miniMap.worldMin.x, miniMap.worldMax.x, worldPos.x);
        float v = Mathf.InverseLerp(miniMap.worldMin.y, miniMap.worldMax.y, worldPos.y);

        int cx = (int)(u * textureSize);
        int cy = (int)(v * textureSize);

        int r = Mathf.RoundToInt(revealRadius);

        for (int x = -r; x <= r; x++)
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y > r * r) continue;

                int px = cx + x;
                int py = cy + y;

                if (px < 0 || px >= textureSize || py < 0 || py >= textureSize) continue;

                fogTex.SetPixel(px, py, Color.clear);
            }

        fogTex.Apply();
    }
}
