using UnityEngine;

namespace MalgarHotel.Core
{
    /// <summary>
    /// Simple placeholder noise system that will later broadcast events to AI agents.
    /// </summary>
    public class NoiseSystem : MonoBehaviour
    {
        public static NoiseSystem Instance { get; private set; }

        [SerializeField] private bool logNoiseEvents;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ReportNoise(Vector3 position, float intensity)
        {
            if (logNoiseEvents)
            {
                Debug.Log($"[NoiseSystem] Noise emitted at {position} intensity {intensity:F2}");
            }
        }
    }
}
