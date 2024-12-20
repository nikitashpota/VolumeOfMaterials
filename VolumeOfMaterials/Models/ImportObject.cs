﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Parameter = Autodesk.Revit.DB.Parameter;

namespace VolumeOfMaterials.Models
{
    public class ImportObject
    {
        public string Code { get; set; } = "";
        public List<string> Name { get; set; } = new List<string>();
        public string Description { get; set; } = "";
        public double Volume { get; set; } = 0;
        public double Length { get; set; } = 0;
        public double Width { get; set; } = 0;
        public double Area { get; set; } = 0;
        public double Height { get; set; } = 0;
        public double Count { get; set; } = 1;
        public double Perimeter { get; set; } = 0;
        public double Thickness { get; set; } = 0;


        public ImportObject(Element element, Element type, List<RuleNameObject> rulesNames)
        {
            Code = type.LookupParameter(Helpers.PARAMETER_NAME_CODE).AsString();
            Description = element.LookupParameter(Helpers.PARAMETER_NAME_DESCRIPTION)?.AsString() ?? "-";
            var lightCode = Code.Substring(0, 3);
            var selectItem = rulesNames.Where(x => x.Tag.Intersect(lightCode).Any()).FirstOrDefault();

            if (selectItem != null)
            {
                var selectParameters = selectItem.Parameters;
                var selectPrefixes = selectItem.Prefixes;
                var selectSuffixes = selectItem.Suffixes;
                var selectDivides = selectItem.Divides;

                var typeParameters = type.Parameters;
                var name = "";
                var nameList = new List<string>();
                for (int i = 0; i < selectParameters.Count; i++)
                {
                    var selectDivide = selectDivides[i] ? "\n" : " ";
                    var selectParameter = selectParameters[i];
                    if (selectParameter != Helpers.NAME_OF_STRUCTURE)
                    {
                        var res = GetNameFromParameters(typeParameters, selectParameter);
                        if (res != null && res != "")
                        {
                            name += $"{selectPrefixes[i]}{res}{selectSuffixes[i]}{selectDivide}";
                            nameList.Add(name);
                        }
                    }
                    else
                    {
                        var layers = (type as HostObjAttributes)?.GetCompoundStructure()?.GetLayers();
                        if (layers != null && layers.Count > 0)
                        {
                            foreach (var layer in layers)
                            {
                                try
                                {
                                    var mat = App.CurrentDocument.GetElement(layer.MaterialId);
                                    if (mat != null)
                                    {
                                        var lkp = mat.LookupParameter("ADSK_Материал наименование");
                                        var widthLayer = Helpers.ToMillimeters(layer.Width);
                                        var resWidth = widthLayer > 0 ? $" - {widthLayer} мм" : "";

                                        if (lkp != null)
                                        {
                                            var res = lkp.AsValueString();
                                            if (res != null && res != "")
                                            {
                                                nameList.Add($"{selectPrefixes[i]}{res}{resWidth}{selectSuffixes[i]}{selectDivide}");
                                            }

                                        }
                                    }
                                }
                                catch
                                {

                                }
                            }

                        }
                    }
                }
                Name = nameList;
            }
            else
            {

            }

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

                        case Helpers.Property.Width:
                            Width = GetWidthElement(element);
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка импортируемого объекта", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private string GetNameFromParameters(ParameterSet parameters, string parameterName)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter.Definition.Name == parameterName)
                {
                    return parameter.AsValueString();
                }
            }

            return "";
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

            if (categoryId == BuiltInCategory.OST_Floors.GetHashCode()
                || categoryId == BuiltInCategory.OST_Ceilings.GetHashCode())
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

        private double GetWidthElement(Element e)
        {
            var width = (double)0;
            var categoryId = GetCategoryId(e);
            var parameter = (Parameter)null;

            if (categoryId == BuiltInCategory.OST_Windows.GetHashCode()
                || categoryId == BuiltInCategory.OST_Doors.GetHashCode())
            {
                parameter = e.get_Parameter(BuiltInParameter.GENERIC_WIDTH);
            }
            if (parameter != null)
            {
                width = parameter.AsDouble();
            }

            return width;
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
            if (categoryId == BuiltInCategory.OST_Windows.GetHashCode()
                || categoryId == BuiltInCategory.OST_Doors.GetHashCode())
            {
                parameter = e.get_Parameter(BuiltInParameter.CASEWORK_HEIGHT);
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
