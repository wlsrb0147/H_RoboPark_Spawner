using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[Serializable]
public class ArduinoSetting
{
    public string value;
}

public class InputManager : MonoBehaviour
{
    [SerializeField] private int currentPage;
    [SerializeField] private int currentIndex;

    #region ArduinoConnection
    
    // 아두이노 테스트용 딕서녀리 
    private readonly Dictionary<Key, string> _keyCodes1 = new ();
    
    // 내가 받을 스트링 딕셔너리
    private readonly Dictionary<string, Key> _keyCodes2 = new ();

    private ArduinoSetting _arduinoSetting;
    public ArduinoSetting ArduinoSetting
    {
        get => _arduinoSetting;
        set
        {
            _arduinoSetting = value;
            SetDictionary();
        }
    }
    
    private void SetDictionary()
    {
        _keyCodes1.Clear();
        _keyCodes2.Clear();

        if (ArduinoSetting == null || string.IsNullOrWhiteSpace(ArduinoSetting.value))
        {
            DebugEx.LogWarning("ArduinoSetting is null or empty. Skipping dictionary setup.");
            return;
        }
        RegisterKeyCodes(Key.Space, _arduinoSetting.value);
    }
    
    private void RegisterKeyCodes(Key key, string value)
    {
        _keyCodes1[key] = value;
        _keyCodes2[value] = key;
    }

    
    // 딕셔너리로 키와 String을 매핑 후, 변환해서 사용
    private void ConvertStringToKey(string input)
    {
        Key key;

        if (string.IsNullOrWhiteSpace(input))
        {
            key = Key.None;
        }
        else
        {
            input = input.Trim().Replace("\n", "");
            key = _keyCodes2.GetValueOrDefault(input, Key.None);
        }
        
        ArduinoInputControl(key);
    }
    
    #endregion
    
    public void ChangeIndex()
    {
        SetCurrentIndex(currentPage, currentIndex+1);
    }
    
    public void SetCurrentIndex(int page, int index)
    {
        currentPage = page;
        currentIndex = index;
        
        // Add logics

        switch (page, index)
        {
            case (0,0):
                break;
            case (2, _) :
                break;
            case (3, >= 1 and <= 5):
                break;
            default:
                DebugEx.Log($"page {page}, index {index} is default");
                break;
        }
    }
    
    public void KeyboardInputControl(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (context.control is not KeyControl keyControl)
        {
            return;
        }
        
        Key key = keyControl.keyCode;
        
        /*if (ArduinoConnected)
        {
            string s = _keyCodes.GetValueOrDefault(key, key.ToString());
            controller.SendSerialMessage(0,s);
            return;
        }*/
        
        DebugEx.Log($"Keyboard Input Come : {key}");
        
        ExecuteInput(key);
    }
    
    public void ArduinoInputControl(Key key)
    {
        DebugEx.Log($"Arduino Input : {key}");
        DebugEx.Log($"Current Page : {currentPage} Current Index : {currentIndex}");
        
        ExecuteInput(key);
    }
    
    private void ExecuteInput(Key key)
    {
        if (key == Key.None)
        {
            DebugEx.LogWarning("Invalid or empty key received.");
            return;
        }
        
        Action<Key> action = currentPage switch
        {
            0 => SelectPage0,
            _ => _ => { }
        };
        
        action(key);
    }

    private void DefaultInput(Key context)
    {
        DebugEx.Log($"P{currentPage}I{currentIndex} : Default - {context}");
        
        switch (context)
        {
            case Key.UpArrow:
                DebugEx.Log($"{currentPage}{currentIndex} : UpArrow Pressed");
                break;
        }
    }
    
    private void SelectPage0(Key context)
    {
        Action<Key> action = currentIndex switch
        {
            0 => P0I0,
            2 => P0I2,
            4 => P0I4,
            _ => DefaultInput
        };
        
        action(context);
    }

    #region page0

    private void P0I0(Key context)
    {
        DebugEx.Log($"Page0 Index0 Selected : {context}");
        switch (context)
        {
            case Key.UpArrow:
            case Key.DownArrow:
                DebugEx.Log("Its Up/Down Arrow");
                break;
            case Key.Space:
                DebugEx.Log("Its Up/Space");
                break;
        }
    }
    private void P0I2(Key context)
    {
        DebugEx.Log($"Page0 Index2 Selected : {context}");
        switch (context)
        {
            case Key.UpArrow:
            case Key.DownArrow:
                DebugEx.Log("Its Up/Down Arrow");
                break;
            case Key.Space:
                DebugEx.Log("Its Up/Space");
                break;
        }
    }
    private void P0I4(Key context)
    {
        DebugEx.Log($"Page0 Index4 Selected : {context}");
        switch (context)
        {
            case Key.UpArrow:
            case Key.DownArrow:
                DebugEx.Log("Its Up/Down Arrow");
                break;
            case Key.Space:
                DebugEx.Log("Its Space ");
                break;
        }
    }

    #endregion
    
}
