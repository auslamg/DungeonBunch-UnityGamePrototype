using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private MeleeBlock meleeBlock;

    //TODO: Restructure
    private void OnValidate()
    {
        meleeAttack =
            meleeAttack != null ? meleeAttack : GetComponentInChildren<MeleeAttack>();
        meleeBlock =
            meleeBlock != null ? meleeBlock : GetComponentInChildren<MeleeBlock>();
    }

    void Update()
    {
        if (true)
        {
            meleeAttack.attackInput = Input.GetMouseButtonDown(0);
            meleeAttack.Run();
        }

        if (meleeBlock)
        {
            meleeBlock.blockInput = Input.GetMouseButton(0);
            meleeBlock.Run();
        }
    }
}