using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANode
{
    public bool isWalkAble;
    public Vector3 worldPos;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public ANode parentNode;

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public ANode(bool nWalkable, Vector3 nWorldPos, int nGridX, int nGridY)
    {
        isWalkAble = nWalkable;
        worldPos = nWorldPos;
        gridX = nGridX;
        gridY = nGridY;
    }
}