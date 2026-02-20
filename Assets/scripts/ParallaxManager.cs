using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerObject;
        [Tooltip("0 = moves with camera (no parallax), 1 = stays completely still")]
        [Range(0f, 1f)]
        public float parallaxFactor;
    }

    [Header("Camera Reference")]
    [Tooltip("Drag your Main Camera here.")]
    public Camera mainCamera;

    [Header("Parallax Layers")]
    [Tooltip("Add each background layer here. Furthest layer gets highest parallax factor.")]
    public ParallaxLayer[] layers;

    private Vector3 _lastCameraPos;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        _lastCameraPos = mainCamera.transform.position;
    }

    private void LateUpdate()
    {
        // How much the camera moved this frame
        Vector3 cameraDelta = mainCamera.transform.position - _lastCameraPos;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerObject == null) continue;

            // Move each layer by a fraction of the camera movement
            // Lower parallaxFactor = moves almost same as camera (foreground)
            // Higher parallaxFactor = moves very little (far background)
            layer.layerObject.position += new Vector3(
                cameraDelta.x * (1f - layer.parallaxFactor),
                cameraDelta.y * (1f - layer.parallaxFactor),
                0f  // don't move on Z so layers stay in place depth-wise
            );
        }

        _lastCameraPos = mainCamera.transform.position;
    }
}