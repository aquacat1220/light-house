namespace LightHouse
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class ProjectileImpactSpawner : MonoBehaviour
    {
        [SerializeField]
        Collision2DEvent _collisionEvent;

        [SerializeField]
        GameObject _projectileImpactPrefab;

        void Awake()
        {
            Assert.IsNotNull(_collisionEvent);
            _collisionEvent.TriggerEnter2D += OnTriggerEnter2D;
            Assert.IsNotNull(_projectileImpactPrefab);
        }

        void OnDestroy()
        {
            _collisionEvent.TriggerEnter2D -= OnTriggerEnter2D;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Skip trigger colliders; we want to spawn impacts only when we crash into something solid.
            if (other.isTrigger)
                return;
            var hit = Physics2D.Raycast(transform.position, transform.up);
            var rotZ = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
            Instantiate(_projectileImpactPrefab, hit.point, Quaternion.Euler(0f, 0f, rotZ));
        }
    }
}