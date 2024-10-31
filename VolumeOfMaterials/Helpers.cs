using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolumeOfMaterials.Models;

namespace VolumeOfMaterials
{
    public class Helpers
    {
        public static string PARAMETER_NAME_CODE { get; } = "PP_Code";
        public static string PARAMETER_NAME_DESCRIPTION { get; } = "PP_Description";

        public static string NAME_OF_STRUCTURE = "<СТРУКТУРА>";
        public static double ToMeters(double feet, int decimals = 2) => Math.Round(UnitUtils.Convert(feet, UnitTypeId.Feet, UnitTypeId.Meters), decimals, MidpointRounding.AwayFromZero);
        public static double ToMillimeters(double feet, int decimals = 2) => Math.Round(UnitUtils.Convert(feet, UnitTypeId.Feet, UnitTypeId.Millimeters), decimals, MidpointRounding.AwayFromZero);
        public static double ToSqMeters(double sqFeet, int decimals = 2) => Math.Round(UnitUtils.Convert(sqFeet, UnitTypeId.SquareFeet, UnitTypeId.SquareMeters), decimals, MidpointRounding.AwayFromZero);
        public static double ToCubeMeters(double cubeFeet, int decimals = 2) => Math.Round(UnitUtils.Convert(cubeFeet, UnitTypeId.CubicFeet, UnitTypeId.CubicMeters), decimals, MidpointRounding.AwayFromZero);
        public static string GetDemensionsFromCode(string code) => code.Split('_').ToList()[1];
        public enum Property
        {
            Volume,
            Length,
            Area,
            Count,
            Height,
            Perimeter,
            Thickness,
            Width,
        }


        public enum BookCategory
        {
            [Description("Стены (WAL)")]
            Wall,
            [Description("Перекрытия (SLB)")]
            Slab,
            [Description("Кровля (ROF)")]
            Roof,
            [Description("Окна и Двери (OPN)")]
            Window,
            [Description("Потолки (CLG)")]
            Celling,
        }

        

        public static object[] SetValuesToExport(ExportObject ex)
        {
            var code = ex.Code;

            Property[] order = GetPropertiesInOrder(GetDemensionsFromCode(code));
            var l = (int)order.Length;
            object[] obj = new object[l];
            for (int i = 0; i < order.Length; i++)
            {
                switch (order[i])
                {
                    case Property.Volume:
                        obj[i] = ex.Volume;
                        break;
                    case Property.Length:
                        obj[i] = ex.Length;
                        break;
                    case Property.Area:
                        obj[i] = ex.Area;
                        break;
                    case Property.Count:
                        obj[i] = ex.Count;
                        break;
                    case Property.Perimeter:
                        obj[i] = ex.Perimeter;
                        break;
                    case Property.Thickness:
                        obj[i] = ex.Thickness;
                        break;
                    case Property.Width:
                        obj[i] = ex.Width;
                        break;
                    case Property.Height:
                        obj[i] = ex.Height;
                        break;
                }
            }
            return obj;
        }

        public static Property[] GetPropertiesInOrder(string order)
        {
            List<Property> properties = new List<Property>();
            foreach (char c in order)
            {
                switch (c)
                {
                    case 'V':
                        properties.Add(Property.Volume);
                        break;
                    case 'L':
                        properties.Add(Property.Length);
                        break;
                    case 'A':
                        properties.Add(Property.Area);
                        break;
                    case 'C':
                        properties.Add(Property.Count);
                        break;
                    case 'P':
                        properties.Add(Property.Perimeter);
                        break;
                    case 'T':
                        properties.Add(Property.Thickness);
                        break;
                    case 'H':
                        properties.Add(Property.Height);
                        break;
                    case 'W':
                        properties.Add(Property.Width);
                        break;
                    default:
                        throw new ArgumentException($"Invalid order: {order}");
                }
            }
            return properties.ToArray();
        }



    }


}
