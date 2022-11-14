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

    public InputData P1InputData = new InputData();
    public InputData P2InputData = new InputData();

    [SerializeReference]
    public float currentPing;
    public int currentDelay;
    public int currentFrame;

    public float startPingTimer;
    public int startPingFrame;
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
            ClearCurrentInput(true, true);

            if (IsHost)
                currentFrame = -1;
            else
                currentFrame = 0;
            //StartCoroutine(ClearCurrentInput());
        }
    }

    public override void OnNetworkSpawn()
    {
        isServer = NetworkManager.Singleton.IsServer;

        if (IsClient && !IsHost)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += onLoadComplete; ;
        }
    }

    private void onLoadComplete(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        ResumeGameServerRpc();
        StartCoroutine(DelayResumeGame());
    }

    IEnumerator DelayResumeGame()
    {
        int latestDelay = currentDelay;

        for (int i = 0; i < latestDelay; i++)
        {
            yield return null;
        }

        Time.timeScale = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    void ResumeGameServerRpc()
    {
        Time.timeScale = 1;
    }

    private void Update()
    {
        if ((IsHost || IsClient) && MainGameManager.Instance != null && MainGameManager.Instance._gameState != MainGameManager.GameState.PreStart)
        {
            if (IsHost)
            {
                if (P2InputData.Frame != 0 && P2InputData.Frame + currentDelay < currentFrame && Time.timeScale != 0)
                {
                    Debug.Log("stop time host");
                    Time.timeScale = 0;
                }
                else if (P2InputData.Frame + currentDelay >= currentFrame && Time.timeScale == 0)
                {
                    Debug.Log("resume time host");
                    Time.timeScale = 1;
                }
            }
            else
            {
                if (P1InputData.Frame != 0 && P1InputData.Frame + currentDelay < currentFrame && Time.timeScale != 0)
                {
                    Debug.Log("stop time client");
                    Time.timeScale = 0;
                }
                else if (P1InputData.Frame + currentDelay >= currentFrame && Time.timeScale == 0)
                {
                    Debug.Log("resume time client");
                    Time.timeScale = 1;
                }
            }
        }

        if (MainGameManager.Instance != null && Time.timeScale == 1)
        {
            currentFrame++;
            Debug.Log("count frame " + currentFrame);
        }
    }

    [ClientRpc]
    public void UpdatePlayerInputClientRpc(string newInput, int frame,bool isPlayer1)
    {
        UpdatePlayerInput(newInput,frame,isPlayer1);
    }

    void UpdatePlayerInput(string newInput, int frame, bool isPlayer1)
    {
        if (isPlayer1 && !IsHost)
        {
            p1CurrentInput = newInput;
            P1InputData.input = newInput;
            P1InputData.Frame = frame;
            Debug.Log("update input " + (frame + currentDelay));
        }
        else if (!isPlayer1 && IsHost)
        {
            p2CurrentInput = newInput;
            P2InputData.input = newInput;
            P2InputData.Frame = frame;
            Debug.Log("update input " + (frame + currentDelay));
        }
    }

    public IEnumerator DelayUpdatePlayerInput(string newInput, bool isPlayer1)//local delay so the other player see the same timing
    {
        int latestDelay = currentDelay;

        for(int i=0;i<latestDelay;i++)
        {
            yield return null;
        }

        if (isPlayer1)
            MainNetworkGameManager.Instance.p1CurrentInput = newInput;
        else
            MainNetworkGameManager.Instance.p2CurrentInput = newInput;
    }

    public void ClearCurrentInput(bool isPlayer1, bool clearAll = false)
    {
        if (isPlayer1)
            p1CurrentInput = "";
        else
            p2CurrentInput = "";

        if(clearAll)
        {
            p1CurrentInput = "";
            p2CurrentInput = "";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RestartGamePlayServerRpc()
    {
        NetworkManager.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void PingCalculateStart()
    {
        startPingTimer = Time.timeSinceLevelLoad;
        startPingFrame = Time.frameCount;
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
        currentDelay = (Time.frameCount - startPingFrame) / 2;
        startPingFrame = Time.frameCount;

        //Debug.Log("Delay " + currentDelay);
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

public class InputData
{
    public string input;
    public int Frame;
}
