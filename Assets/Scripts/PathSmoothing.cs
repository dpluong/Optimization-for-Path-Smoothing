using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathSmoothing : MonoBehaviour
{
    const float Epsilon = 1.192092896e-07F;
    Vector3[] circleCenter;
    float[] radius;
    float[] weight;
    /* Step sizes */
    float[] sigma;
    float[] tau;
    float mu;
    /* Scale factor h */
    float h;
    /* updated nodes with smoother path */
    Vector3[] v;
    Vector3[] vhat;
    /* Dual variables */
    Vector3[] p;
    Vector3[] q;
    CorridorMapNode[] map;

    [Header("Corridor Map Nodes")]
    public List<GameObject> nodeList = new List<GameObject>();

    [Header("Path Smoothing Configuration")]
    [SerializeField]
    Vector3 startPosition = new Vector3(58f, 18.7f, 67f);
    [SerializeField]
    Vector3 goalPosition = new Vector3(420f, 18.7f, 280f);
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

    NavMeshAgent agent;
    int nodeIndex = 2;
    float lerpTime = 0;
    Vector3 movementVector;
    Vector3 targetDirection;

    [Header("Movement Configuration")]
    [SerializeField]
    [Range(0, 0.99f)]
    private float smoothing = 0.25f;
    [SerializeField]
    private float targetLerpSpeed = 1;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        LoadCorridorMap();
        InitAgentPosition();
        InitConstraints();
        InitWeights();
        InitStepsizeVariables();
        InitPrimeDualVariables();
        CreateSmoothPath();
    }

    private void Update() 
    {
        MoveAgent();
    }

    void LoadCorridorMap()
    {
        map = new CorridorMapNode[nodeList.Count];
        for (int i = 0; i < nodeList.Count; ++i)
        {
            map[i].circleCenter = nodeList[i].transform.position;
            map[i].radius = nodeList[i].GetComponent<Node>().radius;
        }
    }

    void InitConstraints()
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
            circleCenter[i + 2].y = 18.7f;
            radius[i + 2] = map[i].radius;
        }
    }

    void InitWeights()
    {
        float ws = startSmoothingWeight;
        float we = goalSmoothingWeight;
        float wm = smoothingWeight;
        weight = new float[map.Length + 4];

        weight[0] = 0;
        weight[weight.Length - 1] = 0;
        ws = smoothingWeight;
        we = smoothingWeight;
        
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
    }

    void InitStepsizeVariables()
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

    Vector3 CalculateDifference(Vector3 pre, Vector3 next, float scale)
    {
        return (next - pre) / scale;
    }

    void InitPrimeDualVariables()
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
    }

    void CreateSmoothPath()
    {
        int pathLength = circleCenter.Length;
        for (int index = 1; index <= numOfIterations; ++ index)
        {
            q[0] = mu * ((vhat[1] - vhat[0]) / h);
            float qL2Norm = q[0].sqrMagnitude;
            for (int i = 1; i < pathLength - 1; ++i)
            {
                Vector3 forwardDifference = CalculateDifference(vhat[i], vhat[i + 1], h);
                Vector3 backwardDifference = CalculateDifference(vhat[i - 1], vhat[i], h);

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
                Vector3 qBackwardDifferencing = CalculateDifference(q[i - 1], q[i], h);
                Vector3 pSumDifferencing = (2 * weight[i] * p[i] - weight[i - 1] * p[i - 1] - weight[i + 1] * p[i + 1]) / h;

                Vector3 nextv = v[i] - tau[i] * (pSumDifferencing - qBackwardDifferencing);
                Vector3 vMinusCenter = nextv - circleCenter[i];
                nextv = circleCenter[i] + vMinusCenter * radius[i] / Mathf.Max(radius[i] + Epsilon, vMinusCenter.magnitude);
                vhat[i] = 2 * nextv - v[i];
                v[i] = nextv;
            }
        }
    }

    void InitAgentPosition()
    {
        startPosition = new Vector3(startPosition.x, gameObject.transform.position.y, startPosition.z);
        this.gameObject.transform.position = startPosition;
        Quaternion startRotation = Quaternion.LookRotation(startFacingDirection, Vector3.up);
        transform.rotation = startRotation;
    }

    void MoveAgent()
    {
        if (nodeIndex >= v.Length)
        {
            return;
        }
        
        if (Vector3.Distance(transform.position, v[nodeIndex] + (agent.baseOffset * Vector3.up)) <= 20f)
        {
            nodeIndex += 1;   
            lerpTime = 0;

            if (nodeIndex >= v.Length)
            {
                return;
            }
        }

        movementVector = (v[nodeIndex] - transform.position);
        targetDirection = Vector3.Lerp(
            targetDirection,
            movementVector,
            Mathf.Clamp01(lerpTime * targetLerpSpeed * (1 - smoothing))
        );

        Vector3 lookDirection = movementVector;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(lookDirection),
                Mathf.Clamp01(lerpTime * targetLerpSpeed * (1 - smoothing))
            );
        }

        agent.Move(targetDirection * agent.speed * Time.deltaTime);
        lerpTime += Time.deltaTime;   
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