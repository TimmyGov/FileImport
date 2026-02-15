using ClosedXML.Excel;

namespace FileImportService.Tests.SampleFiles;

/// <summary>
/// Helper class to generate sample Excel file for testing
/// </summary>
public static class ExcelFileGenerator
{
    public static void GenerateSampleExcel(string filePath)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        // Add headers
        worksheet.Cell("A1").Value = "Id";
        worksheet.Cell("B1").Value = "Name";
        worksheet.Cell("C1").Value = "Email";
        worksheet.Cell("D1").Value = "Amount";
        worksheet.Cell("E1").Value = "Date";

        // Add data rows
        worksheet.Cell("A2").Value = 1;
        worksheet.Cell("B2").Value = "John Doe";
        worksheet.Cell("C2").Value = "john@example.com";
        worksheet.Cell("D2").Value = 1000.50;
        worksheet.Cell("E2").Value = "2026-01-15";

        worksheet.Cell("A3").Value = 2;
        worksheet.Cell("B3").Value = "Jane Smith";
        worksheet.Cell("C3").Value = "jane@example.com";
        worksheet.Cell("D3").Value = 2500.75;
        worksheet.Cell("E3").Value = "2026-01-16";

        worksheet.Cell("A4").Value = 3;
        worksheet.Cell("B4").Value = "Bob Johnson";
        worksheet.Cell("C4").Value = "bob@example.com";
        worksheet.Cell("D4").Value = 750.00;
        worksheet.Cell("E4").Value = "2026-01-17";

        worksheet.Cell("A5").Value = 4;
        worksheet.Cell("B5").Value = "Alice Williams";
        worksheet.Cell("C5").Value = "alice@example.com";
        worksheet.Cell("D5").Value = 3200.25;
        worksheet.Cell("E5").Value = "2026-01-18";

        worksheet.Cell("A6").Value = 5;
        worksheet.Cell("B6").Value = "Charlie Brown";
        worksheet.Cell("C6").Value = "charlie@example.com";
        worksheet.Cell("D6").Value = 1800.00;
        worksheet.Cell("E6").Value = "2026-01-19";

        workbook.SaveAs(filePath);
    }
}
