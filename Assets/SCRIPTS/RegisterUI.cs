using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RegisterUI : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("UI")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button goToLoginButton; // optional
    [SerializeField] private TMP_Text statusText;    // optional

    [Header("Scene names (must be in Build Settings)")]
    [SerializeField] private string nextScene = "Gameplay";
    [SerializeField] private string loginScene = "LOGIN";

    private void Start()
    {
        createButton.onClick.AddListener(OnCreateClicked);
        if (goToLoginButton != null)
            goToLoginButton.onClick.AddListener(() => SceneManager.LoadScene(loginScene));
    }

    private void OnCreateClicked()
    {
        string email = emailInput.text.Trim();
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetStatus("Please fill in all fields.");
            return;
        }

        createButton.interactable = false;
        SetStatus("Creating account...");

        AuthManager.Instance.Register(email, username, password, (success, message) =>
        {
            SetStatus(message);
            if (success) SceneManager.LoadScene(nextScene);
            else createButton.interactable = true;
        });
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }
}