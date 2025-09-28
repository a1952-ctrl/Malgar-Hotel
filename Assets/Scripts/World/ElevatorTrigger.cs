using MalgarHotel.Core;
using UnityEngine;

namespace MalgarHotel.World
{
    [RequireComponent(typeof(Collider))]
    public class ElevatorTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject activationIndicator;

        private Collider _trigger;
        private bool _activated;

        private void Awake()
        {
            _trigger = GetComponent<Collider>();
            _trigger.isTrigger = true;
        }

        private void Update()
        {
            bool ready = GameManager.Instance != null && GameManager.Instance.CollectedFuses >= GameManager.Instance.RequiredFuses;
            if (activationIndicator != null)
            {
                activationIndicator.SetActive(ready);
            }
            _activated = ready;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_activated)
            {
                return;
            }

            if (other.TryGetComponent(out Player.PlayerController _))
            {
                GameManager.Instance?.CompleteRun();
            }
        }
    }
}
