using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class UsernamePanelManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField usernameInput;
    public TextMeshProUGUI errorText;
    public Button startButton;

    private const string GAME_SCENE = "GameScene";
    private const int MIN_LENGTH = 3;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStart);

        if (errorText != null)
            errorText.text = "";

        
        if (usernameInput != null)
            usernameInput.text = "";
    }

    void OnStart()
    {
        string username = usernameInput.text.Trim();

       
        if (string.IsNullOrEmpty(username))
        {
            ShowError("Enter a name!");
            return;
        }

        if (username.Length < MIN_LENGTH)
        {
            ShowError($"Min {MIN_LENGTH} characters!");
            return;
        }

      
        PlayerManager.AddPlayer(username);
        PlayerManager.SetCurrentPlayer(username);

      
        SceneManager.LoadScene(GAME_SCENE);
    }

    void ShowError(string message)
    {
        if (errorText != null)
            errorText.text = message;
    }
}
