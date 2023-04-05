namespace VolumeOfMaterials.Models
{
    public class ExportObject
    {
        public string Name { get; set; } = "";
        public double Volume { get; set; } = 0;
        public double Length { get; set; } = 0;
        public double Area { get; set; } = 0;
        public double Count { get; set; } = 0;
        public string Code { get; set; } = string.Empty;

        public ExportObject(string name)
        {
            Name = name;
        }
    }
}
