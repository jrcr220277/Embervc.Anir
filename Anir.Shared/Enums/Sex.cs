using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Anir.Shared.Enums
{
    public enum Sex
    {
        [Display(Name = "Femenino")]
        F = 1,       // Femenino
        
        [Display(Name = "Masculino")]
        M = 2        // Masculino
    }
}
