using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;


namespace GenerativeImages
{
    internal class Program
    {
        public List<List<string>> imagesPaths;
        public Random random;
        public string CurrentDir;

        public Program()
        {
            imagesPaths = new List<List<string>>();
            random = new Random();
            CurrentDir = Directory.GetCurrentDirectory();
        }

        static async Task Main()
        {
            var GenIm = new Program();

            GenIm.GenerateImagesPaths(10000);
            GenIm.GenerateImages();

        }

        private void GenerateImagesPaths(int amount)
        {
            Console.Write("Generating Paths: ");

            int imagesCount = 0;
            int maxImages = 1;
            foreach (var d in Directory.EnumerateDirectories(CurrentDir + "\\layers"))
            {
                foreach (var f in Directory.EnumerateFiles(d))
                {
                    imagesCount++;
                }
                maxImages *= d.Length;
            }

            if (amount > maxImages)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed");
                Console.ResetColor();

                throw new Exception($"It is unable to generate this much images. Max amount is {maxImages}");
            }

            do
            {
                List<string> imagePath = new List<string>();

                foreach (var d in Directory.EnumerateDirectories(Directory.GetCurrentDirectory() + "\\layers"))
                {
                    var files = Directory.EnumerateFiles(d);
                    var list = files.ToList();
                    var index = random.Next(0, list.Count);

                    imagePath.Add(list[index]);
                }

                if (!imagesPaths.Contains(imagePath))
                {
                    imagesPaths.Add(imagePath);
                }

            } while (amount > imagesPaths.Count);


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done");
            Console.ResetColor();

        }

        private void GenerateImages()
        {
            Console.Write("Generating images: ");

            int imageName = 0;
            foreach (var l in imagesPaths)
            {
                Bitmap bmp = new Bitmap(l[0]);

                foreach (var s in l)
                {
                    var part = new Bitmap(s);

                    for (int x = 0; x < part.Width; x++)
                    {
                        for (int y = 0; y < part.Height; y++)
                        {
                            Color color = part.GetPixel(x, y);
                            if (color.A != 0)
                            {
                                bmp.SetPixel(x, y, color);
                            }
                        }
                    }

                }

                imageName++;
                bmp.Save($"{CurrentDir}\\Output\\{imageName}.png");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done");
            Console.ResetColor();
        }
    }
}
