using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Parser : MonoBehaviour
{
    public TextAsset nodeFile;
    public TextAsset eleFile;

    List<Vector3> vertices;
    List<int> tetraedros;

    public void ParseFiles()
    {
        // Ayuda TextAsset: https://docs.unity3d.com/ScriptReference/TextAsset.html
        // Ayuda Unity de String https://docs.unity3d.com/ScriptReference/String.html
        // Ayuda MSDN de String https://docs.microsoft.com/en-us/dotnet/api/system.string?redirectedfrom=MSDN&view=netframework-4.8
        // Ayuda MSDN de String.Split https://docs.microsoft.com/en-us/dotnet/api/system.string.split?view=netframework-4.8

        vertices = new List<Vector3> { };
        tetraedros = new List<int> { };

        string[] nodes = nodeFile.text.Split(new string[] { " ", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);
        string[] elems = eleFile.text.Split(new string[] { " ", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);

        int numNodes = int.Parse(nodes[0]);
        int tamanoNodes = int.Parse(nodes[1]);

        int numElems = int.Parse(elems[0]);
        int tamanoElems = int.Parse(elems[1]);

        for (int i = 4; i < numNodes * (tamanoNodes + 1) + 4; i+= tamanoNodes + 1)
        {
            int indice = int.Parse(nodes[i]);
            vertices.Add(new Vector3(float.Parse(nodes[i + 1], CultureInfo.InvariantCulture), float.Parse(nodes[i + 2], CultureInfo.InvariantCulture), float.Parse(nodes[i + 3], CultureInfo.InvariantCulture)));
        }

        for (int i = 3; i < numElems * (tamanoElems + 1) + 3; i += tamanoElems + 1)
        {
            int indice = int.Parse(elems[i]);
            for (int j = i + 1; j < i + 1 + tamanoElems; j++)
            {
                tetraedros.Add(int.Parse(elems[j]));
            }
        }
    }

    public List<Vector3> getVertices()
    {
        return this.vertices;
    }

    public List<int> getTetraedros()
    {
        return this.tetraedros;
    }
}
