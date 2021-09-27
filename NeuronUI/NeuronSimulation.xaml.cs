using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiveCharts.Defaults;
using Microsoft.Win32;
using NeuronUI.Data;
using NeuronUI.Models;
using NeuronUI.Models.ViewModels;
using NeuronUI.Utils;

namespace NeuronUI
{
    /// <summary>
    /// Interaction logic for NeuronSimulation.xaml
    /// </summary>
    public partial class NeuronSimulation
    {
        private NeuronViewModel NeuronViewModel { get; set; }

        private List<List<double>> Inputs { get; set; }
        private List<double> ExpectedOutputs { get; set; }

        private List<SimulationData> SimulationData { get; set; }

        public NeuronSimulation()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            SimulationData = new List<SimulationData>();
            DataTable.ItemsSource = SimulationData;
        }

        public NeuronSimulation(ReturnToTraining returnToTraining) : this()
        {
            ReturnToTraining += returnToTraining;
        }

        public event ReturnToTraining ReturnToTraining;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NeuronViewModel = DataContext as NeuronViewModel;
            if (NeuronViewModel is null)
            {
                return;
            }

            NeuronViewModel.VersusSeries[0].Values.Clear();
            NeuronViewModel.VersusSeries[1].Values.Clear();
        }

        private static List<string[]> LoadCsvDataFromFile()
        {
            var fileName = SelectFile();
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var data = CsvDataLoader.LoadCsv(fileName);
            return data;
        }

        private static string SelectFile()
        {
            OpenFileDialog openFileDialog = new()
            {
                DefaultExt = "*.csv",
                Filter = "CSV Files (*.csv)|*.csv|TXT Files (*.txt)|*.txt"
            };

            var result = openFileDialog.ShowDialog();

            return result.HasValue && result.Value ? openFileDialog.FileName : string.Empty;
        }

        private void LoadSimulationDataButton_Click(object sender, RoutedEventArgs e)
        {
            var data = LoadCsvDataFromFile();
            if (data is null || data.Count == 0)
            {
                return;
            }

            Inputs = new();
            ExpectedOutputs = new();

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
                    Inputs.Add(inputs);
                }

                if (double.TryParse(item.Last(), out double outputResult))
                {
                    ExpectedOutputs.Add(outputResult);
                }
            }

            PatternsCountChip.Content = $"Patrones: {Inputs.Count}";
            StartSimulationButton.IsEnabled = true;

            NeuronViewModel.VersusSeries[0].Values.Clear();
            NeuronViewModel.VersusSeries[1].Values.Clear();
            SimulationData.Clear();
            DataTable.Items.Refresh();
        }

        private void StartSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            NeuronViewModel.VersusSeries[0].Values.Clear();
            NeuronViewModel.VersusSeries[1].Values.Clear();

            for (int i = 0; i < Inputs.Count; i++)
            {
                var inputs = Inputs[i];
                double obtainedOutput = NeuronViewModel.Neuron.Output(inputs.ToArray());

                string inputsStr = string.Empty;
                for (int j = 0; j < inputs.Count; j++)
                {
                    inputsStr += $"{inputs[j]}";
                    if (j < inputs.Count - 1)
                    {
                        inputsStr += ", ";
                    }
                }

                SimulationData simulationData = new()
                {
                    Inputs = inputsStr,
                    ObtainedOutput = obtainedOutput,
                    ExpectedOutput = ExpectedOutputs[i]
                };

                SimulationData.Add(simulationData);

                NeuronViewModel.VersusSeries[0].Values.Add(new ObservableValue(ExpectedOutputs[i]));
                NeuronViewModel.VersusSeries[1].Values.Add(new ObservableValue(obtainedOutput));
                DataTable.Items.Refresh();
            }
        }

        private void ReturnToTrainingButton_Click(object sender, RoutedEventArgs e)
        {
            ReturnToTraining?.Invoke();
        }
    }
}
