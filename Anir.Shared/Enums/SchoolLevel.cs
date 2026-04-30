using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Anir.Shared.Enums
{
    public enum SchoolLevel
    {
        ES = 1,      // Educación Secundaria
        EMS = 2,     // Educación Media Superior

        [Display(Name = "12mo Grado")]
        Twelve = 3   // 12mo Grado
    }
}
