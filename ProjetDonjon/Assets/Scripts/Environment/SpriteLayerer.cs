using UnityEngine;

public class SpriteLayerer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int offset;

    [Header("Public Infos")]
    [HideInInspector] public int publicOffset;

    [Header("Private Infos")]
    private Transform referenceTr;
    private bool isInitialised;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;


    public void Initialise(Transform referenceTr)
    {
        this.referenceTr = referenceTr;
        isInitialised = true;
    }


    private void Update()
    {
        if (!isInitialised) return;

        _spriteRenderer.sortingOrder = Mathf.Clamp(100 - (int)((transform.position.y - referenceTr.position.y) * 5) + offset + publicOffset, 1, 200);
    }
}
