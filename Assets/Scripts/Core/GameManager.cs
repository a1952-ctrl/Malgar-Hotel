using System;
using UnityEngine;
using UnityEngine.Audio;
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

        [Header("Audio Settings")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup masterGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup ambienceGroup;
        [SerializeField] private AudioMixerGroup uiGroup;
        [SerializeField] private float defaultMasterVolume = 0.85f;
        [SerializeField] private float defaultSfxVolume = 0.8f;
        [SerializeField] private float defaultAmbienceVolume = 0.8f;
        [SerializeField] private float defaultUiVolume = 0.9f;

        [Header("Input Settings")]
        [SerializeField] private float defaultMouseSensitivity = 1f;
        [SerializeField] private Vector2 mouseSensitivityRange = new Vector2(0.1f, 2f);

        private int _collectedFuses;
        private bool _isPaused;
        private bool _isGameComplete;
        private float _victoryTimer;
        private float _mouseSensitivity;
        private float _masterVolume;
        private float _sfxVolume;
        private float _ambienceVolume;
        private float _uiVolume;

        public event Action<int, int> FuseCountChanged;
        public event Action<float, float> BatteryChanged;
        public event Action<bool> PauseStateChanged;
        public event Action<bool> GameCompleteChanged;
        public event Action<float> MouseSensitivityChanged;
        public event Action<VolumeChannel, float> VolumeChanged;

        public int CollectedFuses => _collectedFuses;
        public int RequiredFuses => requiredFuses;
        public bool IsPaused => _isPaused;
        public bool IsGameComplete => _isGameComplete;
        public bool IsInputBlocked => _isPaused || _isGameComplete;
        public float MouseSensitivity => _mouseSensitivity;
        public float MasterVolume => _masterVolume;
        public float SfxVolume => _sfxVolume;
        public float AmbienceVolume => _ambienceVolume;
        public float UiVolume => _uiVolume;
        public AudioMixerGroup MasterGroup => masterGroup;
        public AudioMixerGroup SfxGroup => sfxGroup;
        public AudioMixerGroup AmbienceGroup => ambienceGroup;
        public AudioMixerGroup UiGroup => uiGroup;
        public Vector2 MouseSensitivityRange => mouseSensitivityRange;
        public float DefaultMouseSensitivity => defaultMouseSensitivity;

        private const string MouseSensitivityKey = "settings.mouseSensitivity";
        private const string MasterVolumeKey = "settings.volume.master";
        private const string SfxVolumeKey = "settings.volume.sfx";
        private const string AmbienceVolumeKey = "settings.volume.ambience";
        private const string UiVolumeKey = "settings.volume.ui";

        private const float MinDb = -80f;
        private const float MaxDb = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioMixer == null)
            {
                audioMixer = Resources.Load<AudioMixer>("Audio/LiminalMixer");
            }

            if (audioMixer != null)
            {
                if (masterGroup == null)
                {
                    var groups = audioMixer.FindMatchingGroups("Master");
                    if (groups.Length > 0)
                    {
                        masterGroup = groups[0];
                    }
                }

                if (sfxGroup == null)
                {
                    var groups = audioMixer.FindMatchingGroups("Master/SFX");
                    if (groups.Length > 0)
                    {
                        sfxGroup = groups[0];
                    }
                }

                if (ambienceGroup == null)
                {
                    var groups = audioMixer.FindMatchingGroups("Master/Ambience");
                    if (groups.Length > 0)
                    {
                        ambienceGroup = groups[0];
                    }
                }

                if (uiGroup == null)
                {
                    var groups = audioMixer.FindMatchingGroups("Master/UI");
                    if (groups.Length > 0)
                    {
                        uiGroup = groups[0];
                    }
                }
            }

            LoadSettings();
            ApplyVolumes();
        }

        private void Start()
        {
            if (victoryScreen != null)
            {
                victoryScreen.SetActive(false);
            }

            FuseCountChanged?.Invoke(_collectedFuses, requiredFuses);
            MouseSensitivityChanged?.Invoke(_mouseSensitivity);
            VolumeChanged?.Invoke(VolumeChannel.Master, _masterVolume);
            VolumeChanged?.Invoke(VolumeChannel.Sfx, _sfxVolume);
            VolumeChanged?.Invoke(VolumeChannel.Ambience, _ambienceVolume);
            VolumeChanged?.Invoke(VolumeChannel.Ui, _uiVolume);
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
                    if (pauseMenu != null && pauseMenu.TryCloseOptionsPanel())
                    {
                        return;
                    }

                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        private void LoadSettings()
        {
            _mouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(MouseSensitivityKey, defaultMouseSensitivity), mouseSensitivityRange.x, mouseSensitivityRange.y);
            _masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, defaultMasterVolume));
            _sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume));
            _ambienceVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(AmbienceVolumeKey, defaultAmbienceVolume));
            _uiVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(UiVolumeKey, defaultUiVolume));
        }

        private void ApplyVolumes()
        {
            if (audioMixer == null)
            {
                return;
            }

            SetVolumeInternal(VolumeChannel.Master, _masterVolume, false);
            SetVolumeInternal(VolumeChannel.Sfx, _sfxVolume, false);
            SetVolumeInternal(VolumeChannel.Ambience, _ambienceVolume, false);
            SetVolumeInternal(VolumeChannel.Ui, _uiVolume, false);
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

            PauseStateChanged?.Invoke(true);
        }

        public void ResumeGame()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            if (pauseMenu != null)
            {
                pauseMenu.Hide();
            }

            PauseStateChanged?.Invoke(false);
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
            GameCompleteChanged?.Invoke(true);
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

        public void SetMouseSensitivity(float value)
        {
            float clamped = Mathf.Clamp(value, mouseSensitivityRange.x, mouseSensitivityRange.y);
            if (!Mathf.Approximately(clamped, _mouseSensitivity))
            {
                _mouseSensitivity = clamped;
                PlayerPrefs.SetFloat(MouseSensitivityKey, _mouseSensitivity);
                PlayerPrefs.Save();
                MouseSensitivityChanged?.Invoke(_mouseSensitivity);
            }
        }

        public void SetVolume(VolumeChannel channel, float normalizedValue)
        {
            SetVolumeInternal(channel, normalizedValue, true);
        }

        private void SetVolumeInternal(VolumeChannel channel, float normalizedValue, bool persist)
        {
            float clamped = Mathf.Clamp01(normalizedValue);
            if (audioMixer != null)
            {
                string parameter = GetVolumeParameterName(channel);
                if (!string.IsNullOrEmpty(parameter))
                {
                    float db = clamped <= 0f ? MinDb : Mathf.Clamp(Mathf.Log10(clamped) * 20f, MinDb, MaxDb);
                    audioMixer.SetFloat(parameter, db);
                }
            }

            switch (channel)
            {
                case VolumeChannel.Master:
                    _masterVolume = clamped;
                    if (persist)
                    {
                        PlayerPrefs.SetFloat(MasterVolumeKey, _masterVolume);
                    }
                    break;
                case VolumeChannel.Sfx:
                    _sfxVolume = clamped;
                    if (persist)
                    {
                        PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
                    }
                    break;
                case VolumeChannel.Ambience:
                    _ambienceVolume = clamped;
                    if (persist)
                    {
                        PlayerPrefs.SetFloat(AmbienceVolumeKey, _ambienceVolume);
                    }
                    break;
                case VolumeChannel.Ui:
                    _uiVolume = clamped;
                    if (persist)
                    {
                        PlayerPrefs.SetFloat(UiVolumeKey, _uiVolume);
                    }
                    break;
            }

            if (persist)
            {
                PlayerPrefs.Save();
            }

            VolumeChanged?.Invoke(channel, clamped);
        }

        private static string GetVolumeParameterName(VolumeChannel channel)
        {
            switch (channel)
            {
                case VolumeChannel.Master:
                    return "MasterVol";
                case VolumeChannel.Sfx:
                    return "SFXVol";
                case VolumeChannel.Ambience:
                    return "AmbienceVol";
                case VolumeChannel.Ui:
                    return "UIVol";
                default:
                    return string.Empty;
            }
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.Save();
        }
    }

    public enum VolumeChannel
    {
        Master,
        Sfx,
        Ambience,
        Ui
    }
}
