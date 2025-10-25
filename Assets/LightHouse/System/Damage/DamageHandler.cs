namespace LightHouse
{
    using System;
    using System.Collections.Generic;
    using NaughtyAttributes;
    using UnityEngine;
    
    [Serializable]
    public abstract class DamageResponseBase
    {
        public float Foo;
    }
    
    [Serializable]
    public class DamageResponseFoo : DamageResponseBase
    {
        public GameObject Target;
    }
    
    [Serializable]
    public class DamageResponseBar : DamageResponseBase
    {
        public MonoBehaviour Component;
    }
    
    public class DamageHandler : MonoBehaviour
    {
        [SerializeReference]
        [ReorderableList]
        public List<DamageResponseBase> DamageResponses;
    }
}
