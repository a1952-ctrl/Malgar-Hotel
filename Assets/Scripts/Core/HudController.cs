using UnityEngine;
using UnityEngine.UI;

namespace MalgarHotel.Core
{
    public class HudController : MonoBehaviour
    {
        [Header("Battery")]
        [SerializeField] private Slider batterySlider;
        [SerializeField] private Image batteryFill;
        [SerializeField] private Text batteryText;

        [Header("Objective")]
        [SerializeField] private Text fuseText;

        [Header("Interaction")]
        [SerializeField] private Text interactPromptText;

        private void Awake()
        {
            EnsureBatteryUi();
            HidePrompt();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BatteryChanged += OnBatteryChanged;
                GameManager.Instance.FuseCountChanged += OnFuseChanged;
                OnFuseChanged(GameManager.Instance.CollectedFuses, GameManager.Instance.RequiredFuses);
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BatteryChanged -= OnBatteryChanged;
                GameManager.Instance.FuseCountChanged -= OnFuseChanged;
            }
        }

        private void OnBatteryChanged(float current, float max)
        {
            float normalized = Mathf.Approximately(max, 0f) ? 0f : Mathf.Clamp01(current / max);

            if (batterySlider != null)
            {
                batterySlider.value = normalized;
            }

            if (batteryFill != null)
            {
                batteryFill.fillAmount = normalized;
                batteryFill.color = Color.Lerp(new Color(0.8f, 0.25f, 0.2f), new Color(0.25f, 0.8f, 0.35f), normalized);
            }

            if (batteryText != null)
            {
                batteryText.text = $"{Mathf.RoundToInt(normalized * 100f)}%";
            }
        }

        private void OnFuseChanged(int collected, int required)
        {
            if (fuseText != null)
            {
                fuseText.text = $"Fuses {collected}/{required}";
            }
        }

        public void ShowPrompt(string text)
        {
            if (interactPromptText == null)
            {
                return;
            }

            interactPromptText.text = text;
            interactPromptText.gameObject.SetActive(true);
        }

        public void HidePrompt()
        {
            if (interactPromptText == null)
            {
                return;
            }

            interactPromptText.text = string.Empty;
            interactPromptText.gameObject.SetActive(false);
        }

        private void EnsureBatteryUi()
        {
            if (batterySlider == null)
            {
                batterySlider = GetComponentInChildren<Slider>(true);
            }

            if (batterySlider == null)
            {
                var sliderObject = new GameObject("BatteryBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
                sliderObject.transform.SetParent(transform, false);
                var sliderRect = sliderObject.GetComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0.02f, 0.02f);
                sliderRect.anchorMax = new Vector2(0.3f, 0.07f);
                sliderRect.offsetMin = Vector2.zero;
                sliderRect.offsetMax = Vector2.zero;
                sliderRect.pivot = new Vector2(0f, 0f);

                var sliderBackground = sliderObject.GetComponent<Image>();
                sliderBackground.color = new Color(0f, 0f, 0f, 0.55f);

                batterySlider = sliderObject.GetComponent<Slider>();
                batterySlider.direction = Slider.Direction.LeftToRight;
                batterySlider.interactable = false;
                batterySlider.minValue = 0f;
                batterySlider.maxValue = 1f;
                batterySlider.value = 1f;

                var fillRoot = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fillRoot.transform.SetParent(sliderObject.transform, false);
                var fillRect = fillRoot.GetComponent<RectTransform>();
                fillRect.anchorMin = new Vector2(0.02f, 0.2f);
                fillRect.anchorMax = new Vector2(0.98f, 0.8f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;

                batteryFill = fillRoot.GetComponent<Image>();
                batteryFill.color = new Color(0.25f, 0.8f, 0.35f, 0.9f);
                batteryFill.type = Image.Type.Filled;
                batteryFill.fillMethod = Image.FillMethod.Horizontal;
                batteryFill.fillOrigin = (int)Image.OriginHorizontal.Left;

                var sliderFill = new GameObject("FillArea", typeof(RectTransform));
                sliderFill.transform.SetParent(sliderObject.transform, false);
                var sliderFillRect = sliderFill.GetComponent<RectTransform>();
                sliderFillRect.anchorMin = Vector2.zero;
                sliderFillRect.anchorMax = Vector2.one;
                sliderFillRect.offsetMin = new Vector2(4f, 4f);
                sliderFillRect.offsetMax = new Vector2(-4f, -4f);

                fillRoot.transform.SetParent(sliderFill.transform, false);

                batterySlider.fillRect = fillRect;
                batterySlider.targetGraphic = batteryFill;
            }

            if (batteryText == null && batterySlider != null)
            {
                var textObject = new GameObject("BatteryLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObject.transform.SetParent(batterySlider.transform, false);
                var textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 1f);
                textRect.anchorMax = new Vector2(0.5f, 1f);
                textRect.anchoredPosition = new Vector2(0f, 28f);
                textRect.sizeDelta = new Vector2(160f, 40f);

                batteryText = textObject.GetComponent<Text>();
                batteryText.alignment = TextAnchor.UpperCenter;
                batteryText.fontSize = 18;
                batteryText.text = "100%";
                batteryText.color = new Color(0.85f, 0.95f, 1f, 0.9f);
            }
        }
    }
}
