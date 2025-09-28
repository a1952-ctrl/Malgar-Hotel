using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MalgarHotel.Core
{
    [RequireComponent(typeof(Canvas))]
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image backdrop;

        [Header("Navigation")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text keybindHint;

        [Header("Panels")]
        [SerializeField] private GameObject buttonsPanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Options Controls")]
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider ambienceVolumeSlider;
        [SerializeField] private Slider uiVolumeSlider;
        [SerializeField] private Button optionsBackButton;
        [SerializeField] private Text optionsHeader;
        [SerializeField] private Text mouseValueText;
        [SerializeField] private Text masterValueText;
        [SerializeField] private Text sfxValueText;
        [SerializeField] private Text ambienceValueText;
        [SerializeField] private Text uiValueText;

        private bool _optionsOpen;
        private bool _suppressSettingEvents;

        public bool IsOptionsOpen => _optionsOpen;

        private void Awake()
        {
            EnsureRuntimeUi();
            HookupButtons();
            HideImmediate();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPauseMenu(this);
                GameManager.Instance.MouseSensitivityChanged += OnMouseSensitivityChanged;
                GameManager.Instance.VolumeChanged += OnVolumeChanged;
                GameManager.Instance.PauseStateChanged += OnPauseStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(OnResumeClicked);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.RemoveListener(OnOptionsClicked);
            }

            if (optionsBackButton != null)
            {
                optionsBackButton.onClick.RemoveListener(CloseOptionsPanel);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
            }

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSliderChanged);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            }

            if (ambienceVolumeSlider != null)
            {
                ambienceVolumeSlider.onValueChanged.RemoveListener(OnAmbienceVolumeChanged);
            }

            if (uiVolumeSlider != null)
            {
                uiVolumeSlider.onValueChanged.RemoveListener(OnUiVolumeChanged);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.MouseSensitivityChanged -= OnMouseSensitivityChanged;
                GameManager.Instance.VolumeChanged -= OnVolumeChanged;
                GameManager.Instance.PauseStateChanged -= OnPauseStateChanged;
            }
        }

        private void EnsureRuntimeUi()
        {
            if (root == null)
            {
                root = gameObject;
            }

            var rootRect = root.GetComponent<RectTransform>();
            if (rootRect == null)
            {
                rootRect = root.AddComponent<RectTransform>();
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            if (backdrop == null)
            {
                backdrop = GetComponentInChildren<Image>();
                if (backdrop == null || backdrop.transform == transform)
                {
                    var imageGo = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    imageGo.transform.SetParent(transform, false);
                    var rect = imageGo.GetComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    backdrop = imageGo.GetComponent<Image>();
                    backdrop.color = new Color(0f, 0f, 0f, 0.35f);
                }
                else
                {
                    backdrop.color = new Color(0f, 0f, 0f, 0.35f);
                }
            }

            if (buttonsPanel == null)
            {
                var existing = transform.Find("MenuRoot");
                if (existing != null)
                {
                    buttonsPanel = existing.gameObject;
                }
            }

            if (buttonsPanel == null)
            {
                buttonsPanel = CreatePanel("MenuRoot", new Vector2(0.5f, 0.5f), new Vector2(640f, 360f));
            }

            if (optionsPanel == null)
            {
                optionsPanel = transform.Find("OptionsPanel")?.gameObject;
            }

            if (optionsPanel == null)
            {
                optionsPanel = CreatePanel("OptionsPanel", new Vector2(0.5f, 0.5f), new Vector2(680f, 420f));
            }

            optionsPanel.SetActive(false);

            if (keybindHint == null)
            {
                keybindHint = FindOrCreateText("KeybindHint", buttonsPanel.transform, new Vector2(0.5f, 0.08f), new Vector2(360f, 32f));
                keybindHint.alignment = TextAnchor.MiddleCenter;
                keybindHint.color = new Color(0.8f, 0.86f, 0.96f, 0.9f);
            }

            var titleText = buttonsPanel.transform.Find("TitleText")?.GetComponent<Text>();
            if (titleText == null)
            {
                titleText = FindOrCreateText("TitleText", buttonsPanel.transform, new Vector2(0.5f, 0.85f), new Vector2(400f, 64f));
                titleText.alignment = TextAnchor.MiddleCenter;
            }
            titleText.text = "Paused";
            titleText.fontSize = 42;
            titleText.color = new Color(0.9f, 0.95f, 1f, 1f);

            resumeButton = EnsureButton(resumeButton, buttonsPanel.transform, "ResumeButton", "Resume", new Vector2(0.5f, 0.6f));
            optionsButton = EnsureButton(optionsButton, buttonsPanel.transform, "OptionsButton", "Options", new Vector2(0.5f, 0.45f));
            quitButton = EnsureButton(quitButton, buttonsPanel.transform, "QuitButton", "Quit", new Vector2(0.5f, 0.3f));

            foreach (Transform child in buttonsPanel.transform)
            {
                if (child == null || child.gameObject == resumeButton.gameObject || child.gameObject == optionsButton.gameObject || child.gameObject == quitButton.gameObject)
                {
                    continue;
                }

                if (child.name.Contains("Options") || child.name.Contains("Resume"))
                {
                    child.gameObject.SetActive(false);
                }
            }

            if (optionsHeader == null)
            {
                optionsHeader = FindOrCreateText("OptionsHeader", optionsPanel.transform, new Vector2(0.5f, 0.88f), new Vector2(420f, 60f));
                optionsHeader.alignment = TextAnchor.MiddleCenter;
                optionsHeader.fontSize = 36;
                optionsHeader.text = "Options";
                optionsHeader.color = new Color(0.9f, 0.95f, 1f, 1f);
            }

            var range = GameManager.Instance != null ? GameManager.Instance.MouseSensitivityRange : new Vector2(0.1f, 2f);
            var sliderInfo = new List<(Slider, Text)>();
            if (sliderInfo != null && sliderInfo.Count > 0)
            {
                sliderInfo.Clear();
            }

            (mouseSensitivitySlider, mouseValueText) = EnsureOptionSlider(mouseSensitivitySlider, mouseValueText, optionsPanel.transform, "MouseSensitivity", "Mouse Sensitivity", range.x, range.y, 0);
            (masterVolumeSlider, masterValueText) = EnsureOptionSlider(masterVolumeSlider, masterValueText, optionsPanel.transform, "MasterVolume", "Master Volume", 0f, 1f, 1);
            (sfxVolumeSlider, sfxValueText) = EnsureOptionSlider(sfxVolumeSlider, sfxValueText, optionsPanel.transform, "SFXVolume", "SFX Volume", 0f, 1f, 2);
            (ambienceVolumeSlider, ambienceValueText) = EnsureOptionSlider(ambienceVolumeSlider, ambienceValueText, optionsPanel.transform, "AmbienceVolume", "Ambience Volume", 0f, 1f, 3);
            (uiVolumeSlider, uiValueText) = EnsureOptionSlider(uiVolumeSlider, uiValueText, optionsPanel.transform, "UIVolume", "UI Volume", 0f, 1f, 4);

            if (optionsBackButton == null)
            {
                optionsBackButton = EnsureButton(null, optionsPanel.transform, "BackButton", "Back", new Vector2(0.5f, 0.1f));
            }
        }

        private void HookupButtons()
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
            optionsButton.onClick.AddListener(OnOptionsClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
            optionsBackButton.onClick.AddListener(CloseOptionsPanel);

            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSliderChanged);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            ambienceVolumeSlider.onValueChanged.AddListener(OnAmbienceVolumeChanged);
            uiVolumeSlider.onValueChanged.AddListener(OnUiVolumeChanged);
        }

        private void HideImmediate()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private GameObject CreatePanel(string name, Vector2 pivot, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.06f, 0.08f, 0.12f, 0.92f);
            return go;
        }

        private Text FindOrCreateText(string name, Transform parent, Vector2 anchor, Vector2 size)
        {
            var existing = parent.Find(name)?.GetComponent<Text>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        private Button EnsureButton(Button existing, Transform parent, string name, string label, Vector2 anchor)
        {
            if (existing != null)
            {
                var labelText = existing.GetComponentInChildren<Text>();
                if (labelText != null)
                {
                    labelText.text = label;
                }
                return existing;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(280f, 56f);
            rect.anchoredPosition = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.12f, 0.16f, 0.22f, 0.92f);

            var text = FindOrCreateText("Label", go.transform, new Vector2(0.5f, 0.5f), rect.sizeDelta - new Vector2(20f, 20f));
            text.text = label;
            text.fontSize = 24;

            return go.GetComponent<Button>();
        }

        private (Slider slider, Text valueText) EnsureOptionSlider(Slider existing, Text valueLabel, Transform parent, string name, string label, float min, float max, int order)
        {
            if (existing != null && valueLabel != null)
            {
                existing.minValue = min;
                existing.maxValue = max;
                return (existing, valueLabel);
            }

            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rect = row.GetComponent<RectTransform>();
            float top = 0.75f - order * 0.14f;
            rect.anchorMin = new Vector2(0.08f, top - 0.07f);
            rect.anchorMax = new Vector2(0.92f, top + 0.07f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var labelText = FindOrCreateText("Label", row.transform, new Vector2(0.1f, 0.5f), new Vector2(260f, 44f));
            labelText.text = label;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.fontSize = 22;
            labelText.color = new Color(0.85f, 0.93f, 1f, 0.95f);

            var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
            sliderGo.transform.SetParent(row.transform, false);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.38f, 0.25f);
            sliderRect.anchorMax = new Vector2(0.98f, 0.75f);
            sliderRect.offsetMin = sliderRect.offsetMax = Vector2.zero;

            var background = sliderGo.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.35f);

            var slider = sliderGo.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.direction = Slider.Direction.LeftToRight;

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(10f, 0f);
            fillAreaRect.offsetMax = new Vector2(-10f, 0f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.4f, 0.74f, 1f, 0.85f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;

            var handleArea = new GameObject("HandleSlideArea", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(24f, 24f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = new Color(0.95f, 0.97f, 1f, 1f);

            slider.fillRect = fillRect;
            slider.targetGraphic = handleImage;
            slider.handleRect = handleRect;

            var valueText = FindOrCreateText("Value", row.transform, new Vector2(0.86f, 0.8f), new Vector2(120f, 36f));
            valueText.alignment = TextAnchor.UpperRight;
            valueText.fontSize = 18;
            valueText.color = new Color(0.8f, 0.9f, 1f, 0.85f);

            return (slider, valueText);
        }

        private void OnPauseStateChanged(bool paused)
        {
            if (!paused)
            {
                ToggleOptionsPanel(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void Show()
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            ToggleOptionsPanel(false);
            RefreshSettingsUi();
            UpdateHint();
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            ToggleOptionsPanel(false);
        }

        public void ShowOptionsMessage()
        {
            OpenOptionsPanel();
        }

        public bool TryCloseOptionsPanel()
        {
            if (!_optionsOpen)
            {
                return false;
            }

            CloseOptionsPanel();
            return true;
        }

        private void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        private void OnOptionsClicked()
        {
            OpenOptionsPanel();
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OpenOptionsPanel()
        {
            ToggleOptionsPanel(true);
            RefreshSettingsUi();
        }

        private void CloseOptionsPanel()
        {
            ToggleOptionsPanel(false);
            UpdateHint();
        }

        private void ToggleOptionsPanel(bool open)
        {
            _optionsOpen = open;
            if (buttonsPanel != null)
            {
                buttonsPanel.SetActive(!open);
            }

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(open);
            }

            UpdateHint();
        }

        private void UpdateHint()
        {
            if (keybindHint == null)
            {
                return;
            }

            keybindHint.text = _optionsOpen ? "Esc - Back" : "Esc - Resume";
        }

        private void RefreshSettingsUi()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            _suppressSettingEvents = true;

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.minValue = GameManager.Instance.MouseSensitivityRange.x;
                mouseSensitivitySlider.maxValue = GameManager.Instance.MouseSensitivityRange.y;
                mouseSensitivitySlider.value = GameManager.Instance.MouseSensitivity;
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = GameManager.Instance.MasterVolume;
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = GameManager.Instance.SfxVolume;
            }

            if (ambienceVolumeSlider != null)
            {
                ambienceVolumeSlider.value = GameManager.Instance.AmbienceVolume;
            }

            if (uiVolumeSlider != null)
            {
                uiVolumeSlider.value = GameManager.Instance.UiVolume;
            }

            _suppressSettingEvents = false;

            UpdateValueLabel(mouseValueText, mouseSensitivitySlider?.value ?? 0f, false);
            UpdateValueLabel(masterValueText, masterVolumeSlider?.value ?? 0f, true);
            UpdateValueLabel(sfxValueText, sfxVolumeSlider?.value ?? 0f, true);
            UpdateValueLabel(ambienceValueText, ambienceVolumeSlider?.value ?? 0f, true);
            UpdateValueLabel(uiValueText, uiVolumeSlider?.value ?? 0f, true);
        }

        private void OnMouseSensitivityChanged(float value)
        {
            if (mouseSensitivitySlider == null)
            {
                return;
            }

            _suppressSettingEvents = true;
            mouseSensitivitySlider.value = value;
            _suppressSettingEvents = false;
            UpdateValueLabel(mouseValueText, value, false);
        }

        private void OnVolumeChanged(VolumeChannel channel, float value)
        {
            _suppressSettingEvents = true;
            switch (channel)
            {
                case VolumeChannel.Master:
                    if (masterVolumeSlider != null)
                    {
                        masterVolumeSlider.value = value;
                        UpdateValueLabel(masterValueText, value, true);
                    }
                    break;
                case VolumeChannel.Sfx:
                    if (sfxVolumeSlider != null)
                    {
                        sfxVolumeSlider.value = value;
                        UpdateValueLabel(sfxValueText, value, true);
                    }
                    break;
                case VolumeChannel.Ambience:
                    if (ambienceVolumeSlider != null)
                    {
                        ambienceVolumeSlider.value = value;
                        UpdateValueLabel(ambienceValueText, value, true);
                    }
                    break;
                case VolumeChannel.Ui:
                    if (uiVolumeSlider != null)
                    {
                        uiVolumeSlider.value = value;
                        UpdateValueLabel(uiValueText, value, true);
                    }
                    break;
            }
            _suppressSettingEvents = false;
        }

        private void OnMouseSliderChanged(float value)
        {
            if (_suppressSettingEvents)
            {
                return;
            }

            GameManager.Instance?.SetMouseSensitivity(value);
            UpdateValueLabel(mouseValueText, value, false);
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (_suppressSettingEvents)
            {
                return;
            }

            GameManager.Instance?.SetVolume(VolumeChannel.Master, value);
            UpdateValueLabel(masterValueText, value, true);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (_suppressSettingEvents)
            {
                return;
            }

            GameManager.Instance?.SetVolume(VolumeChannel.Sfx, value);
            UpdateValueLabel(sfxValueText, value, true);
        }

        private void OnAmbienceVolumeChanged(float value)
        {
            if (_suppressSettingEvents)
            {
                return;
            }

            GameManager.Instance?.SetVolume(VolumeChannel.Ambience, value);
            UpdateValueLabel(ambienceValueText, value, true);
        }

        private void OnUiVolumeChanged(float value)
        {
            if (_suppressSettingEvents)
            {
                return;
            }

            GameManager.Instance?.SetVolume(VolumeChannel.Ui, value);
            UpdateValueLabel(uiValueText, value, true);
        }

        private static void UpdateValueLabel(Text label, float value, bool asPercent)
        {
            if (label == null)
            {
                return;
            }

            label.text = asPercent ? $"{Mathf.RoundToInt(value * 100f)}%" : value.ToString("0.00");
        }
    }
}
