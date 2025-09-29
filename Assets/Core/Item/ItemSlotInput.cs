using UnityEngine;
using UnityEngine.Events;

public class ItemSlotInput : MonoBehaviour
{
    public UnityEvent<bool> Primary;
    public UnityEvent<bool> Secondary;

    bool _blockInputs = true;

    void OnEnable()
    {
        _blockInputs = false;
    }

    void OnDisable()
    {
        _blockInputs = true;
    }

    public void OnPrimary(bool isPerformed)
    {
        if (_blockInputs)
            return;
        Primary?.Invoke(isPerformed);
    }

    public void OnSecondary(bool isPerformed)
    {
        if (_blockInputs)
            return;
        Secondary?.Invoke(isPerformed);
    }
}
