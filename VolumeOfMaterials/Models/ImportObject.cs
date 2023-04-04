using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VolumeOfMaterials.Models
{
    public class ImportObject
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public double Volume { get; set; } = 0;
        public double Length { get; set; } = 0;
        public double Area { get; set; } = 0;
        public ImportObject(Element element, Element type)
        {
            Code = type.LookupParameter("PP_Code").AsString();
            Name = type.LookupParameter("PP_NameElement").AsString();

            try
            {
                //var elementClass = Code.Split('_').ToList()[0];
                var dimensions = Code.Split('_').ToList()[1];
                if (dimensions.Contains("V"))
                {
                    Volume = GetVolumeElement(element);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private double GetVolumeElement(Element element)
        {
            var geometryElement = element.get_Geometry(new Options());
            var solids = new List<Solid>(); 

            foreach (GeometryObject geometryObject in geometryElement)
            {
                var instance = geometryObject as GeometryInstance;
                if (instance != null)
                {
                    GeometryElement geometryElemenInstance = instance.GetInstanceGeometry();
                    foreach (GeometryObject o in geometryElemenInstance)
                    {
                        var solid = (Solid)null;
                        solid = o as Solid;
                        if (solid != null && solid.Volume > 0) solids.Add(solid);
                    }
                }

                else
                {
                    if(geometryObject is Solid)
                    {
                        var solid = (Solid)geometryObject;
                        solids.Add(solid);
                    }
                }
            }

            return solids.Sum(s => s.Volume);
        }
    }
}
