using UnityEngine;

public interface IInteractible
{
    public void CanBePicked();
    public void CannotBePicked();
    public void Interact();

    public Transform GetTransform();
}
