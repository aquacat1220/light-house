namespace LightHouse
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Collision2DEvent : MonoBehaviour
    {
        public Fn.Event<Collision2D> CollisionEnter2D;
        public Fn.Event<Collision2D> CollisionStay2D;
        public Fn.Event<Collision2D> CollisionExit2D;
        public Fn.Event<Collider2D> TriggerEnter2D;
        public Fn.Event<Collider2D> TriggerStay2D;
        public Fn.Event<Collider2D> TriggerExit2D;

        void OnCollisionEnter2D(Collision2D other)
        {
            CollisionEnter2D?.Invoke(other);
        }
        void OnCollisionStay2D(Collision2D other)
        {
            CollisionStay2D?.Invoke(other);
        }
        void OnCollisionExit2D(Collision2D other)
        {
            CollisionExit2D?.Invoke(other);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter2D?.Invoke(other);
        }
        void OnTriggerStay2D(Collider2D other)
        {
            TriggerStay2D?.Invoke(other);
        }
        void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit2D?.Invoke(other);
        }
    }
}