using UnityEngine;
using UnityEngine.InputSystem;
using InputMap;
public class PlayerInput : MonoBehaviour
{
    private InputManager input;
    private IInteractCell cellInteractor;

    private void Awake()
    {
        input = new InputManager();
    }

    private void OnEnable()
    {
        input.Enable();
        // input.MouseInput.Click.performed += OnClick;
        input.MouseInput.Click.performed += OnClick;
        Debug.Log(input.MouseInput.Click.phase);
    }

    private void OnDisable()
    {
        input.MouseInput.Click.performed -= OnClick;
        input.Disable();
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = input.MouseInput.MousePosition.ReadValue<Vector2>();
        Collider2D hit = GameUtility.checkIfMouseHit(mousePosition);
        cellInteractor = new CellInteractor();
        cellInteractor.Interact(hit);
    }
}