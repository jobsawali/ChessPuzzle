using System.Collections.Generic;
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

    [Header("Top Bar")]
    public UnityEngine.UI.Image topBar;
    public UnityEngine.UI.Image colorIndicator;
    public TMPro.TextMeshProUGUI whoToMoveText;

    [Header("Storico Puzzle")]
    public Transform historyPanel;
    public Sprite historyCorrect;  
    public Sprite historyWrong;       
    public int maxHistoryItems = 13;

    private List<Image> historyItems = new List<Image>();


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
        container.transform.SetParent(historyPanel);

        RectTransform containerRt = container.AddComponent<RectTransform>();
        containerRt.sizeDelta = new Vector2(40f, 55f);

        
        UnityEngine.UI.VerticalLayoutGroup vlg = container.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 2f;
        vlg.padding = new RectOffset(2, 2, 2, 2);
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // Quadratino
        GameObject item = new GameObject("HistoryItem");
        item.transform.SetParent(container.transform);

        Image img = item.AddComponent<Image>();
        img.sprite = correct ? historyCorrect : historyWrong;
        img.color = Color.white;

        RectTransform rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(30f, 30f);

        historyItems.Add(img);

        
        GameObject eloObj = new GameObject("EloText");
        eloObj.transform.SetParent(container.transform);

        TMPro.TextMeshProUGUI tmp = eloObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = elo.ToString();
        tmp.fontSize = 30f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = correct ? new Color(0.471f, 0.624f, 0.247f) : new Color(0.769f, 0.180f, 0.161f);

        RectTransform eloRt = eloObj.GetComponent<RectTransform>();
        eloRt.sizeDelta = new Vector2(40f, 16f);
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