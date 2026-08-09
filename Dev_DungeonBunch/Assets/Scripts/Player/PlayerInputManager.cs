using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ActionManager actionManager;
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private MeleeBlock meleeBlock;

    //TODO: Restructure
    private void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;

        actionManager =
            actionManager != null ?
                actionManager :
                GetComponentInChildren<ActionManager>();

        meleeAttack =
            meleeAttack != null ?
                meleeAttack :
                GetComponentInChildren<MeleeAttack>();

        meleeBlock =
            meleeBlock != null ?
                meleeBlock :
                GetComponentInChildren<MeleeBlock>();
    }

    void Update()
    {
        // TODO: Foreach action in ActionManager, subscribe state to their input

        if (meleeAttack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                actionManager.TryExecute<MeleeAttack>();
            }
            meleeAttack.RunUpdate();
        }

        if (meleeBlock)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log($"[Input]: Enable block :{actionManager.TrySetEnabled<MeleeBlock>(true)}");
                
            }
            if (Input.GetMouseButtonUp(1))
            {
                Debug.Log($"[Input]: Disable block :{actionManager.TrySetEnabled<MeleeBlock>(false)}");
            }
            meleeBlock.RunUpdate();
        }
    }
}