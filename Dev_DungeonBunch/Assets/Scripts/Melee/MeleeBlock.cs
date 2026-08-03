using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeleeBlock : MonoBehaviour, IRunnable
{
    enum BlockState
    {
        Ready,
        Parry,
        Blocking,
        Cooldown
    }

    [Header("Input")]
    [SerializeField] public bool blockInput = false;
    bool debugBlockInput => Input.GetKeyDown(debugKey);

    [Header("State")]
    [SerializeField] BlockState state = BlockState.Ready;
    [SerializeField] HashSet<GameObject> hitTargets = new();
    [SerializeField] private CountdownTimer blockCooldown = new(1);
    [SerializeField] private CountdownTimer parryDuration = new(0.25f);
    [SerializeField] private CountdownTimer blockDuration = new(2f);
    [SerializeField] private CountdownTimer staggerTime = new(0.25f);
    [SerializeField] bool isStaggered = false;

    [Header("References")]
    [SerializeField] private BoxCollider hurtBox;

    [Header("Debug UI")]
    [SerializeField] private KeyCode debugKey;

    [Header("Debug UI")]
    [SerializeField] Slider cooldownSlider;
    [SerializeField] private TMP_Text t1;
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite blockCrosshair;
    [SerializeField] private Sprite baseCrosshair;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Run()
    {
        throw new System.NotImplementedException();
    }
}
