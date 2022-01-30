using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex 
{
    public Tetrahedron tetrahedron;
    public float[] w;

    public Vertex (Vector3 pos, Tetrahedron tetrahedron)
    {
        this.tetrahedron = tetrahedron;
        this.w = BaricentricCoords(pos, tetrahedron);
    }

    public float[] BaricentricCoords(Vector3 pos, Tetrahedron tet)
    {
        float[] coords = new float[4];
        for (int i = 0; i < 4; i++)
        {
            coords[i] = tet.BaricentricCoord(i, pos);
        }
        return coords;
    }

    public Vector3 ComputePos()
    {
        return (w[0] * tetrahedron.nodeA.pos +
                w[1] * tetrahedron.nodeB.pos +
                w[2] * tetrahedron.nodeC.pos +
                w[3] * tetrahedron.nodeD.pos);
    }
   
}
