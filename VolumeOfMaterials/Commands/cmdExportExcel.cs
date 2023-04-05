using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VolumeOfMaterials.Models;

namespace VolumeOfMaterials.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class cmdExportExcel : IExternalCommand
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
                var exportObject = new ExportObject(group.Key);
                exportObject.Volume = Helpers.ToCubeMeters(group.Sum(e => e.Volume));
                exportObject.Area = Helpers.ToSqMeters( group.Sum(e => e.Area));
                exportObject.Length =Helpers.ToMeters( group.Sum(e => e.Length));

                exportObjects.Add(exportObject);
            }

            var fileExport = new FileInfo(window.txtExportTable.Text);
            var tableExport = DSOffice.Data.ImportExcel(fileExport, window.txtExportBook.Text);

            foreach (var ex in exportObjects)
            {
                var subArrayIndex = Array.FindIndex(tableExport, row => Array.IndexOf(row, ex.Name) != -1);
                var index = Array.IndexOf(tableExport[subArrayIndex], ex.Name) + 1;

                object[][] exportArray = new object[1][];
                exportArray[0] = new object[] { ex.Volume.ToString(), ex.Area.ToString(), ex.Length.ToString() };
                DSOffice.Data.ExportExcel(window.txtExportTable.Text, window.txtExportBook.Text, subArrayIndex, index, exportArray);
            }

            return Result.Succeeded;
        }

        private bool CheckOfTypeElement(Element t)
        {
            if (t?.LookupParameter("PP_Code") != null
                && t?.LookupParameter("PP_NameElement") != null
                && t?.LookupParameter("PP_Code").AsString() != null
                && t?.LookupParameter("PP_NameElement").AsString() != null
                && t?.LookupParameter("PP_Code").AsString().Length > 0
                && t?.LookupParameter("PP_NameElement").AsString().Length > 0)
            {
                return true;
            }
            else return false;
        }

    }

}
