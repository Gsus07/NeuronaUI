using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace NeuronUI.Models.ViewModels
{
    public class NeuronViewModel : ObservableObject
    {
        private Neuron _neuron;
        private string _weights = "No inicializados";

        private string _maxSteps;
        private string _trainingRate = string.Empty;
        private string _triggerFunction = string.Empty;

        private string _errorTolerance = string.Empty;

        private bool _isReadyForTraining;

        public bool IsReadyForTraining
        {
            get => _isReadyForTraining;
            set => OnPropertyChanged(ref _isReadyForTraining, value);
        }

        public IList<string> TriggerFunctions { get; } = new List<string>
        {
            "Escalón",
            "Lineal",
        };

        private int _steps = 0;

        public NeuronViewModel()
        {
            SetUpNeuron = new AsyncCommand(Init, CanSetUp);
            StartTraining = new AsyncCommand(TrainingNeuron);

            ErrorsSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Error de la iteración",
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(1)
                    },
                    PointGeometry = DefaultGeometries.Circle,
                }
            };

            VersusSeries = new SeriesCollection
            {
                 new LineSeries
                {
                    Title = "Resultado esperado",
                    Values = new ChartValues<ObservableValue>(),
                    PointGeometry = DefaultGeometries.Circle,
                },
                  new LineSeries
                {
                    Title = "Resultado obtenido",
                    Values = new ChartValues<ObservableValue>(),
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 10,
                    Stroke = Brushes.Gray
                }
            };
        }

        public string Weights
        {
            get => _weights;
            set
            {
                _weights = value;
                OnPropertyChanged(nameof(Weights));
            }
        }

        public string TrainingRate
        {
            get => _trainingRate;
            set => OnPropertyChanged(ref _trainingRate, value);
        }

        public string TriggerFunction
        {
            get => _triggerFunction;
            set => OnPropertyChanged(ref _triggerFunction, value);
        }

        public string ErrorTolerance
        {
            get => _errorTolerance;
            set => OnPropertyChanged(ref _errorTolerance, value);
        }

        public int Steps
        {
            get => _steps;
            set => OnPropertyChanged(ref _steps, value);
        }

        public string MaxSteps
        {
            get => _maxSteps;
            set => OnPropertyChanged(ref _maxSteps, value);
        }

        public Neuron Neuron
        {
            get => _neuron;
            set
            {
                _neuron = value;
                OnPropertyChanged(nameof(Neuron));

                ErrorsSeries[0].Values.Clear();
                ErrorsSeries[0].Values.Add(new ObservableValue(1));

                RefreshViewData();
                Steps = 0;
            }
        }

        public ICommand SetUpNeuron { get; }

        public ICommand StartTraining { get; }

        public SeriesCollection ErrorsSeries { get; set; }
        public SeriesCollection VersusSeries { get; set; }

        private void RefreshViewData()
        {
            

            string weightsStr = string.Empty;
            for (int i = 0; i < _neuron.Weights.Count; i++)
            {
                weightsStr += $"{_neuron.Weights[i]}";
                if (i < _neuron.Weights.Count - 1)
                {
                    weightsStr += ", ";
                }
            }

            Weights = weightsStr;

            TriggerFunction = _neuron.TriggerFunction switch
            {
                Models.TriggerFunction.Linear => "Lineal",
                Models.TriggerFunction.Step => "Escalón",
                _ => ""
            };
        }

        private bool CanSetUp()
        {
            return !string.IsNullOrWhiteSpace(_maxSteps) && !string.IsNullOrWhiteSpace(_trainingRate);
        }

        private Task Init(object parameter)
        {
            if (parameter is NeuronSetUpInputModel neuronInput)
            {
                var triggerFunction = neuronInput.TriggerFunction switch
                {
                    "Escalón" => Models.TriggerFunction.Step,
                    "Lienar" => Models.TriggerFunction.Linear,
                    _ => Models.TriggerFunction.Step,
                };
                Neuron = new Neuron(neuronInput.InputsNumber, neuronInput.TrainingRate, triggerFunction);
            }

            return Task.CompletedTask;
        }

        private Task TrainingNeuron(object parameter)
        {
            if (parameter is not NeuronTrainingInputModel neuronTraining)
            {
                return Task.CompletedTask;
            }

            int steps = 0;
            bool sw = false;

            while (!sw && (steps <= neuronTraining.MaxSteps))
            {
                ++steps;
                ++Steps;

                List<double> patternErrors = new();
                for (int i = 0; i < neuronTraining.Inputs.Count; i++)
                {
                    double[] input = neuronTraining.Inputs[i].ToArray();
                    double result = _neuron.Output(input);

                    double linealError = neuronTraining.Outputs[i] - result;
                    double patternError = Math.Abs(linealError);
                    patternErrors.Add(patternError);

                    if (result == neuronTraining.Outputs[i])
                    {
                        continue;
                    }

                    _neuron.Learn(input, neuronTraining.Outputs[i]);
                    RefreshViewData();
                }

                double patterErrorAverage = patternErrors.Average();
                sw = patterErrorAverage <= neuronTraining.ErrorTolerance;

                ErrorsSeries[0].Values.Add(new ObservableValue(patterErrorAverage));
            }

            return Task.CompletedTask;
        }
    }
}
