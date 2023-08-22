using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VolumeOfMaterials.FrontEnd;
using VolumeOfMaterials.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using static VolumeOfMaterials.Helpers;
using Application = Autodesk.Revit.ApplicationServices.Application;

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

            var linksType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsElementType().ToElements().Cast<RevitLinkType>();
            var selectedLinks = new List<RevitLinkType>();
            //var docElements = new List<Element>();
            var importObjects = new List<ImportObject>();
            var window = new FrontEnd.ExportExcelWindow(linksType, doc);
            window.ShowDialog();
            if (!window.DialogResult.Value) return Result.Cancelled;

            if (window.SelectedLinks.Count > 0)
            {
                foreach (var linkType in window.SelectedLinks)
                {
                    var ls = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements().Where(x => x.GetTypeId() == linkType.Id).Cast<RevitLinkInstance>().ToList();
                    ls.ForEach(x =>
                    {
                        var ldoc = x.GetLinkDocument();
                        var ldocElements = new FilteredElementCollector(ldoc).WhereElementIsNotElementType().ToElements().ToList();
                        importObjects.AddRange(new List<ImportObject>(ldocElements
                            .Select(element => (element, ldoc.GetElement(element.GetTypeId())))
                            .Where(tuple => CheckOfTypeElement(tuple.Item2))
                            .Select(tuple => new ImportObject(tuple.element, tuple.Item2, window.RulesNames))
                            .ToList()));

                    });
                }
            }

            var docElements = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements().ToList();
            var descriptions = window.lvDescriptions.Items.Cast<ItemDelete>().Select(item => item.Text).ToList();

            importObjects.AddRange(new List<ImportObject>(docElements
                .Select(element => (element, doc.GetElement(element.GetTypeId())))
                .Where(tuple => CheckOfTypeElement(tuple.Item2))
                .Select(tuple => new ImportObject(tuple.element, tuple.Item2, window.RulesNames))
                .ToList()));

            var exportObjects = importObjects
                .GroupBy(e => e.Code)
                .SelectMany(group =>
                {
                    var realDes = group.Select(e => e.Description).Distinct().ToList();
                    var missingDescriptions = descriptions.Except(realDes);
                    return group
                        .GroupBy(e => e.Description)
                        .Select(element => new ExportObject(group.Key)
                        {
                            Description = element.Key,
                            Name = element.FirstOrDefault().Name,
                            Volume = ToCubeMeters(element.Sum(e => e.Volume)),
                            Area = ToSqMeters(element.Sum(e => e.Area)),
                            Length = ToMeters(element.Sum(e => e.Length)),
                            Width = ToMeters(element.Sum(e => e.Width)),
                            Height = ToMeters(element.Sum(e => e.Height)),
                            Thickness = ToMeters(element.Sum(e => e.Thickness)),
                            Perimeter = ToMeters(element.Sum(e => e.Perimeter)),
                            Count = element.Sum(e => e.Count),
                        })
                        .Concat(missingDescriptions.Select(des => new ExportObject(group.Key)
                        {
                            Description = des,
                            Name = group.FirstOrDefault()?.Name,
                            Volume = 0,
                            Area = 0,
                            Perimeter = 0,
                            Thickness = 0,
                            Length = 0,
                            Code = group.FirstOrDefault()?.Code,
                            Count = 0,
                        }));
                })
                .ToList();


            var fileExport = new FileInfo(window.txtExportTable.Text);
            foreach (var ex in exportObjects)
            {
                foreach (ItemDelete item in window.lbBooks.Items)
                {
                    var book = item.Text;

                    var b = ex.Code.Substring(0, 3);

                    if (!book.Contains(ex.Code.Substring(0, 3))) continue;

                    var tableExport = DSOffice.Data.ImportExcel(fileExport, book);
                    
                    var realDescriptions = new List<string>();
                    var outEx = new List<ExportObject>(); 


                    var des = ex.Description;
                    var name = ex.Name;

                    if (des != null && des.Length > 0)
                    {
                        var subArrayIndex = Array.FindIndex(tableExport, row => Array.IndexOf(row, ex.Code) != -1);
                        int rowIndex = -1;
                        for (int i = 0; i < tableExport.Length; i++)
                        {
                            if (Array.IndexOf(tableExport[i], des) >= 0)
                            {
                                rowIndex = Array.IndexOf(tableExport[i], des);
                                break;
                            }
                        }

                        if (subArrayIndex != -1 && rowIndex != -1)
                        {
                            var index = rowIndex;
                            object[][] exportArray = new object[1][];

                            exportArray[0] = SetValuesToExport(ex);
                            
                            DSOffice.Data.ExportExcel(window.txtExportTable.Text, book, subArrayIndex, index, exportArray);
                            realDescriptions.Add(des);
                            object[][] myArray = new object[1][]; 

                            myArray[0] = new object[1];
                            myArray[0][0] = name;

                            DSOffice.Data.ExportToExcel(window.txtExportTable.Text, book, subArrayIndex, 1, myArray);
                        }
                        else
                        {

                        }
                    }
                }
            }



            return Result.Succeeded;
        }

        private bool CheckOfTypeElement(Element type)
        {
            if (type?.LookupParameter(PARAMETER_NAME_CODE) != null
                && type?.LookupParameter(PARAMETER_NAME_CODE).AsString() != null
                && type?.LookupParameter(PARAMETER_NAME_CODE).AsString().Length > 0)
            {
                return true;
            }
            else return false;
        }
    }
}
