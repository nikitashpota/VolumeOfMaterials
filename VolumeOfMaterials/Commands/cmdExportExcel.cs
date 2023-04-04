using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.IO;
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
            var filterElements = new List<Element>();

            foreach (var element in docElements)
            {
                var type = doc.GetElement(element.GetTypeId());

                //Room.ParametersMap.Contains(name)

                if (type?.LookupParameter("PP_Code") != null
                    && type?.LookupParameter("PP_NameElement") != null
                    && type?.LookupParameter("PP_Code").AsString() != null
                    && type?.LookupParameter("PP_NameElement").AsString() != null
                    && type?.LookupParameter("PP_Code").AsString().Length > 0
                    && type?.LookupParameter("PP_NameElement").AsString().Length > 0)
                {
                    filterElements.Add(element);
                    var importObject = new ImportObject(element, type);
                    var volume = importObject.Volume;
                }
            }

            var fileImport = new FileInfo(window.txtImportTable.Text);
            var tableImport = DSOffice.Data.ImportExcel(fileImport, window.txtImportBook.Text);
            for (int i = 1; i < tableImport.Length - 1; i++)
            {
                var tableString = tableImport[i];
            }

            object[][] myArray = new object[3][];
            //myArray[0] = new object[] { "hello1", "hello2", "hello3" };
            //myArray[1] = new object[] { "world1", "world3", "world3" };
            //myArray[2] = new object[] { "!1", "!", "!3" };
            //DSOffice.Data.ExportExcel(window.txtExportTable.Text, window.txtExportBook.Text, 0, 0, myArray);

            return Result.Succeeded;
        }
    }
}
