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

        public double Count { get; set; } = 1;


        public ImportObject(Element element, Element type)
        {
            Code = type.LookupParameter("PP_Code").AsString();
            Name = type.LookupParameter("PP_NameElement").AsString();

            try
            {
                var dimensions = Code.Split('_').ToList()[1];
                if (dimensions.Contains("V"))
                {
                    Volume = GetVolumeElement(element);
                }
                if (dimensions.Contains("A"))
                {
                    Area = GetAreaElement(element);
                }
                if (dimensions.Contains("L"))
                {
                    Length = GetLengthElement(element);
                }

            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            
        }

        private int GetCategoryId (Element e)
        {
            var categoryId = 0;
            categoryId = e.Category.GetHashCode();
            return categoryId;
        }

        private double GetLengthElement(Element e)
        {
            var length = (double)0;
            var categoryId = GetCategoryId(e);
            var parameter = (Parameter)null;

            if(categoryId == BuiltInCategory.OST_StructuralFraming.GetHashCode() 
                || categoryId == BuiltInCategory.OST_Walls.GetHashCode()
                || categoryId == BuiltInCategory.OST_StairsRailing.GetHashCode()
                || categoryId == BuiltInCategory.OST_Roofs.GetHashCode())
            {
                parameter = e.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
            }

            if (parameter != null)
            {
                length = parameter.AsDouble();
            }

            return length;
        }

        private double GetAreaElement(Element e)
        {
            var parameter = e.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
            var area = (double)0;

            if (parameter != null)
            {
                area = parameter.AsDouble();
            }
            return area;
        }

        private double GetVolumeElement(Element e)
        {
            var geometryElement = e.get_Geometry(new Options());
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
