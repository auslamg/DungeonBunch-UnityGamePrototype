using System;
using Unity.Mathematics;
using UnityEngine;

public class LinkedTransform : MonoBehaviour
{
    [SerializeField] Transform parent;
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private bool copyWorldPosition = true;
    [SerializeField] private bool3 copyWorldPositionAxes = new(true, true, true);
    [SerializeField] private bool copyLocalRotation = true;
    [SerializeField] private bool3 copyLocalRotationAxes = new(true, true, true);

    // Update is called once per frame
    void Update()
    {
        if (!usePhysics)
        {
            ExecuteCopy();

        }
    }

    private void FixedUpdate()
    {
        if (usePhysics)
        {
            ExecuteCopy();
        }
    }

    private void ExecuteCopy()
    {
        if (copyWorldPosition)
        {
            transform.position =
                new Vector3(
                    copyWorldPositionAxes.x ? parent.position.x : transform.position.x,
                    copyWorldPositionAxes.y ? parent.position.y : transform.position.y,
                    copyWorldPositionAxes.z ? parent.position.z : transform.position.z);
        }

        if (copyLocalRotation)
        {
            transform.localRotation =
                Quaternion.Euler(
                    copyLocalRotationAxes.x ? parent.localEulerAngles.x : transform.localEulerAngles.x,
                    copyLocalRotationAxes.y ? parent.localEulerAngles.y : transform.localEulerAngles.y,
                    copyLocalRotationAxes.z ? parent.localEulerAngles.z : transform.localEulerAngles.z);
        }
    }
}
