namespace LightHouse
{
    using FishNet;
    using FishNet.Object;
    using UnityEngine;

    public class ProjectileDamage : NetworkBehaviour
    {
        [SerializeReference]
        [SubclassSelector]
        DamageInfoBase _damageInfo;

        void OnTriggerEnter2D(Collider2D other)
        {
            // All projectile collision logic should only run on the server, since clients are not authoritative and might have discrepencies due to prediction and reconciliation.
            if (!base.IsServerInitialized)
                return;

            // First test if `other` is something that can be damaged.
            if (other.GetComponent<DamageProcessor>() is DamageProcessor processor)
            {
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
