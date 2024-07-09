using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunction:MonoBehaviour
{
    public int dimensions;
    public Tile[] tileObjects;
    public List<Cell> gridComponents;
    public Cell cellObj;

    private int iterations = 0;

    private void Awake()
    {
        gridComponents = new List<Cell>();
        InstantiateGrid();
    }

    private void InstantiateGrid()
    {
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObj, new Vector2(x, y), Quaternion.identity);
                newCell.CreateCell(false, tileObjects);
                gridComponents.Add(newCell);
            }
        }

        StartCoroutine(CheckEntropy());
    }

    private IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>(gridComponents);
        tempGrid.RemoveAll(c => c.collapsed);
        tempGrid.Sort((a, b) => a.tileOptions.Length - b.tileOptions.Length);
        int arrLength = tempGrid[0].tileOptions.Length;
        int stopIndex = default;

        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }
        
        yield return  new WaitForSeconds(0.01f);
        
        CollapseCell(tempGrid);
    }

    private void CollapseCell(List<Cell> tempGrid)
    {
        //随机进行坍缩
        int randomIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        Cell cellToCollapse = tempGrid[randomIndex];

        cellToCollapse.collapsed = true;
        Tile selectedTile = cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)];
        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        Tile foundTile = cellToCollapse.tileOptions[0];
        Instantiate(foundTile.gameObject, cellToCollapse.transform.position, Quaternion.identity);
        UpdateGeneration();
    }

    private void UpdateGeneration()
    {
        List<Cell> newGanerationCell = new List<Cell>(gridComponents);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;
                if (gridComponents[index].collapsed)
                {
                    Debug.Log("collapsed");
                    newGanerationCell[index] = gridComponents[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach (var t in tileObjects)
                    {
                        options.Add(t);
                    }

                    if (y>0)
                    {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (var possibleOption in up.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, t => t == possibleOption);
                            var valid = tileObjects[valOption].upNeighbors;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        
                        CheckValidity(options, validOptions);
                    }

                    if (x > 0)
                    {
                        Cell left = gridComponents[(x - 1) + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (var possibleOption in left.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, t => t == possibleOption);
                            var valid = tileObjects[valOption].leftNeighbors;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }

                    if (x < dimensions - 1)
                    {
                        Cell right = gridComponents[(x + 1) + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (var possibleOption in right.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, t => t == possibleOption);
                            var valid = tileObjects[valOption].rightNeighbors;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }

                    if (y < dimensions - 1)
                    {
                        Cell down = gridComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (var possibleOption in down.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, t => t == possibleOption);
                            var valid = tileObjects[valOption].downNeighbors;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    
                    Tile[] newTileList = new Tile[options.Count];
                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }
                    newGanerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        gridComponents = newGanerationCell;
        iterations++;
        if (iterations < dimensions * dimensions)
        {
            StartCoroutine(CheckEntropy());
        }
    }

    private void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for (int x = optionList.Count-1    ; x>=0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        } 
    }
}