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
        [SerializeField]
        int value = 5;

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
            value -= 1;
            movement.Teleport(resetPoints.Get(transform.position));

            if (value <= 0)
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

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 100, 100), $"health:{value}");
        }
    }
}