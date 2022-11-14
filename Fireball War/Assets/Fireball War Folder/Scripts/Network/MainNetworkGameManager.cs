using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainNetworkGameManager : NetworkBehaviour
{
    [HideInInspector]
    public PlayerNetwork localPlayer;

    public string p1CurrentInput;
    public string p2CurrentInput;

    [SerializeReference]
    public float currentPing;

    public float startPingTimer;
    public bool isServer;

    public static MainNetworkGameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void OnLevelWasLoaded(int level)
    {
        if (IsHost || IsClient)
        {
            Time.timeScale = 0;
            //StartCoroutine(ClearCurrentInput());
        }
    }

    public override void OnNetworkSpawn()
    {
        isServer = NetworkManager.Singleton.IsServer;

        if (IsClient && !IsHost)
        {
            Debug.Log("subscribe");
            NetworkManager.Singleton.SceneManager.OnLoadComplete += onLoadComplete; ;
        }
    }

    private void onLoadComplete(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        ResumeGameServerRpc();
        StartCoroutine(DelayResumeGame((currentPing) / 1000f));
    }

    IEnumerator DelayResumeGame(float delayTime)
    {
        yield return new WaitForSecondsRealtime(delayTime);
        Time.timeScale = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    void ResumeGameServerRpc()
    {
        Time.timeScale = 1;
    }

    private void Update()
    {

    }

    [ClientRpc]
    public void UpdatePlayerInputClientRpc(string newInput,bool isPlayer1)
    {
        if (isPlayer1 && !IsHost)
            p1CurrentInput = newInput;
        else if(!isPlayer1 && IsHost)
            p2CurrentInput = newInput;
    }

    public IEnumerator DelayUpdatePlayerInput(string newInput, bool isPlayer1)//local delay so the other player see the same timing
    {
        yield return new WaitForSecondsRealtime(currentPing);

        if (isPlayer1)
            MainNetworkGameManager.Instance.p1CurrentInput = newInput;
        else
            MainNetworkGameManager.Instance.p2CurrentInput = newInput;
    }

    public void ClearCurrentInput(bool isPlayer1)
    {
        if (isPlayer1)
            p1CurrentInput = "";
        else
            p2CurrentInput = "";
    }

    [ServerRpc(RequireOwnership = false)]
    public void RestartGamePlayServerRpc()
    {
        NetworkManager.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void PingCalculateStart()
    {
        startPingTimer = Time.timeSinceLevelLoad;
        SendPingServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SendPingServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (clientId == 0)
            RecievPingClientRpc(GetClientParam(1));
        else if(clientId == 1)
            RecievPingClientRpc(GetClientParam(0));
    }

    [ClientRpc]
    void RecievPingClientRpc(ClientRpcParams clientRpcParams = default)
    {
        currentPing = (Time.timeSinceLevelLoad - startPingTimer) / 2;
        startPingTimer = Time.timeSinceLevelLoad;
        SendPingServerRpc();
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
