using System;

namespace LightHouse
{
    [Serializable]
    public abstract class DamageInfoBase
    {
        public virtual DamageInfoBase Clone()
        {
            return (DamageInfoBase)this.MemberwiseClone();
        }
    }

}
