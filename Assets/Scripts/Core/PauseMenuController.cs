using UnityEngine;
using UnityEngine.UI;

namespace MalgarHotel.Core
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text optionsMessage;
        [SerializeField] private Text keybindHint;

        private void Awake()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(OnOptionsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            Hide();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPauseMenu(this);
            }
        }

        private void Update()
        {
            if (root != null && root.activeSelf && GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    OnResumeClicked();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    OnOptionsClicked();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    OnQuitClicked();
                }
            }
        }

        public void Show()
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (keybindHint != null)
            {
                keybindHint.text = "1-Resume  2-Options  3-Quit";
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            if (keybindHint != null)
            {
                keybindHint.text = string.Empty;
            }
        }

        public void ShowOptionsMessage()
        {
            if (optionsMessage != null)
            {
                optionsMessage.gameObject.SetActive(true);
                optionsMessage.text = "Options coming soon";
            }
        }

        private void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        private void OnOptionsClicked()
        {
            GameManager.Instance?.OnOptionsRequested();
        }

        private void OnQuitClicked()
        {
            GameManager.Instance?.ReturnToLobby();
        }
    }
}
