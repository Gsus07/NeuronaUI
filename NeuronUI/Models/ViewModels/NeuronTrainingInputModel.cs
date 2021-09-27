using System.Collections.Generic;

namespace NeuronUI.Models.ViewModels
{
    public class NeuronTrainingInputModel
    {
        public List<List<double>> Inputs { get; init; }
        public List<double> Outputs { get; init; }

        public int MaxSteps { get; init; }
        public double ErrorTolerance { get; init; }
    }
}
