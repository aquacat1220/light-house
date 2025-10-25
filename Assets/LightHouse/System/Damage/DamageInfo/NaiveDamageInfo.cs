using System;
using UnityEngine;

namespace LightHouse
{
    [Serializable]
    public class NaiveDamageInfo : DamageInfoBase
    {
        // The gameobject that is inflicting this damage.
        public GameObject Attacker;
        // The value of the damage.
        public float Damage;
    }
}
