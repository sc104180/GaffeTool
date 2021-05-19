using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GaffeTool.Models
{
    public class Response
    {
        public Dictionary<string, string[]> value { get; set; }
        public bool isSuccess { get; set; }
    }
}
