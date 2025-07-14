/*using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpServerUnity : MonoBehaviour
{
    TcpListener server;
    Thread serverThread;
    List<TcpClient> clients = new List<TcpClient>();

    void Start()
    {
        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 8080);
        server.Start();
        Debug.Log("Servidor ouvindo na porta 8080...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);
            Debug.Log("Cliente conectado!");

            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.IsBackground = true;
            clientThread.Start();
        }
    }

    void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (client.Connected)
        {
            int len = stream.Read(buffer, 0, buffer.Length);
            if (len == 0) break;

            string msg = Encoding.UTF8.GetString(buffer, 0, len);
            Debug.Log($"[Servidor] Recebido: {msg}");

            foreach (var c in clients)
            {
                if (c != client && c.Connected)
                {
                    NetworkStream s = c.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    s.Write(data, 0, data.Length);
                }
            }
        }

        client.Close();
        clients.Remove(client);
        Debug.Log("Cliente desconectado.");
    }

    void OnApplicationQuit()
    {
        server?.Stop();
        serverThread?.Abort();
    }
}*/

