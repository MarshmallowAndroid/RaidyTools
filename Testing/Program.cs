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
        static void ExtractGgp(string file)
        {
            using (var reader = new RaidyGgpReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                reader.ExtractImage(file + ".PNG");

                Console.WriteLine("Extracted " + file + ".PNG");
            }
        }

        static void ExtractAssetArchive(string file)
        {
            using (var reader = new RaidyReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                string outputDirectory = file.Substring(file.LastIndexOf('\\') + 1);
                Directory.CreateDirectory(outputDirectory);

                foreach (var item in reader.FileEntries)
                {
                    string sanitizedName = new string(item.fileName).Trim('\0');

                    using (var fileStream = new FileStream(outputDirectory + "\\" + sanitizedName, FileMode.Create, FileAccess.Write))
                    {
                        using (var memoryStream = reader.GetFile(item))
                        {
                            memoryStream.CopyTo(fileStream);
                        }
                    }

                    Console.WriteLine("Extracted " + sanitizedName);
                }
            }
        }

        static void Main(string[] args)
        {
            string path;

            do
            {
                Console.Write("Path to file: ");
                path = Console.ReadLine().Trim('\"');

                if (File.Exists(path))
                {
                    if (path.EndsWith(".GGP"))
                        ExtractGgp(path);
                    else
                        ExtractAssetArchive(path);
                }
                else if (Directory.Exists(path))
                {
                    foreach (var file in Directory.GetFiles(path))
                    {
                        if (file.EndsWith(".GGP"))
                            ExtractGgp(file);
                    }
                }

                Console.WriteLine("Done.");
            } while (path != string.Empty);
        }
    }
}
