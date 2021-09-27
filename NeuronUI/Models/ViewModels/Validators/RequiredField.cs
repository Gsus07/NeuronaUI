using System.Globalization;
using System.Windows.Controls;

namespace NeuronUI.Models.ViewModels.Validators
{
    public class RequiredField : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "Esta campo es requerido")
                : ValidationResult.ValidResult;
        }
    }
}
