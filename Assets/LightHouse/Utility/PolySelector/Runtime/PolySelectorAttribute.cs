namespace LightHouse
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PolySelectorAttribute : PropertyAttribute { }
}