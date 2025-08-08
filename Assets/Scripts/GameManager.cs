using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject red, green;
    [SerializeField] private Text turnMessage;
    [SerializeField] private GameObject boardPrefab;

    private GameObject newBoard;
    private Board boardInstance;

    private bool hasGameFinished = false;

    private const string RED_MESSAGE = "Red's Turn";
    private const string GREEN_MESSAGE = "Green's Turn";

    private Color RED_COLOR = new Color(231, 29, 54, 255) / 255;
    private Color GREEN_COLOR = new Color(0, 222, 1, 255) / 255;

    private bool isLocalRed; // Cada cliente sabe se ele é RED ou GREEN com base no turno

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        turnMessage.text = RED_MESSAGE;
        turnMessage.color = RED_COLOR;
    }

    private async void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log($"Client {clientId} connected");
            if (NetworkManager.Singleton.IsHost &&
                NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                SpawnBoard();
            }

            isLocalRed = clientId == NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
        };

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void SpawnBoard()
    {
        newBoard = Instantiate(boardPrefab);
        var netObj = newBoard.GetComponent<NetworkObject>();
        netObj.Spawn();

        boardInstance = newBoard.GetComponent<Board>();
    }

    private void Update()
    {
        if (!IsOwner || hasGameFinished) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (!hit.collider) return;
            if (!hit.collider.CompareTag("Press")) return;

            Column column = hit.collider.GetComponent<Column>();

            if (column.targetlocation.y > 1.5f) return;

            // Envia jogada ao host
            PlayMoveServerRpc(column.col - 1, column.spawnLocation, column.targetlocation);
        }
    }

    // CLIENT → SERVER
    [ServerRpc(RequireOwnership = false)]
    private void PlayMoveServerRpc(int column, Vector3 spawnPos, Vector3 targetPos, ServerRpcParams rpcParams = default)
    {
        if (hasGameFinished || boardInstance == null) return;

        // Valida jogada
        if (!IsValidMove(column)) return;

        PlayerType CurrentPlayer = boardInstance.IsRedTurn ? PlayerType.RED : PlayerType.GREEN;

        // Aplica a jogada
        GridPos appliedPos = boardInstance.ApplyMove(column, CurrentPlayer);
        boardInstance.CurrentPos = appliedPos;

        // Atualiza altura da coluna
        Column[] allColumns = FindObjectsOfType<Column>();
        foreach (Column col in allColumns)
        {
            if (col.col - 1 == column)
            {
                col.targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);
                break;
            }
        }

        // Instancia círculo visual para todos
        SpawnCircleClientRpc(spawnPos, targetPos, (int)CurrentPlayer);

        // Verifica vitória
        if (boardInstance.CheckWin(CurrentPlayer))
        {
            hasGameFinished = true;
            GameFinishedClientRpc((int)CurrentPlayer);
            return;
        }

        // Muda turno
        boardInstance.IsRedTurn = !boardInstance.IsRedTurn;
        ChangeTurnClientRpc(boardInstance.IsRedTurn);
    }

    private bool IsValidMove(int column)
    {
        return boardInstance != null && boardInstance.IsColumnAvailable(column);
    }

    // SERVER → CLIENT
    [ClientRpc]
    private void SpawnCircleClientRpc(Vector3 spawnPos, Vector3 targetPos, int player)
    {
        GameObject circle = Instantiate((PlayerType)player == PlayerType.RED ? red : green);
        circle.GetComponent<Mover>().targetPostion = targetPos;
        circle.transform.position = spawnPos;
    }

    [ClientRpc]
    private void ChangeTurnClientRpc(bool isRedTurn)
    {
        turnMessage.text = isRedTurn ? RED_MESSAGE : GREEN_MESSAGE;
        turnMessage.color = isRedTurn ? RED_COLOR : GREEN_COLOR;
    }

    [ClientRpc]
    private void GameFinishedClientRpc(int winner)
    {
        turnMessage.text = ((PlayerType)winner == PlayerType.RED ? "Red" : "Green") + " Wins!";
        hasGameFinished = true;
    }

    public void GameStart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
