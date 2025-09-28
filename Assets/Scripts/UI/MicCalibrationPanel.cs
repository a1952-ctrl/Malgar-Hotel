using System;
using MalgarHotel.Audio;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MalgarHotel.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class MicCalibrationPanel : MonoBehaviour
    {
        [SerializeField] private MicCalibration micCalibration;
        [SerializeField] private MicInput micInput;
        [SerializeField] private string panelTitle = "Mic & Noise";
        [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private bool buildOnAwake = true;

        private Slider _levelSlider;
        private Slider _lowSlider;
        private Slider _midSlider;
        private Slider _highSlider;
        private Toggle _muteToggle;
        private Text _statusText;
        private Text _helpText;
        private bool _initialised;
        private bool _updatingUI;

        private void Awake()
        {
            if (buildOnAwake)
            {
                BuildUI();
            }
        }

        private void OnEnable()
        {
            EnsureReferences();
            RegisterCallbacks();
            RefreshUI();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void Update()
        {
            if (!_initialised)
            {
                return;
            }

            EnsureReferences();
            UpdateLevelBar();
            UpdateStatus();
        }

        private void EnsureReferences()
        {
            if (micCalibration == null)
            {
                micCalibration = FindObjectOfType<MicCalibration>();
            }

            if (micInput == null)
            {
                micInput = FindObjectOfType<MicInput>();
            }
        }

        private void RegisterCallbacks()
        {
            if (micCalibration != null)
            {
                micCalibration.OnMicLevelChanged += OnMicLevelChanged;
                micCalibration.OnMicBandChanged += OnMicBandChanged;
            }
        }

        private void UnregisterCallbacks()
        {
            if (micCalibration != null)
            {
                micCalibration.OnMicLevelChanged -= OnMicLevelChanged;
                micCalibration.OnMicBandChanged -= OnMicBandChanged;
            }
        }

        private void OnMicLevelChanged(float _) => UpdateLevelBar();

        private void OnMicBandChanged(MicBand _) => UpdateStatus();

        private void BuildUI()
        {
            if (_initialised)
            {
                return;
            }

            var rect = (RectTransform)transform;
            rect.pivot = new Vector2(0.5f, 0.5f);

            var image = GetComponent<Image>();
            if (image == null)
            {
                image = gameObject.AddComponent<Image>();
            }

            image.color = panelColor;
            var layout = GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.spacing = 8f;
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            var fitter = GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            CreateText("Title", panelTitle, 28, font, FontStyle.Bold, new Color(0.85f, 0.95f, 1f, 1f));
            _statusText = CreateText("Status", string.Empty, 18, font, FontStyle.Normal, new Color(0.8f, 0.9f, 1f, 1f));

            _levelSlider = CreateLabeledSlider("Nivel RMS", font, 1f, false, null);

            _lowSlider = CreateLabeledSlider("Umbral Low", font, 0.3f, true, value =>
            {
                if (!_updatingUI && micCalibration != null)
                {
                    micCalibration.ApplyThresholdLow(value);
                    RefreshSliderValues();
                }
            });

            _midSlider = CreateLabeledSlider("Umbral Mid", font, 0.3f, true, value =>
            {
                if (!_updatingUI && micCalibration != null)
                {
                    micCalibration.ApplyThresholdMid(value);
                    RefreshSliderValues();
                }
            });

            _highSlider = CreateLabeledSlider("Umbral High", font, 0.3f, true, value =>
            {
                if (!_updatingUI && micCalibration != null)
                {
                    micCalibration.ApplyThresholdHigh(value);
                    RefreshSliderValues();
                }
            });

            _muteToggle = CreateToggleRow("Mutear micrófono", font, value =>
            {
                if (!_updatingUI && micCalibration != null)
                {
                    micCalibration.SetMute(value);
                }
            });

            _helpText = CreateText("Help", "Susurro → Low, voz normal → Mid, grito → High", 16, font, FontStyle.Italic, new Color(0.7f, 0.8f, 0.95f, 1f));
            var helpLayout = _helpText.GetComponent<LayoutElement>();
            helpLayout.minWidth = 320f;

            _initialised = true;
        }

        private Text CreateText(string name, string content, int size, Font font, FontStyle style, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rect = go.AddComponent<RectTransform>();
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var layout = go.AddComponent<LayoutElement>();
            layout.minHeight = size + 6f;
            layout.flexibleWidth = 1f;
            return text;
        }

        private Slider CreateLabeledSlider(string label, Font font, float maxValue, bool interactable, UnityAction<float> callback)
        {
            GameObject row = new GameObject(label + "Row");
            row.transform.SetParent(transform, false);
            var rowRect = row.AddComponent<RectTransform>();
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            var labelText = new GameObject("Label");
            labelText.transform.SetParent(row.transform, false);
            var labelRect = labelText.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(130f, 24f);
            var text = labelText.AddComponent<Text>();
            text.text = label;
            text.font = font;
            text.fontSize = 17;
            text.color = new Color(0.85f, 0.93f, 1f, 1f);
            text.alignment = TextAnchor.MiddleLeft;

            var labelLayout = labelText.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 140f;
            labelLayout.minHeight = 26f;

            Slider slider = CreateSlider(row.transform, maxValue, interactable);
            if (callback != null)
            {
                slider.onValueChanged.AddListener(callback);
            }

            var sliderLayout = slider.gameObject.AddComponent<LayoutElement>();
            sliderLayout.preferredWidth = 260f;
            sliderLayout.minHeight = 26f;
            sliderLayout.flexibleWidth = 1f;

            return slider;
        }

        private Slider CreateSlider(Transform parent, float maxValue, bool interactable)
        {
            GameObject go = new GameObject("Slider");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, 24f);

            var slider = go.AddComponent<Slider>();
            slider.interactable = interactable;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = maxValue;

            var background = new GameObject("Background");
            background.transform.SetParent(go.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(4f, 4f);
            fillAreaRect.offsetMax = new Vector2(-4f, -4f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillAreaRect, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.35f, 0.7f, 1f, 0.9f);

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.handleRect = null;

            return slider;
        }

        private Toggle CreateToggleRow(string label, Font font, UnityAction<bool> callback)
        {
            GameObject row = new GameObject(label + "Row");
            row.transform.SetParent(transform, false);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;

            GameObject toggleRoot = new GameObject("Toggle");
            toggleRoot.transform.SetParent(row.transform, false);
            var toggleRect = toggleRoot.AddComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(26f, 26f);

            var toggle = toggleRoot.AddComponent<Toggle>();

            var background = new GameObject("Background");
            background.transform.SetParent(toggleRoot.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            var checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(background.transform, false);
            var checkRect = checkmark.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            var checkImage = checkmark.AddComponent<Image>();
            checkImage.color = new Color(0.7f, 0.95f, 0.3f, 0.95f);

            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            if (callback != null)
            {
                toggle.onValueChanged.AddListener(callback);
            }

            var toggleLayout = toggleRoot.AddComponent<LayoutElement>();
            toggleLayout.preferredWidth = 30f;
            toggleLayout.minHeight = 26f;

            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(row.transform, false);
            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(240f, 24f);
            var labelText = labelObject.AddComponent<Text>();
            labelText.text = label;
            labelText.font = font;
            labelText.fontSize = 17;
            labelText.color = new Color(0.85f, 0.93f, 1f, 1f);
            labelText.alignment = TextAnchor.MiddleLeft;

            var labelLayout = labelObject.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1f;
            labelLayout.minHeight = 26f;

            return toggle;
        }

        private void RefreshUI()
        {
            if (!_initialised)
            {
                BuildUI();
            }

            RefreshSliderValues();
            UpdateLevelBar();
            UpdateStatus();
            UpdateMuteToggle();
        }

        private void RefreshSliderValues()
        {
            if (micCalibration == null)
            {
                return;
            }

            _updatingUI = true;
            _lowSlider.value = micCalibration.ThresholdLow;
            _midSlider.value = micCalibration.ThresholdMid;
            _highSlider.value = micCalibration.ThresholdHigh;
            _updatingUI = false;
        }

        private void UpdateLevelBar()
        {
            if (_levelSlider != null && micCalibration != null)
            {
                _levelSlider.value = Mathf.Clamp01(micCalibration.NormalizedLevel);
            }
        }

        private void UpdateStatus()
        {
            if (_statusText == null)
            {
                return;
            }

            string availability;
            if (micInput == null || !micInput.IsMicAvailable)
            {
                availability = "Micrófono no disponible";
            }
            else if (!micInput.IsCapturing)
            {
                availability = "Micrófono listo";
            }
            else
            {
                availability = string.IsNullOrEmpty(micInput.DeviceName) ? "Capturando" : $"Capturando ({micInput.DeviceName})";
            }

            string band = micCalibration != null ? micCalibration.CurrentBand.ToString() : "None";
            _statusText.text = $"{availability} · Banda actual: {band}";
        }

        private void UpdateMuteToggle()
        {
            if (_muteToggle == null)
            {
                return;
            }

            bool muted = micInput != null && micInput.Mute;
            _updatingUI = true;
            _muteToggle.isOn = muted;
            _updatingUI = false;
        }
    }
}
