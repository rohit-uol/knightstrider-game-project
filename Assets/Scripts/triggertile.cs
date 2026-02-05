using UnityEngine;

public enum SpecialTileType
{
    TileOne,
    TileTwo,
    TileThree,
    TileFour
}

public class SpecialTile : MonoBehaviour
{
    public SpecialTileType tileType;
    private AudioSource source;

    private bool hasPlayed = false;
    private LevelLoader levelLoader;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        levelLoader = FindObjectOfType<LevelLoader>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("playerTrigger")) return;
        if (hasPlayed) return;

        hasPlayed = true;

        switch (tileType)
        {
            case SpecialTileType.TileOne:
                Debug.Log("Player stepped on Tile One!");
                source?.Play();
                break;

            case SpecialTileType.TileTwo:
                Debug.Log("Player stepped on Tile Two!");
                source?.Play();
                break;

            case SpecialTileType.TileThree:
                Debug.Log("Player stepped on Tile Three!");
                source?.Play();
                break;

            case SpecialTileType.TileFour:
                Debug.Log("Player stepped on Tile Four!");
                source?.Play();
                levelLoader.LoadNextLevel(); 
                break;
        }
    }
}
