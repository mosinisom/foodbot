using System.IO;
using OfficeOpenXml;

void ProcessFile()
{
    var lines = File.ReadAllLines("list.txt");
    using (var package = new ExcelPackage(new FileInfo("output.xlsx")))
    {
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");
        for (int i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(" - ");
            if (parts.Length == 2)
            {
                worksheet.Cells[i + 1, 1].Value = parts[0].Trim();
                worksheet.Cells[i + 1, 2].Value = parts[1].Trim().TrimEnd('.');
            }
        }
        package.Save();
    }
}

ProcessFile();
