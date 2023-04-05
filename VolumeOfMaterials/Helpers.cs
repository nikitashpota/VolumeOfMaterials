using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeOfMaterials
{
    public class Helpers
    {
        public static double ToMeters(double feet, int decimals = 2) => Math.Round(UnitUtils.Convert(feet, UnitTypeId.Feet, UnitTypeId.Meters), decimals, MidpointRounding.AwayFromZero);
        public static double ToSqMeters(double sqFeet, int decimals = 2) => Math.Round(UnitUtils.Convert(sqFeet, UnitTypeId.SquareFeet, UnitTypeId.SquareMeters), decimals, MidpointRounding.AwayFromZero);
        public static double ToCubeMeters(double cubeFeet, int decimals = 2) => Math.Round(UnitUtils.Convert(cubeFeet, UnitTypeId.CubicFeet, UnitTypeId.CubicMeters), decimals, MidpointRounding.AwayFromZero);
        
    }
}
