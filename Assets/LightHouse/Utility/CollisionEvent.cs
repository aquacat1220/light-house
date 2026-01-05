namespace LightHouse
{
    using System;
    using UnityEngine;

    public class Collision2DEvent : MonoBehaviour
    {
        [SerializeField]
        Collider2D _collider2D;
        public Collider2D Collider2D
        {
            get => _collider2D;
        }

        public event Action<Collision2D> CollisionEnter2D;
        public event Action<Collision2D> CollisionStay2D;
        public event Action<Collision2D> CollisionExit2D;
        public event Action<Collider2D> TriggerEnter2D;
        public event Action<Collider2D> TriggerStay2D;
        public event Action<Collider2D> TriggerExit2D;

        void OnCollisionEnter2D(Collision2D other)
        {
            if (!this.isActiveAndEnabled)
                return;
            CollisionEnter2D?.Invoke(other);
        }
        void OnCollisionStay2D(Collision2D other)
        {
            if (!this.isActiveAndEnabled)
                return;
            CollisionStay2D?.Invoke(other);
        }
        void OnCollisionExit2D(Collision2D other)
        {
            if (!this.isActiveAndEnabled)
                return;
            CollisionExit2D?.Invoke(other);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!this.isActiveAndEnabled)
                return;
            TriggerEnter2D?.Invoke(other);
        }
        void OnTriggerStay2D(Collider2D other)
        {
            if (!this.isActiveAndEnabled)
                return;
            TriggerStay2D?.Invoke(other);
        }
        void OnTriggerExit2D(Collider2D other)
        {
            if (!this.isActiveAndEnabled)
                return;
            TriggerExit2D?.Invoke(other);
        }
    }
}