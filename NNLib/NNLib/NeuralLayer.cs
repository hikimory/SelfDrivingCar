using System;
using System.Text;
using NNLib.ActivationFunctions;

namespace NNLib
{
    public class NeuralLayer
    {
        #region Members
        private static Random randomizer = new Random();

        /// <summary>
        /// The activation function used by the neurons of this layer.
        /// </summary>
        public IActivationFunction ActivationFunction
        {
            get;
            private set;
        }

        /// <summary>
        /// The amount of neurons in this layer.
        /// </summary>
        public uint NeuronCount
        {
            get;
            private set;
        }

        /// <summary>
        /// The amount of neurons this layer is connected to, i.e., the amount of neurons of the next layer.
        /// </summary>
        public uint OutputCount
        {
            get;
            private set;
        }

        /// <summary>
        /// The weights of the connections of this layer to the next layer.
        /// E.g., weight [i, j] is the weight of the connection from the i-th weight
        /// of this layer to the j-th weight of the next layer.
        /// </summary>
        public float[,] Weights
        {
            get;
            private set;
        }

        /// <summary>
        /// The weights of the connections of this layer to the next layer.
        /// E.g., weight [i, j] is the weight of the connection from the i-th weight
        /// of this layer to the j-th weight of the next layer.
        /// </summary>
        public float[] Biases
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new neural layer for a fully connected feedforward neural network with given 
        /// amount of node and with connections to the given amount of nodes of the next layer.
        /// </summary>
        /// <param name="nodeCount">The amount of nodes in this layer.</param>
        /// <param name="outputCount">The amount of nodes in the next layer.</param>
        public NeuralLayer(uint nodeCount, uint outputCount)
        {
            this.NeuronCount = nodeCount;
            this.OutputCount = outputCount;
            this.ActivationFunction = new SigmoidActivationFunction();
            this.Weights = new float[nodeCount, outputCount];
            this.Biases = new float[nodeCount];
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the weights of this layer to the given values.
        /// </summary>
        /// <param name="weights">
        /// The values to set the weights of the connections from this layer to the next to.
        /// </param>
        /// <remarks>
        /// The values are ordered in neuron order. E.g., in a layer with two neurons with a next layer of three neurons 
        /// the values [0-2] are the weights from neuron 0 of this layer to neurons 0-2 of the next layer respectively and 
        /// the values [3-5] are the weights from neuron 1 of this layer to neurons 0-2 of the next layer respectively.
        /// </remarks>
        public void SetWeights(float[] weights)
        {
            //Check arguments
            if (weights.Length != this.Weights.Length)
                throw new ArgumentException("Input weights do not match layer weight count.");

            // Copy weights from given value array
            int k = 0;
            for (int i = 0; i < this.Weights.GetLength(0); i++)
                for (int j = 0; j < this.Weights.GetLength(1); j++)
                    this.Weights[i, j] = weights[k++];
        }

        public void SetBiases(float[] biases)
        {
            //Check arguments
            if (biases.Length != this.Biases.Length)
                throw new ArgumentException("Input weights do not match layer weight count.");

            // Copy biases from given value array
            for (int i = 0; i < this.Biases.Length; i++)
                this.Biases[i] = biases[i];
        }

        /// <summary>
        /// Processes the given inputs using the current weights to the next layer.
        /// </summary>
        /// <param name="inputs">The inputs to be processed.</param>
        /// <returns>The calculated outputs.</returns>
        public float[] Calculate(float[] inputs)
        {
            //Check arguments
            if (inputs.Length != NeuronCount)
                throw new ArgumentException("Given xValues do not match layer input count.");

            //Calculate sum for each neuron from weighted inputs and bias
            float[] sums = new float[OutputCount];

            for (int j = 0; j < this.Weights.GetLength(1); j++)
                for (int i = 0; i < this.Weights.GetLength(0); i++)
                    sums[j] += inputs[i] * Weights[i, j] + Biases[i];

            //Apply activation function to sum, if set
            if (ActivationFunction != null)
            {
                for (int i = 0; i < sums.Length; i++)
                    sums[i] = (float)ActivationFunction.CalculateOutput(sums[i]);
            }

            return sums;
        }


        public void Mutate(float chance, float val)
        {
            for (int i = 0; i < this.Weights.GetLength(0); i++)
                for (int j = 0; j < this.Weights.GetLength(1); j++)
                    this.Weights[i, j] = (GetRandomValue(0.0f, 1.0f) <= chance) ? this.Weights[i, j] += GetRandomValue(-val, val) : this.Weights[i, j];

            for (int i = 0; i < this.Biases.Length; i++)
                this.Biases[i] = (GetRandomValue(0.0f, 1.0f) <= chance) ? this.Biases[i] += GetRandomValue(-val, val) : this.Biases[i];
        }

        /// <summary>
        /// Copies this NeuralLayer including its weights.
        /// </summary>
        /// <returns>A deep copy of this NeuralLayer</returns>
        public NeuralLayer DeepCopy()
        {
            //Copy weights
            float[,] copiedWeights = new float[this.Weights.GetLength(0), this.Weights.GetLength(1)];
            float[] copiedBiases = new float[this.Biases.Length];

            for (int x = 0; x < this.Weights.GetLength(0); x++)
                for (int y = 0; y < this.Weights.GetLength(1); y++)
                    copiedWeights[x, y] = this.Weights[x, y];

            for (int i = 0; i < this.Biases.Length; i++)
                copiedBiases[i] = this.Biases[i];

            //Create copy
            NeuralLayer newLayer = new NeuralLayer(this.NeuronCount, this.OutputCount);
            newLayer.Weights = copiedWeights;
            newLayer.Biases = copiedBiases;
            newLayer.ActivationFunction = this.ActivationFunction;

            return newLayer;
        }

        private float GetRandomValue(float minValue, float maxValue)
        {
            float range = Math.Abs(minValue - maxValue);
            return minValue + (float)(randomizer.NextDouble() * range);
        }
        /// <summary>
        /// Sets the weights of the connection from this layer to the next to random values in given range.
        /// </summary>
        /// <param name="minValue">The minimum value a weight may be set to.</param>
        /// <param name="maxValue">The maximum value a weight may be set to.</param>
        public void SetRandomWeights(float minValue, float maxValue)
        {
            for (int i = 0; i < Weights.GetLength(0); i++)
                for (int j = 0; j < Weights.GetLength(1); j++)
                    Weights[i, j] = GetRandomValue(minValue, maxValue); //random float between minValue and maxValue
        }

        /// <summary>
        /// Sets the biases of the connection from this layer to the next to random values in given range.
        /// </summary>
        /// <param name="minValue">The minimum value a biases may be set to.</param>
        /// <param name="maxValue">The maximum value a biases may be set to.</param>
        public void SetRandomBiases(float minValue, float maxValue)
        {
            for (int i = 0; i < Biases.Length; i++)
                Biases[i] = GetRandomValue(minValue, maxValue); //random float between minValue and maxValue
        }

        public void SetActivationFunction(IActivationFunction func)
        {
            this.ActivationFunction = func;
        }

        /// <summary>
        /// Returns a string representing this layer's connection weights.
        /// </summary>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append($"Weights{Environment.NewLine}");
            for (int x = 0; x < Weights.GetLength(0); x++)
            {
                for (int y = 0; y < Weights.GetLength(1); y++)
                    output.Append($"[{x},{y}]: {Weights[x, y].ToString("0.00")} ");

                output.Append(Environment.NewLine);
            }
            output.Append($"Biases{Environment.NewLine}");
            for (int i = 0; i < Biases.Length; i++)
                output.Append($"[{ i}]: {Biases[i].ToString("0.00")} ");

            return output.ToString();
        }
        #endregion
    }
}
