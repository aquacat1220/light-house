using UnityEngine;

namespace LightHouse
{
    public class OnlyInPreview : MonoBehaviour
    {
        void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}