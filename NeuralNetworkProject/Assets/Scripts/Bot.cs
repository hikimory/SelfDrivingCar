using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bot : MonoBehaviour
{
    public UnityEvent OnHit;
    public float speed;//Speed Multiplier
    public float rotation;//Rotation multiplier
    public LayerMask raycastMask;//Mask for the sensors

    private float[] input = new float[5];//input to the neural network
    public NNLib.NeuralNetwork network;
    [SerializeField] private GameObject[] m_checkPoints;

    public int position;//Checkpoint number on the course
    public bool collided;//To tell if the car has crashed

    private void Awake()
    {
        OnHit = new UnityEvent();
        position = 0;
        m_checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");
    }

    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
        if (!collided)//if the car has not collided with the wall, it uses the neural network to get an output
        {
            for (int i = 0; i < 5; i++)//draws five debug rays as inputs
            {
                Vector3 newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;//calculating angle of raycast
                RaycastHit hit;
                Ray Ray = new Ray(transform.position, newVector);

                if (Physics.Raycast(Ray, out hit, 10, raycastMask))
                {
                    input[i] = (10 - hit.distance) / 10;//return distance, 1 being close
                    Debug.DrawLine(Ray.origin, hit.point, Color.red);
                }
                else
                {
                    input[i] = 0;//if nothing is detected, will return 0 to network
                    Debug.DrawLine(Ray.origin, hit.point, Color.red);
                }
            }

            float[] output = network.FeedForward(input);//Call to network to feedforward
        
            transform.Rotate(0, output[0] * rotation, 0, Space.World);//controls the cars movement
            transform.position += this.transform.right * output[1] * speed;//controls the cars turning
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))//check if the car passes a gate
        {
            for (int i=0; i < m_checkPoints.Length; i++)
            {
                if (collision.collider.gameObject == m_checkPoints[i] && i == position % m_checkPoints.Length)
                {
                    position++;//if the gate is one ahead of it, it increments the position, which is used for the fitness/performance of the network
                    break;
                }
            }
        }
        else if(collision.collider.gameObject.layer != LayerMask.NameToLayer("Learner"))
        {
            collided = true;//stop operation if car has collided
            OnHit?.Invoke();
        }
    }

    public void UpdateFitness()
    {
        network.Fitness = position;//updates fitness of network for sorting
    }
}
