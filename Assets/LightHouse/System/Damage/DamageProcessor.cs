namespace LightHouse
{
    using System.Collections.Generic;
    using UnityEngine;

    public class DamageProcessor : MonoBehaviour
    {
        [SerializeReference]
        [SubclassSelector]
        public List<DamageHandlerBase> DamageHandlers;

        public void ProcessDamage(DamageInfoBase damageInfo)
        {
            foreach (var handler in DamageHandlers)
            {
                if (handler.Handle(this, damageInfo))
                    break;
            }
        }
    }
}
