using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public PlayerInput _playerInput;

    public static Vector2 moveDir;
    public static bool wantsToJump;
    public static bool isHoldingJump;
    public static bool wantsToInteract;
    public static bool wantsToInventory;
    public static bool wantsToHeroInfo;
    public static bool wantsToRotateLeft;
    public static bool wantsToRotateRight;

    private void Update()
    {
        moveDir = _playerInput.actions["Move"].ReadValue<Vector2>();
        wantsToJump = _playerInput.actions["Jump"].WasPressedThisFrame();
        isHoldingJump = _playerInput.actions["Jump"].IsPressed();
        wantsToInteract = _playerInput.actions["Interact"].WasPressedThisFrame();
        wantsToInventory = _playerInput.actions["Inventory"].WasPressedThisFrame();
        wantsToHeroInfo = _playerInput.actions["HeroInfo"].WasPressedThisFrame();
        wantsToRotateLeft = _playerInput.actions["RotateLeft"].WasPressedThisFrame();
        wantsToRotateRight = _playerInput.actions["RotateRight"].WasPressedThisFrame();
    }
}
