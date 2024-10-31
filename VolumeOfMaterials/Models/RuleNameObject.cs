using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeOfMaterials.Models
{
    public class RuleNameObject
    {
        public string Tag { get; set; }
        public List<string> Parameters { get; set; }
        public List<string> Prefixes { get; set; }
        public List<string> Suffixes { get; set; }
        public List<bool> Divides { get; set; } 
        public RuleNameObject()
        {

        }
    }
}
