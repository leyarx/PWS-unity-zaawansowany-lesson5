using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int x, y;
    public Vector3 position;

    public List<Node> nearestNodes;
    public int hCost;
    public int gCost;
    public int fCost {
        get { return hCost + gCost; }
    }

    public bool walkable;
    public Node parentNode;

    public Node(int x, int y, Vector3 position)
    {
        nearestNodes = new List<Node>();

        this.x = x;
        this.y = y;
        this.position = position;
    }
}
