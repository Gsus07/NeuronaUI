using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NeuronUI.Data;
using NeuronUI.Models;
using NeuronUI.Models.ViewModels;
using NeuronUI.Utils;

namespace NeuronUI
{
    /// <summary>
    /// Interaction logic for NeuronTraining.xaml
    /// </summary>
    public partial class NeuronTraining
    {
        private NeuronViewModel NeuronViewModel { get; set; }

        private List<List<double>> TrainingInputs { get; set; }
        private List<double> Outputs { get; set; }

        public NeuronTraining()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        public NeuronTraining(OnSimulationClick onSimulationClick) : this()
        {
            SimulationClick += onSimulationClick;
        }

        public event OnSimulationClick SimulationClick;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NeuronViewModel = DataContext as NeuronViewModel;
        }

        private void SetUpNeuronButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation.GetHasError(MaxIterationsField) &&
                !Validation.GetHasError(TrainingRateField) &&
                !Validation.GetHasError(ErrorToleranceField))
            {
                if (!string.IsNullOrEmpty(MaxIterationsField.Text) &&
                    !string.IsNullOrEmpty(TrainingRateField.Text) &&
                    !string.IsNullOrEmpty(ErrorToleranceField.Text))
                {
                    double trainingRate = double.Parse(TrainingRateField.Text);

                    NeuronSetUpInputModel neuron = new()
                    {
                        InputsNumber = TrainingInputs[0].Count,
                        TrainingRate = trainingRate,
                        TriggerFunction = TriggerFunctionField.SelectedItem.ToString()
                    };

                    NeuronViewModel.SetUpNeuron.Execute(neuron);

                    StartTraining.IsEnabled = true;
                }
            }
        }

        private static List<string[]> LoadCsvDataFromFile()
        {
            string fileName = SelectFile();
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            List<string[]> data = CsvDataLoader.LoadCsv(fileName);
            return data;
        }

        private static string SelectFile()
        {
            OpenFileDialog openFileDialog = new()
            {
                DefaultExt = "*.csv",
                Filter = "CSV Files (*.csv)|*.csv|TXT Files (*.txt)|*.txt"
            };

            bool? result = openFileDialog.ShowDialog();

            return result.HasValue && result.Value ? openFileDialog.FileName : string.Empty;
        }

        private void LoadNeuronButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                DefaultExt = "*.csv",
                Filter = "JSON files (*.json)|*.json"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                Neuron neuron = JsonFileMapper.LoadObject<Neuron>(openFileDialog.FileName);
                if (neuron != null)
                {
                    NeuronViewModel.Neuron = neuron;
                    StartTraining.IsEnabled = true;
                    StartSimulationButton.IsEnabled = true;
                }
            }
        }

        private void LoadInputsButton_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> data = LoadCsvDataFromFile();
            if (data is null || data.Count == 0)
            {
                return;
            }

            TrainingInputs = new();
            Outputs = new();

            foreach (string[] item in data)
            {
                List<double> inputs = new();
                for (int i = 0; i < item.Length - 1; i++)
                {
                    if (double.TryParse(item[i], out double inputResult))
                    {
                        inputs.Add(inputResult);
                    }
                }

                if (inputs.Any())
                {
                    TrainingInputs.Add(inputs);
                }

                if (double.TryParse(item.Last(), out double outputResult))
                {
                    Outputs.Add(outputResult);
                }
            }

            InputsCountChip.Content = $"Entradas: {TrainingInputs[0].Count}";
            PatternsCountChip.Content = $"Patrones: {TrainingInputs.Count}";

            SetUpNeuronButton.IsEnabled = true;
            LoadNeuronButton.IsEnabled = true;
        }

        private void StartTraining_Click(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(MaxIterationsField) || Validation.GetHasError(TrainingRateField) ||
                Validation.GetHasError(ErrorToleranceField))
            {
                return;
            }

            if (string.IsNullOrEmpty(MaxIterationsField.Text) || string.IsNullOrEmpty(TrainingRateField.Text) ||
                string.IsNullOrEmpty(ErrorToleranceField.Text))
            {
                return;
            }

            if (TrainingInputs == null || TrainingInputs.Count <= 0)
            {
                return;
            }

            double errorTolerance = double.Parse(NeuronViewModel.ErrorTolerance);
            int maxSteps = int.Parse(NeuronViewModel.MaxSteps);
            NeuronTrainingInputModel neuronTraining = new()
            {
                MaxSteps = maxSteps,
                Inputs = TrainingInputs,
                Outputs = Outputs,
                ErrorTolerance = errorTolerance
            };

            NeuronViewModel.StartTraining.Execute(neuronTraining);

            SaveNeuron.IsEnabled = true;
            StartSimulationButton.IsEnabled = true;
        }

        private void SaveNeuron_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                JsonFileMapper.SaveObject(saveFileDialog.FileName, NeuronViewModel.Neuron);
            }
        }

        private void StartSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            SimulationClick?.Invoke();
        }
    }
}
