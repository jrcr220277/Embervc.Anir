using Anir.Shared.Contracts.Organisms;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anir.Infrastructure.Reports.Template.Excel
{
    public class OrganismReportExcel
    {
        public byte[] GenerateOrganismsExcel(List<OrganismDto> organisms)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Organisms");

            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Code";
            worksheet.Cell(1, 3).Value = "ShortName";
            worksheet.Cell(1, 4).Value = "Name";
          
            for (int i = 0; i < organisms.Count; i++)
            {
                var entity = organisms[i];
                worksheet.Cell(i + 2, 1).Value = entity.Id;
                worksheet.Cell(i + 2, 2).Value = entity.Code;
                worksheet.Cell(i + 2, 3).Value = entity.ShortName;
                worksheet.Cell(i + 2, 4).Value = entity.Name;
           
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}