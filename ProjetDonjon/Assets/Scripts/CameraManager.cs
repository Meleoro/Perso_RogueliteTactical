using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

public class CameraManager : GenericSingletonClass<CameraManager>
{
    [Header("Parameters")]
    [SerializeField] private float followSpeed;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private float battleMinSize;
    [SerializeField] private float battleMaxSize;

    [Header("Private Infos")]
    private Transform followedTransform;
    private Vector3 currentWantedPos;
    private float currentWantedSize;
    private float baseSize;
    private bool isInitialised;
    private bool isInBattle;
    private bool isShaking;

    [Header("Public Infos")]
    public Camera Camera { get { return _camera; } }

    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _parentTransform;


    public void Initialise(Transform followedTransform)
    {
        isInitialised = true;

        this.followedTransform = followedTransform;
        baseSize = _camera.orthographicSize;
    }


    private void Update()
    {
        if (!isInitialised) return;
        if (!isInBattle)
        {
            currentWantedPos = followedTransform.position + followOffset;
            currentWantedSize = baseSize;
        }

        transform.position = Vector3.Lerp(transform.position, currentWantedPos, followSpeed * Time.deltaTime);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, currentWantedSize, followSpeed * Time.deltaTime);

        ManageMouseInputs();
    }


    #region Battle Functions

    public void EnterBattle(Vector3 battleCenterPos, float cameraSize)
    {
        isInBattle = true;

        currentWantedPos = battleCenterPos + followOffset;
        currentWantedSize = cameraSize; 
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
        }

        currentWantedSize += InputManager.mouseScroll;
        currentWantedSize = Mathf.Clamp(currentWantedSize, battleMinSize, battleMaxSize);
    }

    public void FocusOnTr(Transform focusedTr, float cameraSize)
    {
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
