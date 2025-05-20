using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class Raycaster : MonoBehaviour
{
    [SerializeField] private Camera cam;

    void Update()
    {
        // 마우스 클릭 감지 (New Input System 기준)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            DetectClick(screenPos);
        }

        // 터치 감지 (모바일 대응)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            DetectClick(touchPos);
        }
    }

    private void DetectClick(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("Robot"))
            {
                SetMaterials setMat = clickedObject.GetComponent<SetMaterials>();
                
                setMat.Clicked();
            }
        }
    }
}
