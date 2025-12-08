namespace LightHouse
{
    using System.Runtime.CompilerServices;
    using FishNet.Object;
    using LightHouse.Fn;
    using UnityEngine;

    public class ProjectileDamage : NetworkBehaviour
    {
        [SerializeReference]
        [SubclassSelector]
        DamageInfoBase _damageInfo;

        [SerializeField]
        GameObject _visual;

        [PolymorphicSelector]
        [SerializeReference]
        public Tuple<int> _test = new();

        [SubclassSelector]
        [SerializeReference]
        public Tuple<int> _test2 = new(1);

        public Tuple<int> _test3 = new(1);

        public int _test4 = 3;

        void Awake()
        {
            if (_visual == null)
            {
                Debug.Log("`_visual` was not set.");
                throw new System.Exception();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // All projectile collision logic should only run on the server, since clients are not authoritative and might have discrepencies due to prediction and reconciliation.
            if (!base.IsServerInitialized)
            {
                // With the exception of visual updates!
                // Make the projectile invisible if it locally collided with a non-trigger collider.
                if (!other.isTrigger)
                {
                    _visual.SetActive(false);
                }
                return;
            }

            // First test if `other` is something that can be damaged.
            if (other.GetComponent<DamageProcessor>() is DamageProcessor processor)
            {
                Debug.Log("DSFS");
                processor.ProcessDamage(_damageInfo.Clone());
            }

            if (!other.isTrigger)
            {
                // We have collided with a non-trigger collider.
                // Despawn the projectile.
                base.Despawn();
                return;
            }
        }
    }
}
