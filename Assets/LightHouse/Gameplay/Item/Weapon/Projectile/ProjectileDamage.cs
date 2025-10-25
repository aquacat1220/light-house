namespace LightHouse
{
    using FishNet;
    using FishNet.Object;
    using UnityEngine;
    
    public class ProjectileDamage : NetworkBehaviour
    {
        // A negative damage will heal the target.
        [SerializeField]
        float _damage;
        void OnTriggerEnter2D(Collider2D other)
        {
            // All projectile collision logic should only run on the server, since clients are not authoritative and might have discrepencies due to prediction and reconciliation.
            if (!base.IsServerInitialized)
                return;
    
            // First test if `other` is something that can be damaged.
            // TODO: Come up with a way to find the vitality component if the collider is deepr down the prefab's hierarchy.
            if (other.GetComponent<Vitality>() is Vitality vitality)
                vitality.ApplyDamage(_damage, canHeal: true);
    
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
