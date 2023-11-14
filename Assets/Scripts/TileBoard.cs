using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager;
    public Tile tilePrefab;
    public TileState[] tileStates;
    private TileGrid grid;
    private List<Tile> tiles;
    private bool waiting;

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }
        
        foreach (var tile in tiles)
        {
            Destroy(tile.gameObject);
        }
        tiles.Clear();
    }
    
    public void CreateTile()
    {
        Tile tile =  Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0], 2);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    private void Update()
    {
        if (!waiting)
        {
            {

            }
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveTiles(Vector2Int.down, 0, 1, grid.Height - 2, -1);
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveTiles(Vector2Int.right, grid.Width - 2, -1, 0, 1);
            }
        }
    }
    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;
        
        for (int x = startX; x >= 0 && x < grid.Width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.Height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.occupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.occupied)
            {
                if (CanMarge(tile, adjacent.tile))
                {
                    Merge(tile, adjacent.tile);
                    return true;
                }
                
                break;
            }

            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }

        return false;
    }

    private bool CanMarge(Tile a, Tile b)
    {
        return a.number == b.number && !b.locked;
    }

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        int number = b.number * 2;
        
        b.SetState(tileStates[index], number);
        
        gameManager.IncreaseScore(number);
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i])
            {
                return i;
            }
        }

        return -1;
    }

    private IEnumerator WaitForChanges()
    {
        waiting = true;

        yield return new WaitForSeconds(0.1f);

        waiting = false;

        foreach (var tile in tiles)
        {
            tile.locked = false;
        }

        if (tiles.Count != grid.Size)
        {
            CreateTile();
        }

        if (CheckForGameOver())
        {
            gameManager.GameOver();
        }
    }

    private bool CheckForGameOver()
    {
        if (tiles.Count != grid.Size)
        {
            return false;
        }

        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if (up != null && CanMarge(tile, up.tile))
            {
                return false;
            }
            if (down != null && CanMarge(tile, down.tile))
            {
                return false;
            }
            if (left != null && CanMarge(tile, left.tile))
            {
                return false;
            }
            if (right != null && CanMarge(tile, right.tile))
            {
                return false;
            }
        }

        return true;
    }
}
