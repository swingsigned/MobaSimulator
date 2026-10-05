using UnityEngine;
using UnityEngine.InputSystem;
using InputManager;
public class PlayerInput : MonoBehaviour
{
    private InputManager.InputManager input;


    private void Awake()
    {
        input = new InputManager.InputManager();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Click.Click.performed += OnClick;
    }

    private void OnDisable()
    {
        input.Click.Click.performed -= OnClick;
        input.Disable();
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePosition =
            input.Click.MousePosition.ReadValue<Vector2>();
        Debug.Log($"Click: {mousePosition}");
    }
}