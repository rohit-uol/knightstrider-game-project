using UnityEngine;

namespace TheMasterPath
{
    public class StepsSFX : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField] AudioClip footsteps;
        [SerializeField] Movement movement;

        void Start()
        {
            movement.StepEnded += OnStepEnded;    
        }

        private void OnStepEnded(Vector2 start, Vector2 end)
        {
            source.pitch = Random.Range(0.8f, 1.2f);
            source.PlayOneShot(footsteps);
        }
    }
}