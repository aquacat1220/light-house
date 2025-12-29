namespace LightHouse
{
    using System;
    using UnityEngine;

    public interface IDamageHandler
    {
        // Handle the damage, and returns `true` if damage propagation should stop here.
        // Ex. If this handler was for applying explosive damage, and the damage wasn't explosive,
        // we would return `false` to signal the `DamageProcessor` to keep iterating handlers.
        public bool Handle(DamageProcessor processor, DamageInfoBase damageInfo);
    }

}
