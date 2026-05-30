using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Vite")]
    public Image life1;
    public Image life2;
    public Image life3;

    [Header("Sprites Vite")]
    public Sprite lifeEmpty;
    public Sprite lifeLost;

    private int livesLost = 0;

    [Header("Suoni")]
    public AudioClip incorrectSound;
    private AudioSource audioSource;
    public AudioClip successSound;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PuzzleSolved()
    {
        audioSource.PlayOneShot(successSound);
    }

    public void LoseLife()
    {
        audioSource.PlayOneShot(incorrectSound);
        livesLost++;

        if (livesLost == 1)
        {
            life1.sprite = lifeLost;
            life1.color = Color.white;
        }
        else if (livesLost == 2)
        {
            life2.sprite = lifeLost;
            life2.color = Color.white;
        }
        else if (livesLost == 3)
        {
            life3.sprite = lifeLost;
            life3.color = Color.white;
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER!");
    }

    public bool IsGameOver()
    {
        return livesLost >= 3;
    }
}