using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    public class Logger{
        private readonly string _filePath;

        public Logger(string fileName){
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string logPath = Path.Combine(exePath, "logs");
            string name = "algorithm_" + fileName + "_log.txt";

            // Создаём подпапку logs, если её нет
            Directory.CreateDirectory(logPath);

            _filePath = Path.Combine(logPath, name);

            if (File.Exists(_filePath)) File.WriteAllText(_filePath, string.Empty); 
            else File.Create(_filePath);
        }

        public void Log(string message) =>
            File.AppendAllText(_filePath, message + Environment.NewLine);
    }
}
