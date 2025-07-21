using UnityEngine;

public class CameraFixer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float camSize;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hero"))
        {
            CameraManager.Instance.LockCamera(transform.position, camSize);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Hero"))
        {
            CameraManager.Instance.UnlockCamera();
        }
    }
}
