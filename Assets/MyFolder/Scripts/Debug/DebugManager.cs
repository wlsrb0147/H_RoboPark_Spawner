using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DebugManager : MonoBehaviour
{
    private bool _cursorVisible;
    [SerializeField] private GameObject debugger;
    
    private void Awake()
    {
        Cursor.visible = false;
        debugger.SetActive(false);
    }

    public void ToggleObjs(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (context.control is not KeyControl control) return;

        Key key = control.keyCode;

        Action action = key switch
        {
            Key.M => ToggleCursor,
            Key.D => ToggleDebugger,
            _ => () => {}
        };
        
        action?.Invoke();
    }

    private void ToggleCursor()
    {
        if (_cursorVisible)
        {
            _cursorVisible = false;
            Cursor.visible = false;
            DebugEx.Log("Hide cursor");
        }
        else
        {
            _cursorVisible = true;
            Cursor.visible = true;
            DebugEx.Log("Show cursor");
        }
    }

    private void ToggleDebugger()
    {
        debugger.SetActive(!debugger.activeSelf);
    }
}
