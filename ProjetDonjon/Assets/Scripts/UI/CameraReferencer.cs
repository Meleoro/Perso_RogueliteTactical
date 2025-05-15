using UnityEngine;

public class CameraReferencer : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    void Start()
    {
        canvas.worldCamera = CameraManager.Instance.Camera;
    }
}
