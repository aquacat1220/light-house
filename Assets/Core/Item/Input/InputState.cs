using System;

public class InputState<T> where T : IEquatable<T>
{
    InputState<T> _parent = null;
    public InputState<T> Parent
    {
        get { return _parent; }
        set
        {
            if (_parent == value)
                return;
            // Unsubscribe the handler from the old parent.
            if (_parent != null)
                _parent._change -= OnChange;

            _parent = value;
            // If the new parent is `null`, reset state to default, and skip handler subscription.
            if (_parent == null)
            {
                State = default;
                return;
            }
            State = _parent.State;
            _parent._change += OnChange;
        }
    }

    T _state;
    public T State
    {
        get { return _state; }
        private set
        {
            if (_blockInputs)
                return;
            if (_state.Equals(value))
                return;
            _state = value;
            _change?.Invoke(_state);
            Change?.Invoke(_state);
        }
    }

    bool _blockInputs = true;

    public void Disable()
    {
        // Reset the state back to default.
        State = default;
        // Then set `_blockInputs` to `true`. This will stop `State` from changing.
        _blockInputs = true;
    }

    public void Enable()
    {
        // Set `_blockInputs` to `false`. This will allow `State` to change.
        _blockInputs = false;
        // Then fetch the parent's state to re-init my state.
        if (Parent != null)
            State = Parent.State;
    }

    void OnChange(T newState)
    {
        State = newState;
    }

    public bool RootChangeState(T newState)
    {
        if (_parent != null)
            return false;
        State = newState;
        return true;
    }

    // Event to use internally between child-parent.
    event Action<T> _change;
    // Event to expose publically for users of the class.
    public event Action<T> Change;
}