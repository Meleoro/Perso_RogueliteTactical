using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [Header("Private Infos")]
    private List<IInteractible> interactiblesAtRange = new List<IInteractible>();
    private IInteractible closestInteractible;

    [Header("Referencess")]
    private Transform currentHeroTransform;

    private void Start()
    {
        closestInteractible = null;
    }

    public void ActualiseCurrentHeroTransform(Transform newTr)
    {
        currentHeroTransform = newTr;
    }


    #region Detect Interactibles

    private void Update()
    {
        transform.position = currentHeroTransform.position;

        if (closestInteractible is null) return;
        if (InputManager.wantsToInteract)
        {
            closestInteractible.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AddInteractibleAtRange(collision.GetComponent<IInteractible>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        RemoveInteractibleAtRange(collision.GetComponent<IInteractible>());
    }

    #endregion


    #region Manage Interactibles

    public void AddInteractibleAtRange(IInteractible interactible)
    {
        interactiblesAtRange.Add(interactible);
        ActualiseClosestInteractible();
    }

    public void RemoveInteractibleAtRange(IInteractible interactible)
    {
        interactible.CannotBePicked();
        interactiblesAtRange.Remove(interactible);
        ActualiseClosestInteractible();
    }

    private void ActualiseClosestInteractible()
    {
        closestInteractible = null;
        if (interactiblesAtRange.Count == 0) return;

        int closestIndex = 0;
        float bestDist = Vector2.Distance(interactiblesAtRange[0].GetTransform().position, currentHeroTransform.position);

        for(int i = 1; i < interactiblesAtRange.Count; i++)
        {
            interactiblesAtRange[i].CannotBePicked();
            float dist = Vector2.Distance(interactiblesAtRange[0].GetTransform().position, currentHeroTransform.position);

            if(dist < bestDist)
            {
                bestDist = dist;
                closestIndex = i;
            }
        }

        interactiblesAtRange[closestIndex].CanBePicked();
        closestInteractible = interactiblesAtRange[closestIndex];
    }

    #endregion
}
