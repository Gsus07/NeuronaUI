using System.Globalization;
using System.Windows.Controls;

namespace NeuronUI.Models.ViewModels.Validators
{
    public class DoubleRange : ValidationRule
    {
        public double Minimum { get; set; }
        public double Maximum { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                return new ValidationResult(false, "Este campo es requerido.");
            }

            if (!double.TryParse(str, out double val))
            {
                return new ValidationResult(false, $"El valor digitado no es un número");
            }

            if (val > Maximum)
            {
                return new ValidationResult(false, $"El valor digitado no debe ser mayor a {Maximum}");
            }

            return val < Minimum
                ? new ValidationResult(false, $"El valor digitado no debe ser menor a {Minimum}")
                : ValidationResult.ValidResult;
        }
    }
}
