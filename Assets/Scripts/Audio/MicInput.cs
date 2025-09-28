using System;
using System.Linq;
using UnityEngine;

namespace MalgarHotel.Audio
{
    /// <summary>
    /// Captures system microphone audio, exposes instantaneous RMS values, and smoothes them
    /// for downstream systems such as calibration UI and noise emission.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-250)]
    public class MicInput : MonoBehaviour
    {
        [Header("Capture")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private int sampleRate = 48000;
        [SerializeField] private int bufferLengthSeconds = 1;
        [SerializeField] private int sampleWindowMs = 256;
        [SerializeField, Range(0.01f, 1f)] private float smoothing = 0.25f;
        [SerializeField] private float devicePollInterval = 2f;

        private AudioClip _micClip;
        private string _currentDevice;
        private float[] _sampleBuffer;
        private float _lastDevicePoll;
        private bool _isCapturing;
        private int _sampleWindowSize;

        /// <summary>Instantaneous RMS calculated from the most recent sample window.</summary>
        public float CurrentRms { get; private set; }

        /// <summary>Exponentially-smoothed RMS for UI display and calibration.</summary>
        public float RmsSmoothed { get; private set; }

        /// <summary>Allows tests to force silence without disabling capture.</summary>
        public bool Mute { get; set; }

        public bool IsMicAvailable => Microphone.devices != null && Microphone.devices.Length > 0;
        public bool IsCapturing => _isCapturing && _micClip != null;
        public string DeviceName => _currentDevice;

        private void Awake()
        {
            _sampleWindowSize = Mathf.Max(64, sampleRate * sampleWindowMs / 1000);
            _sampleBuffer = new float[_sampleWindowSize];
        }

        private void OnEnable()
        {
            if (autoStart)
            {
                TryStartCapture();
            }
        }

        private void OnDisable()
        {
            StopCapture();
        }

        private void Update()
        {
            UpdateDeviceBinding();
            UpdateRms();
        }

        private void UpdateDeviceBinding()
        {
            if (Time.unscaledTime - _lastDevicePoll < devicePollInterval)
            {
                return;
            }

            _lastDevicePoll = Time.unscaledTime;

            if (!IsMicAvailable)
            {
                StopCapture();
                _currentDevice = string.Empty;
                return;
            }

            if (!string.IsNullOrEmpty(_currentDevice) && Microphone.devices.Contains(_currentDevice))
            {
                return;
            }

            _currentDevice = Microphone.devices.FirstOrDefault();
            if (autoStart && !IsCapturing)
            {
                TryStartCapture();
            }
        }

        public void TryStartCapture()
        {
            if (!IsMicAvailable)
            {
                return;
            }

            if (string.IsNullOrEmpty(_currentDevice))
            {
                _currentDevice = Microphone.devices.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(_currentDevice))
            {
                return;
            }

            StopCapture();
            _micClip = Microphone.Start(_currentDevice, true, Mathf.Max(1, bufferLengthSeconds), sampleRate);
            _isCapturing = true;
        }

        public void StopCapture()
        {
            if (!string.IsNullOrEmpty(_currentDevice) && Microphone.IsRecording(_currentDevice))
            {
                Microphone.End(_currentDevice);
            }

            _isCapturing = false;
            CurrentRms = 0f;
            RmsSmoothed = 0f;
        }

        private void UpdateRms()
        {
            if (!IsCapturing || _micClip == null)
            {
                CurrentRms = 0f;
                RmsSmoothed = Mathf.Lerp(RmsSmoothed, 0f, 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime * 60f));
                return;
            }

            int position = Microphone.GetPosition(_currentDevice);
            if (position < 0 || position < _sampleWindowSize)
            {
                return;
            }

            int startPosition = position - _sampleWindowSize;
            if (startPosition < 0)
            {
                startPosition += _micClip.samples;
            }

            _micClip.GetData(_sampleBuffer, startPosition);

            float sum = 0f;
            for (int i = 0; i < _sampleWindowSize; i++)
            {
                float sample = _sampleBuffer[i];
                sum += sample * sample;
            }

            float rms = Mathf.Sqrt(sum / _sampleWindowSize);
            CurrentRms = Mute ? 0f : rms;

            float lerpFactor = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime * 60f);
            RmsSmoothed = Mathf.Lerp(RmsSmoothed, CurrentRms, Mathf.Clamp01(lerpFactor));
        }
    }
}
