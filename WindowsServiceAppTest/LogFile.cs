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

            using (StreamWriter w = File.AppendText(FilePath))
            {
                Log("Test1", w);
                Log("Test2", w);
            }

            using (StreamReader r = File.OpenText(FilePath))
            {
                DumpLog(r);
            }


            //    DirectorySecurity sec = Directory.GetAccessControl(FilePath);
            //    foreach (FileSystemAccessRule acr in sec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            //    {
            //        Console.WriteLine("{0} | {1} | {2} | {3} | {4}", acr.IdentityReference.Value, acr.FileSystemRights, acr.InheritanceFlags, acr.PropagationFlags, acr.AccessControlType);
            //    }
            //    string createText = "Log van " + Name + " op datum " + Date + Environment.NewLine;
            //    File.AppendAllText(FilePath, createText);
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
        }

        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }

        public void AddTextToLogFile(string text)
        {
            File.AppendAllText(FilePath, text);
        }
    }
}
