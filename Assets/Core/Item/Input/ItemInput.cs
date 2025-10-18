using UnityEngine;
using UnityEngine.Events;

public class ItemInput : MonoBehaviour
{
    [SerializeField]
    UnityEvent<bool> _primary;
    [SerializeField]
    UnityEvent<bool> _secondary;
    [SerializeField]
    UnityEvent<bool> _action1;
    [SerializeField]
    UnityEvent<bool> _action2;
    [SerializeField]
    UnityEvent<bool> _reload;

    InputState<bool> _primaryState = new();
    InputState<bool> _secondaryState = new();
    InputState<bool> _action1State = new();
    InputState<bool> _action2State = new();
    InputState<bool> _reloadState = new();

    void Awake()
    {
        _primaryState.Change += OnPrimary;
        _secondaryState.Change += OnSecondary;
        _action1State.Change += OnAction1;
        _action2State.Change += OnAction2;
        _reloadState.Change += OnReload;
    }

    void OnDestroy()
    {
        _primaryState.Change -= OnPrimary;
        _secondaryState.Change -= OnSecondary;
        _action1State.Change -= OnAction1;
        _action2State.Change -= OnAction2;
        _reloadState.Change -= OnReload;
    }

    void OnEnable()
    {
        _primaryState.Enable();
        _secondaryState.Enable();
        _action1State.Enable();
        _action2State.Enable();
        _reloadState.Enable();
    }

    void OnDisable()
    {
        _primaryState.Disable();
        _secondaryState.Disable();
        _action1State.Disable();
        _action2State.Disable();
        _reloadState.Disable();
    }

    public void OnRegister(ItemSlot itemSlot)
    {
        var itemSlotInput = itemSlot.GetComponent<ItemSlotInput>();
        if (itemSlotInput == null)
        {
            Debug.Log("`itemSlot` doesn't have an `ItemSlotInput` component during registering. Is this normal?");
            return;
        }

        _primaryState.Parent = itemSlotInput.PrimaryState;
        _secondaryState.Parent = itemSlotInput.SecondaryState;
        _action1State.Parent = itemSlotInput.Action1State;
        _action2State.Parent = itemSlotInput.Action2State;
        _reloadState.Parent = itemSlotInput.ReloadState;
    }

    public void OnUnregister()
    {
        _primaryState.Parent = null;
        _secondaryState.Parent = null;
        _action1State.Parent = null;
        _action2State.Parent = null;
        _reloadState.Parent = null;
    }

    void OnPrimary(bool newState)
    {
        _primary?.Invoke(newState);
    }

    void OnSecondary(bool newState)
    {
        _secondary?.Invoke(newState);
    }

    void OnAction1(bool newState)
    {
        _action1?.Invoke(newState);
    }

    void OnAction2(bool newState)
    {
        _action2?.Invoke(newState);
    }

    void OnReload(bool newState)
    {
        _reload?.Invoke(newState);
    }
}
