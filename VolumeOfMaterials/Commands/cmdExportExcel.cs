using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VolumeOfMaterials.FrontEnd;
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

            var linksType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsElementType().ToElements().Cast<RevitLinkType>();
            var selectedLinks = new List<RevitLinkType>();
            var docElements = new List<Element>();
            var window = new FrontEnd.ExportExcelWindow(linksType);
            window.ShowDialog();
            if (!window.DialogResult.Value) return Result.Cancelled;

            if(window.SelectedLinks.Count > 0)
            {
                foreach(var linkType in window.SelectedLinks)
                {
                    var ls = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements().Where(x => x.GetTypeId() == linkType.Id).Cast<RevitLinkInstance>().ToList();
                    ls.ForEach(x => {
                        var ldoc = x.GetLinkDocument();
                        docElements.AddRange(new FilteredElementCollector(ldoc).WhereElementIsNotElementType().ToElements().ToList());
                    });
                }
            }

            docElements.AddRange(new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements().ToList());
            var descriptions = window.lvDescriptions.Items.Cast<ItemDelete>().Select(item => item.Text).ToList();

            var importObjects = docElements
                .Select(element => (element, doc.GetElement(element.GetTypeId())))
                .Where(tuple => CheckOfTypeElement(tuple.Item2))
                .Select(tuple => new ImportObject(tuple.element, tuple.Item2))
                .ToList();

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
                            Volume = ToCubeMeters(element.Sum(e => e.Volume)),
                            Area = ToSqMeters(element.Sum(e => e.Area)),
                            Length = ToMeters(element.Sum(e => e.Length)),
                            Thickness = ToMeters(element.Sum(e => e.Thickness)),
                            Perimeter = ToMeters(element.Sum(e => e.Perimeter)),
                            Count = element.Sum(e => e.Count),
                        })
                        .Concat(missingDescriptions.Select(des => new ExportObject(group.Key)
                        {
                            Description = des,
                            Volume = 0,
                            Area = 0,
                            Perimeter= 0,
                            Thickness= 0,
                            Length = 0,
                            Code = group.FirstOrDefault()?.Code,
                            Count = 0,
                        }));
                })
                .ToList();


            var fileExport = new FileInfo(window.txtExportTable.Text);
            foreach (ItemDelete item in window.lbBooks.Items)
            {
                var book = item.Text;
                var tableExport = DSOffice.Data.ImportExcel(fileExport, book);
                foreach (var ex in exportObjects)
                {
                    if (!ex.Code.Intersect(book).Any()) continue;
                    var realDescriptions = new List<string>();

                    var des = ex.Description;

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
                        }
                    }
                }
            }



            return Result.Succeeded;
        }

        private bool CheckOfTypeElement(Element type)
        {
            if (type?.LookupParameter("PP_Code") != null
                && type?.LookupParameter("PP_Code").AsString() != null
                && type?.LookupParameter("PP_Code").AsString().Length > 0)
            {
                return true;
            }
            else return false;
        }
    }
}
