using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace TheMasterPath
{
    [RequireComponent(typeof(Movement))]
    public class Health : MonoBehaviour
    {
        [SerializeField]
        ResetPoints resetPoints;
        [SerializeField]
        float animationTime;
        [SerializeField]
        Animator playerAnimator;
        [SerializeField]
        AudioClip deathSound;

        /// <summary>
        /// The current health value.
        /// </summary>
        [field: SerializeField]
        public int Value { get; private set; } = 5;

        Movement movement;
        AudioSource audioSource;

        void Start()
        {
            movement = GetComponent<Movement>();
            movement.TurnBack += OnTurnBack;
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Called when the player is turned back.
        /// </summary>
        void OnTurnBack(Vector2 stepStart)
        {
            Value -= 1;
            playerAnimator.SetTrigger("death");

            if (Value <= 0 && deathSound != null)
                AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);

            StartCoroutine(WaitForAnimation(() =>
            {
                if (Value > 0)
                {
                    movement.MoveTo(resetPoints.Get(transform.position));
                    StartCoroutine(WaitForTurnBack(() =>
                    {
                        movement.EnableInput = true;
                    }));
                }
                else
                {
                    float remainingDelay = deathSound != null ? deathSound.length - animationTime : 0f;
                    StartCoroutine(DelayedReload(Mathf.Max(0f, remainingDelay)));
                }
            }));
        }

        IEnumerator WaitForAnimation(System.Action callback)
        {
            yield return new WaitForSeconds(animationTime);
            callback.Invoke();
        }

        IEnumerator WaitForTurnBack(System.Action callback)
        {
            yield return new WaitForSeconds(0.25f);
            callback.Invoke();
        }

        /// <summary>
        /// Loads the currently active scene again.
        /// </summary>
        void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        IEnumerator DelayedReload(float delay)
        {
            yield return new WaitForSeconds(delay);
            ReloadScene();
        }
    }
}