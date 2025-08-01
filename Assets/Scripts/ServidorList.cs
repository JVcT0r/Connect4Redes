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
                    Debug.Log($"[Servidor] Cliente conectado. Total: {clients.Count}");
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
        try
        {
            while (client.Connected)
            {
                byte[] buffer = new byte[2048];
                int lenght = stream.Read(buffer, 0, buffer.Length);

                if (lenght <= 0) break;

                string msg = Encoding.UTF8.GetString(buffer, 0, lenght);
                Debug.Log($"[Servidor] Mensagem Recebida: {msg}");
                lock (lockObject)
                {
                    int clientIndex = clients.IndexOf(client);
                    if (clientIndex != currentPlayerIndex)
                    {
                        Debug.Log("[Servidor] Jogador fora de turno");
                        SendMessageToClient(client, "{\"error\":\"Nâo é seu turno\"}");
                    }

                    int nextPlayerIndex = (currentPlayerIndex + 1) % clients.Count;
                    if (clients.Count > 1)
                    {
                        TcpClient nextPlayer = clients[nextPlayerIndex];
                        SendMessageToClient(nextPlayer, msg);
                    }

                    currentPlayerIndex = nextPlayerIndex;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Servidor] Erro no Cliente" + e.Message);
        }
        finally
        {
            lock (clients)
            {
                clients.Remove(client);
            }
            stream.Close();
            client.Close();
        }
    }

    void SendMessageToClient(TcpClient client, string mensagem)
    {
        try
        {
            if (client.Connected)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(mensagem);
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Servidor] Erro ao enviar mensagem" + e.Message);
        }
    }
    void OnApplicationQuit()
    {
        server?.Stop();
        serverThread?.Abort();

        foreach (TcpClient client in clients)
        {
            client?.Close();
        }
    }
}

