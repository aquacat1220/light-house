using System;
using System.Collections;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacterDeath : NetworkBehaviour
{
    [SerializeField]
    MonoBehaviour[] _components_to_disable;

    // Reference to sprite renderer, to change color on death.
    [SerializeField]
    SpriteRenderer _sprite_renderer;

    [SerializeField]
    Color _color_on_death = Color.red;

    [SerializeField]
    float _time_to_respawn = 3.0f;

    [SerializeField]
    float _time_to_despawn = 15.0f;

    public void Awake()
    {
        if (_time_to_despawn < _time_to_respawn)
        {
            _time_to_despawn = _time_to_respawn;
        }
        StartCoroutine(KillSelf());
    }

    IEnumerator KillSelf()
    {
        var health = GetComponent<HealthSystem>();
        while (true)
        {
            health.ApplyDamage(0.1f);
            yield return null;
        }
    }

    public void OnDeath()
    {
        // Disable all components that should be disabled. (Usually input-related.)
        foreach (var component in _components_to_disable)
        {
            component.enabled = false;
        }

        // Then change the color to something that clearly shows death.
        _sprite_renderer.color = _color_on_death;

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
        yield return new WaitForSeconds(_time_to_respawn);
        // ... and spawn the new character.
        if (CharacterSpawner.Singleton == null)
        {
            Debug.Log("`CharacterSpawner.Singleton` was null, implying we do not have a character spawner in this scene.");
            throw new Exception();
        }
        CharacterSpawner.Singleton.SpawnCharacterUnchecked(base.Owner);

        // Then wait for another few seconds...
        yield return new WaitForSeconds(_time_to_despawn - _time_to_respawn);
        // ... to despawn this instance!
        base.Despawn();

        yield return null;
    }
}
