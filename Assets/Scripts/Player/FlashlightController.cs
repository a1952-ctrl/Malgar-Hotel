using MalgarHotel.Core;
using UnityEngine;

namespace MalgarHotel.Player
{
    public class FlashlightController : MonoBehaviour
    {
        [SerializeField] private Light flashlight;
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float drainPerSecond = 4f;
        [SerializeField, Range(0f, 1f)] private float flickerThreshold = 0.2f;
        [SerializeField] private Vector2 flickerRange = new Vector2(0.6f, 1f);
        [SerializeField] private AudioSource toggleAudio;
        [SerializeField] private AudioClip toggleClip;

        private float _currentBattery;
        private bool _isOn = true;
        private float _baseIntensity;

        public float CurrentBattery => _currentBattery;
        public float MaxBattery => maxBattery;
        public bool IsOn => _isOn;

        private void Awake()
        {
            if (flashlight == null)
            {
                flashlight = GetComponentInChildren<Light>();
            }

            if (flashlight != null)
            {
                _baseIntensity = flashlight.intensity;
            }

            if (toggleAudio == null)
            {
                toggleAudio = gameObject.AddComponent<AudioSource>();
                toggleAudio.playOnAwake = false;
                toggleAudio.spatialBlend = 1f;
            }
        }

        private void Start()
        {
            _currentBattery = maxBattery;
            BroadcastBattery();
            ApplyState();
        }

        private void Update()
        {
            if (!_isOn)
            {
                return;
            }

            if (_currentBattery > 0f)
            {
                _currentBattery = Mathf.Max(0f, _currentBattery - drainPerSecond * Time.deltaTime);
                BroadcastBattery();
            }

            if (_currentBattery <= 0.01f)
            {
                SetFlashlightState(false);
                return;
            }

            float percent = _currentBattery / maxBattery;
            if (flashlight != null)
            {
                if (percent <= flickerThreshold)
                {
                    float randomIntensity = _baseIntensity * Random.Range(flickerRange.x, flickerRange.y);
                    flashlight.intensity = randomIntensity;
                    flashlight.enabled = Random.value > 0.1f;
                }
                else
                {
                    flashlight.intensity = _baseIntensity;
                    flashlight.enabled = true;
                }
            }
        }

        public void Toggle()
        {
            SetFlashlightState(!_isOn);
        }

        public void AddBattery(float amount)
        {
            _currentBattery = Mathf.Clamp(_currentBattery + amount, 0f, maxBattery);
            BroadcastBattery();
            if (!_isOn && _currentBattery > 0.1f)
            {
                SetFlashlightState(true);
            }
        }

        private void SetFlashlightState(bool state)
        {
            if (_isOn == state)
            {
                return;
            }

            _isOn = state;
            ApplyState();
            PlayToggleSound();
        }

        private void ApplyState()
        {
            if (flashlight != null)
            {
                flashlight.enabled = _isOn && _currentBattery > 0f;
                if (_isOn)
                {
                    flashlight.intensity = _baseIntensity;
                }
            }
        }

        private void BroadcastBattery()
        {
            GameManager.Instance?.NotifyBatteryChanged(_currentBattery, maxBattery);
        }

        private void PlayToggleSound()
        {
            if (toggleClip != null)
            {
                toggleAudio.PlayOneShot(toggleClip);
            }
        }
    }
}
