using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;
using System.Collections.Generic;

public class ServidorList : MonoBehaviour
{
    private TcpListener server;
    private Thread serverThread;
    private List<TcpClient> clients = new List<TcpClient>();
    private int currentPlayerIndex = 0;
    private object lockObject = new object();

    void Start()
    {
        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }
    void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, 8080);
            server.Start();
            Debug.Log("[Servidor] ouvindo na porta 8080...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                lock (clients)
                {
                    clients.Add(client);
                    Debug.Log("[Servidor] Cliente conectado. Total: {clients.Count}");
                }
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Servidor] Erro" + e.Message);
        }
    }
    void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        while (client.Connected)
        {
            //byte[] buffer
        }
    }    
    void OnApplicationQuit()
    {
        server?.Stop();
        serverThread?.Abort();
    }
}

