using UnityEngine;
using Utilities;

public class Chest : MonoBehaviour, IInteractible
{
    [Header("Parameters")]
    [SerializeField] private Loot lootPrefab;

    [Header("Private Infos")]
    private bool isOpened;
    private PossibleLootData[] possibleLoots;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;


    private void Start()
    {
        possibleLoots = ProceduralGenerationManager.Instance.enviroData.lootPerFloors[ProceduralGenerationManager.Instance.currentFloor].chestPossibleLoots;
    }


    private void GenerateLoot()
    {
        int pickedPercentage = Random.Range(0, 100);
        int currentSum = 0;

        for(int i = 0; i < possibleLoots.Length; i++)
        {
            currentSum += possibleLoots[i].probability;

            if(currentSum > pickedPercentage)
            {
                Loot newLoot = Instantiate(lootPrefab, transform.position, Quaternion.Euler(0, 0, 0));
                newLoot.Initialise(possibleLoots[i].loot);

                break;
            }
        }
    }


    #region Interface Functions

    public void CanBePicked()
    {
        if (isOpened) return;
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 1f, "_OutlineSize");
    }

    public void CannotBePicked()
    {
        if (isOpened) return;
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact()
    {
        if (isOpened) return;

        isOpened = true;
        _animator.SetTrigger("Open");
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
        _collider.enabled = false;

        GenerateLoot();
    }

    #endregion
}
