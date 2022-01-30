using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrahedron
{
    public Node nodeA, nodeB, nodeC, nodeD;
    public Spring[] springs;
    public float volumen;

    public Tetrahedron(Node nodeA, Node nodeB, Node nodeC, Node nodeD, Spring[] springs)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        this.nodeC = nodeC;
        this.nodeD = nodeD;
        this.springs = springs;
        this.volumen = calcularVolumen();
    }

    public float calcularVolumen()
    {
        return Mathf.Abs(Vector3.Dot(nodeB.pos - nodeA.pos, Vector3.Cross(nodeC.pos - nodeA.pos, nodeD.pos - nodeA.pos))) / 6;
    }
    
    public float BaricentricCoord(int i, Vector3 pos)
    {
        Vector3[] aux = { nodeA.pos, nodeB.pos , nodeC.pos , nodeD.pos };
        aux[i] = pos;
        float vi = Math.Abs(Vector3.Dot((aux[1] - aux[0]),
            Vector3.Cross((aux[2] - aux[0]),
                (aux[3] - aux[0])))) / 6;
        return vi / volumen;

    }

    public bool Contiene(Vector3 v)
    {
        return MismoLado(nodeA, nodeB, nodeC, nodeD, v) && MismoLado(nodeB, nodeC, nodeD, nodeA, v) && MismoLado(nodeC, nodeD, nodeA, nodeB, v) && MismoLado(nodeD, nodeA, nodeB, nodeC, v);
    }

    private bool MismoLado(Node node1, Node node2, Node node3, Node node4, Vector3 v)
    {
        Vector3 normal = Vector3.Cross(node2.pos - node1.pos, node3.pos - node1.pos);
        return (Vector3.Dot(node4.pos - node1.pos, normal) * Vector3.Dot(v - node1.pos, normal)) > 0;
    }
}
