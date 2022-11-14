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

    public bool isInputUsedP1;
    public bool isInputUsedP2;

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
        if (IsClient && !IsHost)
        {
            Debug.Log("subscribe");
            NetworkManager.Singleton.SceneManager.OnLoadComplete += onLoadComplete; ;
        }
    }

    private void onLoadComplete(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        var ping = localPlayer.gameObject.GetComponent<Unity.BossRoom.Utils.NetworkStats>().m_UtpRTT.Average;
        Debug.Log("ping " + ping+" "+ (ping / 2f) / 1000f);
        ResumeGameServerRpc();
        StartCoroutine(DelayResumeGame((ping / 2f) / 1000f));
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
        if (NetworkManager.Singleton.IsClient)
        {
            var ping = localPlayer.gameObject.GetComponent<Unity.BossRoom.Utils.NetworkStats>().m_UtpRTT.Average;
            currentPing = (ping / 2f) / 1000f;
        }
    }

    [ClientRpc]
    public void UpdatePlayerInputClientRpc(string newInput,bool isPlayer1)
    {
        if (isPlayer1 && !IsHost)
            p1CurrentInput = newInput;
        else if(!isPlayer1 && IsHost)
            p2CurrentInput = newInput;
    }

    public IEnumerator DelayUpdatePlayerInput(string newInput, bool isPlayer1)
    {
        yield return new WaitForSecondsRealtime(currentPing);

        Debug.Log("wait realtime");

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
}
