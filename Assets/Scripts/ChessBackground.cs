using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RawImage))]
public class ChessBackground : MonoBehaviour
{
    [Header("Colori")]
    public Color darkColor  = new Color(0.118f, 0.071f, 0.031f, 1f);   // #1E1208
    public Color lightColor = new Color(0.784f, 0.663f, 0.431f, 0.05f); // #C8A96E

    [Header("Dimensione quadretti (pixel)")]
    public int tileSize = 40;

    void Start()
    {
        GeneratePattern();
    }

    void GeneratePattern()
    {
        
        int w = Screen.width;
        int h = Screen.height;

        
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode   = TextureWrapMode.Clamp;

        
        Color[] pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isLight = ((x / tileSize) + (y / tileSize)) % 2 == 0;
                pixels[y * w + x] = isLight ? lightColor : darkColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        GetComponent<RawImage>().texture = tex;
    }
}
