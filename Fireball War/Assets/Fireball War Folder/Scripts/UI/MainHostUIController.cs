using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class MainHostUIController : NetworkBehaviour
{
    public TextMeshProUGUI hostRoomCode;
    public TMP_InputField roomCodeInputField;

    [SerializeField]
    GameObject hostingUIGroup;
    [SerializeField]
    GameObject waitingRoomUIGroup;
    [SerializeField]
    Button startButton;
    [SerializeField]
    GameObject PlayerSlot;
    [SerializeField]
    TextMeshProUGUI p1Name;
    [SerializeField]
    TextMeshProUGUI p2Name;
    public TMP_InputField playerNameInputField;

    [HideInInspector]
    public PlayerNetwork localPlayer;

    public static MainHostUIController Instance { get; private set; }
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
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        NetworkManager.Singleton.OnClientConnectedCallback += OnNewPlayerConnect;
    }

    void OnNewPlayerConnect(ulong clientId)
    {
        if (IsClient && !IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnNewPlayerConnect;
        }

        if (IsHost && NetworkManager.Singleton.ConnectedClientsIds.Count > 1)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnNewPlayerConnect;

            startButton.interactable = true;
            MainNetworkGameManager.Instance.PingCalculateStart();
        }
    }

    public void RequestStartGame()
    {
        NetworkManager.SceneManager.LoadScene("Gameplay",UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void HideHostingUI()
    {
        hostingUIGroup.SetActive(false); 
    }
    public void ShowWaitingRoomUI()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            waitingRoomUIGroup.SetActive(true);
        }
        else if(NetworkManager.Singleton.IsClient)
        {

        }

        PlayerSlot.SetActive(true);
    }

    public void SetPlayerSlotName(string authenID, bool isTheHost)
    {
        if (isTheHost)
        {
            p1Name.text = authenID;
        }
        else if(!isTheHost)
        {
            p2Name.text = authenID;
        }
    }

    public void CopyRoomCodeToClipboard()
    {
        GUIUtility.systemCopyBuffer = NGORelayManager.Instance.RelayJoinCode;
    }
}
