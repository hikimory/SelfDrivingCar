using System;

namespace NNLib.ActivationFunctions
{
    public class RectifiedActivationFuncion : IActivationFunction
    {
        public double CalculateDerivative(double input)
        {
            return Math.Max(0, input);
        }

        public double CalculateOutput(double input)
        {
            return Math.Max(0, input);
        }
    }
}
