using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    class LogItem
    {
        public int iterationNumber;
        public List<float> iterationTime = new List<float>();
        public List<float> cumulativeIterationTime = new List<float>();
        public List<float> reward = new List<float>();
        public List<int> checkpointCount = new List<int>();
        public List<float> velocity = new List<float>();
        public bool timedOut = false;
    }


    static class LoggingController
    {
        const string logDir = "./logs";

        public static void LogItem(LogItem logItem, string fileName)
        {
            string filePath = $"{logDir}/{fileName}.json";
            List<LogItem> logItems = new List<LogItem>();

            if (File.Exists(filePath))
            {
                string fileContents = File.ReadAllText(filePath);
                logItems = JsonConvert.DeserializeObject<List<LogItem>>(fileContents);
            }
             
            logItem.iterationNumber = logItems.Count;
            logItems.Add(logItem);

            string json = JsonConvert.SerializeObject(logItems);
            File.WriteAllText(filePath, json);

            Debug.WriteLine($"Wrote episode log to '{Path.GetFullPath(filePath)}'.");
        }
    }
}
