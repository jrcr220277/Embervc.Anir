using Anir.Shared.Contracts.Persons;
using ClosedXML.Excel;

namespace Anir.Infrastructure.Reports.Template.Excel
{
    public class PersonReportExcel
    {
        public byte[] GeneratePersonsExcel(List<PersonDto> persons)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Personas");

            worksheet.Cell(1, 1).Value = "CI";
            worksheet.Cell(1, 2).Value = "Nombre";
            worksheet.Cell(1, 3).Value = "Celular";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Active";

            for (int i = 0; i < persons.Count; i++)
            {
                var entity = persons[i];
                worksheet.Cell(i + 2, 1).Value = entity.Dni;
                worksheet.Cell(i + 2, 2).Value = entity.FullName;
                worksheet.Cell(i + 2, 3).Value = entity.CellPhone;
                worksheet.Cell(i + 2, 4).Value = entity.Email;
                worksheet.Cell(i + 2, 5).Value = entity.Active;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}