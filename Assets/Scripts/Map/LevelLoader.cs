using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using TheMasterPath.Utilities;

namespace TheMasterPath
{
    public class LevelLoader : MonoBehaviour
    {
        public Animator transition;
        public string nextLevelName;
        public TextMeshProUGUI timeText;
        public float waitTime = 1f;
        [SerializeField] private float quadrantHideDelay = 1f;
        private AudioSource source;
        private bool hasPlayed = false;
        public Timer timer;
        public string lastLevelName;
        public TextMeshProUGUI additionalText;

        void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("playerTrigger")) return;
            if (hasPlayed) return;
            hasPlayed = true;
            Debug.Log("Player stepped on Final Tile!");

            // Disable movement immediately
            var player = GameObject.FindWithTag("playerTrigger");
            if (player != null)
            {
                var movement = player.GetComponent<TheMasterPath.Movement>();
                if (movement != null)
                    movement.EnableInput = false;
            }
            MapDestroyer.Instance.HideQuadrant(4);            
            StartCoroutine(LoadLevel(nextLevelName));
        }

        IEnumerator LoadLevel(string levelName)
        {
            yield return new WaitForSeconds(quadrantHideDelay);
            source?.Play();
            timeText.enabled = true;
            additionalText.enabled = true;
            timeText.SetText($"TIME\n{timer.PlayTime:00:00}");
            transition.SetTrigger("fade");
            yield return new WaitForSeconds(waitTime);
            SceneManager.LoadScene(levelName);
        }
    }
}