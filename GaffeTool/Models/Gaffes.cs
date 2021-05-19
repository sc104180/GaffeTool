using System;
using System.Collections.Generic;
using System.Text;

namespace GaffeTool.Models
{
    public class Gaffes
    {
        public List<Program> programs { get; set; }
    }

    public class Program
    {
        public string name { get; set; }
        public List<object> values { get; set; }
    }
}
