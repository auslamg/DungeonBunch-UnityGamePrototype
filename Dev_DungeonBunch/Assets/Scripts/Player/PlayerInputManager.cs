using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeleeAttack meleeAttack;

    private void OnValidate() {
        meleeAttack =
            meleeAttack != null ? meleeAttack : GetComponentInChildren<MeleeAttack>();
    }

    void Update()
    {
        meleeAttack.attackInput = Input.GetMouseButtonDown(0);
        meleeAttack.Run();
    }
}