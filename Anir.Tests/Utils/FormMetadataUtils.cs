using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Anir.Client.Utils
{
    /// <summary>
    /// Utilidades para extraer metadatos de propiedades del modelo
    /// y generar etiquetas y textos de ayuda para formularios Blazor.
    /// </summary>
    public static class FormMetadataUtils
    {
        public static string GetDisplayName<T>(Expression<Func<T>> expression)
        {
            if (expression.Body is MemberExpression member &&
                member.Member is PropertyInfo property)
            {
                var display = property.GetCustomAttribute<DisplayAttribute>();
                return display?.Name ?? property.Name;
            }

            return "Texto no definido";
        }

        public static string GetHelperText<T>(Expression<Func<T>> expression)
        {
            if (expression.Body is MemberExpression member &&
                member.Member is PropertyInfo property)
            {
                var parts = new List<string>();

                if (property.GetCustomAttribute<RequiredAttribute>() != null)
                    parts.Add("Obligatorio");

                var stringLength = property.GetCustomAttribute<StringLengthAttribute>();
                if (stringLength != null)
                {
                    if (stringLength.MinimumLength > 0)
                        parts.Add($"Entre {stringLength.MinimumLength} y {stringLength.MaximumLength} caracteres");
                    else
                        parts.Add($"Máximo {stringLength.MaximumLength} caracteres");
                }

                var maxLength = property.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLength != null)
                    parts.Add($"Máximo {maxLength.Length} caracteres");

                var minLength = property.GetCustomAttribute<MinLengthAttribute>();
                if (minLength != null)
                    parts.Add($"Mínimo {minLength.Length} caracteres");

                if (property.GetCustomAttribute<EmailAddressAttribute>() != null)
                    parts.Add("Correo válido");

                if (property.GetCustomAttribute<PhoneAttribute>() != null)
                    parts.Add("Teléfono válido");

                if (property.GetCustomAttribute<UrlAttribute>() != null)
                    parts.Add("Dirección web válida");

                if (property.GetCustomAttribute<CreditCardAttribute>() != null)
                    parts.Add("Número de tarjeta válido");

                var range = property.GetCustomAttribute<RangeAttribute>();
                if (range != null)
                    parts.Add($"Entre {range.Minimum} y {range.Maximum}");

                if (property.GetCustomAttribute<RegularExpressionAttribute>() != null)
                    parts.Add("Formato válido requerido");

                var dataType = property.GetCustomAttribute<DataTypeAttribute>();
                if (dataType != null)
                {
                    switch (dataType.DataType)
                    {
                        case DataType.Date:
                            parts.Add("Formato de fecha válido");
                            break;
                        case DataType.Currency:
                            parts.Add("Formato monetario válido");
                            break;
                        case DataType.Password:
                            parts.Add("Campo de contraseña");
                            break;
                    }
                }

                return string.Join(" · ", parts);
            }

            return string.Empty;
        }
    }
}
