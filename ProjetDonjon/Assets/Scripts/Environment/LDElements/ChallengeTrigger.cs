using UnityEngine;
using Utilities;

public class ChallengeTrigger : MonoBehaviour, IInteractible
{
    [Header("Private Infos")]
    private bool isActivated = false;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Room _room;


    #region Interface Functions

    public void CanBePicked()
    {
        if (isActivated) return;
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 1f, "_OutlineSize");
    }

    public void CannotBePicked()
    {
        if (isActivated) return;
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact()
    {
        if (isActivated) return;

        isActivated = true;
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
        _collider.enabled = false;

        _room.StartBattle();
    }

    #endregion
}
