namespace LightHouse
{
    using System;
    using FishNet.Object;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Fn;
    using System.Collections.Generic;
    using System.Linq;

    public class PlayerCharacterInventory : NetworkBehaviour
    {
        [SerializeField]
        Transform _mainHandAnchor;
        [SerializeField]
        Transform _subHandAnchor;
        [SerializeField]
        Transform[] _backupAnchors;

        List<(ItemSlot Slot, ItemSlotInput Input)> _itemSlots = new();
        int _mainHandIdx;
        int _subHandIdx;
        List<int> _backupIdxs = new();

        InputState<bool> _primaryState = new();
        InputState<bool> _secondaryState = new();
        InputState<bool> _action1State = new();
        InputState<bool> _action2State = new();
        InputState<bool> _action3State = new();

        bool _blockInputs = true;

        void Awake()
        {
            if (_mainHandAnchor == null || _mainHandAnchor.childCount != 1 || _mainHandAnchor.GetChild(0).GetComponent<ItemSlot>() == null || _mainHandAnchor.GetChild(0).GetComponent<ItemSlotInput>() == null)
            {
                Debug.Log("`_mainHandAnchor` should be a transform, have one child, and the child must have `ItemSlot` and `ItemSlotInput` components.");
                throw new Exception();
            }
            _itemSlots.Add((_mainHandAnchor.GetChild(0).GetComponent<ItemSlot>(), _mainHandAnchor.GetChild(0).GetComponent<ItemSlotInput>()));
            _mainHandIdx = 0;
            if (_subHandAnchor == null || _subHandAnchor.childCount != 1 || _subHandAnchor.GetChild(0).GetComponent<ItemSlot>() == null || _subHandAnchor.GetChild(0).GetComponent<ItemSlotInput>() == null)
            {
                Debug.Log("`_subHandAnchor` should be a transform, have one child, and the child must have `ItemSlot` and `ItemSlotInput` components.");
                throw new Exception();
            }
            _itemSlots.Add((_subHandAnchor.GetChild(0).GetComponent<ItemSlot>(), _subHandAnchor.GetChild(0).GetComponent<ItemSlotInput>()));
            _subHandIdx = 1;

            int backupIdx = 2;
            foreach (var backup in _backupAnchors)
            {
                if (backup == null || backup.childCount != 1 || backup.GetChild(0).GetComponent<ItemSlot>() == null || backup.GetChild(0).GetComponent<ItemSlotInput>() == null)
                {
                    Debug.Log("Element of `_backupAnchors` should be a transform, have one child, and the child must have `ItemSlot` and `ItemSlotInput` components.");
                    throw new Exception();
                }
                _itemSlots.Add((backup.GetChild(0).GetComponent<ItemSlot>(), backup.GetChild(0).GetComponent<ItemSlotInput>()));
                _backupIdxs.Add(backupIdx++);
            }

            var mainInput = _itemSlots[_mainHandIdx].Input;
            mainInput.PrimaryState.Parent = _primaryState;
            mainInput.SecondaryState.Parent = _secondaryState;
            mainInput.Action1State.Parent = _action1State;
            mainInput.Action2State.Parent = _action2State;
            mainInput.Action3State.Parent = _action3State;
        }

        void OnEnable()
        {
            _primaryState.Enable();
            _secondaryState.Enable();
            _action1State.Enable();
            _action2State.Enable();
            _action3State.Enable();
            _blockInputs = false;
        }

        void OnDisable()
        {
            _primaryState.Disable();
            _secondaryState.Disable();
            _action1State.Disable();
            _action2State.Disable();
            _action3State.Disable();
            _blockInputs = true;
        }

        [Server]
        public bool AddItem(Item item)
        {
            if (_itemSlots[_mainHandIdx].Slot.Equip(item))
                return true;
            if (_itemSlots[_subHandIdx].Slot.Equip(item))
                return true;
            foreach (int backupIdx in _backupIdxs)
            {
                if (_itemSlots[backupIdx].Slot.Equip(item))
                    return true;
            }
            return false;
        }

        [Client(RequireOwnership = true)]
        public void OnPrimary(bool newState)
        {
            // Let the input pulse flow down the chain on the client.
            OnPrimaryLocal(newState);

            // If we are the server too (= host), don't do this twice.
            if (base.IsServerInitialized)
                return;
            // If we are not the host, make a RPC call to sync the pulse to the server.
            OnPrimaryRpc(newState);
        }

        [ServerRpc(RequireOwnership = true)]
        void OnPrimaryRpc(bool newState)
        {
            OnPrimaryLocal(newState);
        }

        void OnPrimaryLocal(bool newState)
        {
            // We don't check `_blockInputs` here because `InputState`s have their own `Enable()` `Disable()` logic.
            var rootChangeResult = _primaryState.RootChangeState(newState);
            Assert.IsTrue(rootChangeResult);
        }

        [Client(RequireOwnership = true)]
        public void OnSecondary(bool newState)
        {
            // Let the input pulse flow down the chain on the client.
            OnSecondaryLocal(newState);

            // If we are the server too (= host), don't do this twice.
            if (base.IsServerInitialized)
                return;
            // If we are not the host, make a RPC call to sync the pulse to the server.
            OnSecondaryRpc(newState);
        }

        [ServerRpc(RequireOwnership = true)]
        void OnSecondaryRpc(bool newState)
        {
            OnSecondaryLocal(newState);
        }

        void OnSecondaryLocal(bool newState)
        {
            // We don't check `_blockInputs` here because `InputState`s have their own `Enable()` `Disable()` logic.
            var rootChangeResult = _secondaryState.RootChangeState(newState);
            Assert.IsTrue(rootChangeResult);
        }

        [Client(RequireOwnership = true)]
        public void OnAction1(bool newState)
        {
            // Let the input pulse flow down the chain on the client.
            OnAction1Local(newState);

            // If we are the server too (= host), don't do this twice.
            if (base.IsServerInitialized)
                return;
            // If we are not the host, make a RPC call to sync the pulse to the server.
            OnAction1Rpc(newState);
        }

        [ServerRpc(RequireOwnership = true)]
        void OnAction1Rpc(bool newState)
        {
            OnAction1Local(newState);
        }

        void OnAction1Local(bool newState)
        {
            // We don't check `_blockInputs` here because `InputState`s have their own `Enable()` `Disable()` logic.
            var rootChangeResult = _action1State.RootChangeState(newState);
            Assert.IsTrue(rootChangeResult);
        }

        [Client(RequireOwnership = true)]
        public void OnAction2(bool newState)
        {
            // Let the input pulse flow down the chain on the client.
            OnAction2Local(newState);

            // If we are the server too (= host), don't do this twice.
            if (base.IsServerInitialized)
                return;
            // If we are not the host, make a RPC call to sync the pulse to the server.
            OnAction2Rpc(newState);
        }

        [ServerRpc(RequireOwnership = true)]
        void OnAction2Rpc(bool newState)
        {
            OnAction2Local(newState);
        }

        void OnAction2Local(bool newState)
        {
            // We don't check `_blockInputs` here because `InputState`s have their own `Enable()` `Disable()` logic.
            var rootChangeResult = _action2State.RootChangeState(newState);
            Assert.IsTrue(rootChangeResult);
        }

        [Client(RequireOwnership = true)]
        public void OnAction3(bool newState)
        {
            // Let the input pulse flow down the chain on the client.
            OnAction3Local(newState);

            // If we are the server too (= host), don't do this twice.
            if (base.IsServerInitialized)
                return;
            // If we are not the host, make a RPC call to sync the pulse to the server.
            OnAction3Rpc(newState);
        }

        [ServerRpc(RequireOwnership = true)]
        void OnAction3Rpc(bool newState)
        {
            OnAction3Local(newState);
        }

        void OnAction3Local(bool newState)
        {
            // We don't check `_blockInputs` here because `InputState`s have their own `Enable()` `Disable()` logic.
            var rootChangeResult = _action3State.RootChangeState(newState);
            Assert.IsTrue(rootChangeResult);
        }

        [ServerRpc(RequireOwnership = true)]
        public void OnSwapItem()
        {
            if (_blockInputs)
                return;
            SubToMain();
        }

        [ServerRpc(RequireOwnership = true)]
        public void OnSwapToBackup(int backup)
        {
            if (_blockInputs)
                return;
            BackupToMain(backup);
        }

        [ServerRpc(RequireOwnership = true)]
        public void OnDropItem()
        {
            if (_blockInputs)
                return;
            _itemSlots[_mainHandIdx].Slot.Unequip();
        }

        [Server]
        void SubToMain()
        {
            int newMainHandIdx = _subHandIdx;
            int newSubHandIdx = _mainHandIdx;
            List<int> newBackupIdxs = _backupIdxs.ToList();
            SyncInventory(newMainHandIdx, newSubHandIdx, newBackupIdxs);
        }

        [Server]
        void BackupToMain(int backup)
        {
            int newMainHandIdx = _backupIdxs[backup];
            int newSubHandIdx = _subHandIdx;
            List<int> newBackupIdxs = _backupIdxs.ToList();
            newBackupIdxs[backup] = _mainHandIdx;
            SyncInventory(newMainHandIdx, newSubHandIdx, newBackupIdxs);
        }

        [ObserversRpc(BufferLast = true, RunLocally = true)]
        void SyncInventory(int newMainHandIdx, int newSubHandIdx, List<int> newBackupIdxs)
        {
            if (_mainHandIdx != newMainHandIdx)
            {
                var newMainInput = _itemSlots[newMainHandIdx].Input;
                newMainInput.PrimaryState.Parent = _primaryState;
                newMainInput.SecondaryState.Parent = _secondaryState;
                newMainInput.Action1State.Parent = _action1State;
                newMainInput.Action2State.Parent = _action2State;
                newMainInput.Action3State.Parent = _action3State;
                newMainInput.transform.SetParent(_mainHandAnchor, worldPositionStays: false);
                _mainHandIdx = newMainHandIdx;
            }
            if (_subHandIdx != newSubHandIdx)
            {
                var newSubInput = _itemSlots[newSubHandIdx].Input;
                newSubInput.PrimaryState.Parent = null;
                newSubInput.SecondaryState.Parent = null;
                newSubInput.Action1State.Parent = null;
                newSubInput.Action2State.Parent = null;
                newSubInput.Action3State.Parent = null;
                newSubInput.transform.SetParent(_subHandAnchor, worldPositionStays: false);
                _subHandIdx = newSubHandIdx;
            }
            for (int backup = 0; backup < _backupIdxs.Count; backup++)
            {
                int backupIdx = _backupIdxs[backup];
                int newBackupIdx = newBackupIdxs[backup];
                if (backupIdx != newBackupIdx)
                {
                    var newBackupInput = _itemSlots[newBackupIdx].Input;
                    newBackupInput.PrimaryState.Parent = null;
                    newBackupInput.SecondaryState.Parent = null;
                    newBackupInput.Action1State.Parent = null;
                    newBackupInput.Action2State.Parent = null;
                    newBackupInput.Action3State.Parent = null;
                    newBackupInput.transform.SetParent(_backupAnchors[backup], worldPositionStays: false);
                    _backupIdxs[backup] = newBackupIdx;
                }
            }
        }
    }
}
