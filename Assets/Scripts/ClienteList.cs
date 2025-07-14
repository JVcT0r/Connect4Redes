using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TcpClientUnity : MonoBehaviour
{
    public List<Transform> positionBall;
    [SerializeField]
    public Transform red;
    public Transform green;
    public Transform circle;

    public Vector2 redPosition;
    public Vector2 greenPosition;
    public Vector2 circlePosition;
    public GameObject tchecker;
    public InputField input;
    public Button sendButton;

    void Start()
    {
        List<string> positionBall = new List<string>();
        sendButton.onClick.AddListener(SendMessageToServer);
        Update();
        
    }

    void Update()
    {
        redPosition = red.position;
        greenPosition = green.position;
        circlePosition = circle.position;
    }
    void SendMessageToServer()
    {
        string mensagem = input.text;
        if (string.IsNullOrWhiteSpace(mensagem)) return;

        TcpClient client = new TcpClient("127.0.0.1", 8080);
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.UTF8.GetBytes(mensagem);
        stream.Write(data, 0, data.Length);
        stream.Close();
        client.Close();
    }
}

