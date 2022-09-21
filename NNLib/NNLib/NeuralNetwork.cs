using System;
using System.Text;
using NNLib.ActivationFunctions;

namespace NNLib
{
    public class NeuralNetwork : IComparable<NeuralNetwork>
    {
        #region Members
        /// <summary>
        /// The individual neural layers of this network.
        /// </summary>
        public NeuralLayer[] Layers
        {
            get;
            private set;
        }

        /// <summary>
        /// An array of unsigned integers representing the node count 
        /// of each layer of the network from input to output layer.
        /// </summary>
        public uint[] Topology
        {
            get;
            private set;
        }

        /// <summary>
        /// The amount of overall weights of the connections of this network.
        /// </summary>
        public int WeightCount
        {
            get;
            private set;
        }

        public float Fitness
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new fully connected feedforward neural network with given topology.
        /// </summary>
        /// <param name="topology">An array of unsigned integers representing the node count of each layer from input to output layer.</param>
        public NeuralNetwork(params uint[] topology)
        {
            this.Topology = topology;
            this.Fitness = 0.0f;
            //Calculate overall weight count
            WeightCount = 0;
            for (int i = 0; i < topology.Length - 1; i++)
                WeightCount += (int)((topology[i] + 1) * topology[i + 1]); // + 1 for bias node

            //Initialise layers
            Layers = new NeuralLayer[topology.Length - 1];
            for (int i = 0; i < Layers.Length; i++)
                Layers[i] = new NeuralLayer(topology[i], topology[i + 1]);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Processes the given inputs using the current network's weights.
        /// </summary>
        /// <param name="inputs">The inputs to be processed.</param>
        /// <returns>The calculated outputs.</returns>
        public float[] FeedForward(float[] inputs)
        {
            //Process inputs by propagating values through all layers
            float[] outputs = inputs;

            foreach (NeuralLayer layer in Layers)
                outputs = layer.Calculate(outputs);

            return outputs;

        }

        /// <summary>
        /// Sets the weights of this network to random values in given range.
        /// </summary>
        /// <param name="minValue">The minimum value a weight may be set to.</param>
        /// <param name="maxValue">The maximum value a weight may be set to.</param>
        public void SetRandomLayerValues(float minValue, float maxValue)
        {
            if (Layers != null)
            {
                foreach (NeuralLayer layer in Layers)
                {
                    layer.SetRandomWeights(minValue, maxValue);
                    layer.SetRandomBiases(minValue, maxValue);
                }
            }
        }

        /// <summary>
        /// Sets the activate function for each layer of this network.
        /// </summary>
        /// <param name="func">The activate function.</param>
        public void SetActivationFunction(IActivationFunction func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            foreach (NeuralLayer layer in Layers)
                layer.SetActivationFunction(func);
        }

        /// <summary>
        /// Returns a new NeuralNetwork instance with the same topology and 
        /// activation functions, but the weights set to their default value.
        /// </summary>
        public NeuralNetwork GetTopologyCopy()
        {
            NeuralNetwork copy = new NeuralNetwork(this.Topology);
            for (int i = 0; i < Layers.Length; i++)
                copy.Layers[i].SetActivationFunction(this.Layers[i].ActivationFunction);

            return copy;
        }

        /// <summary>
        /// Copies this NeuralNetwork including its topology and weights.
        /// </summary>
        /// <returns>A deep copy of this NeuralNetwork</returns>
        public NeuralNetwork DeepCopy()
        {
            NeuralNetwork newNet = new NeuralNetwork(this.Topology);
            for (int i = 0; i < this.Layers.Length; i++)
                newNet.Layers[i] = this.Layers[i].DeepCopy();

            return newNet;
        }

        public void Mutate(float chance, float val)
        {
            foreach (NeuralLayer layer in Layers)
                layer.Mutate(chance, val);
        }

        /// <summary>
        /// Returns a string representing this network in layer order.
        /// </summary>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < Layers.Length; i++)
                output.Append($"Layer_{i}:{Environment.NewLine}{Layers[i]}{Environment.NewLine}");

            return output.ToString();
        }

        public int CompareTo(NeuralNetwork other)
        {
            if (other == null) return 1;

            if (Fitness > other.Fitness)
                return 1;
            else if (Fitness < other.Fitness)
                return -1;
            else
                return 0;
        }
        #endregion
    }
}
