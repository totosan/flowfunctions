#r "DocumentFormat.OpenXml.dll"
#r "WindowsBase.dll"

using System.Net;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public static void Run(Stream myBlob, string name, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

    myBlob.Position = 0;
    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(myBlob, false))
    {
        log.Info("getting spread sheet");
        WorkbookPart workbookPart = doc.WorkbookPart;
        SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
        SharedStringTable sst = sstpart.SharedStringTable;

        WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
        Worksheet sheet = worksheetPart.Worksheet;

        var cells = sheet.Descendants<Cell>();
        var rows = sheet.Descendants<Row>();

        log.Info(string.Format("Row count = {0}", rows.LongCount()));
        log.Info(string.Format("Cell count = {0}", cells.LongCount()));

        // One way: go through each cell in the sheet
        foreach (Cell cell in cells)
        {
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                int ssid = int.Parse(cell.CellValue.Text);
                string str = sst.ChildElements[ssid].InnerText;
                log.Info(string.Format("Shared string {0}: {1}", ssid, str));
            }
            else if (cell.CellValue != null)
            {
                log.Info(string.Format("Cell contents: {0}", cell.CellValue.Text));
            }
        }
    }

}
