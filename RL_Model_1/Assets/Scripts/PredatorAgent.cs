using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PredatorAgent : Agent
{
    [SerializeField] private Transform _prey;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f; 

    private Renderer _renderer;

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;


         public override void Initialize()
    {
        Debug.Log("Initialize()");

        _renderer = GetComponent<Renderer>();
        _currentEpisode = 0;
        _cumulativeReward = 0f;
    }
    
    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");

        _currentEpisode++;
        _cumulativeReward = 0f;
        _renderer.material.color = Color.white;

        SpawnObjects();

    }

    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.15f, 0f);

        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        float randomDistance = Random.Range(1f, 2.5f);

        Vector3 preyPosition = transform.localPosition + randomDirection * randomDistance;

        _prey.localPosition = new Vector3(preyPosition.x, 0.3f, preyPosition.z);





    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float preyPosX_normalized = _prey.localPosition.x / 5f;
        float preyPosY_normalized = _prey.localPosition.y / 5f;


        float predatorPosX_normalized = transform.localPosition.x / 5f;
        float predatorPosZ_normalized = transform.localPosition.z / 5f;

        float predatorRotation_nomalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;

        sensor.AddObservation(preyPosX_normalized);
        sensor.AddObservation(preyPosY_normalized);
        sensor.AddObservation(predatorPosX_normalized);
        sensor.AddObservation(predatorPosZ_normalized);
        sensor.AddObservation(predatorRotation_nomalized);


    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);

        AddReward(-2f / MaxStep);

        _cumulativeReward = GetCumulativeReward();
    }


    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];

            switch (action)
            {
                  case 1:
                       transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                       break;
                  case 2:
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                       break;

                   case 3: 
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                        break;
              
             }
    }





    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.05f);

            if(_renderer != null)
            {
                _renderer.material.color = Color.black;
            }
        }
        if (collision.collider.CompareTag("Prey"))
        {
            // predator reward
            AddReward(+1f);

            // let the prey know it was caught (if you have that method)
            collision.collider.GetComponent<PreyAgent>()?.GotCaught();

            EndEpisode();                   // end predator episode
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.01f * Time.deltaTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
            {
                _renderer.material.color = Color.white;
            }
        }
    }



}
