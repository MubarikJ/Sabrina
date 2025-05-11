using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PreyAgent : Agent
{
    [Header("Scene references")]
    [SerializeField] private PredatorAgent predator;      // drag predator script here

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Colours")]
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color caughtColor = Color.red;
    [SerializeField] private Color escapeColor = Color.green;

    private Rigidbody rb;
    private Renderer rend;

    // ------------------------------------------------------------------  Init
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        rend = GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = normalColor;
    }

    // ------------------------------------------------------------------  Reset
    public override void OnEpisodeBegin()
    {
        // reset positions
        transform.localPosition = new Vector3(
            Random.Range(-3f, 3f), 0.3f, Random.Range(-3f, 3f));

        predator.transform.localPosition = new Vector3(0f, 0.3f, 0f);

        rb.linearVelocity = Vector3.zero;

        // reset colour
        if (rend != null)
            rend.material.color = normalColor;
    }

    // ------------------------------------------------------------------  Obs
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 relPred = (predator.transform.localPosition - transform.localPosition) / 5f;
        sensor.AddObservation(relPred);                           // 3 floats

        float rotY = (transform.localEulerAngles.y / 180f) - 1f;  // (-1,1)
        sensor.AddObservation(rotY);                              // 1 float

        sensor.AddObservation((5f - Mathf.Abs(transform.localPosition.x)) / 5f); // 1
        sensor.AddObservation((5f - Mathf.Abs(transform.localPosition.z)) / 5f); // 1

        sensor.AddObservation(rb.linearVelocity.x / 5f);                // 1
        sensor.AddObservation(rb.linearVelocity.z / 5f);                // 1
        // total = 8 floats
    }

    // ------------------------------------------------------------------  Act
    public override void OnActionReceived(ActionBuffers actions)
    {
        int act = actions.DiscreteActions[0];

        // --- move ---
        bool moved = false;
        switch (act)
        {
            case 1:
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
                moved = true;
                break;
            case 2:
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
                moved = true;
                break;
            case 3:
                transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
                moved = true;
                break;
        }

        // ------------- reward shaping -------------
        float distFromPred = Vector3.Distance(transform.localPosition,
                                      predator.transform.localPosition);

        // stay away (smaller coeff)
        AddReward(0.0001f * distFromPred);

        // encourage reaching wall (smaller coeff)
        float minEdge = Mathf.Min(
            5f - Mathf.Abs(transform.localPosition.x),
            5f - Mathf.Abs(transform.localPosition.z));
        AddReward(0.0001f * (1f - minEdge / 5f));

        // living cost every step
        AddReward(-0.005f);

        // movement cost: penalise if it moved at all
        if (moved)
            AddReward(-0.001f);
    }
    private void FixedUpdate()
    {
        AddReward(-0.1f * Time.fixedDeltaTime);    // ?0.1 per simulated second
    }


    // ------------------------------------------------------------------  Caught
    public void GotCaught()
    {
        if (rend != null)
            rend.material.color = caughtColor;   // flash red
        Debug.Log("Prey was caught!");
        AddReward(-1f);
        EndEpisode();
    }

    // ------------------------------------------------------------------  Wall win
    private void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Wall"))
        {
            if (rend != null)
                rend.material.color = escapeColor; // flash green
            Debug.Log("Prey reached the wall and escaped!");
            AddReward(+1f);              // prey wins
            predator.AddReward(-1f);     // predator loses
            EndEpisode();
            predator.EndEpisode();
        }
    }
}
