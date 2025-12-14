namespace LightHouse
{
    using System;

    [Serializable]
    public abstract class DamageInfoBase
    {
        public virtual DamageInfoBase Clone()
        {
            return (DamageInfoBase)this.MemberwiseClone();
        }
    }

}
