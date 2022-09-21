namespace NNLib.ActivationFunctions
{
    /// <summary>
    /// Interface for activation functions.
    /// </summary>
    public interface IActivationFunction
    {
        double CalculateOutput(double input);
        double CalculateDerivative(double input);
    }
}
