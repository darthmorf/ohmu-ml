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
        public int episodeNumber;
        public float iterationTime;
        public float reward;
        public int checkpointCount;
        public float averageVelocity;
        public bool timedOut = false;
    }


    static class LoggingController
    {
        const string logDir = "./training-records";

        public static void LogItem(LogItem logItem, string fileName)
        {
            string filePath = $"{logDir}/{fileName}.json";
            List<LogItem> logItems = new List<LogItem>();

            if (File.Exists(filePath))
            {
                string fileContents = File.ReadAllText(filePath);
                logItems = JsonConvert.DeserializeObject<List<LogItem>>(fileContents);
            }
             
            logItem.episodeNumber = logItems.Count;
            logItems.Add(logItem);

            string json = JsonConvert.SerializeObject(logItems);
            File.WriteAllText(filePath, json);

            Debug.WriteLine($"Wrote episode log to '{Path.GetFullPath(filePath)}'.");
        }
    }
}
