using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine.UI;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class ConnectUiScript : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hostButton.onClick.AddListener(HostButtonOnClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
    }

    [SerializeField] private TextMeshProUGUI JoinCodeText;
    private async void HostButtonOnClick()
    {
        try
        {
            
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            JoinCodeText.text = JoinCode;
            
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    [SerializeField] private TMP_InputField JoinCodeInput;
    private async void ClientButtonOnClick()
    {
        try
        {
           JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(JoinCodeInput.text);
            
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }
}
