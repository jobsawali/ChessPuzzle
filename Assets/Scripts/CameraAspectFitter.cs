using UnityEngine;


[RequireComponent(typeof(Camera))]
public class CameraAspectFitter : MonoBehaviour
{
    [Header("Dimensioni scacchiera")]
    [Tooltip("Larghezza totale della board in world units (8 caselle * squareSize + margine coordinate)")]
    public float boardWidth = 9f;

    [Tooltip("Altezza totale della board in world units")]
    public float boardHeight = 9f;

    [Header("Margine extra (per UI sopra/sotto)")]
    [Tooltip("Spazio aggiuntivo in world units da lasciare libero (top bar, storico, frecce, ecc.)")]
    public float extraVerticalSpace = 4f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        FitCameraToScreen();
    }

    void FitCameraToScreen()
    {
        if (!cam.orthographic) return;

        float screenAspect = (float)Screen.width / Screen.height;

        
        float totalHeight = boardHeight + extraVerticalSpace;
        float halfHeight   = totalHeight / 2f;

       
        float sizeByHeight = halfHeight;

        
        float halfWidth   = boardWidth / 2f;
        float sizeByWidth = halfWidth / screenAspect;

        
        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }

    public void Refresh()
    {
        FitCameraToScreen();
    }
}
