using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
        { { 65.f,44.f },31.f },{ { 79.f,43.f },29.4279f },{ { 93.f,42.f },28.0713f },{ { 110.f,40.f },27.f },
        { { 124.f,39.f },25.4951f },{ { 138.f,38.f },24.1868f },{ { 152.f,37.f },22.8035f },{ { 167.f,37.f },23.7697f },
        { { 188.f,46.f },31.9061f },{ { 184.f,66.f },24.1868f },{ { 177.f,84.f },22.0227f },{ { 165.f,96.f },26.4764f },
        { { 147.f,109.f },36.1248f },{ { 125.f,134.f },56.8859f },{ { 101.f,170.f },88.f },{ { 110.f,189.f },97.f },
        { { 119.f,208.f },106.f },{ { 131.f,229.f },117.618f },{ { 172.f,211.f },85.3815f },{ { 198.f,194.f },66.7308f },
        { { 214.f,179.f },56.8595f },{ { 228.f,161.f },50.2494f },{ { 237.f,149.f },50.9117f },{ { 249.f,136.f },54.5619f },
        { { 264.f,124.f },61.6604f },{ { 285.f,112.f },73.6817f },{ { 312.f,103.f },89.8109f },{ { 328.f,107.f },92.6607f },
        { { 345.f,112.f },98.0816f },{ { 363.f,118.f },105.f },{ { 368.f,136.f },99.6444f },{ { 341.f,182.f },58.3095f },
        { { 316.f,205.f },36.3456f },{ { 301.f,221.f },28.1603f },{ { 295.f,234.f },26.9258f },{ { 291.f,253.f },29.7321f },
        { { 290.f,272.f },35.1141f },{ { 297.f,298.f },50.f },{ { 326.f,284.f },30.1496f },{ { 349.f,268.f },15.f },
        { { 382.f,279.f },25.0799f },{ { 408.f,301.f },47.f },{ { 421.f,300.f },47.f }
*/
//[ExecuteInEditMode]
public class PathSmoothing : MonoBehaviour
{
    public GameObject node;
    List<GameObject> nodeList = new List<GameObject>();
    CorridorMapNode[] map = new CorridorMapNode[] {
    new CorridorMapNode( new Vector3 (65f, 0f, 44f), 31f ), new CorridorMapNode( new Vector3 (79f, 0f, 43f), 29.4279f ), new CorridorMapNode( new Vector3 (93f, 0f, 42f), 28.0713f ), new CorridorMapNode( new Vector3 (110f, 0f, 40f), 27f ),
    new CorridorMapNode( new Vector3 (124f, 0f, 39f), 25.4951f ), new CorridorMapNode( new Vector3 (138f, 0f, 38f), 24.1868f ), new CorridorMapNode( new Vector3 (152f, 0f, 37f), 22.8035f ), new CorridorMapNode( new Vector3 (167f, 0f, 37f), 23.7697f),
    new CorridorMapNode( new Vector3 (188f, 0f, 46f), 31.9061f ), new CorridorMapNode( new Vector3 (184f, 0f, 66f), 24.1868f ), new CorridorMapNode( new Vector3 (177f, 0f, 84f), 22.0227f ), new CorridorMapNode( new Vector3 (165f, 0f, 96f), 26.4764f ),
    new CorridorMapNode( new Vector3 (147f, 0f, 109f), 36.1248f ), new CorridorMapNode( new Vector3 (125f, 0f, 134f), 56.8859f ), new CorridorMapNode( new Vector3 (101f, 0f, 170f), 88f ), new CorridorMapNode( new Vector3 (110f, 0f, 189f), 97f ),
    new CorridorMapNode( new Vector3 (119f, 0f, 208f), 106f ), new CorridorMapNode( new Vector3 (131f, 0f, 229f), 117.618f ), new CorridorMapNode( new Vector3 (172f, 0f, 211f), 85.3815f ), new CorridorMapNode( new Vector3 (198f, 0f, 194f), 66.7308f ),
    new CorridorMapNode( new Vector3 (214f, 0f, 179f), 56.8595f ), new CorridorMapNode( new Vector3 (228f, 0f, 161f), 50.2494f ), new CorridorMapNode( new Vector3 (237f, 0f, 149f), 50.9117f ), new CorridorMapNode( new Vector3 (249f, 0f, 136f), 54.5619f ),
    new CorridorMapNode( new Vector3 (264f, 0f, 124f), 61.6604f ), new CorridorMapNode( new Vector3 (285f, 0f, 112f), 73.6817f ), new CorridorMapNode( new Vector3 (312f, 0f, 103f), 89.8109f ), new CorridorMapNode( new Vector3 (328f, 0f, 107f), 92.6607f ),
    new CorridorMapNode( new Vector3 (345f, 0f, 112f), 98.0816f ), new CorridorMapNode( new Vector3 (363f, 0f, 118f), 105f ), new CorridorMapNode( new Vector3 (368f, 0f, 136f), 99.6444f ), new CorridorMapNode( new Vector3 (341f, 0f, 182f), 58.3095f ),
    new CorridorMapNode( new Vector3 (316f, 0f, 205f), 36.3456f ), new CorridorMapNode( new Vector3 (301f, 0f, 221f), 28.1603f ), new CorridorMapNode( new Vector3 (295f, 0f, 234f), 26.9258f ), new CorridorMapNode( new Vector3 (291f, 0f, 253f), 29.7321f ),
    new CorridorMapNode( new Vector3 (290f, 0f, 272f), 35.1141f ), new CorridorMapNode( new Vector3 (297f, 0f, 298f), 50f ), new CorridorMapNode( new Vector3 (326f, 0f, 284f), 30.1496f ), new CorridorMapNode( new Vector3 (349f, 0f, 268f), 15f ),
    new CorridorMapNode( new Vector3 (382f, 0f, 279f), 25.0799f ), new CorridorMapNode( new Vector3 (408f, 0f, 301f), 47f ), new CorridorMapNode( new Vector3 (421f, 0f, 300f), 47f ) 
    };
    // Start is called before the first frame update
    void Awake()
    {
        InitCorridorMap();
        Debug.Log("Editor causes this Awake");
    }
/*
    private void OnDrawGizmos() 
    {
        if (nodeList.Count == map.Length)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                Handles.DrawWireDisc(nodeList[i].transform.position, new Vector3(0f, 1f, 0f), map[i].radius);
            }
        }
    }*/

    void InitCorridorMap()
    {
        for (int i = 0; i < map.Length; ++i)
        {
            nodeList.Add(Object.Instantiate(node, map[i].circleCenter, Quaternion.identity));
            nodeList[i].GetComponent<Node>().SetNodeRadius(map[i].radius);
        }
    }
}

public struct CorridorMapNode
{
    public Vector3 circleCenter;
    public float radius;
    public CorridorMapNode (Vector3 circleCenter, float radius)
    {
        this.circleCenter = circleCenter;
        this.radius = radius;
    }
}