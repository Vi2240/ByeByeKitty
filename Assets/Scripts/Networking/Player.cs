using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickedSomething;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }

    public static Player LocalInstance { get; private set; }

    public event EventHandler OnPickedSomething;

    [SerializeField] private List<Vector3> spawnPositionList;
    [SerializeField] private PlayerVisual playerVisual;

    private MyObject myObject;

    private void Start()
    {
        PlayerData playerData = MyGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        if (MyGameMultiplayer.Instance.GetPlayerColor(playerData.colorId) == null)
            Debug.Log("nooooooo- it's null, what do we do :(((( ");
        playerVisual.SetPlayerColor(MyGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[MyGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasMyObject())
        {
            MyObject.DestroyMyObject(GetMyObject());
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {}

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {}

    public void SetMyObject(MyObject myObject)
    {
        this.myObject = myObject;

        if (myObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public MyObject GetMyObject() { return myObject; }
    public void ClearMyObject() { myObject = null; }
    public bool HasMyObject() { return myObject != null; }
    public NetworkObject GetNetworkObject() { return NetworkObject; }
}