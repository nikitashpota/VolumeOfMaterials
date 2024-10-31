using System.Collections.Generic;

namespace VolumeOfMaterials.Models
{
    public class ExportObject
    {
        public string Description { get; set; } = "";
        public List<string> Name { get; set; } = new List<string>();
        public double Volume { get; set; } = 0;
        public double Length { get; set; } = 0;
        public double Height { get; set; } = 0;
        public double Width { get; set; } = 0;
        public double Area { get; set; } = 0;
        public double Count { get; set; } = 0;
        public string Code { get; set; } = string.Empty;
        public double Perimeter { get; set; } = 0;
        public double Thickness { get; set; } = 0;
        public ExportObject(string code)
        {
            Code = code;
        }
    }
}
