namespace LightHouse
{
    using UnityEngine;
    using UnityEngine.Assertions;

    public class ProjectileImpactSpawner : MonoBehaviour
    {
        [SerializeField]
        Collision2DEvent _collisionEvent;

        [SerializeField]
        GameObject _projectileImpactPrefab;
        [SerializeField]
        float _lifetime = 2f;


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
            var projectileImpact = Instantiate(_projectileImpactPrefab, hit.point, Quaternion.Euler(0f, 0f, rotZ));

            TimerManager.Singleton.AddAlarm(
                cooldown: _lifetime,
                callback: (_) => Destroy(projectileImpact),
                startImmediately: true,
                armImmediately: true,
                autoRestart: false,
                autoRearm: false,
                initialCooldown: _lifetime,
                destroyAfterTriggered: true
            );
        }
    }
}