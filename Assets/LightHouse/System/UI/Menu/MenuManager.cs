namespace LightHouse
{
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class MenuManager : MonoBehaviour
    {
        [SerializeField]
        [Required]
        UIDocument _menuDocument;

        [SerializeField]
        [Required]
        VisualTreeAsset _menuRoot;

        void Awake()
        {
            _menuDocument.visualTreeAsset = _menuRoot;
        }
    }
}
