using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class PolygonTilemapPainter : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase tileToPlace;
    [SerializeField] PolygonVertexData sourceVertices;

    // Optional: clear before painting
    [SerializeField] bool clearBeforePaint = true;

    public void PaintTilesFromPolygon()
    {
        if(tilemap == null || tileToPlace == null || sourceVertices == null || sourceVertices.worldVertices == null)
        {
            Debug.LogWarning("TilemapPainter not fully configured.");
            return;
        }

        if(clearBeforePaint)
            tilemap.ClearAllTiles();

        GridLayout grid = tilemap.layoutGrid;
        List<Vector3Int> allCells = new List<Vector3Int>();

        for(int i = 0; i < sourceVertices.worldVertices.Length - 1; i++)
        {
            Vector3 start = sourceVertices.worldVertices[i];
            Vector3 end = sourceVertices.worldVertices[i + 1];
            LineToCells(grid, start, end, allCells);
        }

        if(allCells.Count > 0)
        {
            TileBase[] tiles = new TileBase[allCells.Count];
            for(int i = 0; i < allCells.Count; i++)
                tiles[i] = tileToPlace;
            tilemap.SetTiles(allCells.ToArray(), tiles);
        }

        tilemap.RefreshAllTiles();
        Debug.Log($"Painted {allCells.Count} tiles across {sourceVertices.worldVertices.Length - 1} segments.");
    }



    public void PaintNowInEditor()
    {
        PaintTilesFromPolygon();
    }

    void LineToCells(GridLayout grid, Vector3 start, Vector3 end, List<Vector3Int> cells)
    {
        Vector3Int cellStart = grid.WorldToCell(start);
        Vector3Int cellEnd = grid.WorldToCell(end);

        Vector3Int delta = new Vector3Int(
            Mathf.Abs(cellEnd.x - cellStart.x),
            Mathf.Abs(cellEnd.y - cellStart.y),
            0
        );

        int stepX = cellStart.x < cellEnd.x ? 1 : -1;
        int stepY = cellStart.y < cellEnd.y ? 1 : -1;
        int err = delta.x - delta.y;

        Vector3Int current = cellStart;
        cells.Add(current);

        while(current != cellEnd)
        {
            int e2 = 2 * err;
            if(e2 > -delta.y)
            {
                err -= delta.y;
                current.x += stepX;
            }
            if(e2 < delta.x)
            {
                err += delta.x;
                current.y += stepY;
            }
            cells.Add(current);
        }
    }


}
