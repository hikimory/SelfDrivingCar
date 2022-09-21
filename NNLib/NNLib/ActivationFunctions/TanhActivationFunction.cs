using System;

namespace NNLib.ActivationFunctions
{
    public class TanhActivationFunction : IActivationFunction
    {
        public double CalculateDerivative(double input)
        {
            return 1 - (input * input);
        }

        public double CalculateOutput(double input)
        {
            return Math.Tanh(input);
        }
    }
}
