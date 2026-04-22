using Anir.Shared.Contracts.AnirWorks;
using Anir.Shared.Contracts.Companies;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anir.Infrastructure.Reports.Template.Excel
{
    public class AnirWorkReportExce
    {
        public byte[] GenerateAnirWorksExcel(List<AnirWorkDto> works)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Trabajos ANIR");

            // Cabeceras
            worksheet.Cell(1, 1).Value = "Número ANIR";
            worksheet.Cell(1, 2).Value = "Título";
            worksheet.Cell(1, 3).Value = "Empresa";
            worksheet.Cell(1, 4).Value = "UEB";
            worksheet.Cell(1, 5).Value = "Fecha";
            worksheet.Cell(1, 6).Value = "Estado";

            // Datos
            for (int i = 0; i < works.Count; i++)
            {
                var w = works[i];
                worksheet.Cell(i + 2, 1).Value = w.AnirNumber;
                worksheet.Cell(i + 2, 2).Value = w.Title;
                worksheet.Cell(i + 2, 3).Value = w.CompanyName;
                worksheet.Cell(i + 2, 4).Value = w.UebName;
                worksheet.Cell(i + 2, 5).Value = w.Date.ToString("dd/MM/yyyy");
                worksheet.Cell(i + 2, 6).Value = w.State.ToString();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}