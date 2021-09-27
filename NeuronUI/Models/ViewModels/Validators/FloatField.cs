using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NeuronUI.Models.ViewModels.Validators
{
    public class FloatField : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                return new ValidationResult(false, "Este campo es requerido.");
            }

            string pattern = @"([0-9]*[.])?[0-9]+";
            Regex regex = new(pattern, RegexOptions.Singleline, TimeSpan.FromSeconds(1));

            return !regex.Match(str).Success
                ? new ValidationResult(false, "Este campo debe ser númerico")
                : ValidationResult.ValidResult;
        }
    }
}
