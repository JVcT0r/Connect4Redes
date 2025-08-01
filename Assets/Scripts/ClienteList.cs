using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine;
//using UnityEngine.UI;

public class ClienteList : MonoBehaviour
{
    public Transform redBall;
    public Transform greenBall;
    public Transform circleWhite;
    
    private Vector2 lastSendPosition;
    private Vector2 initialRedPosition;
    private Vector2 initialGreenPosition;
    private Vector2 initialWhitePosition;
    private bool myTurn = true;
    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToServer();
        
        initialRedPosition = redBall.position;
        initialGreenPosition = greenBall.position;
        initialWhitePosition = circleWhite.position;
        lastSendPosition = circleWhite.position;
    }
    void Update()
    {
        if (!myTurn || client == null || !client.Connected) return;
        Vector2 currentPos = circleWhite.position;
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
    void SendActionMessage()
    {
        Vector2 whitePos = circleWhite.position;
        Vector2 greenPos = greenBall.position;
        Vector2 redPos = redBall.position;
        string mensagem = $"{{\"type\":\"action\"," +
                          $"\"circleWhite\":{{\"x\":{whitePos.x},\"y\":{whitePos.y}}}," +
                          $"\"greenBall\":{{\"x\":{greenPos.x},\"y\":{greenPos.y}}}" +
                          $"\"redBall\":{{\"x\":{redPos.x},\"y\":{redPos.y}}}}}" + 
                          $"\"initialPositions\":{{" +
                            $"\"circleWhite\":{{\"x\":{initialWhitePosition.x},\"y\":{initialWhitePosition.y}}}," +
                            $"\"greenBall\":{{\"x\":{initialGreenPosition.x},\"y\":{initialGreenPosition.y}}}," +
                            $"\"redBall\":{{\"x\":{initialRedPosition.x},\"y\":{initialRedPosition.y}}}" +
                          $"}}}}";
        SendToServer(mensagem);
    }
    void SendEndTurn()
    {
        string mensagem = "{\"type\":\"senEndTurn\"}";
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
        stream?.Close();
        client?.Close();
    }
}

