using UnityEngine;

namespace MalgarHotel.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController _characterController;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            // Placeholder for player movement and interaction handling.
        }
    }
}
