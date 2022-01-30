using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sample code for accessing MeshFilter data.
/// </summary>
public class ElasticSolid : MonoBehaviour
{
    /// <summary>
    /// Default constructor. Zero all. 
    /// </summary>
    public ElasticSolid()
    {
        this.Paused = true;
        this.TimeStep = 0.001f;
        this.Gravity = new Vector3(0.0f, -9.81f, 0.0f);
        this.IntegrationMethod = Integration.Symplectic;
        this.massDensity = 1.0f;
        this.stiffnessDensity = 1000.0f;
        this.damping = 10.0f;

        this.nodes = new List<Node> { };
        this.springs = new List<Spring> { };
        this.tetrahedrons = new List<Tetrahedron> { };
        this.vertexTets = new List<Vertex> { };
    }

    /// <summary>
	/// Integration method.
	/// </summary>
    public enum Integration
    {
        Explicit = 0,
        Symplectic = 1,
    };

    #region InEditorVariables

    public bool Paused;
    public float TimeStep;
    public Vector3 Gravity;
    public Integration IntegrationMethod;
    public float massDensity;
    public float stiffnessDensity;
    public float damping;

    private Mesh mesh;
    private Vector3[] vertices;

    #endregion

    #region OtherVariables

    private List<Node> nodes;
    private List<Spring> springs;
    private List<Tetrahedron> tetrahedrons;
    private List<Vertex> vertexTets;

    #endregion

    #region MonoBehaviour

    public void Awake()
    {
        this.mesh = this.GetComponentInChildren<MeshFilter>().mesh;
        this.vertices = this.mesh.vertices;

        GetComponent<Parser>().ParseFiles();
        List<Vector3> nodePos = GetComponent<Parser>().getVertices();
        List<int> tetraedros = GetComponent<Parser>().getTetraedros();

        //For simulation purposes, transform the points to global coordinates
        createNodes(nodePos);
        createTetraedros(tetraedros);

        foreach (Tetrahedron tet in tetrahedrons)
        {
            tet.nodeA.mass += tet.volumen * massDensity / 4;
            tet.nodeB.mass += tet.volumen * massDensity / 4;
            tet.nodeC.mass += tet.volumen * massDensity / 4;
            tet.nodeD.mass += tet.volumen * massDensity / 4;

            foreach (Spring spring in tet.springs)
            {
                spring.volumen += tet.volumen / 6;
            }
        }
        
        createCoordBaricentricas();
    }

    public List<Node> getNodes()
    {
        return this.nodes;
    }
    
    public void setNodes(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public List<Spring> getSprings()
    {
        return this.springs;
    }
    
    public void createNodes(List<Vector3> nodePos)
    {
        for (int i = 0; i <= nodePos.Count-1; i++)
        {
            Vector3 pos = transform.TransformPoint(nodePos[i]);

            Node newNode = new Node(pos, this.Gravity, 0);
            nodes.Add(newNode);
        }
    }
    
    public void createTetraedros(List<int> tetraedros)
    {
        float numTetraedros = tetraedros.Count / 4;
        for (int i = 0; i <= numTetraedros - 1; i ++)
        {
            int j = i * 4;
            int nodeA = tetraedros[j];
            int nodeB = tetraedros[j + 1];
            int nodeC = tetraedros[j + 2];
            int nodeD = tetraedros[j + 3];

            Spring spring1 = new Spring(nodes[nodeA], nodes[nodeB], stiffnessDensity, damping);
            if (!springs.Contains(spring1)) springs.Add(spring1);
            Spring spring2 = new Spring(nodes[nodeA], nodes[nodeC], stiffnessDensity, damping);
            if (!springs.Contains(spring2)) springs.Add(spring2);
            Spring spring3 = new Spring(nodes[nodeA], nodes[nodeD], stiffnessDensity, damping);
            if (!springs.Contains(spring3)) springs.Add(spring3);
            Spring spring4 = new Spring(nodes[nodeB], nodes[nodeC], stiffnessDensity, damping);
            if (!springs.Contains(spring4)) springs.Add(spring4);
            Spring spring5 = new Spring(nodes[nodeD], nodes[nodeC], stiffnessDensity, damping);
            if (!springs.Contains(spring5)) springs.Add(spring5);
            Spring spring6 = new Spring(nodes[nodeB], nodes[nodeD], stiffnessDensity, damping);
            if (!springs.Contains(spring6)) springs.Add(spring6);

            Spring[] aristas = { springs[springs.IndexOf(spring1)], springs[springs.IndexOf(spring2)], springs[springs.IndexOf(spring3)], springs[springs.IndexOf(spring4)], springs[springs.IndexOf(spring5)], springs[springs.IndexOf(spring6)] };

            Tetrahedron newTetrahedron = new Tetrahedron(nodes[nodeA], nodes[nodeB], nodes[nodeC], nodes[nodeD], aristas);
            tetrahedrons.Add(newTetrahedron);
        }
    }

    public void createCoordBaricentricas()
    {
        foreach (Vector3 vertice in this.vertices)
        {
            bool find = false;
            int i = 0;
            while (!find && i < tetrahedrons.Count)
            {
                if (tetrahedrons[i].Contiene(transform.TransformPoint(vertice)))
                {
                    find = true;
                    vertexTets.Add(new Vertex(transform.TransformPoint(vertice), tetrahedrons[i]));
                }
                i++;
            }
        }
    }

    public void Update()
    {

    }

    public void FixedUpdate()
    {
        if (this.Paused)
            return; // Not simulating

        // Select integration method
        switch (this.IntegrationMethod)
        {
            case Integration.Explicit: this.stepExplicit(); break;
            case Integration.Symplectic: this.stepSymplectic(); break;
            default:
                throw new System.Exception("[ERROR] Should never happen!");
        }

    }
    #endregion

    /// <summary>
    /// Performs a simulation step in 1D using Explicit integration.
    /// </summary>
    private void stepExplicit()
    {
        foreach (Node node in nodes)
        {
            node.force = Vector3.zero;
            node.ComputeForces();
        }

        foreach (Spring spring in springs)
        {
            spring.ComputeForces();
        }

        foreach (Node node in nodes)
        {
            if (!node.isFixed)
            {
                node.pos += TimeStep * node.vel;
                node.vel += TimeStep / node.mass * node.force;
            }
        }

        foreach (Spring spring in springs)
        {
            spring.UpdateLength();
        }

        for (int i = 0; i <= this.vertices.Length - 1; i++)
        {
            Vector3 pos = vertexTets[i].ComputePos();
            this.vertices[i] = transform.InverseTransformPoint(pos);
        }

        mesh.vertices = vertices;
    }

    /// <summary>
	/// Performs a simulation step in 1D using Symplectic integration.
	/// </summary>
	private void stepSymplectic()
    {
        foreach (Node node in nodes)
        {
            node.force = Vector3.zero;
            node.ComputeForces();
        }

        foreach (Spring spring in springs)
        {
            spring.ComputeForces();
        }

        foreach (Node node in nodes)
        {
            if (!node.isFixed)
            {
                node.vel += TimeStep / node.mass * node.force;
                node.pos += TimeStep * node.vel;
            }
        }

        foreach (Spring spring in springs)
        {
            spring.UpdateLength();
        }

        for (int i = 0; i <= this.vertices.Length - 1; i++)
        {
            Vector3 pos = vertexTets[i].ComputePos();
                this.vertices[i] = transform.InverseTransformPoint(pos);
        }

        mesh.vertices = vertices;
    }
}
