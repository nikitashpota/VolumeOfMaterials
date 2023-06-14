using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using Parameter = Autodesk.Revit.DB.Parameter;

namespace VolumeOfMaterials.Models
{
    public class ImportObject
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public double Volume { get; set; } = 0;
        public double Length { get; set; } = 0;
        public double Area { get; set; } = 0;
        public double Height { get; set; } = 0;
        public double Count { get; set; } = 1;
        public double Perimeter { get; set; } = 0;
        public double Thickness { get; set; } = 0;


        public ImportObject(Element element, Element type)
        {
            Code = type.LookupParameter("PP_Code").AsString();
            Description = element.LookupParameter("PP_Description").AsString();

            try
            {
                var dimensions = Helpers.GetDemensionsFromCode(Code);
                var props = Helpers.GetPropertiesInOrder(dimensions);

                foreach (var prop in props)
                {
                    switch (prop)
                    {
                        case Helpers.Property.Volume:
                            Volume = GetVolumeElement(element);
                            break;

                        case Helpers.Property.Length:
                            Length = GetLengthElement(element);
                            break;

                        case Helpers.Property.Area:
                            Area = GetAreaElement(element);
                            break;

                        case Helpers.Property.Count:
                            Count = 1;
                            break;

                        case Helpers.Property.Height:
                            Height = GetHeightElement(element);
                            break;

                        case Helpers.Property.Perimeter:
                            Perimeter = GetPerimeterElement(element);
                            break;

                        case Helpers.Property.Thickness:
                            Thickness = GetThicknessElement(element);
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка импортируемого объекта", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private int GetCategoryId(Element e)
        {
            var categoryId = e.Category.GetHashCode();
            return categoryId;
        }


        private double GetThicknessElement(Element e)
        {
            var thickness = (double)0;
            var categoryId = GetCategoryId(e);
            var parameter = (Parameter)null;

            if (categoryId == BuiltInCategory.OST_Floors.GetHashCode())
            {
                parameter = e.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
            }
            if (parameter != null)
            {
                thickness = parameter.AsDouble();
            }

            return thickness;
        }

        private double GetPerimeterElement(Element e)
        {
            var perimeter = (double)0;
            var categoryId = GetCategoryId(e);
            var parameter = (Parameter)null;

            if (categoryId == BuiltInCategory.OST_Floors.GetHashCode())
            {
                parameter = e.get_Parameter(BuiltInParameter.HOST_PERIMETER_COMPUTED);
            }
            if (parameter != null)
            {
                perimeter = parameter.AsDouble();
            }

            return perimeter;
        }

        private double GetLengthElement(Element e)
        {
            var length = (double)0;
            var categoryId = GetCategoryId(e);
            var parameter = (Parameter)null;

            if (categoryId == BuiltInCategory.OST_StructuralFraming.GetHashCode()
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
        private double GetHeightElement(Element e)
        {
            var height = (double)0;
            var categoryId = GetCategoryId(e);
            var parameter = (Parameter)null;

            if (categoryId == BuiltInCategory.OST_StructuralColumns.GetHashCode()
                || categoryId == BuiltInCategory.OST_Walls.GetHashCode())
            {
                parameter = e.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANELS_HEIGHT);
            }
            if (parameter != null)
            {
                height = parameter.AsDouble();
            }
            return height;
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
                    if (geometryObject is Solid)
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
