using UnityEngine;
using UnityEngine.Events;

public class ItemInput : MonoBehaviour
{
    [SerializeField]
    UnityEvent<bool> _primary;
    [SerializeField]
    UnityEvent<bool> _secondary;

    InputState<bool> _primaryState = new();
    InputState<bool> _secondaryState = new();

    void Awake()
    {
        _primaryState.Change += OnPrimary;
        _secondaryState.Change += OnSecondary;
    }

    void OnDestroy()
    {
        _primaryState.Change -= OnPrimary;
        _secondaryState.Change -= OnSecondary;
    }

    void OnEnable()
    {
        _primaryState.Enable();
        _secondaryState.Enable();
    }

    void OnDisable()
    {
        _primaryState.Disable();
        _secondaryState.Disable();
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
    }

    public void OnUnregister()
    {
        _primaryState.Parent = null;
        _secondaryState.Parent = null;
    }

    void OnPrimary(bool newState)
    {
        _primary?.Invoke(newState);
    }

    void OnSecondary(bool newState)
    {
        _secondary?.Invoke(newState);
    }
}
