using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

public class CameraManager : GenericSingletonClass<CameraManager>
{
    [Header("Parameters")]
    [SerializeField] private float followSpeed;
    [SerializeField] private float sizeFollowSpeed;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private float battleMinSize;
    [SerializeField] private float battleMaxSize;

    [Header("Camera Auto")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float baseSizeExplo;
    [SerializeField] private float maxSizeModifierExplo;
    [SerializeField] private float maxRaycastDistExplo;
    [SerializeField] private int raycastAmount;

    [Header("Private Infos")]
    private Transform followedTransform;
    private Vector3 currentWantedPos;
    private float currentWantedSize;
    private float baseSize;
    private bool isInitialised;
    private bool isInBattle;
    private bool isShaking;
    private bool isLocked;
    private float angleBetweenRaycasts;
    private Vector2 bottomLeftLimit;
    private Vector2 upRightLimit;

    [Header("Public Infos")]
    public Camera Camera { get { return _camera; } }

    [Header("Actions")]
    public Action OnCameraMouseInput;

    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _parentTransform;


    public void Initialise(Transform followedTransform)
    {
        isInitialised = true;

        this.followedTransform = followedTransform;
        baseSize = _camera.orthographicSize;

        angleBetweenRaycasts = 360f / raycastAmount;
    }


    private void LateUpdate()
    {
        if (!isInBattle && isInitialised && !isLocked)
        {
            currentWantedPos = followedTransform.position + followOffset;
            currentWantedSize = baseSize + GetEnviroSizeModificator();
        }

        transform.position = Vector3.Lerp(transform.position, currentWantedPos, followSpeed * Time.deltaTime);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, currentWantedSize, sizeFollowSpeed * Time.deltaTime);

        ManageMouseInputs();
    }


    #region Explo Camera Functions

    private float GetEnviroSizeModificator()
    {
        float currentAngle = 0;
        float averageDist = 0;

        for(int i = 0; i < raycastAmount; i++)
        {
            Vector2 dir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
            RaycastHit2D hit = Physics2D.Raycast(currentWantedPos, dir, maxRaycastDistExplo, wallLayerMask);

            if (hit) averageDist += hit.distance;
            else averageDist += maxRaycastDistExplo;

            currentAngle += angleBetweenRaycasts;
        }
        
        return Mathf.Lerp(-maxSizeModifierExplo, 0, ((averageDist / raycastAmount) / maxRaycastDistExplo));
    }

    public void LockCamera(Vector2 position, float size)
    {
        currentWantedPos = (Vector3)position - Vector3.forward * 10;
        currentWantedSize = size;

        isLocked = true;
    }

    public void UnlockCamera()
    {
        isLocked = false;
    }

    #endregion


    #region Battle Functions

    public void EnterBattle(Vector3 battleCenterPos, float cameraSize)
    {
        isInBattle = true;

        currentWantedPos = battleCenterPos + followOffset;
        currentWantedSize = cameraSize;

        bottomLeftLimit = battleCenterPos - new Vector3(10, 5);
        upRightLimit = battleCenterPos + new Vector3(10, 5);
    }

    public void ExitBattle()
    {
        isInBattle = false;
    }

    private void ManageMouseInputs()
    {
        if (Input.GetMouseButton(1))
        {
            currentWantedPos -= (Vector3)InputManager.mouseDelta * (Time.deltaTime * 4);
            if(InputManager.mouseDelta.sqrMagnitude > 0.1f)
            {
                OnCameraMouseInput?.Invoke();
            }

            currentWantedPos = new Vector3(
                Mathf.Clamp(currentWantedPos.x, bottomLeftLimit.x, upRightLimit.x),
                Mathf.Clamp(currentWantedPos.y, bottomLeftLimit.y, upRightLimit.y), 
                currentWantedPos.z);
        }

        currentWantedSize += InputManager.mouseScroll;
        currentWantedSize = Mathf.Clamp(currentWantedSize, battleMinSize, battleMaxSize);
    }

    public void FocusOnTr(Transform focusedTr, float cameraSize)
    {
        if (isLocked) return;

        currentWantedPos = focusedTr.position + followOffset;
        currentWantedSize = cameraSize;
    }

    public IEnumerator FocusOnTrCoroutine(Transform focusedTr, float cameraSize, float duration)
    {
        float timer = 0;
        Vector3 startPos = currentWantedPos;
        float startSize = currentWantedSize;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            currentWantedPos = Vector3.Lerp(startPos, focusedTr.position + followOffset, timer / duration);
            currentWantedSize = Mathf.Lerp(startSize, cameraSize, timer / duration);

            yield return new WaitForEndOfFrame();
        }

        currentWantedPos = focusedTr.position + followOffset;
        currentWantedSize = cameraSize;
    }

    public IEnumerator DoAttackFeelCoroutine(UnitAnimsInfos animInfos, bool isCrit)
    {
        currentWantedSize = 4f;

        while (true)
        {
            if (animInfos.PlaySkillEffect)
                break;

            currentWantedSize -= Time.deltaTime;
            currentWantedSize = Mathf.Clamp(currentWantedSize, battleMinSize, battleMaxSize);

            yield return new WaitForEndOfFrame();
        }

        currentWantedSize = _camera.orthographicSize + 1.5f;

        float timer = 0;
        while (timer < 0.5f)
        {
            currentWantedSize -= Time.deltaTime * (_camera.orthographicSize - 4);

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }

    public void FocusOnPosition(Vector3 focusedPos, float cameraSize)
    {
        currentWantedPos = focusedPos + followOffset;
        currentWantedSize = cameraSize;
    }

    #endregion


    #region Feel Functions

    public void DoCameraShake(float duration, float strength, float vibrato)
    {
        if (isShaking) return;

        _parentTransform.UShakePosition(duration, strength, vibrato, ShakeLockType.Z);

        StartCoroutine(CameraShakeCooldownCoroutine(duration));
    }

    private IEnumerator CameraShakeCooldownCoroutine(float duration)
    {
        isShaking = true;

        yield return new WaitForSeconds(duration);

        isShaking = false;
    }

    #endregion
}
