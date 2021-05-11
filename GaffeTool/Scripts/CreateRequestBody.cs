using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Text.Json;
using System.Linq;

namespace ControlPanel
{
    public static class CreateRequestBody
    {
        public static string GetGaffes()
        {
            var obj = new
            {
                backendId = ConfigurationManager.AppSettings.Get("BackendID"),
                serviceId = "GetGaffes"
            };
            return JsonSerializer.Serialize(obj);
        }

        public static string Gaffe(string GaffeName)
        {
            var obj = new
            {
                backendId = ConfigurationManager.AppSettings.Get("BackendID"),
                backendServiceArguments = new { Name = GaffeName },
                serviceId = "Gaffe"
            };
            return JsonSerializer.Serialize(obj);
        }

        public static string ClearRandomNumberQueue()
        {
            var obj = new
            {
                backendId = ConfigurationManager.AppSettings.Get("BackendID"),
                serviceId = "ClearRandomNumberQueue"
            };
            return JsonSerializer.Serialize(obj);
        }

        public static string CustomRandomNumbers(List<object> numberList)
        {
            var obj = new
            {
                backendId = ConfigurationManager.AppSettings.Get("BackendID"),
                backendServiceArguments = new { RandomNumberQueue = numberList },
                serviceId = "CustomRandomNumbers"
            };
            return JsonSerializer.Serialize(obj);
        }
    }
}
