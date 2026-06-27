using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class MainMenuManager : MonoBehaviour
{
    [Header("Bottoni principali")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Pannello impostazioni")]
    public GameObject settingsPanel;
    public Button closeSettingsBtn;

    [Header("High Score")]
    public TextMeshProUGUI highScoreText;

    
    private const string HIGH_SCORE_KEY = "HighScore";

    private const string GAME_SCENE = "GameScene";

    void Start()
    {
       
        if (playButton     != null) playButton.onClick.AddListener(OnPlay);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnOpenSettings);
        if (closeSettingsBtn != null) closeSettingsBtn.onClick.AddListener(OnCloseSettings);
        if (quitButton     != null) quitButton.onClick.AddListener(OnQuit);

        
        if (settingsPanel != null) settingsPanel.SetActive(false);

        UpdateHighScoreUI();
    }


    void OnPlay()
    {
        SceneManager.LoadScene(GAME_SCENE);
    }

    void OnOpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    void OnCloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }



 
    public static void TrySaveHighScore(int score)
    {
        int current = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (score > current)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
            PlayerPrefs.Save();
        }
    }

    void UpdateHighScoreUI()
    {
        if (highScoreText == null) return;
        int best = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        highScoreText.text = best > 0 ? $"Best: {best}" : "";
    }
}
