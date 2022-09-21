using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NNLib;
using NNLib.ActivationFunctions;
using System.Linq;
using UnityEngine.Events;

public class GeneticManager : MonoBehaviour
{
    //[SerializeField] private float m_timeframe;
    [SerializeField] private int m_populationSize;//creates population size
    [SerializeField] private GameObject m_prefab;//holds bot prefab
    [SerializeField] private GameObject m_targetPoint;//holds bot prefab

    [SerializeField] private uint[] m_layers = new uint[3] { 5, 3, 2 };//initializing network to the right size

    [SerializeField, Range(0.0001f, 1f)] private float m_mutationChance = 0.01f;
    [SerializeField, Range(0f, 5f)] private float m_mutationStrength = 0.5f;

    [SerializeField, Range(0.0001f, 1f)] private float m_crossoverChance = 0.01f;
    [SerializeField, Range(0.0001f, 1f)] private float m_crossoverProbability = 0.01f;
    [SerializeField, Range(0.1f, 10f)] private float m_gamespeed = 1f;

    public UnityEvent<StatsInfo> InfoChanged;

    private List<NeuralNetwork> _networks;
    private List<Bot> _cars;
    private int _aliveCount = 0;
    private float _prevMaxFitness = 0.0f;
    private float _prevMedianFitness = 0.0f;
    private int generationNumber = 0;
    private System.Diagnostics.Stopwatch _stopwatch;

    // Start is called before the first frame update
    void Start()
    {
        if (m_populationSize % 2 != 0)
            m_populationSize = 50;//if population size is not even, sets it to fifty

        _aliveCount = m_populationSize;
        InitNetworks();
        CreateBots();
        //InvokeRepeating("CreateBots", 0.1f, timeframe);//repeating function
    }

    public void InitNetworks()
    {
        _networks = new List<NeuralNetwork>();
        for (int i = 0; i < m_populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(m_layers);
            net.SetRandomLayerValues(-5.0f, 5.0f);
            net.SetActivationFunction(new TanhActivationFunction());
            _networks.Add(net);
        }
    }

    public void CreateBots()
    {
        Time.timeScale = m_gamespeed;//sets gamespeed, which will increase to speed up training
        if (_cars != null)
        {
            EvolveOneGeneration();
            for (int i = 0; i < _cars.Count; i++)
            {
                GameObject.Destroy(_cars[i].gameObject);//if there are Prefabs in the scene this will get rid of them
            }
        }

        _cars = new List<Bot>();
        for (int i = 0; i < m_populationSize; i++)
        {
            Bot car = (Instantiate(m_prefab, m_targetPoint.transform.position, m_targetPoint.transform.rotation)).GetComponent<Bot>();//create botes
            car.network = _networks[i];//deploys network to each learner
            car.OnHit.AddListener(UpdateCars);
            _cars.Add(car);
        }
        _stopwatch = System.Diagnostics.Stopwatch.StartNew();
    }

    private void EvolveOneGeneration()
    {
        CalculateFitnes();
        UpdateCarsInfo();
        var parents = SelectParents();
        var offspring = Cross(parents);
        Mutate(offspring);
        Reinsert(offspring);
    }
    private List<NeuralNetwork> SelectParents()
    {
        var ordered = _networks.OrderByDescending(p => p.Fitness);
        return ordered.Take(m_populationSize).ToList();
    }

    private List<NeuralNetwork> Cross(List<NeuralNetwork> parents)
    {
        var offspring = new List<NeuralNetwork>(m_populationSize);

        for (int i = 0; i < m_populationSize; i += 2)
        {
            var selectedParents = parents.Skip(i).Take(2).ToList();
            var children = SelectParentsAndCross(selectedParents);
            if (children != null)
            {
                offspring.AddRange(children);
            }
        }
        return offspring;
    }

    private List<NeuralNetwork> SelectParentsAndCross(List<NeuralNetwork> parents)
    {
        if (UnityEngine.Random.Range(0.0f, 1.0f) < m_crossoverProbability)
        {
            NeuralNetwork parent1 = parents[0];
            NeuralNetwork parent2 = parents[1];

            NeuralNetwork child1 = new NeuralNetwork(m_layers);
            child1.SetActivationFunction(new TanhActivationFunction());
            NeuralNetwork child2 = new NeuralNetwork(m_layers);
            child2.SetActivationFunction(new TanhActivationFunction());

            for (int j = 0; j < m_layers.Length - 1; j++)
            {
                for (int k = 0; k < parent1.Layers[j].Weights.GetLength(0); k++)
                {
                    for (int n = 0; n < parent1.Layers[j].Weights.GetLength(1); n++)
                    {
                        if (UnityEngine.Random.Range(0.0f, 1.0f) < m_crossoverChance)
                        {
                            child1.Layers[j].Weights[k, n] = parent1.Layers[j].Weights[k, n];
                            child2.Layers[j].Weights[k, n] = parent2.Layers[j].Weights[k, n];
                        }
                        else
                        {
                            child2.Layers[j].Weights[k, n] = parent1.Layers[j].Weights[k, n];
                            child1.Layers[j].Weights[k, n] = parent2.Layers[j].Weights[k, n];
                        }
                    }
                }

                for (int k = 0; k < parent1.Layers[j].Biases.Length; k++)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) < m_crossoverChance)
                    {
                        child1.Layers[j].Biases[k] = parent1.Layers[j].Biases[k];
                        child2.Layers[j].Biases[k] = parent2.Layers[j].Biases[k];
                    }
                    else
                    {
                        child2.Layers[j].Biases[k] = parent1.Layers[j].Biases[k];
                        child1.Layers[j].Biases[k] = parent2.Layers[j].Biases[k];
                    }
                }
            }
            return new List<NeuralNetwork>() { child1, child2 };
        }
        return null;
    }
    private void Mutate(List<NeuralNetwork> networks)
    {
        foreach (var network in networks)
        {
            network.Mutate(m_mutationChance, m_mutationStrength);
        }
    }

    private void Reinsert(List<NeuralNetwork> offspring)
    {
        var diff = m_populationSize - offspring.Count;

        if (diff > 0)
        {
            var bestParents = _networks.OrderByDescending(p => p.Fitness).Take(diff).ToList();

            for (int i = 0; i < bestParents.Count; i++)
            {
                offspring.Add(bestParents[i]);
            }
        }

        _networks = offspring;
    }

    private void CalculateFitnes()
    {
        for (int i = 0; i < m_populationSize; i++)
        {
            _cars[i].UpdateFitness();//gets bots to set their corrosponding networks fitness
        }
    }

    private void UpdateCarsInfo()
    {
        float maxFitness = 0.0f, medianFitness = 0.0f;
        for (int i = 0; i < _networks.Count; i++)
        {
            if (maxFitness < _networks[i].Fitness) maxFitness = _networks[i].Fitness;

            medianFitness += _networks[i].Fitness;
        }
        medianFitness /= _networks.Count;
        generationNumber++;

        StatsInfo info = new StatsInfo()
        {
            Number = generationNumber,
            Population = m_populationSize,
            PreviousMaxFitness = _prevMaxFitness,
            PreviousMedianFitness = _prevMedianFitness,
            MaxFitness = maxFitness,
            MedianFitness = medianFitness,
            Duration = _stopwatch.Elapsed
        };

        InfoChanged?.Invoke(info);

        _prevMaxFitness = maxFitness;
        _prevMedianFitness = medianFitness;

    }

    public void UpdateCars()
    {
        _aliveCount--;
        if (_aliveCount == 0)
        {
            _stopwatch.Stop();
            _aliveCount = m_populationSize;
            CreateBots();
        }
    }
}
