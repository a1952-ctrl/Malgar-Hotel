using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalgarHotel.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Objective")]
        [SerializeField] private int requiredFuses = 5;
        [SerializeField] private float autoReturnToLobbyDelay = 4f;
        [SerializeField] private string lobbySceneName = "Lobby";

        [Header("UI")]
        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private PauseMenuController pauseMenu;

        private int _collectedFuses;
        private bool _isPaused;
        private bool _isGameComplete;
        private float _victoryTimer;

        public event Action<int, int> FuseCountChanged;
        public event Action<float, float> BatteryChanged;

        public int CollectedFuses => _collectedFuses;
        public int RequiredFuses => requiredFuses;
        public bool IsPaused => _isPaused;
        public bool IsGameComplete => _isGameComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (victoryScreen != null)
            {
                victoryScreen.SetActive(false);
            }

            FuseCountChanged?.Invoke(_collectedFuses, requiredFuses);
        }

        private void Update()
        {
            if (_isGameComplete)
            {
                if (victoryScreen != null && !victoryScreen.activeSelf)
                {
                    victoryScreen.SetActive(true);
                }

                if (autoReturnToLobbyDelay > 0f)
                {
                    _victoryTimer += Time.deltaTime;
                    if (_victoryTimer >= autoReturnToLobbyDelay)
                    {
                        ReturnToLobby();
                    }
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        public void RegisterPauseMenu(PauseMenuController menu)
        {
            pauseMenu = menu;
        }

        public void NotifyBatteryChanged(float current, float max)
        {
            BatteryChanged?.Invoke(current, max);
        }

        public void RegisterFuseCollected()
        {
            _collectedFuses = Mathf.Clamp(_collectedFuses + 1, 0, requiredFuses);
            FuseCountChanged?.Invoke(_collectedFuses, requiredFuses);

            if (_collectedFuses >= requiredFuses)
            {
                OnObjectiveComplete();
            }
        }

        public void ResetFuses()
        {
            _collectedFuses = 0;
            FuseCountChanged?.Invoke(_collectedFuses, requiredFuses);
        }

        public void PauseGame()
        {
            if (_isGameComplete)
            {
                return;
            }

            _isPaused = true;
            Time.timeScale = 0f;
            if (pauseMenu != null)
            {
                pauseMenu.Show();
            }
        }

        public void ResumeGame()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            if (pauseMenu != null)
            {
                pauseMenu.Hide();
            }
        }

        public void OnOptionsRequested()
        {
            if (pauseMenu != null)
            {
                pauseMenu.ShowOptionsMessage();
            }
        }

        public void CompleteRun()
        {
            if (_isGameComplete)
            {
                return;
            }

            _isGameComplete = true;
            _victoryTimer = 0f;
            if (victoryScreen != null)
            {
                victoryScreen.SetActive(true);
            }

            ResumeGame();
        }

        public void ReturnToLobby()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(lobbySceneName);
        }

        private void OnObjectiveComplete()
        {
            // This can be expanded later to trigger cinematics or network events.
        }
    }
}
