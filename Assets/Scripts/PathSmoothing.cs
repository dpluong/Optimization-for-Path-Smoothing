using UnityEngine;


public class PathSmoothing : MonoBehaviour
{
    //public GameObject node;
    //List<GameObject> nodeList = new List<GameObject>();
    Vector3[] circleCenter;
    float[] radius;
    float[] weight;
    /* Step sizes */
    float[] sigma;
    float[] tau;
    float mu;
    /* Scale factor h */
    float h;
    /* updates nodes with smoother path */
    Vector3[] v;
    Vector3[] vhat;
    /* Dual variables */
    Vector3[] p;
    Vector3[] q;

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

    Vector3 startPosition = new Vector3(58f, 0f, 67f);
    Vector3 goalPosition = new Vector3(420f, 0f, 280f);
    const float Epsilon = 1.192092896e-07F;

    [SerializeField]
    Vector3 startFacingDirection = new Vector3(-0.316228f, 0f, -0.948683f);
    [SerializeField]
    Vector3 goalFacingDirection = new Vector3(0.707107f, 0f, -0.707107f);
    [SerializeField]
    float startSmoothingWeight = 10;
    [SerializeField]
    float goalSmoothingWeight = 10;
    [SerializeField]
    float smoothingWeight = 2;
    [SerializeField]
    float stepSizeAlpha = 0.0625f;
    [SerializeField]
    float stepSizeBeta = 10;
    [SerializeField]
    int numOfIterations = 100;

    // Start is called before the first frame update
    void Start()
    {
        InitConstraint();
        InitWeight();
        InitStepsize();
        CreateSmoothPath();
    }

   /* void InitCorridorMap()
    {
        for (int i = 0; i < map.Length; ++i)
        {
            nodeList.Add(Object.Instantiate(node, map[i].circleCenter, Quaternion.identity));
            nodeList[i].GetComponent<Node>().SetNodeRadius(map[i].radius);
        }
    }*/

    void InitConstraint()
    {
        int numOfNewMapNodes = map.Length + 4;
        circleCenter = new Vector3[numOfNewMapNodes];
        radius = new float[numOfNewMapNodes];
        
        float pathLength = 0;
        for (int i = 1; i < map.Length; ++i)
        {
            pathLength += Vector3.Distance(map[i].circleCenter, map[i - 1].circleCenter);
        }
        h = pathLength / (map.Length - 1);

        circleCenter[0] = startPosition - startFacingDirection * h;
        radius[0] = 0;
        circleCenter[1] = startPosition;
        radius[1] = 0;

        circleCenter[numOfNewMapNodes - 1] = goalPosition + goalFacingDirection * h;
        radius[numOfNewMapNodes - 1] = 0;
        circleCenter[numOfNewMapNodes - 2] = goalPosition;
        radius[numOfNewMapNodes - 2] = 0;

        for (int i = 0; i < numOfNewMapNodes - 4; ++i)
        {
            circleCenter[i + 2] = map[i].circleCenter;
            radius[i + 2] = map[i].radius;
        }
    }

    void InitWeight()
    {
        float ws = startSmoothingWeight;
        float we = goalSmoothingWeight;
        float wm = smoothingWeight;
        weight = new float[map.Length + 4];
        bool hasStartFacingDirection = true;
        bool hasGoalFacingDirection = true;
        if (startFacingDirection.x == 0 && startFacingDirection.z == 0)
        {
            hasStartFacingDirection = false;
        }

        if (goalFacingDirection.x == 0 && goalFacingDirection.z == 0)
        {
            hasGoalFacingDirection = false;
        }
        weight[0] = 0;
        weight[weight.Length - 1] = 0;

        if (!hasStartFacingDirection)
        {
            ws = smoothingWeight;
        }
        if (!hasGoalFacingDirection)
        {
            we = smoothingWeight;
        }
        float bound = (float)(weight.Length - 2) / (2f * (float)(weight.Length - 3));
        for (int i = 1; i < weight.Length - 1; ++i)
        {
            float weightCurve = Mathf.Pow(2f * (float)(i - 1) / (float)(weight.Length - 3) - 1, 4); 
            
            if ((float)(i - 1) / (float)(weight.Length - 3) <= bound)
            {
                weight[i] = wm + (ws - wm) * weightCurve;
            } 
            else
            {
                weight[i] = wm + (we - wm) * weightCurve;
            }
        }

        if (!hasStartFacingDirection)
        {
            weight[1] = 0;
        }
        if (!hasGoalFacingDirection)
        {
            weight[weight.Length - 2] = 0;
        }
    }

    void InitStepsize()
    {
        sigma = new float[map.Length + 4];
        tau = new float[map.Length + 4];
        sigma[0] = 0;
        tau[0] = 0;
        sigma[sigma.Length - 1] = 0;
        tau[tau.Length - 1] = 0;
        float hPowAlpha = Mathf.Pow(h, stepSizeAlpha);
        float twoPowAlpha = Mathf.Pow(2f, stepSizeAlpha);

        float preWeightPowTwoMinusAlpha = 0;
        float currentWeightPowTwoMinusAlpha = Mathf.Pow(weight[1], 2f - stepSizeAlpha);
        for (int i = 1; i < sigma.Length - 1; ++i)
        {
            if (weight[i] != 0)
            {
                sigma[i] = hPowAlpha / ((2f + twoPowAlpha) * stepSizeBeta * Mathf.Pow(weight[i], stepSizeAlpha));
            }
            else
            {
                sigma[i] = 0;
            }
            float nextWeightPowTwoMinusAlpha = Mathf.Pow(weight[i + 1], 2f - stepSizeAlpha);
            
            tau[i] = (stepSizeBeta * h * h / hPowAlpha) / (2f + preWeightPowTwoMinusAlpha + (Mathf.Pow(2f, 2) / twoPowAlpha) * currentWeightPowTwoMinusAlpha + nextWeightPowTwoMinusAlpha);
            preWeightPowTwoMinusAlpha = currentWeightPowTwoMinusAlpha;
            currentWeightPowTwoMinusAlpha = nextWeightPowTwoMinusAlpha;
        }
        mu = hPowAlpha / (2f * stepSizeBeta);
    }

    void CreateSmoothPath()
    {
        int pathLength = circleCenter.Length;
        v = new Vector3[pathLength];
        vhat = new Vector3[pathLength];
        p = new Vector3[pathLength];
        q = new Vector3[pathLength];

        for (int i = 0; i < pathLength; ++i)
        {
            v[i] = circleCenter[i];
            vhat[i] = circleCenter[i];
            p[i] = new Vector3(0f, 0f, 0f);
            q[i] = new Vector3(0f, 0f, 0f);
        }

        for (int index = 1; index <= numOfIterations; ++ index)
        {
            q[0] = mu * ((vhat[1] - vhat[0]) / h);
            float qL2Norm = q[0].sqrMagnitude;
            for (int i = 1; i < pathLength - 1; ++i)
            {
                Vector3 forwardDifference = (vhat[i + 1] - vhat[i]) / h;
                Vector3 backwardDifference = (vhat[i] - vhat[i - 1]) / h;

                p[i] = (p[i] + sigma[i] * weight[i] * (-forwardDifference + backwardDifference)) / (1f + sigma[i]);
                q[i] = q[i] + mu * forwardDifference;
                qL2Norm += q[i].sqrMagnitude;
            }

            if (qL2Norm > 1f)
            {
                for (int i = 0; i < pathLength - 1; ++i)
                {
                    q[i] = q[i] / qL2Norm;
                }
            }

            for (int i = 1; i < pathLength - 1; ++i)
            {
                Vector3 qBackwardDifferencing = (q[i] - q[i - 1]) / h;
                Vector3 pSumDifferencing = (2 * weight[i] * p[i] - weight[i - 1] * p[i - 1] - weight[i + 1] * p[i + 1]) / h;

                Vector3 nextv = v[i] - tau[i] * (pSumDifferencing - qBackwardDifferencing);
                Vector3 vMinusCenter = nextv - circleCenter[i];
                nextv = circleCenter[i] + vMinusCenter * radius[i] / Mathf.Max(radius[i] + Epsilon, vMinusCenter.magnitude);
                vhat[i] = 2 * nextv - v[i];
                v[i] = nextv;
            }
        }
        for (int i = 0; i < pathLength; ++i)
        {
            Debug.Log(v[i]);
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