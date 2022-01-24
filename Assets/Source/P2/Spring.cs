using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring {

    public Node nodeA, nodeB;

    public float Length0;
    public float Length;

    public float stiffness;
    public float volumen;
    //public float damp;

    // Use this for initialization
    public Spring(Node nodeA, Node nodeB, float stiffness)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        this.stiffness = stiffness;
        this.volumen = 0;
        //this.damp = damp;

        UpdateLength();
        Length0 = Length;
    }
	
    public void UpdateLength ()
    {
        Length = (nodeA.pos - nodeB.pos).magnitude;
    }

    public void ComputeForces()
    {
        Vector3 u = nodeA.pos - nodeB.pos;
        u.Normalize();
        Vector3 force = - volumen / (float)Math.Pow(Length0, 2) * stiffness * (Length - Length0) * u;
        //force += -damp * Vector3.Project((nodeA.vel - nodeB.vel), u);
        nodeA.force += force;
        nodeB.force -= force;
    }

    public bool Equals(Spring s)
    {
        if ((this.nodeA == s.nodeA) && (this.nodeB == s.nodeB))
            return true;
        else
            return false;
    }
}
