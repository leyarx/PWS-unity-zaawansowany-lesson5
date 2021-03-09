using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NodesGrid : MonoBehaviour
{
    public Node[,] gridNodes;
    List<Node> openNodes;
    List<Node> closedNodes;

    public List<Node> path;

    [SerializeField]
    int gridWidth = 30;
    [SerializeField]
    int gridHeight = 30;
    [SerializeField]
    float nodeSize = 2f;
    [SerializeField]
    public LayerMask obstacleLayers;

    private void Awake()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        gridNodes = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                Vector3 nodePosition = new Vector3(x * nodeSize, 0, y * nodeSize);
                Node currentNode = gridNodes[x, y] = new Node(x, y, nodePosition);

                if (!Physics.CheckBox(nodePosition, Vector3.one * nodeSize / 2, Quaternion.identity, obstacleLayers))
                {
                    currentNode.walkable = true;
                }

                if (x > 0) 
                    LinkNodes(currentNode, gridNodes[x - 1, y]);
                if (y > 0)
                    LinkNodes(currentNode, gridNodes[x, y - 1]);
                if (x > 0 && y > 0)
                    LinkNodes(currentNode, gridNodes[x - 1, y - 1]);
                if (x > 0 && y < gridHeight - 1)
                    LinkNodes(currentNode, gridNodes[x - 1, y + 1]);
            }
        }
    }

    void LinkNodes(Node nodeA, Node nodeB)
    {
        nodeA.nearestNodes.Add(nodeB);
        nodeB.nearestNodes.Add(nodeA);
    }

    void OnDrawGizmos()
    {
        if (gridNodes == null)
            return;
        
        foreach(Node node in gridNodes)
        {
            if(node.walkable)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.red;

            if(path != null && path.Contains(node))
                Gizmos.color = Color.green;

            Gizmos.DrawWireCube(node.position, (new Vector3(nodeSize, 0, nodeSize)) * 0.9f);
            //Handles.Label(node.position + new Vector3(-nodeSize * 0.4f, 0 , -nodeSize * 0.4f), node.gCost.ToString());
            //Handles.Label(node.position + new Vector3(-nodeSize * 0.4f, 0, nodeSize * 0.4f), node.fCost.ToString());
            //Handles.Label(node.position + new Vector3(nodeSize * 0.4f, 0, -nodeSize * 0.4f), node.hCost.ToString());
        }
    }

    // A* Search Algorithm
    public List<Node> FindPath(Vector3 start, Vector3 end)
    {
        Node startNode = NodeFromWorldPoint(start);
        Node endNode = NodeFromWorldPoint(end);

        // Initialize the open list
        openNodes = new List<Node>();
        // Initialize the closed list
        closedNodes = new List<Node>();
        // put the starting node on the open list
        openNodes.Add(startNode);

        while(openNodes.Count > 0)
        {
            Node currentNode = getNodeWithMinF();

            if(currentNode == endNode)
            {
                Debug.Log("Path found");

                return TracePath(startNode, endNode);
            }

            foreach(Node node in currentNode.nearestNodes)
            {
                if (!node.walkable || closedNodes.Contains(node))
                    continue;

                int nearGCost = (Mathf.Abs(currentNode.x - node.x) + Mathf.Abs(currentNode.y - node.y)) == 1 ? 10 : 14;
                int tmpGCost = currentNode.gCost + nearGCost;

                if (node.gCost > tmpGCost || !openNodes.Contains(node))
                {
                    node.parentNode = currentNode;
                    node.gCost = tmpGCost;
                    node.hCost = CountHCost(node, endNode);

                    if(!openNodes.Contains(node))
                    {
                        openNodes.Add(node);
                    }
                }
            }

            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);
        }

        return null;
    }

    public List<Node> TracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();

        Node node = endNode;

        while(node != startNode)
        {
            path.Add(node);
            node = node.parentNode;
        }
        
        path.Add(startNode);
        path.Reverse();

        return path;
    }

    public Node NodeFromWorldPoint(Vector3 worldPositon)
    {
        float percentX = worldPositon.x / gridWidth * nodeSize;
        float percentY = worldPositon.z / gridHeight * nodeSize;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt(percentX * (gridWidth));
        int y = Mathf.RoundToInt(percentY * (gridHeight));

        return gridNodes[x, y];
    }

    Node getNodeWithMinF()
    {
        Node minFNode = null;

        foreach(Node node in openNodes)
        {
            if (minFNode == null)
            {
                minFNode = node;
                continue;
            }

            if (node.fCost < minFNode.fCost)
                minFNode = node;
        }

        return minFNode;
    }

    int CountHCost(Node nodeA, Node nodeB)
    { 
        return (Mathf.Abs(nodeB.x - nodeA.x) + Mathf.Abs(nodeB.y - nodeA.y)) * 10;
    }

    public List<Node> GetNearestNodesInRange2(Node currentNode, int range = 3)
    {
        List<Node> nearestNodes = new List<Node>();

        if (range == 0)
        {
            nearestNodes.Add(currentNode);
        }
        else
        {
            foreach (Node node in currentNode.nearestNodes)
            {
                foreach (Node node2 in GetNearestNodesInRange2(node, range - 1))
                {
                    if (!nearestNodes.Contains(node2))
                        nearestNodes.Add(node2);
                }
            }
        }

        return nearestNodes;
    }

    public List<Node> GetNearestNodesInRange(Node currentNode, int range = 1)
    {
        List<Node> nearestNodes = new List<Node>();

        for(int r = 1; r <= range; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (y >= 1 - r && y <= r - 1 && x >= 1 - r && x <= r - 1)
                        continue;

                    int checkX = currentNode.x + x;
                    int checkY = currentNode.y + y;

                    if (checkX >= 0 && checkY >= 0 && checkX < gridWidth && checkY < gridHeight)
                        nearestNodes.Add(gridNodes[checkX, checkY]);
                }
            }
        }

        return nearestNodes;
    }

    public List<Node> GetNearestNodes(Node currentNode, bool onlyWalkable = false)
    {
        List<Node> nearestNodes = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (y == 0 && x == 0)
                    continue;

                int checkX = currentNode.x + x;
                int checkY = currentNode.y + y;

                if (checkX >= 0 && checkY >= 0 && checkX < gridWidth && checkY < gridHeight)
                { 
                    if (onlyWalkable && !gridNodes[checkX, checkY].walkable)
                        continue;

                    nearestNodes.Add(gridNodes[checkX, checkY]);
                }
            }
        }

        return nearestNodes;
    }

}
