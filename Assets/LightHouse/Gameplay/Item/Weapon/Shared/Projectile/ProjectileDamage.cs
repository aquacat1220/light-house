namespace LightHouse
{
    using FishNet;
    using FishNet.Object;
    using NaughtyAttributes;
    using UnityEngine;

    public class ProjectileDamage : NetworkBehaviour
    {
        [SerializeReference]
        [SubclassSelector]
        DamageInfoBase _damageInfo;

        [SerializeField]
        [Required]
        GameObject _visual;

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
