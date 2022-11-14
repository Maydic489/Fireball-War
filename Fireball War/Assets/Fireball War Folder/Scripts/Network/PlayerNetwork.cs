using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    string currentInput;

    public NetworkVariable<FixedString128Bytes> playerName = new NetworkVariable<FixedString128Bytes>();

    private void Start()
    {

    }

    private void Update()
    {
        if(IsLocalPlayer && IsClient)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Joystick1Button2))
            {
                currentInput = "fb1";
            }
            else if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button3))
            {
                currentInput = "fb2";
            }
            else
            {
                currentInput = "";
            }

            if (!string.IsNullOrEmpty(currentInput))
            {
                StartCoroutine(MainNetworkGameManager.Instance.DelayUpdatePlayerInput(currentInput, IsHost));
                SendInputServerRpc(currentInput, IsHost);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendInputServerRpc(string currentInput, bool isPlayer1)
    {
        MainNetworkGameManager.Instance.UpdatePlayerInputClientRpc(currentInput, isPlayer1);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
            return;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "HostScene")
        {
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
