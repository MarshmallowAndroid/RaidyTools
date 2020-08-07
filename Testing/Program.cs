using RaidyFileUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            string file;

            Console.Write("Path to file: ");
            file = Console.ReadLine().Trim('\"');

            if (file != string.Empty)
            {
                RaidyReader reader = new RaidyReader(new FileStream(file, FileMode.Open, FileAccess.Read));

                string outputDirectory = file.Substring(file.LastIndexOf('\\') + 1);
                Directory.CreateDirectory(outputDirectory);

                foreach (var item in reader.FileEntries)
                {
                    using (var fileStream = new FileStream(outputDirectory + "\\" + item.fileName, FileMode.Create, FileAccess.Write))
                    {
                        using (var memoryStream = reader.GetFile(item))
                        {
                            memoryStream.CopyTo(fileStream);
                        }
                    }

                    Console.WriteLine("Extracted " + item.fileName);
                }
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
