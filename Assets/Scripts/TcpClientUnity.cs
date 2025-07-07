// using System.Net.Sockets;
// using System.Text;
// using System.Threading;
// using UnityEngine;
//
// public class TcpClientUnity : MonoBehaviour
// {
//     public string serverIP = "127.0.0.1";
//     TcpClient client;
//     NetworkStream stream;
//     Thread listenThread;
//     public GameManager gameManager;
//
//     void Start()
//     {
//         client = new TcpClient(serverIP, 8080);
//         stream = client.GetStream();
//
//         listenThread = new Thread(ListenToServer);
//         listenThread.IsBackground = true;
//         listenThread.Start();
//     }
//
//     void ListenToServer()
//     {
//         byte[] buffer = new byte[1024];
//
//         while (client.Connected)
//         {
//             int len = stream.Read(buffer, 0, buffer.Length);
//             if (len == 0) break;
//
//             string msg = Encoding.UTF8.GetString(buffer, 0, len);
//             Debug.Log($"[Cliente] Recebido: {msg}");
//
//             string[] parts = msg.Split('|');
//             int col = int.Parse(parts[0]);
//             string player = parts[1];
//
//             UnityMainThreadDispatcher.Instance().Enqueue(() =>
//             {
//                 gameManager.ApplyRemoteMove(col, player);
//             });
//         }
//     }
//
//     public void SendMove(int col, string player)
//     {
//         string msg = $"{col}|{player}";
//         byte[] data = Encoding.UTF8.GetBytes(msg);
//         stream.Write(data, 0, data.Length);
//     }
//
//     void OnApplicationQuit()
//     {
//         stream?.Close();
//         client?.Close();
//         listenThread?.Abort();
//     }
// }
//
//
