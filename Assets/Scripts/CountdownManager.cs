using System.Collections;
using UnityEngine;
using TMPro;


public class CountdownManager : MonoBehaviour
{
    [Header("Riferimenti")]
    public TextMeshProUGUI countdownText;
    public PuzzleManager   puzzleManager;

    [Header("Impostazioni")]
    public float stepDuration = 0.8f;   
    public float goDuration  = 0.6f;   

    [Header("Colori")]
    public Color colorNumbers = Color.white;
    public Color colorGo = Color.white;

    [Header("Animazione scala")]
    public float scaleFrom = 1.8f;
    public float scaleTo   = 0.8f;

    [Header("Audio")]
    public AudioClip tickSound;
    public AudioClip goSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        gameObject.SetActive(false);
    }

  
    public void StartCountdown()
    {
        gameObject.SetActive(true);
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        string[] steps = { "3", "2", "1", "GO!" };

        for (int i = 0; i < steps.Length; i++)
        {
            string step = steps[i];
            bool isGo = step == "GO!";

            countdownText.text = step;
            countdownText.color = Color.white;
            countdownText.transform.localScale = Vector3.one;

            if (isGo)
            {
                if (goSound != null)
                    if (PlayerPrefs.GetInt("SFX", 1) == 1)
                        audioSource.PlayOneShot(goSound);
            }
            else
                if (PlayerPrefs.GetInt("SFX", 1) == 1)
                    audioSource.PlayOneShot(tickSound);
            yield return new WaitForSeconds(isGo ? goDuration : stepDuration);
        }

        gameObject.SetActive(false);
        puzzleManager.isCountdownActive = false;
        puzzleManager.uiManager.whoToMoveText.gameObject.SetActive(true);
        puzzleManager.uiManager.ShowTopBar();
        puzzleManager.LoadNextPuzzle();
    }
}
