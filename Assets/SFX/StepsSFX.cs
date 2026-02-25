using UnityEngine;

namespace TheMasterPath
{
    public class StepsSFX : MonoBehaviour
    {
        [SerializeField] AudioSource footSource;
        [SerializeField] AudioSource armorSource;
        [SerializeField] AudioClip footsteps;
        [SerializeField] Movement movement;
        [SerializeField] AudioClip[] armorClips;
        int lastRandomArmorClipIndex = 0;

        void Start()
        {
            movement.StepEnded += OnStepEnded;
        }
        void OnStepEnded(Vector2 start, Vector2 end)
        {
            PlayFootSound();
            PlayArmorSound();
        }

        void PlayFootSound()
        {
            footSource.pitch = Random.Range(0.8f, 1.2f);
            footSource.PlayOneShot(footsteps);
        }

        void PlayArmorSound()
        {
            var armorClipIndex = ChooseRandomArmorClipIndex();
            armorSource.Stop();
            armorSource.PlayOneShot(armorClips[armorClipIndex]);
            lastRandomArmorClipIndex = armorClipIndex;
        }

        int ChooseRandomArmorClipIndex()
        {
            var armorClipIndex = Random.Range(0, armorClips.Length);
            if (armorClipIndex == lastRandomArmorClipIndex)
            {
                armorClipIndex = (armorClipIndex + 1) % armorClips.Length;
            }

            return armorClipIndex;
        }
    }
}