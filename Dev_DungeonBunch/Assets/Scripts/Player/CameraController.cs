using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] float mouseX;
    [SerializeField] float mouseY;

    [Header("Clamping")]
    [Range(70, 90)][SerializeField] float maxYAngle = 89;

    [Header("Sensitivity stats")]
    [Range(.5f, 4)][SerializeField] float globalSensitivity = 1;
    [Range(50f, 400)][SerializeField] float sensitivityX = 100;
    [Range(50f, 400)][SerializeField] float sensitivityY = 100;

    [Header("Misc")]
    [SerializeField] const float hardcodeSensMult = 3;
    [SerializeField] bool flipY = false;

    [Header("Debug")]
    [SerializeField] float rotationX = 0f;
    [SerializeField] float rotationY = 0f;

    [SerializeField] float clampXValue = 0f;
    [SerializeField] float clampYValue = 0f;


    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("Loaded " + this.GetType().Name + " correctly on " + this.gameObject.name + " gameObject");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //Read input and process
        mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX * globalSensitivity * hardcodeSensMult;
        mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY * globalSensitivity * hardcodeSensMult;

        //Clamp input if necessary
        ClampMouseInput(ref mouseX, ref mouseY);

        //Register processed input as rotation values
        rotationY += mouseX;
        rotationX += flipY ? mouseY : -mouseY;

        //Clamp Pitch (Y axis)
        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);

        //Apply rotation
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        UpdatePlayerModelDirection();
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

    private void UpdatePlayerModelDirection()
    {
        //TODO
        //throw new NotImplementedException();
    }

    public void SetClampValues(float limitX, float limitY)
    {
        this.clampXValue = limitX;
        this.clampYValue = limitY;
    }
}
