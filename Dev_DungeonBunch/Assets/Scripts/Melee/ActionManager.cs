using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles action concurrency so that <see cref="IExclusiveAction"/> types don't execute concurrently.
/// All action executions for any <see cref="Actor"/> should pass through an <see cref="ActionManager"/> instance in order to validate it.
/// </summary>
/// <remarks>
/// Future work may add an IOverlappingAction type for actions that aren't meant to exlude others or be excluded (or more dependeing on further rulesets).
/// </remarks>
public class ActionManager : MonoBehaviour
{
    [SerializeField] private bool isBusy = false;

    [SerializeField] private IExclusiveAction _currentAction;
    public IExclusiveAction CurrentAction
    {
        get => _currentAction;
        private set
        {
            if (_currentAction != null)
            {
                _currentAction.OnActionStart -= CurrentAction_OnActionStart;
                _currentAction.OnActionEnd -= CurrentAction_OnActionEnd;
            }

            _currentAction = value;

            if (_currentAction != null)
            {
                _currentAction.OnActionStart += CurrentAction_OnActionStart;
                _currentAction.OnActionEnd += CurrentAction_OnActionEnd;
            }
        }
    }

    /// <summary>
    /// Stores available <see cref="IExclusiveAction"/> components for execution.
    /// </summary>
    /// <remarks>
    /// This collection is fetched automatically on validation (<see cref="OnValidate()"/>).
    /// </remarks>
    [SerializeField] List<IExclusiveAction> exclusiveActions = new();

    [Header("Debug UI")]
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite baseCrosshair;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (gameObject.IsPrefabDefinition()) return;

        gameObject.GetComponents(exclusiveActions);

        baseCrosshair =
                crosshair != null ?
                    crosshair.sprite :
                    null;
    }

    private void UpdateUI()
    {
        if (crosshair)
        {
            crosshair.sprite = (CurrentAction == null) && isBusy ?
                baseCrosshair :
                CurrentAction != null ?
                    CurrentAction.ActionCrosshair :
                    baseCrosshair;
        }
    }

    void CurrentAction_OnActionStart(object sender, EventArgs e)
    {
        isBusy = true;
        UpdateUI();
    }

    void CurrentAction_OnActionEnd(object sender, EventArgs e)
    {
        isBusy = false;
        CurrentAction = null;
        UpdateUI();
    }

    /// <summary>
    /// Tries to execute a <see cref="IExclusiveInstanceAction"/> of type <see cref="T"/> if it is present within the <see cref="exclusiveActions"/> collection. This collection is fetched automatically on validation (<see cref="OnValidate()"/>).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>
    /// <list type="bullet">
    /// <item><c>-1</c> if the action wasn't found.</item>
    /// <item><c>0</c> if the action couldn't execute.</item>
    /// <item><c>1</c> if the action executes successfully.</item>
    /// </list>
    /// </returns>
    public int TryExecute<T>() where T : MonoBehaviour, IExclusiveInstanceAction
    {
        var action = exclusiveActions.OfType<T>().FirstOrDefault();

        if (action == null)
            return -1;

        if (isBusy)
        {
            return 0;
        }

        if (CurrentAction != null && CurrentAction != action)
        {
            if (CurrentAction.State == ActionState.InProgress)
            {
                CurrentAction.Interrupt();
            }
        }
        CurrentAction = action;

        return action.TryExecuteAction() ? 1 : 0;
    }

    public int TrySetEnabled<T>(bool onOff) where T : MonoBehaviour, IExclusiveHeldAction
    {
        var action = exclusiveActions.OfType<T>().FirstOrDefault();

        if (action == null)
            return -1;

        if (onOff == true)
        {
            if (isBusy)
            {
                return 0;
            }

            if (CurrentAction != null && CurrentAction != action)
            {
                if (CurrentAction.State == ActionState.InProgress)
                {
                    CurrentAction.Interrupt();
                }
            }
            CurrentAction = action;
        }

        return action.SetActionEnabled(onOff) ? 1 : 0;
    }
}

/// <summary>
/// Used to mark an <see cref="Actor"/>'s action with persistent duration that cannot be executed
/// at the same time as other actions, nor allows others to execute during its duration. 
/// </summary>
/// <remarks>
/// Do not implement directly; implement
/// <see cref="IExclusiveInstanceAction"/> or
/// <see cref="IExclusiveHeldAction"/> instead.
/// </remarks>
public interface IExclusiveAction
{
    /// <summary>
    /// Pointer to the <see cref="global::ActionManager"/> instance associated with this action.
    /// </summary>
    /// <remarks>
    /// While currently unused, it might be useful to keep for action removal/addition from <see cref="ActionManager.exclusiveActions"/> in the future.
    /// </remarks>
    public ActionManager ActionManager { get; set; }

    /// <summary>
    /// Current <see cref="ActionState"/> of the action, used to determin its stage and wether it is interruptible.
    /// </summary>
    public ActionState State { get; }

    /// <summary>
    /// Marks action start, invoked by starting successful executions.
    /// </summary>
    public event EventHandler OnActionStart;

    /// <summary>
    /// Marks action end, invoked by finishing successful executions or abruptly interruped ones.
    /// </summary>
    public event EventHandler OnActionEnd;

    /// <summary>
    /// Accounts for time passage in the action's internal timers.
    /// </summary>
    public void Tick(float deltaTime);

    /// <summary>
    /// Interrupts the current action, falling back onto <see cref="ActionState.Cooldown"/> state and finishing the action abruptly.
    /// </summary>
    public void Interrupt();

    public Sprite ActionCrosshair { get; }
}

public interface IExclusiveInstanceAction : IExclusiveAction
{
    public bool TryExecuteAction();
}

/// <summary>
/// Used to mark an <see cref="IExclusiveAction"/> that remains active while a condition is true or its internal timer runs out.
/// </summary>
/// <remarks>
/// There might be no internal time limit, making the action duration infinite as long as the condition is met.
/// Charged attacks or other actions that require a held condition and then remain busy on release MAY or MAY NOT require a new interface. //REVIEW
/// </remarks>
public interface IExclusiveHeldAction : IExclusiveAction
{
    public bool IsActive { get; }

    /// <summary>
    /// Sets the <see cref="IExclusiveHeldAction"/> <see cref="IsActive"/> state to the opposite value.
    /// </summary>
    /// <param name="endState">Resulting state of the <see cref="IsActive"/> property.</param>
    public void Switch(out bool endState);

    /// <summary>
    /// Sets the <see cref="IExclusiveHeldAction"/> <see cref="IsActive"/> state to the assigned value.
    /// </summary>
    /// <param name="onOff">Whether to enable or disable the action.</param>
    /// <returns><c>true</c> if the change took effect, <c>false</c> if it didn't.</returns>
    public bool SetActionEnabled(bool onOff);
}

public interface IStaggerable
{
    public void Stagger();
}

public enum ActionState
{
    Ready,
    InProgress,
    Busy,
    Cooldown,
}
