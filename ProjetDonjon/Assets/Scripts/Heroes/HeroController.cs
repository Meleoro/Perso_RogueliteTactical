using System;
using System.Collections;
using UnityEngine;
public class HeroController : MonoBehaviour
{
    enum ControllerState
    {
        Idle,
        Walk,
        Jump,
        Fall
    }

    [Header("Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float holdJumpForce;
    [SerializeField] private float holdJumpDuration;

    [Header("Private Infos")]
    private ControllerState currentControllerState;
    private bool isInBattle;
    private bool isAutoMoving;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rbSprite;
    [SerializeField] private ParticleSystem _walkParticleSystem;
    [SerializeField] private ParticleSystem _landParticleSystem;
    private Rigidbody2D _rb;


    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }


    public void UpdateController()
    {
        if (isAutoMoving) return;
        if (isInBattle) 
        {
            _rb.linearVelocity = Vector2.zero;
            Move(Vector2.zero);
            return;
        }

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


    public IEnumerator AutoMoveCoroutine(Vector3 aimedPos)
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

        while (_rbSprite.transform.localPosition.y > 0)
        {
            if(_rbSprite.linearVelocity.y < 0 && !isGoingDown)
            {
                isGoingDown = true;
                _animator.SetTrigger("JumpNext");
            }

            _rbSprite.transform.localPosition = new Vector3(0, _rbSprite.transform.localPosition.y, 0);

            yield return new WaitForFixedUpdate();
        }

        _animator.SetTrigger("JumpNext");
        _landParticleSystem.Play();

        _rbSprite.transform.localPosition = Vector2.zero;
        _rbSprite.linearVelocity = Vector2.zero;
        _rbSprite.bodyType = RigidbodyType2D.Kinematic;

        currentControllerState = ControllerState.Idle;
    }

    #endregion


    public void EnterBattle()
    {
        isInBattle = true;
    }

    public void ExitBattle()
    {
        isInBattle = false;
    }
}
