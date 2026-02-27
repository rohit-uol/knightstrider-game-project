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

        /// <summary>
        /// The current health value.
        /// </summary>
        [field: SerializeField]
        public int Value { get; private set; } = 5;

        Movement movement;

        void Start()
        {
            movement = GetComponent<Movement>();
            movement.TurnBack += OnTurnBack;
        }

        /// <summary>
        /// Called when the player is turned back.
        /// </summary>
        void OnTurnBack(Vector2 stepStart)
        {
            Value -= 1;
            playerAnimator.SetTrigger("death");
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
                    ReloadScene();
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
    }
}