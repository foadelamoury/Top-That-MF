using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager instance;
    private PlayerInput playerInput;

    void Awake() {
        if (instance == null) instance = this;
        playerInput = GetComponent<PlayerInput>();
    }
}
