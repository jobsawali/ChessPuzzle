using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Game Over")]
    public TextMeshProUGUI playerNameText;    
    public TextMeshProUGUI finalScoreText;    
    public Button restartButton;
    public Button menuButton;

    [Header("Leaderboard")]
    public Transform leaderboardContainer;   
    public GameObject rowPrefab;             
    public int maxRows = 5;

    [Header("Colori")]
    public Color currentPlayerColor = new Color(0.784f, 0.663f, 0.431f, 1f); 
    public Color normalColor        = new Color(0.906f, 0.835f, 0.690f, 0.7f); 

    private const string GAME_SCENE = "GameScene";
    private const string MENU_SCENE = "MainMenu";

   

    
    public void Show(int score)
    {
        string currentPlayer = PlayerManager.GetCurrentPlayer();
        PlayerManager.TrySaveScore(currentPlayer, score);
        PopulateLeaderboard(currentPlayer);
    }

    void PopulateLeaderboard(string currentPlayer)
    {
        if (leaderboardContainer == null) return;

   
        foreach (Transform child in leaderboardContainer)
            Destroy(child.gameObject);

        List<(string name, int score)> leaderboard = PlayerManager.GetLeaderboard();
        int count = Mathf.Min(leaderboard.Count, maxRows);

        for (int i = 0; i < count; i++)
        {
            var (name, score) = leaderboard[i];
            bool isCurrentPlayer = name == currentPlayer;

           
            GameObject row = CreateRow(i + 1, name, score, isCurrentPlayer);
            row.transform.SetParent(leaderboardContainer, false);
        }
    }

    GameObject CreateRow(int position, string name, int score, bool isCurrentPlayer)
    {
        GameObject row = new GameObject($"Row_{position}");

        // Layout orizzontale
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 44);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment       = TextAnchor.MiddleLeft;
        hlg.spacing              = 8f;
        hlg.padding              = new RectOffset(12, 12, 0, 0);
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        
        if (isCurrentPlayer)
        {
            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.784f, 0.663f, 0.431f, 0.2f);
        }

        Color textColor = isCurrentPlayer ? currentPlayerColor : normalColor;

        
        string posStr = position == 1 ? "1°" : position == 2 ? "2°" : position == 3 ? "3°" : $"{position}.";
        AddText(row, posStr, textColor, 32f, 50f);


        string displayName = isCurrentPlayer ? $"> {name}" : name; ;
        AddText(row, displayName, textColor, 32f, 200f, isCurrentPlayer);

        AddText(row, score.ToString(), textColor, 32f, 60f, isCurrentPlayer);




        return row;
    }

    void AddText(GameObject parent, string text, Color color, float fontSize, float width, bool bold = false)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent.transform, false);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 80);

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredWidth  = width;
        le.preferredHeight = 44;
        le.flexibleWidth   = 0;
    }

}
