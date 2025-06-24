using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public enum ControllerState
{
    Idle,
    Walk,
    Jump,
    Fall
}

public class HeroController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float holdJumpForce;
    [SerializeField] private float holdJumpDuration;

    [Header("Private Infos")]
    private ControllerState currentControllerState;
    private bool isInBattle;
    private bool noControl;
    private bool isAutoMoving;
    private Vector2 saveSpriteLocalPos;
    private Vector2 oldPos;
    public List<Vector3> savePositions = new List<Vector3>();
    private Coroutine autoMoveCoroutine;

    [Header("Public Infos")]
    public ControllerState CurrentControllerState { get { return currentControllerState; } }

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rbSprite;
    [SerializeField] private ParticleSystem _walkParticleSystem;
    [SerializeField] private ParticleSystem _landParticleSystem;
    private Rigidbody2D _rb;


    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        saveSpriteLocalPos = _rbSprite.transform.localPosition;
        savePositions.Add(transform.position);
    }


    public void UpdateController()
    {
        if (isAutoMoving) return;
        if (currentControllerState == ControllerState.Fall) return;
        if (isInBattle) 
        {
            _rb.linearVelocity = Vector2.zero;
            Move(Vector2.zero);
            return;
        }
        if (noControl) return;

        Move(InputManager.moveDir);

        if (InputManager.wantsToJump)
        {
            switch (currentControllerState)
            {
                case ControllerState.Idle:
                    Jump();
                    break;

                case ControllerState.Walk:
                    Jump();
                    break;
            }
        }
    }


    #region Base Movement Functions

    private void Move(Vector2 inputDir)
    {
        _rb.linearVelocity = inputDir * moveSpeed;

        Rotate(inputDir);

        if(currentControllerState == ControllerState.Walk)
        {
            if (Vector2.Distance(savePositions[^1], transform.position) > 0.1f)
            {
                savePositions.Add(transform.position);
                if (savePositions.Count > 5)
                {
                    savePositions.RemoveAt(0);
                }
            }
        }

        if (inputDir != Vector2.zero)
        {
            _animator.SetBool("IsWalking", true);

            if (currentControllerState == ControllerState.Jump) return;

            currentControllerState = ControllerState.Walk;
            if(!_walkParticleSystem.isPlaying)
                _walkParticleSystem.Play();

            _walkParticleSystem.transform.right = inputDir;
        }
        else
        {
            _animator.SetBool("IsWalking", false);

            if (currentControllerState == ControllerState.Jump) return;

            currentControllerState = ControllerState.Idle;
            if (_walkParticleSystem.isEmitting)
                _walkParticleSystem.Stop();
        }
    }

    private void Rotate(Vector2 inputDir)
    {
        if(inputDir.x < -0.01)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if(inputDir.x > 0.01)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }


    public void AutoMove(Vector3 aimedPos)
    {
        if (isAutoMoving) return;

        autoMoveCoroutine = StartCoroutine(AutoMoveCoroutine(aimedPos));
    }

    private IEnumerator AutoMoveCoroutine(Vector3 aimedPos)
    {
        isAutoMoving = true;

        while (Vector2.Distance(aimedPos, transform.position) > 0.1f)
        {
            Move((aimedPos - transform.position).normalized);

            yield return new WaitForEndOfFrame();
        }

        transform.position = aimedPos;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        isAutoMoving = false;
    }

    public void StopAutoMove()
    {
        isAutoMoving = false;

        if(autoMoveCoroutine != null)
        {
            StopCoroutine(autoMoveCoroutine);
        }
    }


    public Action EndAutoMoveAction;
    public IEnumerator AutoMoveCoroutineEndBattle(Transform aimedTr)
    {
        isAutoMoving = true;

        while (Vector2.Distance(aimedTr.position, transform.position) > 0.1f)
        {
            Move((aimedTr.position - transform.position).normalized);

            yield return new WaitForEndOfFrame();
        }

        transform.position = aimedTr.position;
        isAutoMoving = false;

        EndAutoMoveAction.Invoke();
    }

    #endregion


    #region Jump Junctions 

    private void Jump()
    {
        currentControllerState = ControllerState.Jump;

        oldPos = transform.position;

        _rbSprite.bodyType = RigidbodyType2D.Dynamic;
        _rbSprite.linearVelocity = Vector2.zero;
        _rbSprite.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        _animator.SetTrigger("Jump");
        _walkParticleSystem.Stop();

        StartCoroutine(ManageJumpHoldCoroutine());
        StartCoroutine(VerifyJumpEndCoroutine());
    }

    private IEnumerator ManageJumpHoldCoroutine()
    {
        float timer = 0;

        while (timer < holdJumpDuration)
        {
            if (!InputManager.isHoldingJump) break;

            timer += Time.fixedDeltaTime;
            _rbSprite.AddForce(Vector2.up * holdJumpForce * Time.fixedDeltaTime, ForceMode2D.Force);

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator VerifyJumpEndCoroutine()
    {
        yield return new WaitForFixedUpdate();

        bool isGoingDown = false;

        while (_rbSprite.transform.localPosition.y > saveSpriteLocalPos.y)
        {
            if(_rbSprite.linearVelocity.y < 0 && !isGoingDown)
            {
                isGoingDown = true;
                _animator.SetTrigger("JumpNext");
            }

            float currentYDif = transform.position.y - oldPos.y;
            _rbSprite.transform.localPosition += new Vector3(0, currentYDif, 0);

            oldPos = transform.position;

            _rbSprite.transform.localPosition = new Vector3(saveSpriteLocalPos.x, _rbSprite.transform.localPosition.y, 0);

            yield return new WaitForEndOfFrame();
        }

        _animator.SetTrigger("JumpNext");
        _landParticleSystem.Play();

        _rbSprite.transform.localPosition = saveSpriteLocalPos;
        _rbSprite.linearVelocity = Vector2.zero;
        _rbSprite.bodyType = RigidbodyType2D.Kinematic;

        currentControllerState = ControllerState.Idle;
    }

    #endregion


    #region Others

    public IEnumerator FallCoroutine()
    {
        currentControllerState = ControllerState.Fall;
        _rb.linearVelocity = Vector2.zero;

        _rbSprite.transform.UChangeScale(0.5f, Vector3.zero, CurveType.EaseInOutSin);

        yield return new WaitForSeconds(0.5f);

        currentControllerState = ControllerState.Idle;
        transform.position = savePositions[0];
        _rbSprite.transform.localScale = Vector3.one;
    }


    public IEnumerator TakeStairsCoroutine()
    {
        StartCoroutine(AutoMoveCoroutine(transform.position + new Vector3(0, 2, 0)));

        yield return new WaitForSeconds(1);
    }


    public void StopControl()
    {
        noControl = true;
        _rb.linearVelocity = new Vector3(0, 0, 0);
    }

    public void RestartControl()
    {
        noControl = false;  
    }

    public void EnterBattle()
    {
        isInBattle = true;
    }

    public void ExitBattle()
    {
        isInBattle = false;
    }

    #endregion
}
