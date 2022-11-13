using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainNetworkGameManager : NetworkBehaviour
{
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
        if (IsHost)
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
        Debug.Log("sync complete");
        ResumeGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void ResumeGameServerRpc()
    {
        Debug.Log("Resume");
        Time.timeScale = 1;
    }
}
