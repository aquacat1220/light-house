using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;

public class Death : NetworkBehaviour
{
    [SerializeField]
    HealthSystem _healthSystem;

    // Array fo components to disable on death.
    [SerializeField]
    MonoBehaviour[] _componentsToDisable;

    // Reference to sprite renderer, to change color on death.
    [SerializeField]
    SpriteRenderer _spriteRenderer;

    [SerializeField]
    Color _colorOnDeath = Color.red;

    [SerializeField]
    float _timeToRespawn = 3.0f;

    [SerializeField]
    float _timeToDespawn = 15.0f;

    bool _isInputBlocked = true;
    bool _isDead = false;

    void Awake()
    {
        if (_timeToDespawn < _timeToRespawn)
        {
            _timeToDespawn = _timeToRespawn;
        }
        if (_healthSystem == null)
        {
            Debug.Log("`_healthSystem` wasn't set.");
            throw new Exception();
        }
        _healthSystem.HealthChange += OnHealthChange;
        // StartCoroutine(KillSelf());
    }

    public override void OnStartClient()
    {
        if (base.IsOwner)
        {
            AllowInputs();
        }
    }

    public override void OnStopClient()
    {
        BlockInputs();
    }

    void OnEnable()
    {
        if (base.IsClientInitialized && base.IsOwner)
        {
            AllowInputs();
        }
    }

    void OnDisable()
    {
        BlockInputs();
    }

    void AllowInputs()
    {
        if (_isInputBlocked)
        {
            _isInputBlocked = false;
        }
    }

    void BlockInputs()
    {
        if (!_isInputBlocked)
        {
            _isInputBlocked = true;
        }
    }

    // Called to notify die input.
    [Client(RequireOwnership = true)]
    public void OnDie()
    {
        // If input is blocked, ignore it.
        if (_isInputBlocked) { return; }
        RequestDeath();
    }

    [ServerRpc]
    void RequestDeath()
    {
        Die();
    }

    void OnHealthChange(float prev, float next, bool asServer)
    {
        if (next == 0f && asServer && base.IsServerInitialized)
        {
            // We are the server, and health is zero.
            // Kill the character.
            Die();
        }
    }

    [Server]
    public void Die()
    {
        NotifyDeath();
    }

    [ObserversRpc(RunLocally = true)]
    void NotifyDeath()
    {
        if (_isDead)
            return;
        _isDead = true;

        // Disable all components that should be disabled. (Usually input-related.)
        foreach (var component in _componentsToDisable)
        {
            component.enabled = false;
        }

        // Then change the color to something that clearly shows death.
        _spriteRenderer.color = _colorOnDeath;

        // And if we are the server, initiate the after-death sequence, which respawns the character.
        if (base.IsServerInitialized)
        {
            StartCoroutine(AfterDeath());
        }
    }

    // Called only on the server to spawn a new character, and despawn this one.
    IEnumerator AfterDeath()
    {
        Debug.Log("Starting afterdeath sequence.");
        // Wait for few seconds...
        yield return new WaitForSeconds(_timeToRespawn);
        // ... and spawn the new character.
        if (CharacterSpawner.Singleton == null)
        {
            Debug.Log("`CharacterSpawner.Singleton` was null, implying we do not have a character spawner in this scene.");
            throw new Exception();
        }
        if (base.Owner.IsActive)
        {
            // The owner is still here!
            // Respawn a character for them.
            CharacterSpawner.Singleton.SpawnCharacter(base.Owner);
        }

        // Then wait for another few seconds...
        yield return new WaitForSeconds(_timeToDespawn - _timeToRespawn);
        // ... to despawn this instance!
        base.Despawn();

        yield return null;
    }
}
