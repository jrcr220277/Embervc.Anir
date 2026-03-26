using System;
using System.Collections.Generic;
using System.Text;

namespace Anir.Shared.Contracts
{
    public class MunicipalityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
        public bool IsProvinceCapital { get; set; }
    }
}
