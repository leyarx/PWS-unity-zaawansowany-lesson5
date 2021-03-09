using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHideBy : MonoBehaviour
{
    public NodesGrid grid;
    public Node positionNode;
    public GameObject player;
    public float speed = 1.0f;

    public int rangeToHide = 5;
    public bool isMoving = false;
    Vector3 hidePosition;

    public List<Node> hiddenNodes;
    public List<Node> path;

    private void Awake()
    {
        hiddenNodes = new List<Node>();
    }

    void Start()
    {
        positionNode = grid.NodeFromWorldPoint(gameObject.transform.position);
        hidePosition = transform.position;

        InvokeRepeating("HideFromPlayer", 0f, 0.5f);
    }

    Node GetNodeToHide()
    {
        hiddenNodes.Clear();

        List<Node> tempNodes = new List<Node>();

        foreach(Node node in grid.gridNodes)
        {
            if(!node.walkable)
            {
                tempNodes.AddRange(grid.GetNearestNodes(node, true));
            }
        }

        // remove duplicates
        tempNodes = tempNodes.Distinct().ToList();

        foreach(Node node in tempNodes)
        {
            if(IsHidden(node.position, player.transform.position))
            {
                hiddenNodes.Add(node);
            }
        }

        // order nodes by distance to npc
        hiddenNodes = hiddenNodes.OrderBy(d => Vector3.Distance(d.position, transform.position)).ToList();

        // just for showing in scene view
        grid.path = hiddenNodes;

        return hiddenNodes[0];
    }

    void HideFromPlayer()
    {
        if (!isMoving && !IsHidden(transform.position, player.transform.position))
        {
            isMoving = true;
            path = grid.FindPath(transform.position, GetNodeToHide().position);
        }
    }

    bool IsHidden(Vector3 npcPosition, Vector3 playerPosition)
    {
        RaycastHit hit;
        Vector3 direction = playerPosition - npcPosition;
        bool isHidden = Physics.Raycast(npcPosition, direction, out hit, direction.magnitude, grid.obstacleLayers);
        if (isHidden)
            Debug.DrawRay(npcPosition, direction.normalized * hit.distance, Color.green);
        else
            Debug.DrawRay(npcPosition, direction, Color.red);
        return isHidden;
    }

    void Update()
    {
        if (isMoving)
        {
            if (path.Count > 0)
            {
                float dist = Vector3.Distance(transform.position, hidePosition);

                if (dist < 0.001f)
                {
                    hidePosition = path[0].position;
                    path.RemoveAt(0);
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, hidePosition) < 0.001f)
                    isMoving = false;
            }

            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, hidePosition, step);
        }
    }
}
