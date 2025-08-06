using System.Net.Sockets;
using System.Text;
using System;
using System.Threading;
using UnityEngine;

public class ClienteList : MonoBehaviour
{
    public Transform redBall;
    public Transform greenBall;
    
    private Vector2 lastSendPosition;
    private Vector2 initialRedPosition;
    private Vector2 initialGreenPosition;
    private bool myTurn = true;
    private TcpClient client;
    private NetworkStream stream;
    private Thread listenThread;

    void Start()
    {
        ConnectToServer();

        listenThread = new Thread(ListenServer);
        listenThread.IsBackground = true;
        listenThread.Start();
        
        initialRedPosition = redBall.position;
        initialGreenPosition = greenBall.position;
        lastSendPosition = redBall.position;
    }
    void Update()
    {
        if (!myTurn || client == null || !client.Connected) return;
        Vector2 currentPos = redBall.position;
        if (currentPos != lastSendPosition)
        {
            SendActionMessage();
            lastSendPosition = currentPos;
            
            SendEndTurn();
            myTurn = false;
        }
    }
    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 8080);
            stream = client.GetStream();
            Debug.Log("Conectado ao Servidor de turnos");
        }
        catch (Exception e)
        {
            Debug.Log("Erro ao conectar" + e.Message);
        }
    }
    void ListenServer()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (client != null && client.Connected)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0) continue;
                
                string mensagemRecebida = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("[Cliente] Mensagem recebida do servidor: " + mensagemRecebida);
                if (mensagemRecebida.Contains("\type\":\"action\""))
                {
                    myTurn = true;
                    Debug.Log("[Cliente] Seu turno começou.");
                }
                if (mensagemRecebida.Contains("\"Error\""))
                {
                    Debug.LogWarning("[Cliente] Aviso do servidor" + mensagemRecebida);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Cliente] Erro ao escutar servidor" + e.Message);
        }
    }
    void SendActionMessage()
    {
        Vector2 greenPos = greenBall.position;
        Vector2 redPos = redBall.position;
        string mensagem = $"{{\"type\":\"action\"," +
                          $"\"greenBall\":{{\"x\":{greenPos.x},\"y\":{greenPos.y}}}," +
                          $"\"redBall\":{{\"x\":{redPos.x},\"y\":{redPos.y}}}}}," + 
                          $"\"initialPositions\":{{" +
                            $"\"greenBall\":{{\"x\":{initialGreenPosition.x},\"y\":{initialGreenPosition.y}}}," +
                            $"\"redBall\":{{\"x\":{initialRedPosition.x},\"y\":{initialRedPosition.y}}}" +
                          $"}}}}";
        SendToServer(mensagem);
    }
    void SendEndTurn()
    {
        string mensagem = "{\"type\":\"sendEndTurn\"}";
        SendToServer(mensagem);
    }
    void SendToServer(string mensagem)
    {
        try 
        {
            if (stream != null && stream.CanWrite)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(mensagem);
                stream.Write(buffer, 0, buffer.Length);
                Debug.Log("Enviado" + mensagem);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Erro ao Enviar" + e.Message);
        }
    }
    void OnApplicationQuit()
    {
        listenThread.Abort();
        stream?.Close();
        client?.Close();
    }
}

