namespace LightHouse
{
    using FishNet.Object;
    using UnityEngine;

    public class ProjectileDamage : NetworkBehaviour
    {
        [SerializeReference]
        [PolySelector]
        DamageInfoBase _damageInfo;

        public void OnTriggerEnter2D(Collider2D other)
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
