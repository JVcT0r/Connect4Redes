using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [Header("Prefabs")]
    public GameObject redPrefab, greenPrefab;

    [Header("UI")]
    public Text turnMessage;

    [SyncVar(hook = nameof(OnTurnChanged))]
    private bool isRedTurn = true;

    [SyncVar]
    private bool hasGameFinished = false;

    private Board board;

    // Referência para as colunas na cena
    private Column[] columns;

    void Start()
    {
        if (isServer)
        {
            board = new Board();
            columns = FindObjectsOfType<Column>();
        }

        UpdateTurnMessage();
    }

    [Command]
    public void CmdTryPlay(int columnNumber)
    {
        if (hasGameFinished) return;
        if (!isRedTurn && connectionToClient != GetGreenPlayerConnection()) return;
        if (isRedTurn && connectionToClient != GetRedPlayerConnection()) return;

        if (board == null)
        {
            Debug.LogWarning("Board não inicializado no servidor!");
            return;
        }

        int colIndex = columnNumber - 1;

        if (!board.CanPlay(colIndex)) return;

        // Encontra a coluna correspondente para pegar as posições de spawn e alvo
        Column col = null;
        foreach (var c in columns)
        {
            if (c.col == columnNumber)
            {
                col = c;
                break;
            }
        }

        if (col == null)
        {
            Debug.LogWarning("Coluna não encontrada!");
            return;
        }

        // Atualiza lógica do tabuleiro
        board.UpdateBoard(colIndex, isRedTurn);

        // Instancia a peça na posição de spawn
        Vector3 spawnPos = col.spawnLocation;
        Vector3 targetPos = col.targetlocation;

        GameObject piece = Instantiate(isRedTurn ? redPrefab : greenPrefab, spawnPos, Quaternion.identity);
        piece.GetComponent<Mover>().targetPostion = targetPos;
        NetworkServer.Spawn(piece);

        // Atualiza posição alvo para a próxima peça na coluna
        col.targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);

        // Verifica se o jogador atual ganhou
        if (board.Result(isRedTurn))
        {
            hasGameFinished = true;
            RpcDeclareWinner(isRedTurn ? "Red" : "Green");
            return;
        }

        // Alterna o turno
        isRedTurn = !isRedTurn;
    }

    // Você pode implementar aqui como identificar as conexões dos jogadores
    // Por enquanto, permito que qualquer um jogue (ou refina essa lógica)

    private NetworkConnection GetRedPlayerConnection()
    {
        // Implementar identificação do jogador vermelho
        return null; // placeholder
    }

    private NetworkConnection GetGreenPlayerConnection()
    {
        // Implementar identificação do jogador verde
        return null; // placeholder
    }

    void OnTurnChanged(bool oldVal, bool newVal)
    {
        UpdateTurnMessage();
    }

    void UpdateTurnMessage()
    {
        turnMessage.text = hasGameFinished
            ? ""
            : isRedTurn ? "Red's Turn" : "Green's Turn";

        turnMessage.color = isRedTurn
            ? new Color(231f / 255f, 29f / 255f, 54f / 255f)
            : new Color(0f, 222f / 255f, 1f);
    }

    [ClientRpc]
    void RpcDeclareWinner(string winner)
    {
        hasGameFinished = true;
        turnMessage.text = $"{winner} Wins!";
    }
}


