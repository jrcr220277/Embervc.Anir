using Anir.Shared.Contracts.Uebs;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anir.Infrastructure.Reports.Template.Excel
{
    public class UebReportExcel
    {
        public byte[] GenerateUebsExcel(List<UebDto> uebs)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Uebs");

            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Code";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Address";
            worksheet.Cell(1, 5).Value = "Phone";
            worksheet.Cell(1, 6).Value = "Email";
            worksheet.Cell(1, 7).Value = "CompanyName";
            worksheet.Cell(1, 8).Value = "ProvinceName";
            worksheet.Cell(1, 9).Value = "MunicipalityName";
            worksheet.Cell(1, 10).Value = "Active";

            for (int i = 0; i < uebs.Count; i++)
            {
                var c = uebs[i];
                worksheet.Cell(i + 2, 1).Value = c.Id;
                worksheet.Cell(i + 2, 2).Value = c.Code;
                worksheet.Cell(i + 2, 3).Value = c.Name;
                worksheet.Cell(i + 2, 4).Value = c.Address;
                worksheet.Cell(i + 2, 5).Value = c.Phone;
                worksheet.Cell(i + 2, 6).Value = c.Email;
                worksheet.Cell(i + 2, 7).Value = c.CompanyName;
                worksheet.Cell(i + 2, 8).Value = c.ProvinceName;
                worksheet.Cell(i + 2, 9).Value = c.MunicipalityName;
                worksheet.Cell(i + 2, 10).Value = c.Active;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}