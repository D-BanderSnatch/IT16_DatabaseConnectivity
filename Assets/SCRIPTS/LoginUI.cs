using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("UI")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button goToRegisterButton; // optional
    [SerializeField] private TMP_Text statusText;       // optional

    [Header("Scene names (must be in Build Settings)")]
    [SerializeField] private string nextScene = "Gameplay";
    [SerializeField] private string registerScene = "REGISTER";

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        if (goToRegisterButton != null)
            goToRegisterButton.onClick.AddListener(() => SceneManager.LoadScene(registerScene));
    }

    private void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter your email and password.");
            return;
        }

        loginButton.interactable = false;
        SetStatus("Logging in...");

        AuthManager.Instance.Login(email, password, (success, message) =>
        {
            SetStatus(message);
            if (success) SceneManager.LoadScene(nextScene);
            else loginButton.interactable = true;
        });
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }
}