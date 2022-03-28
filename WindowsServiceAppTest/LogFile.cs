using System;
using System.Configuration;
using System.IO;
using System.Security.AccessControl;

namespace WindowsFormsAppTest
{
    class LogFile
    {
        public LogFile()
        {
            Date = DateTime.Today.ToString("d").Replace("-", "");
            Time = DateTime.Now.ToLongTimeString().Replace(":", "");
        }
        public string Date { get; set; }
        public string Time { get; set; }
        public string FilePath { get; set; }

        public void MakeLogFile(string Name)
        {
            FilePath = "d:\\LogFiles\\" + Name + ".txt";
            File.Create(FilePath);
            //DirectorySecurity sec = Directory.GetAccessControl(FilePath);
            //foreach (FileSystemAccessRule acr in sec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            //{
            //    Console.WriteLine("{0} | {1} | {2} | {3} | {4}", acr.IdentityReference.Value, acr.FileSystemRights, acr.InheritanceFlags, acr.PropagationFlags, acr.AccessControlType);
            //}
            string createText = "Log van " + Name + " op datum " + Date + Environment.NewLine;
            File.AppendAllText(FilePath, createText);
        }

        public void AddTextToLogFile(string text)
        {
            File.AppendAllText(FilePath, text);
        }
    }
}
