#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endregion

namespace VolumeOfMaterials
{
    internal class App : IExternalApplication
    {
        private Stopwatch stopWatch = new Stopwatch();
        public Result OnStartup(UIControlledApplication application)
        {
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            DirectoryInfo thisAssemblyDirectoryInfo = new DirectoryInfo(assembly.Location);

            var ribbonPanel = application.CreateRibbonPanel("VoM");

            var pbImportExport = ribbonPanel.AddItem(new PushButtonData("ExportExcel", "Export to\nExcel",
                thisAssemblyDirectoryInfo.FullName,
                typeof(Commands.CmdExportExcel).FullName)) as PushButton;

            pbImportExport.LargeImage = GetResourceImage(assembly, "VolumeOfMaterials.Resources.vop24.png");
            pbImportExport.Image = GetResourceImage(assembly, "VolumeOfMaterials.Resources.vop16.png");


            //try
            //{
            //    application.ControlledApplication.DocumentOpening += new EventHandler<DocumentOpeningEventArgs>(application_DocumentOpening);
            //    application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(application_DocumentOpened);
            //}

            //catch (Exception)
            //{
            //    return Result.Failed;
            //}

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            //application.ControlledApplication.DocumentOpening -= application_DocumentOpening;
            //application.ControlledApplication.DocumentOpened -= application_DocumentOpened;
            return Result.Succeeded;
        }

        public void application_DocumentOpening(object sender, DocumentOpeningEventArgs args)
        {
            stopWatch.Start();
        }

        public void application_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {

            Document doc = args.Document;
            

            stopWatch.Stop();
            
            // Get the elapsed time as a TimeSpan value.
            var ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            stopWatch.Reset();

            TaskDialog.Show("Hello", $"Document is opened, time {elapsedTime}");
        }

        public ImageSource GetResourceImage(Assembly assembly, string imageName)
        {
            try
            {
                Stream resource = assembly.GetManifestResourceStream(imageName);
                return BitmapFrame.Create(resource);
            }
            catch
            {
                return null;
            }
        }


    }
}
