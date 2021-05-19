using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.Json;
using GaffeTool.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace GaffeTool
{
    public static class Utility
    {
        static string frontendExe;
        static string backendExe;
        public static bool isBuildAccess = false;

        static Utility()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(ConfigurationManager.AppSettings.Get("FrontEndDirPath"));
                frontendExe = dir.GetFiles("*.exe").Where(e => e.Name != "UnityCrashHandler64.exe").First().FullName;
                dir = new DirectoryInfo(ConfigurationManager.AppSettings.Get("BackEndDirPath"));
                backendExe = dir.GetFiles("*.exe").First().FullName;
                isBuildAccess = true;
            }
            catch (Exception)
            {
                isBuildAccess = false;
            }
        }

        public static async Task<List<string>> GetGaffeNameList()
        {
            try
            {
                var gaffesJson = File.ReadAllText(ConfigurationManager.AppSettings.Get("GaffesPath"));
                var gaffesJsonNameList = JsonSerializer.Deserialize<Gaffes>(gaffesJson).programs.Select(e => e.name).ToList();
                if (gaffesJsonNameList.Count == 0) throw new Exception("no gaffe found!");
                var gaffeNameList = new List<string>() { "--Select a Gaffe--" };
                gaffeNameList.AddRange(gaffesJsonNameList);
                var includedGaffes = File.ReadAllText(".gaffeinclude").Split("\n").Select(e => e.Trim()).ToList();
                gaffeNameList.AddRange(includedGaffes);
                var ignoredGaffes = File.ReadAllText(".gaffeignore").Split("\n").Select(e => e.Trim()).ToList();
                gaffeNameList.RemoveAll(e => ignoredGaffes.Contains(e));
                return gaffeNameList;
            }
            catch (Exception ex)
            {
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        StringContent content = new StringContent(CreateRequestBody.GetGaffes(), Encoding.UTF8, "application/json");
                        var responseData = await httpClient.PostAsync(ConfigurationManager.AppSettings.Get("Url"), content);
                        if (responseData.IsSuccessStatusCode)
                        {
                            string responseString = await responseData.Content.ReadAsStringAsync();
                            var response = JsonSerializer.Deserialize<Response>(responseString);
                            if (response.isSuccess)
                            {
                                var gaffeNameList = new List<string>() { "--Select a Gaffe--" };
                                var responseNameList = response.value["gaffes"].Select(e => e.Replace("\"", "")).ToList();
                                gaffeNameList.AddRange(responseNameList);
                                var includedGaffes = File.ReadAllText(".gaffeinclude").Split("\n").Select(e => e.Trim()).ToList();
                                gaffeNameList.AddRange(includedGaffes);
                                var ignoredGaffes = File.ReadAllText(".gaffeignore").Split("\n").Select(e => e.Trim()).ToList();
                                gaffeNameList.RemoveAll(e => ignoredGaffes.Contains(e));
                                return gaffeNameList;
                            }
                            else
                            {
                                return new List<string>() { "Something went wrong!" };
                            }
                        }
                        else
                        {
                            return new List<string>() { "Internal Server Error!" };
                        }
                    }
                    catch (Exception)
                    {
                        return new List<string>() { "BackEnd not reachable!" };
                    }
                }
            }
        }

        public static string GetGaffeHelpText()
        {
            return File.ReadAllText(".gaffehelp");
        }

        public static int CloseApps()
        {
            int count = 0;
            var procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(frontendExe));
            foreach (Process p in procs) { p.Kill(); count++; }

            procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(backendExe));
            foreach (Process p in procs) { p.Kill(); count++; }

            return count;
        }

        public static int StartApps(Label statusLabel)
        {
            int count = 0;
            try
            {
                var procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(backendExe));
                Process pr = new Process();
                pr.StartInfo.FileName = backendExe;
                pr.StartInfo.WorkingDirectory = Path.GetDirectoryName(backendExe);
                pr.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                pr.Start();
                count++;

                procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(frontendExe));
                pr = new Process();
                pr.StartInfo.FileName = frontendExe;
                pr.StartInfo.WorkingDirectory = Path.GetDirectoryName(frontendExe);
                pr.Start();
                count++;
            }
            catch (Exception)
            {
                if (count >= 1)
                    Utility.DisplayToast(statusLabel, "FrontEnd exe not found!");
                else
                    Utility.DisplayToast(statusLabel, "BackEnd exe not found!");
            }
            return count;
        }

        public static void ClearStorage(Label statusLabel)
        {
            try
            {
                File.WriteAllText(ConfigurationManager.AppSettings.Get("StoragePath"), string.Empty);
                DisplayToast(statusLabel, "Success!");
            }
            catch (Exception)
            {
                DisplayToast(statusLabel, "Storage not found!");
            }
        }

        public static async void PostAsync(string body, Label statusLabel)
        {
            string status = "Failed!";
            using (var httpClient = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
                    var responseData = await httpClient.PostAsync(ConfigurationManager.AppSettings.Get("Url"), content);
                    if (responseData.IsSuccessStatusCode)
                    {
                        string responseString = await responseData.Content.ReadAsStringAsync();
                        var response = JsonSerializer.Deserialize<Response>(responseString);
                        if (response.isSuccess)
                            status = "Success!";
                        else
                            status = "Something went wrong!";
                    }
                    else
                        status = "Internal Server Error!";
                }
                catch (Exception)
                {
                    status = "BackEnd not reachable!";
                }
            }

            DisplayToast(statusLabel, status);
        }

        public static async void DisplayToast(Label statusLabel, string msg)
        {
            statusLabel.Visibility = Visibility.Visible;
            statusLabel.Content = msg;
            if (msg == "Success!")
                statusLabel.Background = Brushes.DarkGreen;
            else
                statusLabel.Background = Brushes.DarkRed;

            await Task.Delay(2000);
            statusLabel.Visibility = Visibility.Hidden;
        }
    }
}
