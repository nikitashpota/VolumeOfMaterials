#region Namespaces
using Autodesk.Revit.UI;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endregion

namespace VolumeOfMaterials
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            DirectoryInfo thisAssemblyDirectoryInfo = new DirectoryInfo(assembly.Location);


            var ribbonPanel = application.CreateRibbonPanel("VoM");

            var pbImportExport = ribbonPanel.AddItem(new PushButtonData("ExportExcel", "Export to\nExcel",
                thisAssemblyDirectoryInfo.FullName,
                typeof(Commands.cmdExportExcel).FullName)) as PushButton;

            pbImportExport.LargeImage = GetResourceImage(assembly, "VolumeOfMaterials.Resources.vop24.png");
            pbImportExport.Image = GetResourceImage(assembly, "VolumeOfMaterials.Resources.vop16.png");

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
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
