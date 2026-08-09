using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// TODO: Refactor: extract input
public class CameraController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private float mouseX;
    [SerializeField] private float mouseY;

    [Header("Clamping")]
    [Range(70, 90)][SerializeField] private float maxYAngle = 89;

    [Header("Sensitivity stats")]
    [Range(.5f, 4)][SerializeField] private float globalSensitivity = 1;
    [Range(50f, 400)][SerializeField] private float sensitivityX = 100;
    [Range(50f, 400)][SerializeField] private float sensitivityY = 100;

    [Header("Misc")]
    [SerializeField] private const float BASE_SENSITIVITY = 3;
    [SerializeField] private bool flipY = false;

    [Header("References")]
    [SerializeField] private ViewController viewController;

    [Header("Debug")]
    [SerializeField] private float rotationX = 0f;
    [SerializeField] private float rotationY = 0f;

    [SerializeField] private float clampXValue = 0f;
    [SerializeField] private float clampYValue = 0f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnValidate()
    {
        viewController =
            viewController != null ?
                viewController :
                GetComponentInParent<Actor>().GetComponentInChildren<ViewController>();
    }

    private void Update()
    {
        //Read input and process
        mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX * globalSensitivity * BASE_SENSITIVITY;
        mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY * globalSensitivity * BASE_SENSITIVITY;

        //Clamp input if necessary
        ClampMouseInput(ref mouseX, ref mouseY);

        //Register processed input as rotation values
        rotationY += mouseX;
        rotationX += flipY ? mouseY : -mouseY;

        //Clamp Pitch (Y axis)
        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);

        //Apply rotation
        viewController.SetRotation(Quaternion.Euler(rotationX, rotationY, 0));
    }

    private void ClampMouseInput(ref float mouseX, ref float mouseY)
    {
        if (clampXValue > 0)
        {
            mouseX = Mathf.Clamp(mouseX, -clampXValue, clampXValue);
        }
        if (clampYValue > 0)
        {
            mouseY = Mathf.Clamp(mouseY, -clampYValue, clampYValue);
        }
    }

    

    public void SetClampValues(float limitX, float limitY)
    {
        this.clampXValue = limitX;
        this.clampYValue = limitY;
    }
}
