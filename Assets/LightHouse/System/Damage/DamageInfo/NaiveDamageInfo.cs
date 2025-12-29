namespace LightHouse
{
    using System;
    using UnityEngine;
    [Serializable]
    public class NaiveDamageInfo : DamageInfoBase
    {
        // The gameobject that is inflicting this damage.
        public GameObject Attacker;
        // The value of the damage.
        public float Damage;
    }
}
