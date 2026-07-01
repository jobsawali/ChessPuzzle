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



    [Header("High Score")]
    public TextMeshProUGUI highScoreText;

    [Header("Pannelli")]
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;
    public Button backButton;
    public GameObject usernamePanel;

    [Header("Options")]
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Toggle sfxToggle;



    private const string HIGH_SCORE_KEY = "HighScore";

    private const string GAME_SCENE = "GameScene";

    void Start()
    {
        mainMenuPanel.SetActive(true);
        optionsMenuPanel.SetActive(false);

        if (playButton != null) playButton.onClick.AddListener(OnPlay);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnOpenSettings);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        if (backButton != null) backButton.onClick.AddListener(OnBack);

        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("Music", 1f);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxToggle != null)
        {
            sfxToggle.isOn = PlayerPrefs.GetInt("SFX", 1) == 1;
            sfxToggle.onValueChanged.AddListener(OnSFXChanged);
        }

   

        UpdateHighScoreUI();
    }


    void OnPlay()
    {
        usernamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    void OnOpenSettings()
    {
        mainMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(true);
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void OnBack()
    {
        optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

   
    void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat("Music", value);
        PlayerPrefs.Save();
     
    }

    void OnSFXChanged(bool value)
    {
        PlayerPrefs.SetInt("SFX", value ? 1 : 0);
        PlayerPrefs.Save();
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
