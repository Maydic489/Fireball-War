using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainNetworkGameManager : NetworkBehaviour
{
    [HideInInspector]
    public PlayerNetwork localPlayer;

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
            Time.timeScale = 0;
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
}
