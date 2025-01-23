using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MyObject : NetworkBehaviour
{


    [SerializeField] private MyObjectSO myObjectSO;


    private IMyObjectParent myObjectParent;
    private FollowTransform followTransform;


    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public MyObjectSO GetMyObjectSO()
    {
        return myObjectSO;
    }

    public void SetMyObjectParent(IMyObjectParent myObjectParent)
    {
        SetMyObjectParentServerRpc(myObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMyObjectParentServerRpc(NetworkObjectReference myObjectParentNetworkObjectReference)
    {
        SetMyObjectParentClientRpc(myObjectParentNetworkObjectReference);
    }

    [ClientRpc]
    private void SetMyObjectParentClientRpc(NetworkObjectReference myObjectParentNetworkObjectReference)
    {
        myObjectParentNetworkObjectReference.TryGet(out NetworkObject myObjectParentNetworkObject);
        IMyObjectParent myObjectParent = myObjectParentNetworkObject.GetComponent<IMyObjectParent>();

        if (this.myObjectParent != null)
        {
            this.myObjectParent.ClearMyObject();
        }

        this.myObjectParent = myObjectParent;

        if (myObjectParent.HasMyObject())
        {
            Debug.LogError("IMyObjectParent already has a MyObject!");
        }

        myObjectParent.SetMyObject(this);

        followTransform.SetTargetTransform(myObjectParent.GetMyObjectFollowTransform());
    }

    public IMyObjectParent GetMyObjectParent()
    {
        return myObjectParent;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void ClearMyObjectOnParent()
    {
        myObjectParent.ClearMyObject();
    }

    public static void SpawnMyObject(MyObjectSO myObjectSO, IMyObjectParent myObjectParent)
    {
        MyGameMultiplayer.Instance.SpawnMyObject(myObjectSO, myObjectParent);
    }

    public static void DestroyMyObject(MyObject myObject)
    {
        MyGameMultiplayer.Instance.DestroyMyObject(myObject);
    }

}