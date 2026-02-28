using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public string nextLevelName;
    public TextMeshProUGUI timeText;
    public float waitTime = 1f;
    private AudioSource source;
    private bool hasPlayed = false;

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

        source?.Play();
        StartCoroutine(LoadLevel(nextLevelName));
    }

    IEnumerator LoadLevel(string levelName)
    {
        timeText.enabled = true;
        timeText.SetText($"TIME\n{Time.realtimeSinceStartup:00:00}");
        transition.SetTrigger("fade");
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(levelName);
    }
}