using MalgarHotel.Audio;
using MalgarHotel.Core;
using UnityEngine;

namespace MalgarHotel.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float crouchHeight = 1.2f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float footstepInterval = 0.5f;

        [Header("Look")]
        [SerializeField] private Transform viewRoot;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float lookSensitivity = 90f;
        [SerializeField] private float minPitch = -75f;
        [SerializeField] private float maxPitch = 75f;

        [Header("Interaction")]
        [SerializeField] private float interactDistance = 2.5f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Audio")]
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private float footstepNoiseIntensity = 0.25f;
        [SerializeField] private float sprintFootstepNoiseIntensity = 0.5f;
        [SerializeField] private float crouchFootstepNoiseIntensity = 0.15f;
        [SerializeField] private Transform footstepOrigin;
        [SerializeField] private float footstepSurfaceProbeDistance = 1.6f;
        [SerializeField] private LayerMask surfaceMask = ~0;
        [SerializeField] private SurfaceType defaultSurfaceType = SurfaceType.Tile;

        [Header("Systems")]
        [SerializeField] private FlashlightController flashlightController;
        [SerializeField] private HudController hudController;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private Vector2 _lookAngles;
        private float _stepTimer;
        private float _originalHeight;
        private Vector3 _originalCenter;
        private bool _cursorLocked = true;
        private IInteractable _hoveredInteractable;
        private AudioClip _generatedFootstepClip;
        private bool _isSprinting;
        private bool _isCrouching;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _originalHeight = _characterController.height;
            _originalCenter = _characterController.center;

            if (footstepSource == null)
            {
                footstepSource = gameObject.AddComponent<AudioSource>();
                footstepSource.spatialBlend = 1f;
            }

            if (footstepClips == null || footstepClips.Length == 0)
            {
                _generatedFootstepClip = CreateFootstepClip();
                footstepSource.clip = _generatedFootstepClip;
                footstepSource.playOnAwake = false;
            }
        }

        private void Start()
        {
            if (playerCamera == null && viewRoot != null)
            {
                playerCamera = viewRoot.GetComponentInChildren<Camera>();
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                return;
            }

            HandleCursor();
            HandleLook();
            HandleMovement();
            HandleInteraction();
            HandleFlashlight();
        }

        private void HandleCursor()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _cursorLocked = !_cursorLocked;
            }

            Cursor.lockState = _cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !_cursorLocked;
        }

        private void HandleLook()
        {
            if (!_cursorLocked || playerCamera == null)
            {
                return;
            }

            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

            transform.Rotate(Vector3.up * mouseX);
            _lookAngles.x = Mathf.Clamp(_lookAngles.x - mouseY, minPitch, maxPitch);

            if (viewRoot != null)
            {
                viewRoot.localEulerAngles = new Vector3(_lookAngles.x, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            bool wantsSprint = Input.GetKey(KeyCode.LeftShift);
            bool wantsCrouch = Input.GetKey(KeyCode.LeftControl);

            float targetSpeed = walkSpeed;
            if (wantsCrouch)
            {
                targetSpeed = crouchSpeed;
            }
            else if (wantsSprint)
            {
                targetSpeed = sprintSpeed;
            }

            Vector3 worldMove = transform.TransformDirection(input) * targetSpeed;

            _isSprinting = wantsSprint && !wantsCrouch && input.sqrMagnitude > 0.01f;
            _isCrouching = wantsCrouch;

            if (_characterController.isGrounded)
            {
                _velocity.y = -2f;
            }

            _velocity.x = worldMove.x;
            _velocity.z = worldMove.z;
            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);

            HandleCrouch(wantsCrouch);
            HandleFootsteps(worldMove);
        }

        private void HandleCrouch(bool isCrouching)
        {
            float targetHeight = isCrouching ? crouchHeight : _originalHeight;
            float heightDelta = targetHeight - _characterController.height;
            if (Mathf.Abs(heightDelta) > 0.01f)
            {
                _characterController.height = Mathf.Lerp(_characterController.height, targetHeight, Time.deltaTime * 8f);
                Vector3 center = _characterController.center;
                center.y = Mathf.Lerp(center.y, _originalCenter.y - (_originalHeight - _characterController.height) * 0.5f, Time.deltaTime * 8f);
                _characterController.center = center;
            }
        }

        private void HandleFootsteps(Vector3 planarVelocity)
        {
            Vector3 horizontal = new Vector3(planarVelocity.x, 0f, planarVelocity.z);
            float speed = horizontal.magnitude;
            if (speed <= 0.1f || !_characterController.isGrounded)
            {
                _stepTimer = 0f;
                return;
            }

            _stepTimer += Time.deltaTime;
            if (_stepTimer >= footstepInterval)
            {
                _stepTimer = 0f;
                PlayFootstep();
                EmitFootstepNoise();
            }
        }

        private void EmitFootstepNoise()
        {
            float baseIntensity = footstepNoiseIntensity;
            if (_isSprinting)
            {
                baseIntensity = sprintFootstepNoiseIntensity;
            }
            else if (_isCrouching)
            {
                baseIntensity = crouchFootstepNoiseIntensity;
            }

            SurfaceType surface = SampleSurfaceType();
            float multiplier = NoiseSystem.Instance != null ? NoiseSystem.Instance.GetSurfaceMultiplier(surface) : 1f;
            float finalIntensity = Mathf.Clamp01(baseIntensity * multiplier);

            if (finalIntensity <= 0f)
            {
                return;
            }

            Vector3 origin = footstepOrigin != null ? footstepOrigin.position : transform.position;
            NoiseSystem.Instance?.EmitNoise(origin, finalIntensity, 0, 0.75f, NoiseTag.Footstep);
        }

        private SurfaceType SampleSurfaceType()
        {
            Vector3 origin = footstepOrigin != null ? footstepOrigin.position : transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, footstepSurfaceProbeDistance, surfaceMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.TryGetComponent<SurfaceNoiseTag>(out var tag))
                {
                    return tag.SurfaceType;
                }
            }

            return defaultSurfaceType;
        }

        private void PlayFootstep()
        {
            if (footstepClips != null && footstepClips.Length > 0)
            {
                var clip = footstepClips[Random.Range(0, footstepClips.Length)];
                footstepSource.PlayOneShot(clip);
            }
            else if (_generatedFootstepClip != null)
            {
                footstepSource.pitch = Random.Range(0.9f, 1.1f);
                footstepSource.PlayOneShot(_generatedFootstepClip);
            }
        }

        private AudioClip CreateFootstepClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Clamp01(1f - t * 5f);
                data[i] = Random.Range(-0.6f, 0.6f) * envelope;
            }

            var clip = AudioClip.Create("Footstep", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void HandleInteraction()
        {
            if (playerCamera == null)
            {
                return;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
            {
                _hoveredInteractable = hit.collider.GetComponentInParent<IInteractable>();
                if (_hoveredInteractable != null)
                {
                    hudController?.ShowPrompt($"[{interactKey}] {_hoveredInteractable.InteractionPrompt}");
                    if (Input.GetKeyDown(interactKey))
                    {
                        _hoveredInteractable.Interact(this);
                    }
                    return;
                }
            }

            _hoveredInteractable = null;
            hudController?.HidePrompt();
        }

        private void HandleFlashlight()
        {
            if (flashlightController == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                flashlightController.Toggle();
            }
        }

        public void RefillBattery(float amount)
        {
            flashlightController?.AddBattery(amount);
        }

        private void OnDestroy()
        {
            if (_generatedFootstepClip != null)
            {
                Destroy(_generatedFootstepClip);
            }
        }
    }
}
