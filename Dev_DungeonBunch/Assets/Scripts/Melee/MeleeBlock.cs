using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

enum BlockState
{
    Ready,
    Parry,
    Blocking,
    Cooldown
}

public class MeleeBlock : MonoBehaviour, IRunnable, IStaggerable, IExclusiveHeldAction
{
    [Header("Input")]
    [SerializeField] private KeyCode debugKey;
    bool DebugBlockInput => Input.GetKeyDown(debugKey);

    [Header("Action interface")]
    private ActionManager actionManager;
    public ActionManager ActionManager
    {
        get => actionManager;
        set => actionManager = value;
    }
    [SerializeField] private bool isActive = false;
    public bool IsActive => isActive;
    ActionState IExclusiveAction.State => GetActionState();

    public event EventHandler OnActionStart;
    public event EventHandler OnActionEnd;

    [Header("State")]
    [SerializeField] BlockState state = BlockState.Ready;
    [SerializeField] private CountdownTimer blockCooldown = new(0.75f);
    [SerializeField] private CountdownTimer parryDuration = new(0.25f);
    [SerializeField] private CountdownTimer blockDuration = new(0.75f);
    [SerializeField] private CountdownTimer staggerTime = new(0.25f);
    [SerializeField] bool isStaggered = false;

    [Header("References")]
    [SerializeField] private Transform orientation;

    [Header("Debug UI")]
    [SerializeField] Slider cooldownSlider;
    [SerializeField] private TMP_Text t1;
    [SerializeField] private Sprite actionCrosshair;
    public Sprite ActionCrosshair => actionCrosshair;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;
        
        orientation =
            orientation != null ?
                orientation :
                GetComponentInParent<Actor>().GetComponentInChildren<CameraController>().transform; // TODO: Refactor to CharacterView flag component
        

        parryDuration.Reset();
        blockDuration.Reset();
        blockCooldown.Reset();

        UpdateUI();
    }

    public void Switch(out bool endState)
    {
        isActive = !isActive;
        endState = isActive;
    }

    /// <summary>
    /// Sets the <see cref="IExclusiveHeldAction"/> enabled state to the assigned value.
    /// </summary>
    /// <param name="onOff">Whether to enable or disable the action.</param>
    /// <returns><c>true</c> if the change took effect, <c>false</c> if it didn't.</returns>
    public bool SetActionEnabled(bool onOff)
    {
        if (isActive == onOff)
        {
            return false;
        }
        else
        {
            isActive = onOff;
            return true;
        }
    }

    public void RunUpdate()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        switch (state)
        {
            case BlockState.Ready:
                // Input
                if ((isActive || DebugBlockInput) &&
                    !isStaggered)
                {
                    // Start attack
                    state = BlockState.Parry;
                    OnActionStart?.Invoke(this, EventArgs.Empty);
                }
                break;

            case BlockState.Parry:
                // If the parry duration is over but input is held, end parry and start blocking phase
                if (parryDuration.Tick(deltaTime))
                {
                    state = BlockState.Blocking;
                    parryDuration.Reset();
                }

                // Parry cancelation alternative
                /* if ((blockInput || debugBlockInput) &&
                        !isStaggered)
                {
                    // If the parry duration is over but input is held, end parry and start blocking phase
                    if (parryDuration.Tick(deltaTime))
                    {
                        state = BlockState.Blocking;
                        parryDuration.Reset();
                    }
                }
                else
                {
                    state = BlockState.Cooldown;
                    parryDuration.Reset();
                } */

                break;

            case BlockState.Blocking:
                if ((isActive || DebugBlockInput) &&
                    !isStaggered)
                {
                    // If the release duration is over, end attack and start cooldown
                    if (blockDuration.Tick(deltaTime))
                    {
                        state = BlockState.Cooldown;
                        blockDuration.Reset();
                        OnActionEnd?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    state = BlockState.Cooldown;
                    blockDuration.Reset();
                    OnActionEnd?.Invoke(this, EventArgs.Empty);
                }

                break;

            case BlockState.Cooldown:
                // If the attack cooldown is over, set ready
                if (blockCooldown.Tick(deltaTime))
                {
                    state = BlockState.Ready;
                    blockCooldown.Reset();
                }
                break;
        }

        if (isStaggered)
        {
            // If the stagger duration is over, end stagger
            if (staggerTime.Tick(deltaTime))
            {
                isStaggered = false;
            }
        }

        UpdateUI();
    }

    /// <summary>
    /// Checks wether the <see cref="Actor"/> can block the incoming <see cref="Attack"/> by checking the adequate <see cref="BlockState"/> and that the angle between both is inferior to the maximum threshold.
    /// </summary>
    /// <param name="attack"></param>
    /// <returns><c>true</c> if the block was successful, <c>false</c> if it wasn't.</returns>
    public bool TryBlock(Attack attack)
    {
        // IDEA: Setting the block immediately ready (with no cooldown) after successfully parrying (not blocking) would incentivize smart defense
        Vector3 reverseSightline = -orientation.position + attack.source.transform.position;

        // Check if line of sight difference is within max angle
        bool withinAngle = Vector3.Angle(orientation.forward, reverseSightline) <= 75; // TODO: Parametrize angle
        if (!withinAngle)
        {
            return false;
        }
        else
        {
            if (state == BlockState.Parry)
            {
                // IDEA: add bonus
            }

            return state == BlockState.Parry || state == BlockState.Blocking;
        }
    }

    public void Stagger()
    {
        isStaggered = true;
        staggerTime.Reset();
    }

    public ActionState GetActionState()
    {
        return state switch
        {
            BlockState.Ready => ActionState.Ready,
            BlockState.Parry => ActionState.Busy,
            BlockState.Blocking => ActionState.InProgress,
            BlockState.Cooldown => ActionState.Cooldown,
            _ => ActionState.Ready,
        };
    }

    public void Interrupt()
    {
        parryDuration.Reset();
        blockDuration.Reset();
        blockCooldown.Reset();
        state = BlockState.Cooldown;
        OnActionEnd?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateUI()
    {
        if (cooldownSlider)
        {
            cooldownSlider.value = state == BlockState.Cooldown ? blockCooldown.ProgressPercent : blockDuration.RemainingPercent;
            cooldownSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = BlockStateColor();
        }

        if (t1)
        {
            t1.text = $"{state}";
            t1.color = BlockStateColor();
        }
    }

    Color BlockStateColor()
    {
        return state switch
        {
            BlockState.Ready => new Color(0.000f, 0.835f, 0.945f, 1.000f),
            BlockState.Parry => new Color(0.365f, 1.000f, 0.969f, 1.000f),
            BlockState.Blocking => new Color(0.000f, 0.835f, 0.945f, 1.000f),
            BlockState.Cooldown => new Color(0.251f, 0.682f, 0.855f, 1.000f),
            _ => Color.white,
        };
    }
}
