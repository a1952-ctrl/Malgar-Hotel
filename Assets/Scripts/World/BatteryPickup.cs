using MalgarHotel.Core;
using MalgarHotel.Player;
using UnityEngine;

namespace MalgarHotel.World
{
    [RequireComponent(typeof(Collider))]
    public class BatteryPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private float amount = 25f;
        [SerializeField] private string prompt = "Battery";
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

            ApplyMixerGroups();
        }

        private void OnEnable()
        {
            ApplyMixerGroups();
        }

        public string InteractionPrompt => prompt;

        public void Interact(PlayerController player)
        {
            if (_collected)
            {
                return;
            }

            _collected = true;
            player.RefillBattery(amount);
            if (pickupClip != null)
            {
                pickupAudio.PlayOneShot(pickupClip);
            }

            gameObject.SetActive(false);
        }

        private void ApplyMixerGroups()
        {
            if (GameManager.Instance != null && GameManager.Instance.SfxGroup != null && pickupAudio != null)
            {
                pickupAudio.outputAudioMixerGroup = GameManager.Instance.SfxGroup;
            }
        }
    }
}
