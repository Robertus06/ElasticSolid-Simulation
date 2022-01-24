using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoExample : MonoBehaviour {

    public List<Node> nodes;
    public List<Spring> springs;

	void Start () {
        nodes = GetComponent<ElasticSolid>().getNodes();
        springs = GetComponent<ElasticSolid>().getSprings();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (nodes != null)
        {
            foreach (Node node in nodes)
            {
                Gizmos.DrawSphere(node.pos, 0.1f);
            }
        }

        Gizmos.color = Color.red;
        
        if (springs != null)
        {
            foreach (Spring spring in springs)
            {
                Gizmos.DrawLine(spring.nodeA.pos, spring.nodeB.pos);
            }
        }
    }

}
