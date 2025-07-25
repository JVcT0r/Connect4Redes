using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class ClienteList : MonoBehaviour
{
    //public List<Transform> positionBall;
    public Transform redBall;
    public Transform greenBall;
    public Transform circleWhite;
    public GameObject tchecker;
    public InputField input;
    public Button sendButton;
    
    private TcpClient client;
    private NetworkStream stream;
    private Thread sendThread;
    private bool isRunning = false;

    void Start()
    {
        ConnectToServer();
        sendButton.onClick.AddListener(SendManualMessage);
        
        isRunning = true;
        sendThread = new Thread(SendPositionsLoop);
        sendThread.Start();
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 8080);
            stream = client.GetStream();
            Debug.Log("Conectado ao Servidor");
        }
        catch (Exception e)
        {
            Debug.Log("Erro ao conectar" + e.Message);
        }
    }
    void SendManualMessage()
    {
        string mensagem = input.text;
        if (string.IsNullOrWhiteSpace(mensagem)) return;
        //SendToServer(mensagem);
    }

    void SendPositionsLoop()
    {
        while (isRunning)
        {
            if (client != null && stream != null && stream.CanWrite)
            {
                Vector2 redPos = redBall.position;
                Vector2 greenPos = greenBall.position;
                Vector2 whitePos = circleWhite.position;
                
                //string posData = $"{{\"red
            }
        }
    }
    
}

