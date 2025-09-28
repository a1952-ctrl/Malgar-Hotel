using UnityEngine;
using UnityEngine.UI;

namespace MalgarHotel.Core
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private Text batteryText;
        [SerializeField] private Text fuseText;
        [SerializeField] private Text interactPromptText;

        private void Awake()
        {
            if (interactPromptText != null)
            {
                interactPromptText.text = string.Empty;
            }
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
            if (batteryText == null)
            {
                return;
            }

            float percent = Mathf.Approximately(max, 0f) ? 0f : current / max;
            batteryText.text = $"Battery: {(int)current}/{(int)max} ({percent:P0})";
        }

        private void OnFuseChanged(int collected, int required)
        {
            if (fuseText != null)
            {
                fuseText.text = $"Fuses: {collected}/{required}";
            }
        }

        public void ShowPrompt(string text)
        {
            if (interactPromptText == null)
            {
                return;
            }

            interactPromptText.text = text;
        }

        public void HidePrompt()
        {
            if (interactPromptText == null)
            {
                return;
            }

            interactPromptText.text = string.Empty;
        }
    }
}
