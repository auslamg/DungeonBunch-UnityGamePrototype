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

    [Header("Sensitivity stats")]
    [Range(.5f, 4)][SerializeField] float globalSens = 1;
    [Range(50f, 400)][SerializeField] float sensX = 100;
    [Range(50f, 400)][SerializeField] float sensY = 100;

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
        mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX * globalSens * hardcodeSensMult;
        mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY * globalSens * hardcodeSensMult;

        //Clamp input if necessary
        ClampMouseInput(ref mouseX, ref mouseY);

        //Register processed input as rotation values
        rotationY += mouseX;
        rotationX += flipY ? mouseY : -mouseY;

        //Clamp Pitch (Y axis)
        rotationX = Mathf.Clamp(rotationX, -90, 90);

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
