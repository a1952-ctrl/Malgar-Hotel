using MalgarHotel.Core;
using MalgarHotel.Player;
using UnityEngine;

namespace MalgarHotel.World
{
    [RequireComponent(typeof(Collider))]
    public class FusePickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Retrieve Fuse";
        [SerializeField] private AudioSource pickupAudio;
        [SerializeField] private AudioClip pickupClip;

        private bool _collected;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;

            if (pickupAudio == null)
            {
                pickupAudio = gameObject.AddComponent<AudioSource>();
                pickupAudio.playOnAwake = false;
                pickupAudio.spatialBlend = 1f;
            }
        }

        public string InteractionPrompt => prompt;

        public void Interact(PlayerController player)
        {
            if (_collected)
            {
                return;
            }

            _collected = true;
            GameManager.Instance?.RegisterFuseCollected();
            if (pickupClip != null)
            {
                pickupAudio.PlayOneShot(pickupClip);
            }

            gameObject.SetActive(false);
        }
    }
}
