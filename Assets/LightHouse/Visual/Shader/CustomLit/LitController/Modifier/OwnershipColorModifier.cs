namespace LightHouse
{
    using System;
    using FishNet.Connection;
    using FishNet.Object;
    using UnityEngine;

    public class OwnershipColorModifier : NetworkBehaviour
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


        void Awake()
        {
            if (_controller == null)
            {
                Debug.Log("`_controller` was not set.");
                throw new Exception();
            }
            _controller.AddModifier(Modifier, _order);
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            if (base.IsOwner)
            {
                _controller.AlwaysLit = _ownerAlwaysLit;
                _controller.AlwaysVisible = _ownerAlwaysVisible;
            }
            else
            {
                _controller.AlwaysLit = _nonownerAlwaysLit;
                _controller.AlwaysVisible = _nonownerAlwaysVisible;
            }

            _controller.ReevaluateModifiers();
        }

        void OnDestroy()
        {
            _controller.RemoveModifier(Modifier);
        }

        Color Modifier(Color _)
        {
            if (base.IsOwner)
                return _ownerColor;
            return _nonownerColor;
        }
    }
}