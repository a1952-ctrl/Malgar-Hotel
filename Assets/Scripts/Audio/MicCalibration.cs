using System;
using UnityEngine;

namespace MalgarHotel.Audio
{
    public enum MicBand
    {
        None,
        Low,
        Mid,
        High
    }

    /// <summary>
    /// Maps microphone RMS values to normalized 0..1 levels and discrete bands while
    /// persisting calibration thresholds for future sessions.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-200)]
    public class MicCalibration : MonoBehaviour
    {
        private const string PrefLow = "MalgarHotel.Mic.Low";
        private const string PrefMid = "MalgarHotel.Mic.Mid";
        private const string PrefHigh = "MalgarHotel.Mic.High";
        private const float MinBandGap = 0.005f;

        [SerializeField] private MicInput micInput;
        [SerializeField] private float thresholdLow = 0.02f;
        [SerializeField] private float thresholdMid = 0.06f;
        [SerializeField] private float thresholdHigh = 0.12f;

        public event Action<float> OnMicLevelChanged;
        public event Action<MicBand> OnMicBandChanged;

        public float ThresholdLow
        {
            get => thresholdLow;
            set
            {
                thresholdLow = Mathf.Max(0f, value);
                EnforceOrdering();
                SavePreferences();
            }
        }

        public float ThresholdMid
        {
            get => thresholdMid;
            set
            {
                thresholdMid = Mathf.Max(0f, value);
                EnforceOrdering();
                SavePreferences();
            }
        }

        public float ThresholdHigh
        {
            get => thresholdHigh;
            set
            {
                thresholdHigh = Mathf.Max(0f, value);
                EnforceOrdering();
                SavePreferences();
            }
        }

        public float NormalizedLevel { get; private set; }
        public MicBand CurrentBand { get; private set; } = MicBand.None;

        private void Awake()
        {
            if (micInput == null)
            {
                micInput = GetComponent<MicInput>();
            }

            LoadPreferences();
            EnforceOrdering();
        }

        private void Update()
        {
            float smoothed = micInput != null ? micInput.RmsSmoothed : 0f;
            NormalizedLevel = Normalize(smoothed);
            OnMicLevelChanged?.Invoke(NormalizedLevel);

            MicBand newBand = EvaluateBand(smoothed);
            if (newBand != CurrentBand)
            {
                CurrentBand = newBand;
                OnMicBandChanged?.Invoke(CurrentBand);
            }
        }

        public void ApplyThresholdLow(float value)
        {
            ThresholdLow = value;
            AdjustMidForLow();
            ClampHigh();
            RaiseThresholdEvents();
        }

        public void ApplyThresholdMid(float value)
        {
            ThresholdMid = value;
            ClampMid();
            ClampHigh();
            RaiseThresholdEvents();
        }

        public void ApplyThresholdHigh(float value)
        {
            ThresholdHigh = value;
            ClampHigh();
            RaiseThresholdEvents();
        }

        public void SetMute(bool mute)
        {
            if (micInput != null)
            {
                micInput.Mute = mute;
            }
        }

        private float Normalize(float rms)
        {
            if (thresholdHigh <= 0f)
            {
                return 0f;
            }

            if (rms <= thresholdLow)
            {
                return 0f;
            }

            float range = Mathf.Max(MinBandGap, thresholdHigh - thresholdLow);
            return Mathf.Clamp01((rms - thresholdLow) / range);
        }

        private MicBand EvaluateBand(float rms)
        {
            if (micInput != null && micInput.Mute)
            {
                return MicBand.None;
            }

            if (rms >= thresholdHigh)
            {
                return MicBand.High;
            }

            if (rms >= thresholdMid)
            {
                return MicBand.Mid;
            }

            if (rms >= thresholdLow)
            {
                return MicBand.Low;
            }

            return MicBand.None;
        }

        private void EnforceOrdering()
        {
            thresholdLow = Mathf.Max(0f, thresholdLow);
            thresholdMid = Mathf.Max(thresholdLow + MinBandGap, thresholdMid);
            thresholdHigh = Mathf.Max(thresholdMid + MinBandGap, thresholdHigh);
        }

        private void ClampMid()
        {
            thresholdMid = Mathf.Clamp(thresholdMid, thresholdLow + MinBandGap, thresholdHigh - MinBandGap);
        }

        private void ClampHigh()
        {
            thresholdHigh = Mathf.Max(thresholdMid + MinBandGap, thresholdHigh);
        }

        private void AdjustMidForLow()
        {
            if (thresholdMid <= thresholdLow)
            {
                thresholdMid = thresholdLow + MinBandGap;
            }
        }

        private void LoadPreferences()
        {
            if (PlayerPrefs.HasKey(PrefLow))
            {
                thresholdLow = PlayerPrefs.GetFloat(PrefLow, thresholdLow);
            }

            if (PlayerPrefs.HasKey(PrefMid))
            {
                thresholdMid = PlayerPrefs.GetFloat(PrefMid, thresholdMid);
            }

            if (PlayerPrefs.HasKey(PrefHigh))
            {
                thresholdHigh = PlayerPrefs.GetFloat(PrefHigh, thresholdHigh);
            }
        }

        private void SavePreferences()
        {
            PlayerPrefs.SetFloat(PrefLow, thresholdLow);
            PlayerPrefs.SetFloat(PrefMid, thresholdMid);
            PlayerPrefs.SetFloat(PrefHigh, thresholdHigh);
            PlayerPrefs.Save();
        }

        private void RaiseThresholdEvents()
        {
            float smoothed = micInput != null ? micInput.RmsSmoothed : 0f;
            NormalizedLevel = Normalize(smoothed);
            OnMicLevelChanged?.Invoke(NormalizedLevel);

            MicBand newBand = EvaluateBand(smoothed);
            CurrentBand = newBand;
            OnMicBandChanged?.Invoke(CurrentBand);
            SavePreferences();
        }
    }
}
