using System.Collections;
using UnityEngine;
using Utilities;

public class ChallengeTrigger : MonoBehaviour, IInteractible
{
    [Header("Parameters")]
    [SerializeField] private float displayedDitherValue;
    [SerializeField] private float hiddenDitherValue;
    [SerializeField] private Loot lootPrefab;

    [Header("Private Infos")]
    private bool isActivated = false;
    private PossibleLootData[] possibleLoots;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Room _room;
    [SerializeField] private Animator _animator;
    [SerializeField] private ParticleSystem _chestVFX;
    [SerializeField] private SpriteRenderer _shadowSpriteRenderer;


    private void Start()
    {
        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));
        possibleLoots = ProceduralGenerationManager.Instance.enviroData.lootPerFloors[ProceduralGenerationManager.Instance.CurrentFloor].chestPossibleLoots;
    }

    private void GenerateLoot()
    {
        int pickedPercentage = Random.Range(0, 100);
        int currentSum = 0;

        for (int i = 0; i < possibleLoots.Length; i++)
        {
            currentSum += possibleLoots[i].probability;

            if (currentSum > pickedPercentage)
            {
                Loot newLoot = Instantiate(lootPrefab, transform.position, Quaternion.Euler(0, 0, 0));
                newLoot.Initialise(possibleLoots[i].loot);

                break;
            }
        }
    }


    private IEnumerator InteractCoroutine(float openDuration)
    {
        CameraManager.Instance.FocusOnTr(transform, 3f);

        transform.UShakePosition(openDuration * 0.3f, 0.2f, 0.04f);

        yield return new WaitForSeconds(openDuration * 0.3f);

        _animator.SetTrigger("Open");

        yield return new WaitForSeconds(openDuration * 0.25f);

        GenerateLoot();
        //_chestLight.ULerpIntensity(openDuration * 0.03f, 3f);

        yield return new WaitForSeconds(openDuration * 0.03f);

        //_chestLight.ULerpIntensity(openDuration * 0.05f, 0f);
    }


    public void Show()
    {
        _spriteRenderer.material.ULerpMaterialFloat(0.5f, displayedDitherValue, "_DitherProgress");

        _shadowSpriteRenderer.enabled = true;

        StartCoroutine(InteractCoroutine(1.5f));
    }


    public void Hide()
    {
        _spriteRenderer.material.ULerpMaterialFloat(0.5f, hiddenDitherValue, "_DitherProgress");
        //_spriteRenderer.material.ULerpMaterialFloat(0.2f, 0f, "_OutlineSize");
        _spriteRenderer.material.SetFloat("_OutlineSize", 0.2f);

        _chestVFX.Stop();
        _shadowSpriteRenderer.enabled = false;

        BattleManager.Instance.OnBattleEnd += Show;
    }


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

        Hide();
        _room.StartBattle();
    }

    #endregion
}
