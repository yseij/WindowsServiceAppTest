using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;

namespace WindowsFormsAppTest
{
    class LogFile
    {
        private int eventId = 1;
        public LogFile()
        {
            Date = DateTime.Today.ToString("d").Replace("/", "");
            Time = DateTime.Now.ToLongTimeString().Replace(":", "");
        }
        public string Date { get; set; }
        public string Time { get; set; }
        public string FilePath { get; set; }

        public void MakeLogFile(string Name)
        {
            FilePath = "d:\\LogFiles\\" + Name + "_op_datum_" + Date + Time + ".txt";
            File.WriteAllText(FilePath, "test");
            DirectorySecurity sec = Directory.GetAccessControl(FilePath);
            foreach (FileSystemAccessRule acr in sec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            {
                Console.WriteLine("{0} | {1} | {2} | {3} | {4}", acr.IdentityReference.Value, acr.FileSystemRights, acr.InheritanceFlags, acr.PropagationFlags, acr.AccessControlType);
            }
        }

        public void AddTitleToLogFile(string title)
        {
            using (StreamWriter w = File.AppendText(FilePath))
            {
                w.WriteLine($"  :{title}:");
            }

            using (StreamReader r = File.OpenText(FilePath))
            {
                DumpLog(r);
            }
        }

        public void AddTextToLogFile(string text)
        {
            using (StreamWriter w = File.AppendText(FilePath))
            {
                w.WriteLine($"  :{text}");
                w.WriteLine("-------------------------------");
            }

            using (StreamReader r = File.OpenText(FilePath))
            {
                DumpLog(r);
            }
        }

        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
