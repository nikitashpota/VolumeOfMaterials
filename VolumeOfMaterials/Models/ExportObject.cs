﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeOfMaterials.Models
{
    public class ExportObject
    {
        public string Name { get; set; } = "";
        public double Volume { get; set; } = 0;
        public double Length { get; set; } = 0;
        public double Area { get; set; } = 0;
        public double Count { get; set; } = 1;

        public ExportObject(string name) { 
            Name = name;   
        }
    }
}