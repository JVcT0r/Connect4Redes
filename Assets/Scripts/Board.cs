using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PlayerType { NONE, RED, GREEN }

public struct GridPos
{
    public int row, col;
}

public class Board : NetworkBehaviour
{
    private PlayerType[][] playerBoard;
    private GridPos currentPos;

    private bool isRedTurn = true;

    public PlayerType[][] PlayerBoard {get; set;}
    public bool IsRedTurn { get; set; } = true;
    public GridPos CurrentPos { get; set; }

    public bool IsColumnAvailable(int col)
    {
        return playerBoard[0][col] == PlayerType.NONE;
    }



    void Awake()
    {
        // Inicializa o tabuleiro com NONE
        playerBoard = new PlayerType[6][];
        for (int i = 0; i < playerBoard.Length; i++)
        {
            playerBoard[i] = new PlayerType[7];
            for (int j = 0; j < playerBoard[i].Length; j++)
            {
                playerBoard[i][j] = PlayerType.NONE;
            }
        }
    }

    // Chamada local do cliente quando ele tenta jogar
    public void TryMakeMove(int column)
    {
        MakeMoveServerRpc(column);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MakeMoveServerRpc(int column, ServerRpcParams rpcParams = default)
    {
        // Verifica se a jogada é válida
        if (!IsValidMove(column)) return;

        // Determina o jogador atual
        PlayerType currentPlayer = isRedTurn ? PlayerType.RED : PlayerType.GREEN;

        // Atualiza o tabuleiro no host
        GridPos updatedPos = ApplyMove(column, currentPlayer);

        // Atualiza posição atual
        currentPos = updatedPos;

        // Verifica se venceu
        if (CheckWin(currentPlayer))
        {
            Debug.Log($"{currentPlayer} venceu!");
            // Aqui você pode implementar lógica de fim de jogo
        }

        // Envia atualização para todos os clientes
        BroadcastMoveClientRpc(updatedPos.row, updatedPos.col, (int)currentPlayer);

        // Alterna o turno
        isRedTurn = !isRedTurn;
    }

    [ClientRpc]
    private void BroadcastMoveClientRpc(int row, int col, int player)
    {
        playerBoard[row][col] = (PlayerType)player;
        currentPos = new GridPos { row = row, col = col };
    }

    private bool IsValidMove(int col)
    {
        return playerBoard[0][col] == PlayerType.NONE;
    }

    public GridPos ApplyMove(int col, PlayerType player)
    {
        for (int i = 5; i >= 0; i--)
        {
            if (playerBoard[i][col] == PlayerType.NONE)
            {
                playerBoard[i][col] = player;
                return new GridPos { row = i, col = col };
            }
        }

        return new GridPos { row = -1, col = col }; // Coluna cheia (não deve acontecer por causa do IsValidMove)
    }

    // ========== Verificação de vitória ==========
    public bool CheckWin(PlayerType player)
    {
        return IsHorizontal(player) || IsVertical(player) || IsDiagonal(player) || IsReverseDiagonal(player);
    }

    private bool IsHorizontal(PlayerType player)
    {
        GridPos start = GetEndPoint(new GridPos { row = 0, col = -1 });
        List<GridPos> toSearchList = GetPlayerList(start, new GridPos { row = 0, col = 1 });
        return SearchResult(toSearchList, player);
    }

    private bool IsVertical(PlayerType player)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = 0 });
        List<GridPos> toSearchList = GetPlayerList(start, new GridPos { row = 1, col = 0 });
        return SearchResult(toSearchList, player);
    }

    private bool IsDiagonal(PlayerType player)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = -1 });
        List<GridPos> toSearchList = GetPlayerList(start, new GridPos { row = 1, col = 1 });
        return SearchResult(toSearchList, player);
    }

    private bool IsReverseDiagonal(PlayerType player)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = 1 });
        List<GridPos> toSearchList = GetPlayerList(start, new GridPos { row = 1, col = -1 });
        return SearchResult(toSearchList, player);
    }

    private GridPos GetEndPoint(GridPos diff)
    {
        GridPos result = new GridPos { row = currentPos.row, col = currentPos.col };
        while (result.row + diff.row < 6 &&
               result.col + diff.col < 7 &&
               result.row + diff.row >= 0 &&
               result.col + diff.col >= 0)
        {
            result.row += diff.row;
            result.col += diff.col;
        }
        return result;
    }

    private List<GridPos> GetPlayerList(GridPos start, GridPos diff)
    {
        List<GridPos> resList = new List<GridPos> { start };
        GridPos result = new GridPos { row = start.row, col = start.col };
        while (result.row + diff.row < 6 &&
               result.col + diff.col < 7 &&
               result.row + diff.row >= 0 &&
               result.col + diff.col >= 0)
        {
            result.row += diff.row;
            result.col += diff.col;
            resList.Add(result);
        }

        return resList;
    }

    private bool SearchResult(List<GridPos> searchList, PlayerType player)
    {
        int counter = 0;

        for (int i = 0; i < searchList.Count; i++)
        {
            PlayerType compare = playerBoard[searchList[i].row][searchList[i].col];
            if (compare == player)
            {
                counter++;
                if (counter == 4)
                    return true;
            }
            else
            {
                counter = 0;
            }
        }

        return false;
    }
}
