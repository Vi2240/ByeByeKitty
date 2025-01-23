using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        MyGameMultiplayer.Instance.OnTryingToJoinGame += MyGameMultiplayer_OnTryingToJoinGame;
        MyGameMultiplayer.Instance.OnFailedToJoinGame += MyGameManager_OnFailedToJoinGame;

        Hide();
    }

    private void MyGameManager_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void MyGameMultiplayer_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
        Show();
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
        MyGameMultiplayer.Instance.OnTryingToJoinGame -= MyGameMultiplayer_OnTryingToJoinGame;
        MyGameMultiplayer.Instance.OnFailedToJoinGame -= MyGameManager_OnFailedToJoinGame;
    }

}