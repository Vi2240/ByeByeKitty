using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IMyObjectParent
{

    public Transform GetMyObjectFollowTransform();

    public void SetMyObject(MyObject myObject);

    public MyObject GetMyObject();

    public void ClearMyObject();

    public bool HasMyObject();

    public NetworkObject GetNetworkObject();

}