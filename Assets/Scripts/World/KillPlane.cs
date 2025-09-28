using MalgarHotel.Player;
using UnityEngine;

namespace MalgarHotel.World
{
    [RequireComponent(typeof(BoxCollider))]
    public class KillPlane : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private float respawnYOffset = 1.2f;

        public void SetRespawnPoint(Transform target)
        {
            respawnPoint = target;
        }

        private void Reset()
        {
            var box = GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(100f, 5f, 100f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (respawnPoint == null)
            {
                return;
            }

            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller == null)
            {
                controller = other.GetComponentInParent<PlayerController>();
            }

            if (controller == null)
            {
                return;
            }

            var characterController = controller.GetComponent<CharacterController>();
            if (characterController == null)
            {
                return;
            }

            Vector3 targetPosition = respawnPoint.position;
            targetPosition.y += respawnYOffset;

            characterController.enabled = false;
            controller.transform.position = targetPosition;
            characterController.enabled = true;
        }
    }
}
