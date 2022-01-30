using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fixer : MonoBehaviour
{
    public GameObject Objeto;
    
    private List<Vector3> fixedNodesPos;
    private List<int> fixedNodesIdx;

    // Possibilities of the Fixer
    void Start ()
    {
        fixedNodesPos = new List<Vector3> { };
        fixedNodesIdx = new List<int> { };

        List<Node> nodes = Objeto.GetComponent<ElasticSolid>().getNodes();

        Bounds bounds = GetComponent<Collider>().bounds;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (bounds.Contains(nodes[i].pos))
            {
                nodes[i].setFixed(true);
                fixedNodesIdx.Add(i);
                fixedNodesPos.Add(transform.InverseTransformPoint(nodes[i].pos));
            }
        }
    }
    
    private void Update()
    {
        List<Node> nodes = Objeto.GetComponent<ElasticSolid>().getNodes();
        int i = 0;
        foreach (int idx in fixedNodesIdx)
        {
            nodes[idx].pos = transform.TransformPoint(fixedNodesPos[i]);
            i++;
        }
        Objeto.GetComponent<ElasticSolid>().setNodes(nodes);
    }
}