using UnityEngine;
using UnityEngine.Events;

public class ItemInput : MonoBehaviour
{
    [SerializeField]
    UnityEvent<bool> _primary;
    [SerializeField]
    UnityEvent<bool> _secondary;

    bool _blockInputs = true;
    ItemSlotInput _itemSlotInput;

    void OnEnable()
    {
        _blockInputs = false;
    }

    void OnDisable()
    {
        _blockInputs = true;
    }

    public void OnRegister(ItemSlot itemSlot)
    {
        _itemSlotInput = itemSlot.GetComponent<ItemSlotInput>();
        if (_itemSlotInput == null)
        {
            Debug.Log("`itemSlot` doesn't have an `ItemSlotInput` component during registering. Is this normal?");
            return;
        }

        _itemSlotInput.Primary.AddListener(OnPrimary);
        _itemSlotInput.Secondary.AddListener(OnSecondary);
    }

    public void OnUnregister()
    {
        if (_itemSlotInput == null)
        {
            Debug.Log("`_itemSlotInput` is null during unregistering. Is this normal?");
            return;
        }

        _itemSlotInput.Primary.RemoveListener(OnPrimary);
        _itemSlotInput.Secondary.RemoveListener(OnSecondary);

        // Just in case inputs were currently "performed", make sure they are canceled.
        OnPrimary(false);
        OnSecondary(false);
    }

    void OnPrimary(bool isPerformed)
    {
        if (_blockInputs)
            return;
        _primary?.Invoke(isPerformed);
    }

    void OnSecondary(bool isPerformed)
    {
        if (_blockInputs)
            return;
        _secondary?.Invoke(isPerformed);
    }
}
