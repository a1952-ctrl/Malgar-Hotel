using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalgarHotel.Audio
{
    /// <summary>
    /// Lightweight static noise registry used to coordinate gameplay reactions to
    /// player-made sounds (footsteps, microphone input, etc.). The current
    /// implementation focuses on recording events for later queries without
    /// enforcing any runtime dependencies.
    /// </summary>
    public static class NoiseSystem
    {
        private static readonly List<NoiseEvent> Events = new List<NoiseEvent>();
        private static int _maxEvents = 48;
        private static float _defaultLife = 0.75f;
        private static bool _occlusionEnabled;
        private static float _occlusionAttenuation = 0.6f;
        private static LayerMask _occluderMask = ~0;
        private static Transform _occlusionListener;
        private static SurfaceNoiseProfile _surfaceProfile;

        /// <summary>
        /// Configures the maximum stored events and default lifetime. Optional; sane
        /// defaults are provided if never called.
        /// </summary>
        public static void Configure(int maxEvents, float defaultLifeSeconds)
        {
            _maxEvents = Mathf.Max(1, maxEvents);
            _defaultLife = Mathf.Max(0.01f, defaultLifeSeconds);
        }

        public static void SetSurfaceProfile(SurfaceNoiseProfile profile)
        {
            _surfaceProfile = profile;
        }

        public static void SetOcclusionListener(Transform listener)
        {
            _occlusionListener = listener;
        }

        public static void SetOcclusionSettings(bool enabled, float attenuation, LayerMask mask)
        {
            _occlusionEnabled = enabled;
            _occlusionAttenuation = Mathf.Clamp01(attenuation);
            _occluderMask = mask;
        }

        public static void EmitNoise(Vector3 pos, float intensity01, int priority = 0, float life = 0.75f, NoiseTag tag = NoiseTag.Generic)
        {
            float intensity = Mathf.Clamp01(intensity01);
            if (intensity <= 0f)
            {
                return;
            }

            float now = Time.unscaledTime;
            float lifeToUse = life > 0f ? life : _defaultLife;

            if (_occlusionEnabled && _occlusionListener != null && _occlusionAttenuation < 1f)
            {
                Vector3 listenerPos = _occlusionListener.position;
                Vector3 direction = listenerPos - pos;
                float distance = direction.magnitude;
                if (distance > 0.05f)
                {
                    direction /= distance;
                    if (Physics.Raycast(pos, direction, out RaycastHit hit, distance, _occluderMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider != null && hit.collider.GetComponent<NoiseOccluder>() != null)
                        {
                            intensity *= Mathf.Clamp01(_occlusionAttenuation);
                        }
                    }
                }
            }

            PruneExpired(now);

            if (Events.Count >= _maxEvents)
            {
                Events.RemoveAt(0);
            }

            Events.Add(new NoiseEvent
            {
                Position = pos,
                Intensity = intensity,
                Priority = priority,
                StartTime = now,
                DieTime = now + lifeToUse,
                Tag = tag
            });
        }

        public static bool TryGetStrongest(out NoiseEvent strongest)
        {
            float now = Time.unscaledTime;
            PruneExpired(now);

            strongest = default;
            float bestScore = float.MinValue;
            for (int i = 0; i < Events.Count; i++)
            {
                var evt = Events[i];
                float score = evt.Score;
                if (score > bestScore)
                {
                    bestScore = score;
                    strongest = evt;
                }
            }

            return bestScore > float.MinValue;
        }

        public static int GetRecentEvents(List<NoiseEvent> buffer, float windowSec)
        {
            if (buffer == null)
            {
                return 0;
            }

            float now = Time.unscaledTime;
            PruneExpired(now);

            buffer.Clear();
            float windowStart = now - Mathf.Max(0.01f, windowSec);
            for (int i = 0; i < Events.Count; i++)
            {
                var evt = Events[i];
                if (evt.StartTime >= windowStart)
                {
                    buffer.Add(evt);
                }
            }

            return buffer.Count;
        }

        public static float GetSurfaceMultiplier(SurfaceType type)
        {
            if (_surfaceProfile == null)
            {
                return 1f;
            }

            return _surfaceProfile.GetMultiplier(type);
        }

        private static void PruneExpired(float now)
        {
            for (int i = Events.Count - 1; i >= 0; i--)
            {
                if (Events[i].DieTime <= now)
                {
                    Events.RemoveAt(i);
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
