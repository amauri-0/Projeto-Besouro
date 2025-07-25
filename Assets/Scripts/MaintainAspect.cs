using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MaintainAspect : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // barras acima/abaixo
            Rect r = cam.rect;
            r.width = 1f;
            r.height = scaleHeight;
            r.x = 0;
            r.y = (1f - scaleHeight) / 2f;
            cam.rect = r;
        }
        else
        {
            // barras nas laterais
            float scaleWidth = 1f / scaleHeight;
            Rect r = cam.rect;
            r.width = scaleWidth;
            r.height = 1f;
            r.x = (1f - scaleWidth) / 2f;
            r.y = 0;
            cam.rect = r;
        }
    }
}
