using UnityEngine;

namespace TheMasterPath
{
    public class ReturnSFX : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField] Movement movement;
        [SerializeField] AudioClip clip;

        void Start()
        {
            movement.TurnBack += OnTurnBack;
        }

        void OnTurnBack(Vector2 stepStart)
        {
            source.pitch = Random.Range(0.9f, 1.1f);
            source.PlayOneShot(clip);
        }
    }
}