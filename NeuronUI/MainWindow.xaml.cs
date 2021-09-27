using System.Windows;

namespace NeuronUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in MainGrid.Children)
            {
                if (item is NeuronTraining neuronTraining)
                {
                    neuronTraining.SimulationClick += NeuronTraining_SimulationClick;
                }
            }
        }

        private void NeuronTraining_SimulationClick()
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(new NeuronSimulation(ReturnToTrainingPanel));
        }

        private void ReturnToTrainingPanel()
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(new NeuronTraining(NeuronTraining_SimulationClick));
        }
    }
}
