using MalgarHotel.Core;
using UnityEngine;

namespace MalgarHotel.Audio
{
    /// <summary>
    /// Bridges microphone calibration output with the unified noise system, emitting
    /// events at configurable intervals based on the detected voice band.
    /// </summary>
    [DisallowMultipleComponent]
    public class MicNoiseEmitter : MonoBehaviour
    {
        [SerializeField] private MicCalibration calibration;
        [SerializeField] private Transform emissionOrigin;
        [SerializeField] private float tickInterval = 0.15f;
        [SerializeField] private float cooldown = 0.2f;
        [SerializeField] private float minDelta = 0.05f;
        [SerializeField] private float lowIntensity = 0.25f;
        [SerializeField] private float midIntensity = 0.55f;
        [SerializeField] private float highIntensity = 0.9f;

        private float _timer;
        private float _lastEmitTime;
        private MicBand _lastBand = MicBand.None;
        private float _lastLevel;

        private void OnEnable()
        {
            if (calibration == null)
            {
                calibration = FindObjectOfType<MicCalibration>();
            }
        }

        private void Update()
        {
            if (calibration == null)
            {
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                return;
            }

            _timer += Time.unscaledDeltaTime;
            if (_timer < tickInterval)
            {
                return;
            }

            _timer = 0f;
            MicBand band = calibration.CurrentBand;
            if (band == MicBand.None)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (now - _lastEmitTime < cooldown)
            {
                return;
            }

            float level = calibration.NormalizedLevel;
            if (Mathf.Abs(level - _lastLevel) < minDelta && band == _lastBand)
            {
                return;
            }

            float intensity = GetIntensityForBand(band);
            if (intensity <= 0f)
            {
                return;
            }

            Vector3 origin = emissionOrigin != null ? emissionOrigin.position : transform.position;
            NoiseSystem.EmitNoise(origin, intensity, 1, 0.75f, NoiseTag.Mic);

            _lastEmitTime = now;
            _lastLevel = level;
            _lastBand = band;
        }

        private float GetIntensityForBand(MicBand band)
        {
            switch (band)
            {
                case MicBand.Low:
                    return lowIntensity;
                case MicBand.Mid:
                    return midIntensity;
                case MicBand.High:
                    return highIntensity;
                default:
                    return 0f;
            }
        }
    }
}
