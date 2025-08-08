using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PlayerType { NONE, RED, GREEN }

public struct GridPos { public int row, col; }

public class Board : NetworkBehaviour
{
    public NetworkList<int> columnHeights; // 7 colunas, cada valor representa a próxima linha vazia (de 0 a 5)
    PlayerType[][] playerBoard;
    GridPos currentPos;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            columnHeights = new NetworkList<int>();
            for (int i = 0; i < 7; i++) columnHeights.Add(0);
        }
    }

    private void Awake()
    {
        playerBoard = new PlayerType[6][];
        for (int i = 0; i < 6; i++)
        {
            playerBoard[i] = new PlayerType[7];
            for (int j = 0; j < 7; j++)
                playerBoard[i][j] = PlayerType.NONE;
        }
    }

    public void UpdateBoard(int row, int col, bool isPlayer)
    {
        playerBoard[row][col] = isPlayer ? PlayerType.RED : PlayerType.GREEN;
        currentPos = new GridPos { row = row, col = col };
    }

    public bool Result(bool isPlayer)
    {
        PlayerType current = isPlayer ? PlayerType.RED : PlayerType.GREEN;
        return IsHorizontal(current) || IsVertical(current) || IsDiagonal(current) || IsReverseDiagonal(current);
    }

    #region WinCheck (igual ao seu original)

    bool IsHorizontal(PlayerType current)
    {
        GridPos start = GetEndPoint(new GridPos { row = 0, col = -1 });
        List<GridPos> toSearchList = GetPlayerList(start, new GridPos { row = 0, col = 1 });
        return SearchResult(toSearchList, current);
    }

    bool IsVertical(PlayerType current)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = 0 });
        List<GridPos> toSearchList = GetPlayerList(start, new GridPos { row = 1, col = 0 });
        return SearchResult(toSearchList, current);
    }

    bool IsDiagonal(PlayerType current)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = -1 });
        List<GridPos> toSearchList = GetPlayerList(start, new GridPos { row = 1, col = 1 });
        return SearchResult(toSearchList, current);
    }

    bool IsReverseDiagonal(PlayerType current)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = 1 });
        List<GridPos> toSearchList = GetPlayerList(start, new GridPos { row = 1, col = -1 });
        return SearchResult(toSearchList, current);
    }

    GridPos GetEndPoint(GridPos diff)
    {
        GridPos result = currentPos;
        while (result.row + diff.row < 6 && result.col + diff.col < 7 &&
               result.row + diff.row >= 0 && result.col + diff.col >= 0)
        {
            result.row += diff.row;
            result.col += diff.col;
        }
        return result;
    }

    List<GridPos> GetPlayerList(GridPos start, GridPos diff)
    {
        List<GridPos> resList = new List<GridPos> { start };
        GridPos result = start;
        while (result.row + diff.row < 6 && result.col + diff.col < 7 &&
               result.row + diff.row >= 0 && result.col + diff.col >= 0)
        {
            result.row += diff.row;
            result.col += diff.col;
            resList.Add(result);
        }

        return resList;
    }

    bool SearchResult(List<GridPos> searchList, PlayerType current)
    {
        int counter = 0;

        foreach (var pos in searchList)
        {
            if (playerBoard[pos.row][pos.col] == current)
            {
                counter++;
                if (counter == 4) break;
            }
            else counter = 0;
        }

        return counter >= 4;
    }

    #endregion
}
