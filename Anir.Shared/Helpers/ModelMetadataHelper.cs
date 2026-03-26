using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Anir.Shared.Helpers
{
    /// <summary>
    /// ModelMetadataHelper
    /// -------------------
    /// Clase auxiliar para extraer metadatos de modelos basados en DataAnnotations.
    /// 
    /// Usos principales:
    /// - Formularios: generar etiquetas (Label) y textos de ayuda (HelperText, Placeholder) automáticamente.
    /// - Tablas: obtener encabezados amigables para columnas a partir de Display(Name).
    /// - Reportes: usar nombres descriptivos en exportaciones (Excel, PDF).
    /// - Documentación: listar propiedades con sus restricciones y validaciones.
    /// 
    /// Ejemplo de uso en Blazor con MudBlazor:
    /// <MudTextField Label="@ModelMetadataHelper.GetDisplayName(() => usuario.Email)"
    ///               HelperText="@ModelMetadataHelper.GetHelperText(() => usuario.Email)"
    ///               @bind-Value="usuario.Email" />
    /// 
    /// Resultado:
    /// - Label: "Correo electrónico" (si está definido en [Display(Name="...")])
    /// - HelperText: "Obligatorio · Correo válido" (si tiene [Required] y [EmailAddress])
    /// </summary>
    public static class ModelMetadataHelper
    {
        /// <summary>
        /// Obtiene el Display(Name) de una propiedad, o el nombre de la propiedad si no existe.
        /// </summary>
        public static string GetDisplayName<T>(Expression<Func<T>> expression)
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo property)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    return displayAttribute.GetName()
                        ?? displayAttribute.GetShortName()
                        ?? property.Name;
                }
                return property.Name; // fallback
            }
            return "Texto no definido";
        }

        /// <summary>
        /// Obtiene un texto de ayuda basado en las restricciones de DataAnnotations
        /// (Required, StringLength, Email, Range, etc.) para mostrar en formularios.
        /// </summary>
        public static string GetHelperText<T>(Expression<Func<T>> expression)
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo property)
            {
                var parts = new List<string>();

                // Required
                var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
                if (requiredAttr != null)
                    parts.Add("Obligatorio");

                // StringLength
                var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
                if (stringLengthAttr != null)
                {
                    if (stringLengthAttr.MinimumLength > 0)
                        parts.Add($"Entre {stringLengthAttr.MinimumLength} y {stringLengthAttr.MaximumLength} caracteres");
                    else
                        parts.Add($"Máximo {stringLengthAttr.MaximumLength} caracteres");
                }

                // MaxLength
                var maxLengthAttr = property.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLengthAttr != null)
                    parts.Add($"Máximo {maxLengthAttr.Length} caracteres");

                // MinLength
                var minLengthAttr = property.GetCustomAttribute<MinLengthAttribute>();
                if (minLengthAttr != null)
                    parts.Add($"Mínimo {minLengthAttr.Length} caracteres");

                // Email
                if (property.GetCustomAttribute<EmailAddressAttribute>() != null)
                    parts.Add("Correo válido");

                // Phone
                if (property.GetCustomAttribute<PhoneAttribute>() != null)
                    parts.Add("Teléfono válido");

                // Url
                if (property.GetCustomAttribute<UrlAttribute>() != null)
                    parts.Add("Dirección web válida");

                // CreditCard
                if (property.GetCustomAttribute<CreditCardAttribute>() != null)
                    parts.Add("Número de tarjeta válido");

                // Range
                var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
                if (rangeAttr != null)
                    parts.Add($"Entre {rangeAttr.Minimum} y {rangeAttr.Maximum}");

                // Regex
                if (property.GetCustomAttribute<RegularExpressionAttribute>() != null)
                    parts.Add("Formato válido requerido");

                // DataType
                var dataTypeAttr = property.GetCustomAttribute<DataTypeAttribute>();
                if (dataTypeAttr != null)
                {
                    switch (dataTypeAttr.DataType)
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
