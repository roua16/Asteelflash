using ClosedXML.Excel;
using ITStockM.Domain.Entities;

namespace ITStockM.Services;

public static class ExcelExportService
{
    public static byte[] ExportMaterials(IEnumerable<Materiel> materials)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Materials");

        string[] headers = ["Name", "Type", "Qty IT Stock", "Serial #", "Supplier", "Warranty Expires", "Status"];
        WriteHeader(ws, headers);

        int row = 2;
        foreach (var m in materials)
        {
            ws.Cell(row, 1).Value = m.MaterielName ?? "";
            ws.Cell(row, 2).Value = m.Type ?? "";
            ws.Cell(row, 3).Value = m.QuantityITStock;
            ws.Cell(row, 4).Value = m.SerialNumber ?? "";
            ws.Cell(row, 5).Value = m.Supplier?.SupplierName ?? "";
            ws.Cell(row, 6).Value = m.Warranty != default ? m.Warranty.ToString("yyyy-MM-dd") : "";
            ws.Cell(row, 7).Value = m.LifecycleStatus ?? "";
            row++;
        }

        AutoFit(ws, headers.Length);
        return WorkbookBytes(wb);
    }

    public static byte[] ExportAssignments(IEnumerable<Assignment> assignments)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Assignments");

        string[] headers = ["ID", "Description", "Assigned To", "Assigned By", "Project", "Date", "Return Limit", "Provisional", "Materials"];
        WriteHeader(ws, headers);

        int row = 2;
        foreach (var a in assignments)
        {
            ws.Cell(row, 1).Value = a.Id;
            ws.Cell(row, 2).Value = a.Descipriton ?? "";
            ws.Cell(row, 3).Value = a.AssignedEmployee != null ? $"{a.AssignedEmployee.FullName}" : a.AssignedTo?.ToString() ?? "";
            ws.Cell(row, 4).Value = a.Employee != null ? $"{a.Employee.FullName}" : a.AssignedBy.ToString();
            ws.Cell(row, 5).Value = a.Project?.ProjectName ?? a.ProjectId?.ToString() ?? "";
            ws.Cell(row, 6).Value = a.Date.ToString("yyyy-MM-dd");
            ws.Cell(row, 7).Value = a.RestoreDateLimit.HasValue ? a.RestoreDateLimit.Value.ToString("yyyy-MM-dd") : "";
            ws.Cell(row, 8).Value = a.IsProvisional ? "Yes" : "No";
            ws.Cell(row, 9).Value = a.AssignmentMateriels != null
                ? string.Join(", ", a.AssignmentMateriels.Select(am => am.Materiel?.MaterielName ?? am.MaterielId.ToString()))
                : "";
            row++;
        }

        AutoFit(ws, headers.Length);
        return WorkbookBytes(wb);
    }

    public static byte[] ExportRequests(IEnumerable<Request> requests)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Purchase Requests");

        string[] headers = ["ID", "Title", "Employee", "Project", "Material Type", "Date", "Status", "Offers"];
        WriteHeader(ws, headers);

        int row = 2;
        foreach (var r in requests)
        {
            ws.Cell(row, 1).Value = r.Id;
            ws.Cell(row, 2).Value = r.Title ?? "";
            ws.Cell(row, 3).Value = r.Employee?.FullName ?? r.EmployeeId.ToString();
            ws.Cell(row, 4).Value = r.ProjectName ?? "";
            ws.Cell(row, 5).Value = r.MaterialType ?? "";
            ws.Cell(row, 6).Value = r.Date.ToString("yyyy-MM-dd");
            ws.Cell(row, 7).Value = r.Status ?? "";
            ws.Cell(row, 8).Value = r.Offers != null ? r.Offers.Count.ToString() : "0";
            row++;
        }

        AutoFit(ws, headers.Length);
        return WorkbookBytes(wb);
    }

    public static byte[] ExportEmployees(IEnumerable<Employee> employees)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Employees");

        string[] headers = ["Full Name", "Email", "Post", "Service", "Role", "Phone"];
        WriteHeader(ws, headers);

        int row = 2;
        foreach (var e in employees)
        {
            ws.Cell(row, 1).Value = e.FullName ?? "";
            ws.Cell(row, 2).Value = e.Email ?? "";
            ws.Cell(row, 3).Value = e.Post ?? "";
            ws.Cell(row, 4).Value = e.Service ?? "";
            ws.Cell(row, 5).Value = e.Role ?? "";
            ws.Cell(row, 6).Value = e.PhoneNumber ?? "";
            row++;
        }

        AutoFit(ws, headers.Length);
        return WorkbookBytes(wb);
    }

    private static void WriteHeader(IXLWorksheet ws, string[] headers)
    {
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2D3A4A");
            cell.Style.Font.FontColor = XLColor.White;
        }
    }

    private static void AutoFit(IXLWorksheet ws, int colCount)
    {
        for (int c = 1; c <= colCount; c++)
            ws.Column(c).AdjustToContents();
    }

    private static byte[] WorkbookBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
