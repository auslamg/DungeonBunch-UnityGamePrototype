using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Used to mark the top parent <see cref="GameObject"/> of an actor (character or similar) in the hierarchy, in order to centralize the root for obtaining components.
/// </summary>
public class Actor : MonoBehaviour
{
    public GUID guid { get; private set; }
    [SerializeField] string guidPreview;
    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        guid = GUID.Generate();
        guidPreview = StringExtensions.FormatGuid(guid);
    }
}
