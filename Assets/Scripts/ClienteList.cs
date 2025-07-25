using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ClienteList : MonoBehaviour
{
    public Transform redBall;
    public Transform greenBall;
    public Transform circleWhite;
    public InputField input;
    public Button sendButton;
    public Button endTurnButton;
    
    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToServer();
        sendButton.onClick.AddListener(SendActionMessage);
        endTurnButton.onClick.AddListener(SendEndTurn);
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
    void SendActionMessage()
    {
        if (client == null || client.Connected) return;
        string userText = input.text.Trim();
        Vector2 circleWhitePos = circleWhite.position;
        string mensagem = $"{{\"type\":\"action\",\"text\":\"{userText}\", \"circleWhite\":{{\"x\":{circleWhitePos.x},\"y\":{circleWhitePos.y}}}}}";
        SendToServer(mensagem);
    }
    void SendEndTurn()
    {
        if (client == null || !client.Connected) return;
        
        string mensagem = "{\"type\":\"senEndTurn\"}";
        SendToServer(mensagem);
    }
    void SendToServer(string mensagem)
    {
        try 
        {
            if (stream != null && stream.CanWrite)
            {
                byte[] data = Encoding.UTF8.GetBytes(mensagem);
                stream.Write(data, 0, data.Length);
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
        if (stream != null)
            stream.Close();
        
        if (client != null)
            client.Close();
    }
}

