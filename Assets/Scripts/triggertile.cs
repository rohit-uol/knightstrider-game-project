using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;   // Animator for fade effect
    public string nextLevelName;  // Name of the next scene
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

        source?.Play();

        // Start fade + scene change
        StartCoroutine(LoadLevel(nextLevelName));
    }

    IEnumerator LoadLevel(string levelName)
    {
        timeText.enabled = true;
        timeText.SetText($"TIME\n{Time.realtimeSinceStartup:00:00}");
        transition.SetTrigger("fade");  // Capital S

        yield return new WaitForSeconds(waitTime); // Capital W

        SceneManager.LoadScene(levelName);
    }
}