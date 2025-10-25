using System;
using UnityEngine;

namespace LightHouse
{
    [Serializable]
    public abstract class DamageHandlerBase
    {
        // Handle the damage, and returns `true` if damage propagation should stop here.
        // Ex. If this handler was for applying explosive damage, and the damage wasn't explosive,
        // we would return `false` to signal the `DamageProcessor` to keep iterating handlers.
        public abstract bool Handle(DamageProcessor processor, DamageInfoBase damageInfo);
    }

}
