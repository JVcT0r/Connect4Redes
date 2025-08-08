using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject red, green;
    [SerializeField] Text turnMessage;
    [SerializeField] GameObject boardPrefab;
    GameObject boardGO;
    Board board;

    bool isPlayerTurn = true;
    bool hasGameFinished = false;

    static readonly string RED_MESSAGE = "Red's Turn";
    static readonly string GREEN_MESSAGE = "Green's Turn";
    static readonly Color RED_COLOR = new Color(231, 29, 54, 255) / 255;
    static readonly Color GREEN_COLOR = new Color(0, 222, 1, 255) / 255;

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        isPlayerTurn = true;
        hasGameFinished = false;
        turnMessage.text = RED_MESSAGE;
        turnMessage.color = RED_COLOR;
    }

    private async void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (NetworkManager.Singleton.IsHost &&
                NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                SpawnBoard();
            }
        };

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    void SpawnBoard()
    {
        boardGO = Instantiate(boardPrefab);
        boardGO.GetComponent<NetworkObject>().Spawn();
        board = boardGO.GetComponent<Board>();
    }

    private void Update()
    {
        if (!IsOwner || hasGameFinished) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider && hit.collider.CompareTag("Press"))
            {
                int column = hit.collider.GetComponent<Column>().col - 1;
                RequestPlacePieceServerRpc(column);
            }
        }
    }

    [Rpc(SendTo.Server)]
    void RequestPlacePieceServerRpc(int column)
    {
        if (board.columnHeights[column] >= 6 || hasGameFinished) return;

        int row = board.columnHeights[column];
        board.columnHeights[column]++;

        board.UpdateBoard(row, column, isPlayerTurn);

        Vector3 spawnPos = new Vector3(column - 3f, 4f, 0); // Acima da grade
        Vector3 targetPos = new Vector3(column - 3f, -2.1f + 0.7f * row, 0);

        SpawnPieceClientRpc(spawnPos, targetPos, isPlayerTurn);

        if (board.Result(isPlayerTurn))
        {
            GameOverClientRpc(isPlayerTurn);
            hasGameFinished = true;
            return;
        }

        ChangeTurnClientRpc();
    }

    [ClientRpc]
    void SpawnPieceClientRpc(Vector3 spawnPos, Vector3 targetPos, bool isRed)
    {
        GameObject piece = Instantiate(isRed ? red : green);
        piece.GetComponent<Mover>().targetPostion = targetPos;
        piece.transform.position = spawnPos;
    }

    [ClientRpc]
    void ChangeTurnClientRpc()
    {
        isPlayerTurn = !isPlayerTurn;
        turnMessage.text = isPlayerTurn ? RED_MESSAGE : GREEN_MESSAGE;
        turnMessage.color = isPlayerTurn ? RED_COLOR : GREEN_COLOR;
    }

    [ClientRpc]
    void GameOverClientRpc(bool winnerIsRed)
    {
        hasGameFinished = true;
        turnMessage.text = (winnerIsRed ? "Red" : "Green") + " Wins!";
        turnMessage.color = winnerIsRed ? RED_COLOR : GREEN_COLOR;
    }

    public void GameStart() => UnityEngine.SceneManagement.SceneManager.LoadScene(0);

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
