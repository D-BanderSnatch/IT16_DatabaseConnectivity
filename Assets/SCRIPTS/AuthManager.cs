using System;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    public bool IsReady { get; private set; }
    public FirebaseUser CurrentUser => auth != null ? auth.CurrentUser : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                IsReady = true;
                Debug.Log("Firebase Auth ready");
            }
            else
            {
                Debug.LogError("Firebase dependencies not available: " + task.Result);
            }
        });
    }

    // onDone(success, message)
    public void Register(string email, string username, string password, Action<bool, string> onDone)
    {
        if (!IsReady) { onDone?.Invoke(false, "Still connecting, try again in a moment."); return; }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                onDone?.Invoke(false, GetErrorMessage(task.Exception));
                return;
            }

            FirebaseUser user = task.Result.User;

            // Store the username on the auth profile
            var profile = new UserProfile { DisplayName = username };
            user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(_ =>
            {
                // Create the player's record in the database
                if (DatabaseManager.Instance != null)
                {
                    DatabaseManager.Instance.CreatePlayer(user.UserId, username);
                }
                onDone?.Invoke(true, "Account created!");
            });
        });
    }

    public void Login(string email, string password, Action<bool, string> onDone)
    {
        if (!IsReady) { onDone?.Invoke(false, "Still connecting, try again in a moment."); return; }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                onDone?.Invoke(false, GetErrorMessage(task.Exception));
                return;
            }
            onDone?.Invoke(true, "Logged in!");
        });
    }

    public void Logout()
    {
        if (auth != null) auth.SignOut();
    }

    private string GetErrorMessage(Exception ex)
    {
        Debug.LogError("Auth error (full): " + ex);

        FirebaseException fbEx = ex != null ? ex.GetBaseException() as FirebaseException : null;
        if (fbEx == null) return "Something went wrong. Please try again.";

        Debug.LogError("Auth error code: " + (AuthError)fbEx.ErrorCode + " (" + fbEx.ErrorCode + ")");

        switch ((AuthError)fbEx.ErrorCode)
        {
            case AuthError.EmailAlreadyInUse: return "That email is already registered.";
            case AuthError.InvalidEmail: return "That email address isn't valid.";
            case AuthError.WeakPassword: return "Password must be at least 6 characters.";
            case AuthError.MissingEmail: return "Please enter your email.";
            case AuthError.MissingPassword: return "Please enter your password.";
            case AuthError.WrongPassword:
            case AuthError.UserNotFound: return "Incorrect email or password.";
            case AuthError.NetworkRequestFailed: return "Network error. Check your connection.";
            default: return fbEx.Message;
        }
    }
}