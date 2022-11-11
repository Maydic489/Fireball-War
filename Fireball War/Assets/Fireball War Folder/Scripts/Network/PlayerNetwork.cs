using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> playerName = new NetworkVariable<FixedString128Bytes>();

    private void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("On spawn");
        base.OnNetworkSpawn();
        if (IsOwner)
            return;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "HostScene")
        {
            Debug.Log("host scene");
            MainHostUIController.Instance.SetPlayerSlotName(playerName.Value.ToString(), NetworkObject.IsOwnedByServer);
        }
    }

    [ServerRpc]
    public void SetPlayerNameServerRpc(string value, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        playerName.Value = value;

        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            SetPlayerNameClientRpc(playerName.Value.ToString());
        }
    }

    [ClientRpc]
    void SetPlayerNameClientRpc(string playerName,ClientRpcParams clientRpcParams = default)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "HostScene")
        {
            Debug.Log("set from relay");
            MainHostUIController.Instance.SetPlayerSlotName(playerName, NetworkObject.IsOwnedByServer);
        }
    }

    ClientRpcParams GetClientParam(ulong clientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        return clientRpcParams;
    }
}
