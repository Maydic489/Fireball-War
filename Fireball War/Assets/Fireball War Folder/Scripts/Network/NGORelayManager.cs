using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using NetworkEvent = Unity.Networking.Transport.NetworkEvent;

public class NGORelayManager : MonoBehaviour
{
    const int m_MaxConnections = 2;

    string playerAuthenID;//not use currently
    public string RelayJoinCode;

    public static NGORelayManager Instance { get; private set; }
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

    public void StartHostRelay()
    {
        if (string.IsNullOrEmpty(MainHostUIController.Instance.playerNameInputField.text))
        {
            Debug.Log("Enter Name!");
            return;
        }

        Example_AuthenticatingAPlayer();
        StartCoroutine(Example_ConfigureTransportAndStartNgoAsHost());
    }

    public void JoiningRelay()
    {
        if (string.IsNullOrEmpty(MainHostUIController.Instance.roomCodeInputField.text) || string.IsNullOrEmpty(MainHostUIController.Instance.playerNameInputField.text))
        {
            Debug.Log("Enter Join Code or Name!");
            return;
        }
        else
        {
            RelayJoinCode = MainHostUIController.Instance.roomCodeInputField.text;
        }
        Example_AuthenticatingAPlayer();
        StartCoroutine(Example_ConfigreTransportAndStartNgoAsConnectingPlayer());
    }

    public async void Example_AuthenticatingAPlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerAuthenID = AuthenticationService.Instance.PlayerId;
            //Debug.Log("Authen with Player ID " + playerAuthenID);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public static async Task<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
    {
        Allocation allocation;
        string createJoinCode;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        //Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.AllocationId} {allocation.Region}");

        try
        {
            createJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            //Debug.Log("Join Code is " + createJoinCode);
            NGORelayManager.Instance.RelayJoinCode = createJoinCode;
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    }

    void SetRelayJoinCode(string joinCode)
    {
        RelayJoinCode = joinCode;
    }

    public IEnumerator Example_ConfigureTransportAndStartNgoAsHost()
    {
        while(AuthenticationService.Instance == null || !AuthenticationService.Instance.IsAuthorized)
        {
            yield return null;
        }

        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(m_MaxConnections/*, "asia-southeast1"*/);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }
        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = serverRelayUtilityTask.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();

        while(!NetworkManager.Singleton.IsHost)//wait for connection
        {
            yield return null;
        }

        MainHostUIController.Instance.localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>();
        MainHostUIController.Instance.localPlayer.SetPlayerNameServerRpc(MainHostUIController.Instance.playerNameInputField.text);
        MainHostUIController.Instance.HideHostingUI();
        MainHostUIController.Instance.ShowWaitingRoomUI();
        MainHostUIController.Instance.hostRoomCode.text = RelayJoinCode;
        MainHostUIController.Instance.CopyRoomCodeToClipboard();

        yield return null;
    }

    public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        //Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        //Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        //Debug.Log($"client: {allocation.AllocationId}");

        return new RelayServerData(allocation, "dtls");
    }

    public IEnumerator Example_ConfigreTransportAndStartNgoAsConnectingPlayer()
    {
        while (AuthenticationService.Instance == null || !AuthenticationService.Instance.IsAuthorized)
        {
            yield return null;
        }

        // Populate RelayJoinCode beforehand through the UI
        var clientRelayUtilityTask = JoinRelayServerFromJoinCode(RelayJoinCode);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = clientRelayUtilityTask.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        while(!NetworkManager.Singleton.IsConnectedClient)
        {
            yield return null;
        }

        MainHostUIController.Instance.localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>();
        MainHostUIController.Instance.localPlayer.SetPlayerNameServerRpc(MainHostUIController.Instance.playerNameInputField.text);
        MainHostUIController.Instance.HideHostingUI();
        MainHostUIController.Instance.ShowWaitingRoomUI();

        yield return null;
    }
}
