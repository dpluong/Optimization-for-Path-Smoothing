using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;

    [Header("Agent Configuration")]
    [SerializeField] bool clickToMove = false;

    [SerializeField]
    Vector3 startFacingDirection = new Vector3(-0.316228f, 0f, -0.948683f);
    [SerializeField]
    GameObject goalPosition;
    [SerializeField]
    GameObject startPosition;

    void Start()
    {
        transform.position = startPosition.transform.position;
        Quaternion startRotation = Quaternion.LookRotation(startFacingDirection, Vector3.up);
        transform.rotation = startRotation;
        SetAgentDestination();
    }
    // Update is called once per frame
    void Update()
    {
        if (clickToMove)
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    agent.SetDestination(hit.point);
                }
            }
        }
    }

    void SetAgentDestination()
    {
        agent.SetDestination(goalPosition.transform.position);
    }
}
