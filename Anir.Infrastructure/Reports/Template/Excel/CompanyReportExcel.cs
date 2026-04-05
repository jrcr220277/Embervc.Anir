using Anir.Shared.Contracts.Companies;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anir.Infrastructure.Reports.Template.Excel
{
    public class CompanyReportExcel
    {
        public byte[] GenerateCompaniesExcel(List<CompanyDto> companies)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Companies");

            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Municipality";
            worksheet.Cell(1, 4).Value = "Province";
            worksheet.Cell(1, 5).Value = "Active";

            for (int i = 0; i < companies.Count; i++)
            {
                var c = companies[i];
                worksheet.Cell(i + 2, 1).Value = c.Id;
                worksheet.Cell(i + 2, 2).Value = c.Name;
                worksheet.Cell(i + 2, 3).Value = c.MunicipalityName;
                worksheet.Cell(i + 2, 4).Value = c.ProvinceName;
                worksheet.Cell(i + 2, 5).Value = c.Active;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}