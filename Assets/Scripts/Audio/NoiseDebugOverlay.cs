using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MalgarHotel.Audio
{
    public class NoiseDebugOverlay : MonoBehaviour
    {
        [SerializeField] private MicInput micInput;
        [SerializeField] private MicCalibration micCalibration;
        [SerializeField] private KeyCode toggleKey = KeyCode.F9;
        [SerializeField] private Vector2 panelOffset = new Vector2(20f, 20f);
        [SerializeField] private bool startVisible;

        private bool _visible;
        private GUIStyle _style;
        private readonly List<NoiseEvent> _buffer = new List<NoiseEvent>();

        private void Awake()
        {
            _visible = startVisible;
        }

        private void OnEnable()
        {
            if (micInput == null)
            {
                micInput = FindObjectOfType<MicInput>();
            }

            if (micCalibration == null)
            {
                micCalibration = FindObjectOfType<MicCalibration>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                _visible = !_visible;
            }
        }

        private void OnGUI()
        {
            if (!_visible || Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true
                };
            }

            float rms = micInput != null ? micInput.CurrentRms : 0f;
            float smoothed = micInput != null ? micInput.RmsSmoothed : 0f;
            float normalized = micCalibration != null ? micCalibration.NormalizedLevel : 0f;
            MicBand band = micCalibration != null ? micCalibration.CurrentBand : MicBand.None;
            string deviceName = micInput != null ? micInput.DeviceName : string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("=== Noise Debug ===");
            sb.AppendLine($"Mic: {(micInput != null && micInput.IsMicAvailable ? "OK" : "Missing")}");
            sb.AppendLine($"Device: {(string.IsNullOrEmpty(deviceName) ? "<none>" : deviceName)}");
            sb.AppendLine($"RMS: {rms:F3} (smooth {smoothed:F3})");
            sb.AppendLine($"Normalized: {normalized:F3}");
            sb.AppendLine($"Band: {band}");

            _buffer.Clear();
            if (NoiseSystem.Instance != null)
            {
                NoiseSystem.Instance.GetRecentEvents(_buffer, 3f);
                sb.AppendLine($"Active events: {_buffer.Count}");
                _buffer.Sort((a, b) => b.Score.CompareTo(a.Score));

                Vector3 origin = transform.position;
                for (int i = 0; i < Mathf.Min(3, _buffer.Count); i++)
                {
                    var evt = _buffer[i];
                    float distance = Vector3.Distance(origin, evt.Position);
                    sb.AppendLine($"{i + 1}. {evt.Tag} I:{evt.Intensity:F2} P:{evt.Priority} D:{distance:F1}");
                }
            }
            else
            {
                sb.AppendLine("NoiseSystem missing");
            }

            string content = sb.ToString();
            Vector2 size = _style.CalcSize(new GUIContent(content));
            Rect rect = new Rect(panelOffset.x, panelOffset.y, Mathf.Max(260f, size.x + 24f), size.y + 24f);
            GUI.Box(rect, content, _style);
        }
    }
}
