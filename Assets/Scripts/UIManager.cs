using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Top Bar")]
    public UnityEngine.UI.Image topBar;
    public UnityEngine.UI.Image colorIndicator;
    public TMPro.TextMeshProUGUI whoToMoveText;

    [Header("Storico Puzzle")]
    public Transform historyPanel;
    public Sprite historyCorrect;  
    public Sprite historyWrong;       
    public int maxHistoryItems = 11;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TMPro.TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button menuButton;

    public LeaderboardManager leaderboardManager;



    [Header("Riferimenti")]
    public GameObject boardObject;


    private const string MENU_SCENE = "MainMenu";
    private const string GAME_SCENE = "GameScene";

    private List<Image> historyItems = new List<Image>();
    private int totalSolved = 0;


    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);
        if (menuButton != null)
            menuButton.onClick.AddListener(OnGoToMenu);
    }

    public void PuzzleSolved()
    {
        if (PlayerPrefs.GetInt("SFX", 1) == 1)
            audioSource.PlayOneShot(successSound);
    }

    public void LoseLife()
    {
        if (PlayerPrefs.GetInt("SFX", 1) == 1)
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

    public void UpdateWhoToMove(bool isWhite)
    {
        if (isWhite)
        {
            topBar.color = new Color(0.796f, 0.796f, 0.796f); 
            colorIndicator.color = Color.white; 
            whoToMoveText.text = "White to move";
            whoToMoveText.color = new Color(0.212f, 0.212f, 0.212f); 
        }
        else
        {
            topBar.color = new Color(0.082f, 0.075f, 0.067f); 
            colorIndicator.color = Color.black;
            whoToMoveText.text = "Black to move";
            whoToMoveText.color = new Color(0.812f, 0.808f, 0.816f); 
        }
    }

    public void ShowPuzzleCorrect()
    {
        topBar.color = new Color(0.471f, 0.624f, 0.247f);
        whoToMoveText.text = "Puzzle solved!";
        whoToMoveText.color = Color.white;
        colorIndicator.color = new Color(0.471f, 0.624f, 0.247f);
    }
    public void ShowPuzzleIncorrect()
    {
        topBar.color = new Color(0.769f, 0.180f, 0.161f); 
        whoToMoveText.text = "Wrong move!";
        whoToMoveText.color = Color.white;
        colorIndicator.color = new Color(0.769f, 0.180f, 0.161f);
    }

    public void AddHistoryItem(bool correct, int elo)
    {
        if (historyItems.Count >= maxHistoryItems)
        {
            Image oldest = historyItems[0];
            historyItems.RemoveAt(0);
            Destroy(oldest.gameObject.transform.parent.gameObject);
        }

        GameObject container = new GameObject("HistoryContainer");
        container.transform.SetParent(historyPanel, false);

        RectTransform containerRt = container.AddComponent<RectTransform>();
        containerRt.sizeDelta = new Vector2(45f, 55f);

        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 2f;
        vlg.padding = new RectOffset(2, 2, 2, 2);
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        GameObject item = new GameObject("HistoryItem");
        item.transform.SetParent(container.transform, false);

        Image img = item.AddComponent<Image>();
        img.sprite = correct ? historyCorrect : historyWrong;
        img.color = Color.white;

        RectTransform rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(30f, 30f);

        historyItems.Add(img);

        GameObject eloObj = new GameObject("EloText");
        eloObj.transform.SetParent(container.transform, false);

        TMPro.TextMeshProUGUI tmp = eloObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = elo.ToString();
        tmp.fontSize = 30f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = correct ? new Color(0.471f, 0.624f, 0.247f) : new Color(0.769f, 0.180f, 0.161f);

        RectTransform eloRt = eloObj.GetComponent<RectTransform>();
        eloRt.sizeDelta = new Vector2(50f, 30f);
    }


    private void GameOver()
    {
        gameOverPanel.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = $"Puzzles solved: {totalSolved}";
        if (leaderboardManager != null)
            leaderboardManager.Show(totalSolved);
    }

    public bool IsGameOver()
    {
        return livesLost >= 3;
    }

    private void OnRestart()
    {
        SceneManager.LoadScene(GAME_SCENE);
    }

    private void OnGoToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE);
    }

    public void RegisterSolvedPuzzle(int count)
    {
        totalSolved = count;
    }

    public void HideTopBar()
    {
        topBar.gameObject.SetActive(false);
        colorIndicator.gameObject.SetActive(false);
        whoToMoveText.gameObject.SetActive(false);
    }

    public void ShowTopBar()
    {
        topBar.gameObject.SetActive(true);
        colorIndicator.gameObject.SetActive(true);
        whoToMoveText.gameObject.SetActive(true);
    }


}