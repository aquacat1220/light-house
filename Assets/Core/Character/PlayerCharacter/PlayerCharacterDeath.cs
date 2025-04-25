using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterDeath : NetworkBehaviour
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

    [SerializeField]
    InputActionReference _deathAction;

    public void Awake()
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
        _healthSystem.HealthZero += OnHealthZero;
        // StartCoroutine(KillSelf());

    }

    public override void OnStartClient()
    {
        if (_deathAction != null && base.IsOwner)
        {
            _deathAction.action.performed += (_) => KillSelf();
        }
    }

    [ServerRpc]
    void KillSelf()
    {
        _healthSystem.ApplyDamage(100000f);
    }

    public void OnHealthZero()
    {
        Die();
    }

    [ObserversRpc]
    void Die()
    {
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
        CharacterSpawner.Singleton.SpawnCharacterUnchecked(base.Owner);

        // Then wait for another few seconds...
        yield return new WaitForSeconds(_timeToDespawn - _timeToRespawn);
        // ... to despawn this instance!
        base.Despawn();

        yield return null;
    }
}
