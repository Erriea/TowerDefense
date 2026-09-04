using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatioCamera : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f;

    private Camera cam;
    private float lastKnownAspect;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        float windowAspect = (float)Screen.width / Screen.height;

        if (Mathf.Approximately(windowAspect, lastKnownAspect))
            return;

        lastKnownAspect = windowAspect;

        float scaleHeight = windowAspect / targetAspect;
        Rect rect = cam.rect;

        if (scaleHeight < 1f)
        {
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0f;
        }

        cam.rect = rect;
    }
}