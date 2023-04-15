using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VolumeOfMaterials.Models;
using static VolumeOfMaterials.Helpers;

namespace VolumeOfMaterials.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdExportExcel : IExternalCommand
    {
        [System.Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            var window = new FrontEnd.ExportExcelWindow();
            window.ShowDialog();
            if (!window.DialogResult.Value) return Result.Cancelled;

            var docElements = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements();
            var importObjects = new List<ImportObject>();

            foreach (var element in docElements)
            {
                var type = doc.GetElement(element.GetTypeId());
                if (CheckOfTypeElement(type))
                {
                    var importObject = new ImportObject(element, type);
                    importObjects.Add(importObject);
                }
            }

            var exportObjects = new List<ExportObject>();

            foreach (var group in importObjects.GroupBy(e => e.Name))
            {
                var exportObject = new ExportObject(group.Key)
                {
                    Volume = ToCubeMeters(group.Sum(e => e.Volume)),
                    Area = ToSqMeters(group.Sum(e => e.Area)),
                    Length = ToMeters(group.Sum(e => e.Length)),
                    Code = group.FirstOrDefault()?.Code,
                    Count = group.Sum(e => e.Count),
                };

                exportObjects.Add(exportObject);
            }

            var fileExport = new FileInfo(window.txtExportTable.Text);
            var tableExport = DSOffice.Data.ImportExcel(fileExport, window.txtExportBook.Text);

            foreach (var ex in exportObjects)
            {
                var subArrayIndex = Array.FindIndex(tableExport, row => Array.IndexOf(row, ex.Code) != -1);
                if (subArrayIndex != -1)
                {
                    var index = Array.IndexOf(tableExport[subArrayIndex], ex.Code) + 2;
                    object[][] exportArray = new object[1][];
                    exportArray[0] = SetValuesToExport(ex);
                    DSOffice.Data.ExportExcel(window.txtExportTable.Text, window.txtExportBook.Text, subArrayIndex, index, exportArray);
                }
            }
            return Result.Succeeded;
        }

        private bool CheckOfTypeElement(Element type)
        {
            if (type?.LookupParameter("PP_Code") != null
                && type?.LookupParameter("PP_NameElement") != null
                && type?.LookupParameter("PP_Code").AsString() != null
                && type?.LookupParameter("PP_NameElement").AsString() != null
                && type?.LookupParameter("PP_Code").AsString().Length > 0
                && type?.LookupParameter("PP_NameElement").AsString().Length > 0)
            {
                return true;
            }
            else return false;
        }
    }
}
