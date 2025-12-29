namespace LightHouse
{
    using System;
    using FishNet.Connection;
    using FishNet.Object;
    using UnityEngine;
    using Fn;

    public class OwnerColorSelector : NetworkBehaviour
    {
        [SerializeField]
        LitController _controller;

        [SerializeField]
        Color _ownerColor = Color.white;
        [SerializeField]
        bool _ownerAlwaysLit = true;
        [SerializeField]
        bool _ownerAlwaysVisible = true;

        [SerializeField]
        Color _nonownerColor = Color.white;
        [SerializeField]
        bool _nonownerAlwaysLit = false;
        [SerializeField]
        bool _nonownerAlwaysVisible = false;

        [SerializeField]
        int _order = 0;

        FuncFn<Color, Color> _modifier;

        void Awake()
        {
            if (_controller == null)
            {
                Debug.Log("`_controller` was not set.");
                throw new Exception();
            }
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            if (_modifier != null)
            {
                _controller.RemoveModifier(_modifier);
                _modifier = null;
            }

            if (base.IsOwner)
            {
                _modifier = new FuncFn<Color, Color>((_) => _ownerColor);
                _controller.AddModifier(_modifier, _order);
                _controller.AlwaysLit = _ownerAlwaysLit;
                _controller.AlwaysVisible = _ownerAlwaysVisible;
            }
            else
            {
                _modifier = new FuncFn<Color, Color>((_) => _nonownerColor);
                _controller.AddModifier(_modifier, _order);
                _controller.AlwaysLit = _nonownerAlwaysLit;
                _controller.AlwaysVisible = _nonownerAlwaysVisible;
            }
        }
    }
}