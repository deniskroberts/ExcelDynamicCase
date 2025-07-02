using UnityEngine;
using UnityEngine.Audio;

namespace RPGM.Gameplay
{
    public class MusicController : MonoBehaviour
    {
        static MusicController instance;

        public AudioMixerGroup audioMixerGroup;
        public AudioClip audioClip;

        public AudioClip battleAudio;
        public AudioClip championAudio;

        public float crossFadeTime = 3;

        AudioSource audioSourceA, audioSourceB;
        float audioSourceAVolumeVelocity, audioSourceBVolumeVelocity;

        public void CrossFade(AudioClip audioClip)
        {
            var t = audioSourceA;
            audioSourceA = audioSourceB;
            audioSourceB = t;
            audioSourceA.clip = audioClip;
            audioSourceA.Play();
            audioSourceB.Stop();
        }

        public void CrossFadeIntoBattle(AudioClip audioClip)
        {
            CrossFade(audioClip ?? battleAudio);
        }

        public void CrossFadeOutOfBattle()
        {
            CrossFade(audioClip);
        }

        void Awake()
        {
            //var musicController = GetComponent<MusicController>();
            if (instance != null)
                Destroy(instance);
            else
                instance = this;
        }

        void Update()
        {
            audioSourceA.volume = Mathf.SmoothDamp(audioSourceA.volume, 0.2f, ref audioSourceAVolumeVelocity, crossFadeTime, 1);
            audioSourceB.volume = Mathf.SmoothDamp(audioSourceB.volume, 0f, ref audioSourceBVolumeVelocity, crossFadeTime, 1);
        }

        void OnEnable()
        {
            audioSourceA = gameObject.AddComponent<AudioSource>();
            audioSourceA.spatialBlend = 0;
            audioSourceA.clip = audioClip;
            audioSourceA.loop = true;
            audioSourceA.outputAudioMixerGroup = audioMixerGroup;
            audioSourceA.volume = 0.2f;
            audioSourceA.Play();

            audioSourceB = gameObject.AddComponent<AudioSource>();
            audioSourceB.spatialBlend = 0;
            audioSourceB.loop = true;
            audioSourceA.volume = 0f;
            audioSourceB.outputAudioMixerGroup = audioMixerGroup;
        }
    }
}