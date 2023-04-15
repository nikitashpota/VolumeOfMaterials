using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolumeOfMaterials.Models;

namespace VolumeOfMaterials
{
    public class Helpers
    {
        public static double ToMeters(double feet, int decimals = 2) => Math.Round(UnitUtils.Convert(feet, UnitTypeId.Feet, UnitTypeId.Meters), decimals, MidpointRounding.AwayFromZero);
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
                    default:
                        throw new ArgumentException($"Invalid order: {order}");
                }
            }
            return properties.ToArray();
        }



    }


}
