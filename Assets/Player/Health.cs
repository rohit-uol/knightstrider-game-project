using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMasterPath
{
    [RequireComponent(typeof(Movement))]
    public class Health : MonoBehaviour
    {
        [SerializeField]
        ResetPoints resetPoints;

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
        void OnTurnBack()
        {
            Value -= 1;
            movement.Teleport(resetPoints.Get(transform.position));

            if (Value <= 0)
            {
                ReloadScene();
            }
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