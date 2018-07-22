//#r "DocumentFormat.OpenXml.dll"
//#r "WindowsBase.dll"

using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace FlowFunctionsTT
{
    public static class CockpitFileImporter
    {
        [FunctionName("CockpitFileImporter")]
        public static async Task Run(
            [BlobTrigger("flowfiles/{name}")] Stream myBlob, string name, TraceWriter log,
            [EventHub("floweventhubinstance", Connection = "EventhubConnection")] IAsyncCollector<EventData> outputQueue)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var runId = Guid.NewGuid();
            log.Info($"This is Run Id {runId}");

            var dictSheetIds = new Dictionary<string, string>();
            dictSheetIds["Jan"] = "";
            dictSheetIds["Feb"] = "";
            dictSheetIds["Mrz"] = "";
            dictSheetIds["Apr"] = "";
            dictSheetIds["Mai"] = "";
            dictSheetIds["Jun"] = "";
            dictSheetIds["Jul"] = "";
            dictSheetIds["Aug"] = "";
            dictSheetIds["Sep"] = "";
            dictSheetIds["Okt"] = "";
            dictSheetIds["Nov"] = "";
            dictSheetIds["Dez"] = "";

            List<DataStruct> dataStructs = new List<DataStruct>();

            myBlob.Position = 0;
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(myBlob, false))
            {
                log.Info("getting spread sheet");
                WorkbookPart workbookPart = doc.WorkbookPart;
                SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                SharedStringTable sst = sstpart.SharedStringTable;

                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                Worksheet sheet = worksheetPart.Worksheet;

                var sheets = workbookPart.Workbook.Sheets.Cast<Sheet>().ToList();
                sheets.ForEach(x =>
                {
                    if (dictSheetIds.ContainsKey(x.Name))
                        dictSheetIds[x.Name] = x.Id.Value;
                });

                var monthCounter = 1;
                var dataStruct = new DataStruct();

                foreach (var dictSheet in dictSheetIds)
                {
                    dataStruct = new DataStruct();
                    dataStruct.CorrellationId = runId;
                    dataStruct.MonthName = dictSheet.Key;
                    dataStruct.Month = monthCounter++;
                    dataStruct.TimeStamp = DateTime.UtcNow;

                    dataStruct.Rows = new List<List<string>>();
                    var specialPart = (WorksheetPart)workbookPart.GetPartById(dictSheet.Value);
                    var myRows = specialPart.Worksheet.Descendants<Row>();

                    int i = 0;
                    foreach (var myRow in myRows)
                    {
                        List<string> cellsList = new List<string>();

                        if (i < 21)
                        {
                            i++;
                            continue;
                        }

                        var cells = myRow.Descendants<Cell>();
                        var emptyRow = true;
                        foreach (var cell in cells)
                        {
                            var cellValue = GetCellValue(log, sst, cell);
                            if (emptyRow && !string.IsNullOrEmpty(cellValue))
                            {
                                emptyRow = false;
                            }
                            cellsList.Add(cellValue);
                        }

                        if (emptyRow)
                        {
                            continue;
                        }
                        dataStruct.Rows.Add(cellsList);
                        i++;
                    }
                    dataStructs.Add(dataStruct);
                    try
                    {
                        await SendMessageAsync(outputQueue, dataStruct);
                    }
                    catch (Exception e)
                    {
                        log.Error("queueing error", e);
                    }
                }


            }

        }

        private static async Task SendMessageAsync(IAsyncCollector<EventData> outputQueue, DataStruct dataStruct)
        {
            var eventBody = JsonConvert.SerializeObject(dataStruct);
            var eventData = new EventData(System.Text.Encoding.Default.GetBytes(eventBody));
            eventData.PartitionKey = "cockpit";
            await outputQueue.AddAsync(eventData);
        }

        private static string GetCellValue(TraceWriter log, SharedStringTable sst, Cell cell)
        {
            var str = "";
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                int ssid = int.Parse(cell.CellValue.Text);
                str = sst.ChildElements[ssid].InnerText;
            }
            else if (cell.CellValue != null)
            {
                str = cell.CellValue.Text;
            }
            return str;
        }
    }
}
