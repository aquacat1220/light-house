namespace LightHouse
{
    using FishNet.Object;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class ProjectileDamage : NetworkBehaviour
    {
        [SerializeField]
        Collision2DEvent _collisionEvent;
        [SerializeReference]
        [PolySelector]
        DamageInfoBase _damageInfo;

        void Awake()
        {
            Assert.IsNotNull(_collisionEvent);
            _collisionEvent.TriggerEnter2D += OnTriggerEnter2D;

            Assert.IsNotNull(_damageInfo);
        }

        void OnDestroy()
        {
            _collisionEvent.TriggerEnter2D -= OnTriggerEnter2D;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (base.IsServerInitialized)
            {
                // If we are the server, test for damage.
                if (other.GetComponent<DamageProcessor>() is DamageProcessor processor)
                    processor.ProcessDamage(_damageInfo.Clone());
                if (!other.isTrigger)
                    base.Despawn();
                return;
            }
            else
            {
                if (!other.isTrigger)
                    gameObject.SetActive(false);
                return;
            }
        }

        public override void OnStopNetwork()
        {
            gameObject.SetActive(true);
        }
    }
}
