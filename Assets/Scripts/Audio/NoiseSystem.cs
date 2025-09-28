using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalgarHotel.Audio
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-175)]
    public class NoiseSystem : MonoBehaviour
    {
        public static NoiseSystem Instance { get; private set; }

        [Header("Lifecycle")]
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private int maxEvents = 48;
        [SerializeField] private float defaultLife = 0.75f;

        [Header("Occlusion")]
        [SerializeField] private bool enableOcclusion = true;
        [SerializeField, Range(0.1f, 1f)] private float occlusionAttenuation = 0.6f;
        [SerializeField] private LayerMask occluderMask = ~0;
        [SerializeField] private Transform occlusionListener;

        [Header("Surfaces")]
        [SerializeField] private SurfaceNoiseProfile surfaceProfile;

        private readonly List<NoiseEvent> _events = new List<NoiseEvent>();

        private float Now => Time.unscaledTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            PruneExpired();
        }

        public void EmitNoise(Vector3 position, float intensity01, int priority = 0, float life = -1f, NoiseTag tag = NoiseTag.Generic)
        {
            if (!enabled)
            {
                return;
            }

            float intensity = Mathf.Clamp01(intensity01);
            if (intensity <= 0f)
            {
                return;
            }

            if (enableOcclusion && occlusionListener != null && occlusionAttenuation < 1f)
            {
                Vector3 listenerPos = occlusionListener.position;
                Vector3 direction = listenerPos - position;
                float distance = direction.magnitude;
                if (distance > 0.05f)
                {
                    direction /= distance;
                    if (Physics.Raycast(position, direction, out RaycastHit hit, distance, occluderMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider != null && hit.collider.GetComponent<NoiseOccluder>() != null)
                        {
                            intensity *= Mathf.Clamp01(occlusionAttenuation);
                        }
                    }
                }
            }

            float now = Now;
            float lifetime = life > 0f ? life : defaultLife;
            if (_events.Count >= maxEvents)
            {
                _events.RemoveAt(0);
            }

            _events.Add(new NoiseEvent
            {
                Position = position,
                Intensity = intensity,
                Priority = priority,
                StartTime = now,
                DieTime = now + lifetime,
                Tag = tag
            });
        }

        public bool TryGetStrongest(out NoiseEvent strongest)
        {
            strongest = default;
            float bestScore = float.MinValue;
            float now = Now;

            for (int i = _events.Count - 1; i >= 0; i--)
            {
                var e = _events[i];
                if (e.DieTime <= now)
                {
                    _events.RemoveAt(i);
                    continue;
                }

                float score = e.Score;
                if (score > bestScore)
                {
                    bestScore = score;
                    strongest = e;
                }
            }

            return bestScore > float.MinValue;
        }

        public int GetRecentEvents(List<NoiseEvent> buffer, float windowSec)
        {
            if (buffer == null)
            {
                return 0;
            }

            buffer.Clear();
            float now = Now;
            float windowStart = now - Mathf.Max(0.01f, windowSec);

            for (int i = _events.Count - 1; i >= 0; i--)
            {
                var e = _events[i];
                if (e.DieTime <= now)
                {
                    _events.RemoveAt(i);
                    continue;
                }

                if (e.StartTime >= windowStart)
                {
                    buffer.Add(e);
                }
            }

            return buffer.Count;
        }

        public float GetSurfaceMultiplier(SurfaceType type)
        {
            return surfaceProfile != null ? surfaceProfile.GetMultiplier(type) : 1f;
        }

        public void SetOcclusionListener(Transform listener)
        {
            occlusionListener = listener;
        }

        private void PruneExpired()
        {
            float now = Now;
            for (int i = _events.Count - 1; i >= 0; i--)
            {
                if (_events[i].DieTime <= now)
                {
                    _events.RemoveAt(i);
                }
            }
        }
    }

    [Serializable]
    public struct NoiseEvent
    {
        public Vector3 Position;
        public float Intensity;
        public int Priority;
        public float StartTime;
        public float DieTime;
        public NoiseTag Tag;

        public float Score => Intensity * (1f + Priority);
    }

    public enum NoiseTag
    {
        Generic,
        Mic,
        Footstep,
        Door,
        MiniGameFail
    }

    public enum SurfaceType
    {
        Tile,
        Carpet,
        Wood
    }
}
