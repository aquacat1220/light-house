namespace LightHouse
{
    using FishNet.Object;
    using FishNet.Serializing;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public static class FooSer
    {
        public static void WriteFoo(this Writer writer, Foo value)
        {
            Debug.Log("Writing foo.");
        }

        public static Foo ReadFoo(this Reader reader)
        {
            Debug.Log("Reading foo.");
            return new();
        }
    }

    public class Foo
    {
        public float foo = 0f;
    }

    public class BufferLastReferenceTest : NetworkBehaviour
    {
        Foo foo = new();
        float bar = 0f;

        void Update()
        {
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                Rpc(foo, bar);
                Debug.Log("Send RPC.");
            }
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                foo.foo = 10f;
                Debug.Log("Changing Foo to 10f.");
            }
        }

        [ObserversRpc(BufferLast = true)]
        void Rpc(Foo foo, float bar)
        {
            Debug.Log($"Foo says: {foo.foo}, Bar says {bar}.");
        }
    }
}