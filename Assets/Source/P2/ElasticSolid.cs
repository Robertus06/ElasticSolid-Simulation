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

        this.vertices = new List<Vector3> { };
        this.tetraedros = new List<int> { };

        this.nodes = new List<Node> { };
        this.springs = new List<Spring> { };
        this.tetrahedrons = new List<Tetrahedron> { };
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

    private Mesh mesh;
    private Vector3[] verticesModelo;
    private Vector4[] coordBaricentricas;

    private List<Vector3> vertices;
    private List<int> tetraedros;
    private int[] tetraPertenece;

    private List<Node> nodes;
    private List<Spring> springs;
    private List<Tetrahedron> tetrahedrons;

    #endregion

    #region OtherVariables

    #endregion

    #region MonoBehaviour

    public void Awake()
    {
        this.mesh = this.GetComponentInChildren<MeshFilter>().mesh;
        this.verticesModelo = mesh.vertices;
        this.coordBaricentricas = new Vector4[verticesModelo.Length];
        this.tetraPertenece = new int[verticesModelo.Length];

        GetComponent<Parser>().ParseFiles();
        this.vertices = GetComponent<Parser>().getVertices();
        this.tetraedros = GetComponent<Parser>().getTetraedros();

        //For simulation purposes, transform the points to global coordinates
        createNodes();
        createTetraedros();
        createCoordBaricentricas();

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
    }

    public List<Node> getNodes()
    {
        return this.nodes;
    }

    public List<Spring> getSprings()
    {
        return this.springs;
    }

    public void createNodes()
    {
        for (int i = 0; i <= this.vertices.Count-1; i++)
        {
            Vector3 pos = transform.TransformPoint(this.vertices[i]);

            Node newNode = new Node(pos, this.Gravity, 0);
            nodes.Add(newNode);
        }
    }
    public void createTetraedros()
    {
        float numTetraedros = this.tetraedros.Count / 4;
        for (int i = 0; i <= numTetraedros - 1; i ++)
        {
            int j = i * 4;
            int nodeA = this.tetraedros[j];
            int nodeB = this.tetraedros[j + 1];
            int nodeC = this.tetraedros[j + 2];
            int nodeD = this.tetraedros[j + 3];

            Spring spring1 = new Spring(nodes[nodeA], nodes[nodeB], stiffnessDensity);
            if (!springs.Contains(spring1)) springs.Add(spring1);
            Spring spring2 = new Spring(nodes[nodeA], nodes[nodeC], stiffnessDensity);
            if (!springs.Contains(spring2)) springs.Add(spring2);
            Spring spring3 = new Spring(nodes[nodeA], nodes[nodeD], stiffnessDensity);
            if (!springs.Contains(spring3)) springs.Add(spring3);
            Spring spring4 = new Spring(nodes[nodeB], nodes[nodeC], stiffnessDensity);
            if (!springs.Contains(spring4)) springs.Add(spring4);
            Spring spring5 = new Spring(nodes[nodeD], nodes[nodeC], stiffnessDensity);
            if (!springs.Contains(spring5)) springs.Add(spring5);
            Spring spring6 = new Spring(nodes[nodeB], nodes[nodeD], stiffnessDensity);
            if (!springs.Contains(spring6)) springs.Add(spring6);

            Spring[] aristas = { springs[springs.IndexOf(spring1)], springs[springs.IndexOf(spring2)], springs[springs.IndexOf(spring3)], springs[springs.IndexOf(spring4)], springs[springs.IndexOf(spring5)], springs[springs.IndexOf(spring6)] };

            Tetrahedron newTetrahedron = new Tetrahedron(nodes[nodeA], nodes[nodeB], nodes[nodeC], nodes[nodeD], aristas);
            tetrahedrons.Add(newTetrahedron);
        }
    }

    public void createCoordBaricentricas()
    {
        bool encontrado;
        int j;
        for (int i = 0; i <= this.verticesModelo.Length - 1; i++)
        {
            encontrado = false;
            j = 0;
            while (!encontrado && j < this.tetrahedrons.Count)
            {
                if (this.tetrahedrons[j].Contiene(this.verticesModelo[i]))
                {
                    encontrado = true;
                    this.tetraPertenece[i] = j;
                    this.coordBaricentricas[i] = calcularCoordBaricentricas(this.tetrahedrons[j], this.verticesModelo[i]);
                }
                j++;
            }
        }
    }

    public Vector4 calcularCoordBaricentricas(Tetrahedron t, Vector3 v)
    {
        Vector4 coordBar = new Vector4();
        float volumen =  t.volumen;
        coordBar.x = (Mathf.Abs(Vector3.Dot(t.nodeB.pos - v, Vector3.Cross(t.nodeC.pos - v, t.nodeD.pos - v))) / 6) / volumen;
        coordBar.y = (Mathf.Abs(Vector3.Dot(v - t.nodeA.pos, Vector3.Cross(t.nodeC.pos - t.nodeA.pos, t.nodeD.pos - t.nodeA.pos))) / 6) / volumen;
        coordBar.z = (Mathf.Abs(Vector3.Dot(t.nodeB.pos - t.nodeA.pos, Vector3.Cross(v - t.nodeA.pos, t.nodeD.pos - t.nodeA.pos))) / 6) / volumen;
        coordBar.w = (Mathf.Abs(Vector3.Dot(t.nodeB.pos - t.nodeA.pos, Vector3.Cross(t.nodeC.pos - t.nodeA.pos, v - t.nodeA.pos))) / 6) / volumen;
        return coordBar;
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

        for (int i = 0; i <= this.verticesModelo.Length - 1; i++)
        {
            Vector3 pos = coordBaricentricas[i].x * tetrahedrons[tetraPertenece[i]].nodeA.pos +
                coordBaricentricas[i].y * tetrahedrons[tetraPertenece[i]].nodeB.pos +
                coordBaricentricas[i].z * tetrahedrons[tetraPertenece[i]].nodeC.pos +
                coordBaricentricas[i].w * tetrahedrons[tetraPertenece[i]].nodeD.pos;

            this.verticesModelo[i] = transform.InverseTransformPoint(pos);
        }

        this.mesh.vertices = this.verticesModelo;
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

        for (int i = 0; i <= this.verticesModelo.Length - 1; i++)
        {
            Vector3 pos = coordBaricentricas[i].x * tetrahedrons[tetraPertenece[i]].nodeA.pos +
                coordBaricentricas[i].y * tetrahedrons[tetraPertenece[i]].nodeB.pos +
                coordBaricentricas[i].z * tetrahedrons[tetraPertenece[i]].nodeC.pos +
                coordBaricentricas[i].w * tetrahedrons[tetraPertenece[i]].nodeD.pos;

            this.verticesModelo[i] = transform.InverseTransformPoint(pos);
        }

        this.mesh.vertices = this.verticesModelo;
    }
}
