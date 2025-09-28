using UnityEngine;

namespace MalgarHotel.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class ProceduralLoopSource : MonoBehaviour
    {
        [SerializeField] private float toneFrequency = 110f;
        [SerializeField] private float volume = 0.2f;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool generateNoise;

        private AudioSource _source;
        private AudioClip _generatedClip;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;

            _generatedClip = generateNoise ? CreateNoiseClip() : CreateToneClip();
            _source.clip = _generatedClip;
            _source.volume = volume;
        }

        private void Start()
        {
            if (playOnStart)
            {
                _source.Play();
            }
        }

        private AudioClip CreateToneClip()
        {
            const int sampleRate = 44100;
            const float duration = 2f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                data[i] = Mathf.Sin(2f * Mathf.PI * toneFrequency * t);
            }

            var clip = AudioClip.Create("ProceduralTone", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateNoiseClip()
        {
            const int sampleRate = 44100;
            const float duration = 1.2f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                data[i] = Random.Range(-1f, 1f);
            }

            var clip = AudioClip.Create("ProceduralNoise", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (_generatedClip != null)
            {
                Destroy(_generatedClip);
            }
        }
    }
}
