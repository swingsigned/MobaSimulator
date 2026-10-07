using UnityEngine;
using UnityEngine.InputSystem;
using InputMap;
public class PlayerInput : MonoBehaviour
{
    private InputManager input;
    private IInteractCell cellInteractor;
    private IInteractChamp champInteracter;

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
        if (GameUtility.hasChampSelected)
        {
            GameUtility.checkIfMouseHit<Cell>(mousePosition, out Collider2D hit);
            cellInteractor = new CellInteractor();
            cellInteractor.Interact(hit);
        }
        else
        {
            GameUtility.checkIfMouseHit<Champion>(mousePosition, out Collider2D hit);
            champInteracter = new ChampInteractor();
            cellInteractor.Interact(hit);
        }
    }
}