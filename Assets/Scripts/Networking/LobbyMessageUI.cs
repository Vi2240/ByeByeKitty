
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{


    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;


    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        MyGameMultiplayer.Instance.OnFailedToJoinGame += MyGameMultiplayer_OnFailedToJoinGame;
        MyGameLobby.Instance.OnCreateLobbyStarted += MyGameLobby_OnCreateLobbyStarted;
        MyGameLobby.Instance.OnCreateLobbyFailed += MyGameLobby_OnCreateLobbyFailed;
        MyGameLobby.Instance.OnJoinStarted += MyGameLobby_OnJoinStarted;
        MyGameLobby.Instance.OnJoinFailed += MyGameLobby_OnJoinFailed;
        MyGameLobby.Instance.OnQuickJoinFailed += MyGameLobby_OnQuickJoinFailed;

        Hide();
    }

    private void MyGameLobby_OnQuickJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Could not find a Lobby to Quick Join!");
    }

    private void MyGameLobby_OnJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join Lobby!");
    }

    private void MyGameLobby_OnJoinStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Joining Lobby...");
    }

    private void MyGameLobby_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to create Lobby!");
    }

    private void MyGameLobby_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    private void MyGameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
        {
            ShowMessage("Failed to connect");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void ShowMessage(string message)
    {
        Show();
        messageText.text = message;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        MyGameMultiplayer.Instance.OnFailedToJoinGame -= MyGameMultiplayer_OnFailedToJoinGame;
        MyGameLobby.Instance.OnCreateLobbyStarted -= MyGameLobby_OnCreateLobbyStarted;
        MyGameLobby.Instance.OnCreateLobbyFailed -= MyGameLobby_OnCreateLobbyFailed;
        MyGameLobby.Instance.OnJoinStarted -= MyGameLobby_OnJoinStarted;
        MyGameLobby.Instance.OnJoinFailed -= MyGameLobby_OnJoinFailed;
        MyGameLobby.Instance.OnQuickJoinFailed -= MyGameLobby_OnQuickJoinFailed;
    }

}