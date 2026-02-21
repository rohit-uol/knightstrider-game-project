using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;   // Animator for fade effect
    public string nextLevelName;  // Name of the next scene

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
        transition.SetTrigger("fade");  // Capital S

        yield return new WaitForSeconds(1f); // Capital W

        SceneManager.LoadScene(levelName);
    }
}