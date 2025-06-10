using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public PlayerInput _playerInput;

    public static Vector2 moveDir;
    public static Vector2 mouseDelta;
    public static float mouseScroll;
    public static bool wantsToJump;
    public static bool isHoldingJump;
    public static bool wantsToInteract;
    public static bool wantsToInventory;
    public static bool wantsToHeroInfo;
    public static bool wantsToRotateLeft;
    public static bool wantsToRotateRight;
    public static bool wantsToReturn;
    public static bool wantsToRightClick;

    private void Update()
    {
        moveDir = _playerInput.actions["Move"].ReadValue<Vector2>();
        mouseDelta = _playerInput.actions["MouseDelta"].ReadValue<Vector2>();
        mouseScroll = _playerInput.actions["MouseScroll"].ReadValue<float>();
        wantsToJump = _playerInput.actions["Jump"].WasPressedThisFrame();
        isHoldingJump = _playerInput.actions["Jump"].IsPressed();
        wantsToInteract = _playerInput.actions["Interact"].WasPressedThisFrame();
        wantsToInventory = _playerInput.actions["Inventory"].WasPressedThisFrame();
        wantsToHeroInfo = _playerInput.actions["HeroInfo"].WasPressedThisFrame();
        wantsToRotateLeft = _playerInput.actions["RotateLeft"].WasPressedThisFrame();
        wantsToRotateRight = _playerInput.actions["RotateRight"].WasPressedThisFrame();
        wantsToReturn = _playerInput.actions["Return"].WasPressedThisFrame();
        wantsToRightClick = _playerInput.actions["RightClick"].WasPressedThisFrame();
    }
}
