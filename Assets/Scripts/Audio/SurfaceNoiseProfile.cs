using System;
using UnityEngine;

namespace MalgarHotel.Audio
{
    [CreateAssetMenu(menuName = "MalgarHotel/Noise/Surface Noise Profile", fileName = "SurfaceNoiseProfile")]
    public class SurfaceNoiseProfile : ScriptableObject
    {
        [Serializable]
        private struct SurfaceEntry
        {
            public SurfaceType surfaceType;
            [Range(0f, 2f)] public float multiplier;
        }

        [SerializeField] private float defaultMultiplier = 1f;
        [SerializeField] private SurfaceEntry[] entries;

        public float GetMultiplier(SurfaceType surfaceType)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].surfaceType == surfaceType)
                    {
                        return Mathf.Max(0f, entries[i].multiplier);
                    }
                }
            }

            return Mathf.Max(0f, defaultMultiplier);
        }
    }
}
