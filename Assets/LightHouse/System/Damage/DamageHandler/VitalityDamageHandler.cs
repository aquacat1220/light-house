namespace LightHouse
{
    using System;
    using UnityEngine;

    [Serializable]
    public class VitalityDamageHandler : IDamageHandler
    {
        public Vitality Vitality;
        public bool CanHarmSelf;

        // Handle the damage, and returns `true` if damage propagation should stop here.
        // Ex. If this handler was for applying explosive damage, and the damage wasn't explosive,
        // we would return `false` to signal the `DamageProcessor` to keep iterating handlers.
        public bool Handle(DamageProcessor processor, DamageInfoBase damageInfo)
        {
            if (Vitality == null)
                return false;
            if (damageInfo is NaiveDamageInfo info)
            {
                if (!CanHarmSelf && info.Attacker == processor.gameObject)
                    return false;
                Vitality.ApplyDamage(info.Damage, canHeal: true);
                return true;
            }
            return false;
        }
    }

}
