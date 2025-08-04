using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PlayerType { NONE, RED, GREEN }
public struct GridPos { public int row, col; }

public class Board
{
    PlayerType[][] playerBoard;
    GridPos currentPos;

    public Board()
    {
        playerBoard = new PlayerType[6][];
        for (int i = 0; i < 6; i++)
        {
            playerBoard[i] = new PlayerType[7];
            for (int j = 0; j < 7; j++)
            {
                playerBoard[i][j] = PlayerType.NONE;
            }
        }
    }

    public bool CanPlay(int col)
    {
        return playerBoard[0][col - 1] == PlayerType.NONE;
    }

    public void UpdateBoard(int col, bool isRed)
    {
        int updatePos = 6;
        for (int i = 5; i >= 0; i--)
        {
            if (playerBoard[i][col] == PlayerType.NONE)
            {
                updatePos--;
            }
            else
            {
                break;
            }
        }

        playerBoard[updatePos][col] = isRed ? PlayerType.RED : PlayerType.GREEN;
        currentPos = new GridPos { row = updatePos, col = col };
    }

    public bool Result(bool isRed)
    {
        PlayerType current = isRed ? PlayerType.RED : PlayerType.GREEN;
        return IsHorizontal(current) || IsVertical(current) || IsDiagonal(current) || IsReverseDiagonal(current);
    }

    // Verificações
    bool IsHorizontal(PlayerType current)
    {
        GridPos start = GetEndPoint(new GridPos { row = 0, col = -1 });
        List<GridPos> list = GetPlayerList(start, new GridPos { row = 0, col = 1 });
        return SearchResult(list, current);
    }

    bool IsVertical(PlayerType current)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = 0 });
        List<GridPos> list = GetPlayerList(start, new GridPos { row = 1, col = 0 });
        return SearchResult(list, current);
    }

    bool IsDiagonal(PlayerType current)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = -1 });
        List<GridPos> list = GetPlayerList(start, new GridPos { row = 1, col = 1 });
        return SearchResult(list, current);
    }

    bool IsReverseDiagonal(PlayerType current)
    {
        GridPos start = GetEndPoint(new GridPos { row = -1, col = 1 });
        List<GridPos> list = GetPlayerList(start, new GridPos { row = 1, col = -1 });
        return SearchResult(list, current);
    }

    GridPos GetEndPoint(GridPos diff)
    {
        GridPos result = new GridPos { row = currentPos.row, col = currentPos.col };
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
        List<GridPos> res = new List<GridPos> { start };
        GridPos result = start;
        while (result.row + diff.row < 6 && result.col + diff.col < 7 &&
               result.row + diff.row >= 0 && result.col + diff.col >= 0)
        {
            result.row += diff.row;
            result.col += diff.col;
            res.Add(result);
        }
        return res;
    }

    bool SearchResult(List<GridPos> list, PlayerType current)
    {
        int counter = 0;
        foreach (var pos in list)
        {
            if (playerBoard[pos.row][pos.col] == current)
            {
                counter++;
                if (counter >= 4) return true;
            }
            else
            {
                counter = 0;
            }
        }
        return false;
    }
}
