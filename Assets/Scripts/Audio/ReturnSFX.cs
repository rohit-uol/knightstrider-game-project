using UnityEngine;

namespace TheMasterPath
{
    public class ReturnSFX : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField] Movement movement;
        [SerializeField] AudioClip[] clips;

        void Start()
        {
            movement.TurnBack += OnTurnBack;
        }

        void OnTurnBack(Vector2 stepStart)
        {
            foreach (var clip in clips)
            {
                source.PlayOneShot(clip);
            }
        }
    }
}