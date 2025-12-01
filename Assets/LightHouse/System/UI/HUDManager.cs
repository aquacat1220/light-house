namespace LightHouse
{
    using NaughtyAttributes;
    using UnityEngine;
    using UnityEngine.UIElements;
    
    public class HUDManager : MonoBehaviour
    {
        [SerializeField]
        [Required]
        UIDocument _hudDocument;
    }
}
