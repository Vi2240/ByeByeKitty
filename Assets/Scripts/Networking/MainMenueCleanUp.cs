using Unity.Netcode;
using UnityEngine;

public class MainMenueCleanUp : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if (MyGameMultiplayer.Instance != null)
        {
            Destroy(MyGameMultiplayer.Instance.gameObject);
        }

        if (MyGameLobby.Instance != null)
        {
            Destroy(MyGameLobby.Instance.gameObject);
        }
    }
}
