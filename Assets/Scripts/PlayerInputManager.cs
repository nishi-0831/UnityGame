using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    [SerializeField] private GamepadCursorController cursorController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if(playerInput == null )
        {
            Debug.LogWarning("Player Input is null");
        }

        SwitchActionMap("UI");
        SwitchActionMap("Player");
    }
    void Start()
    {
        cursorController.gameObject.SetActive( false );
        PauseManager.Instance.OnPauseEnd(HandleResume);
        PauseManager.Instance.OnPauseStart(HandlePause);
    }
    
    // Update is called once per frame
    void Update()
    {
    }
    
    void HandlePause()
    {
        cursorController.gameObject.SetActive(true);
        cursorController.InitCursorPos();
        SwitchActionMap("UI");
    }
    void HandleResume()
    {
        cursorController.gameObject.SetActive(false);
        SwitchActionMap("Player");
    }
    public void SwitchActionMap(string name)
    {
        playerInput.currentActionMap.Disable();
        playerInput.SwitchCurrentActionMap(name);
        playerInput.currentActionMap.Enable();
    }
}
