using MalgarHotel.Audio;
using UnityEngine;

namespace MalgarHotel.World
{
    /// <summary>
    /// Simple helper for door animations or interactables to hook into the noise system.
    /// </summary>
    public class DoorNoiseEmitter : MonoBehaviour
    {
        [SerializeField] private float openIntensity = 0.45f;
        [SerializeField] private float closeIntensity = 0.55f;
        [SerializeField] private float eventLife = 0.75f;
        [SerializeField] private int priority;

        public void EmitOpen()
        {
            NoiseSystem.EmitNoise(transform.position, openIntensity, priority, eventLife, NoiseTag.Door);
        }

        public void EmitClose()
        {
            NoiseSystem.EmitNoise(transform.position, closeIntensity, priority, eventLife, NoiseTag.Door);
        }
    }
}
