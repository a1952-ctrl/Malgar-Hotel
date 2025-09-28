using System.Collections;
using MalgarHotel.Audio;
using MalgarHotel.Core;
using MalgarHotel.Player;
using UnityEngine;

namespace MalgarHotel.World
{
    public class FusePanelMiniGame : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Repair Panel";
        [SerializeField] private KeyCode[] sequence =
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3
        };
        [SerializeField] private float maxStepDelay = 2.5f;
        [SerializeField] private AudioSource feedbackAudio;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip failureClip;

        private int _currentIndex;
        private bool _isRunning;
        private float _stepTimer;
        private HudController _hud;
        private bool _completed;
        private Coroutine _feedbackRoutine;

        private void Awake()
        {
            if (feedbackAudio == null)
            {
                feedbackAudio = gameObject.AddComponent<AudioSource>();
                feedbackAudio.playOnAwake = false;
                feedbackAudio.spatialBlend = 1f;
            }
        }

        private void Start()
        {
            _hud = FindObjectOfType<HudController>();
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            _stepTimer += Time.unscaledDeltaTime;
            if (_stepTimer >= maxStepDelay)
            {
                Fail();
                return;
            }

            if (_currentIndex >= sequence.Length)
            {
                return;
            }

            if (Input.GetKeyDown(sequence[_currentIndex]))
            {
                _currentIndex++;
                _stepTimer = 0f;
                if (_currentIndex >= sequence.Length)
                {
                    Complete();
                }
                else
                {
                    ShowInstruction();
                }
            }
            else if (Input.anyKeyDown)
            {
                Fail();
            }
        }

        public string InteractionPrompt => prompt;

        public void Interact(PlayerController player)
        {
            if (_completed || _isRunning)
            {
                return;
            }

            StartCoroutine(RunMiniGame());
        }

        private IEnumerator RunMiniGame()
        {
            _isRunning = true;
            _currentIndex = 0;
            _stepTimer = 0f;
            ShowInstruction();
            yield return null;
        }

        private void ShowInstruction()
        {
            if (_hud != null && _currentIndex < sequence.Length)
            {
                if (_feedbackRoutine != null)
                {
                    StopCoroutine(_feedbackRoutine);
                    _feedbackRoutine = null;
                }

                _hud.ShowPrompt($"Press {sequence[_currentIndex]}");
            }
        }

        private void Complete()
        {
            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = null;
            }
            _isRunning = false;
            _completed = true;
            _hud?.HidePrompt();
            GameManager.Instance?.RegisterFuseCollected();
            if (successClip != null)
            {
                feedbackAudio.PlayOneShot(successClip);
            }
            else
            {
                feedbackAudio.Play();
            }
            gameObject.SetActive(false);
        }

        private void Fail()
        {
            _isRunning = false;
            _currentIndex = 0;
            _hud?.ShowPrompt("Sequence failed");
            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
            }
            _feedbackRoutine = StartCoroutine(HidePromptAfterDelay());
            if (failureClip != null)
            {
                feedbackAudio.PlayOneShot(failureClip);
            }

            NoiseSystem.Instance?.EmitNoise(transform.position, 0.85f, 2, 1f, NoiseTag.MiniGameFail);
        }

        private IEnumerator HidePromptAfterDelay()
        {
            yield return new WaitForSecondsRealtime(1.25f);
            if (!_isRunning)
            {
                _hud?.HidePrompt();
            }
            _feedbackRoutine = null;
        }
    }
}
