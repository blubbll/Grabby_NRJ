using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace grabby_NRJ
{
    public class LinkExtractor

    {
        public static List<string> Extract(string html)
        {
            List<string> list = new List<string>(); Regex regex = new Regex("(?:href|src)=[\"|']?(.*?)[\"|'|>]+", RegexOptions.Singleline | RegexOptions.CultureInvariant); if (regex.IsMatch(html)) { foreach (Match match in regex.Matches(html)) { list.Add(match.Groups[1].Value); } }
            return list;
        }
    }

    public class SongExtractor

    {
        public static List<string> Extract(string html)
        {
            List<string> list = new List<string>(); Regex regex = new Regex("(?:data-track-name)=[\"|']?(.*?)[\"|'|>]+", RegexOptions.Singleline | RegexOptions.CultureInvariant); if (regex.IsMatch(html)) { foreach (Match match in regex.Matches(html)) { list.Add(match.Groups[1].Value); } }
            return list;
        }
    }



    internal class Program
    {
        private static bool isRu(String str)
        {
            return Regex.IsMatch(str, @"^[0-1a-zA-Z]+$");
        }
        private static void Main(string[] args)
        {
            string html = new System.Net.WebClient().DownloadString("http://radioenergymusic.com/index.php?a=explore");

            List<string> list = LinkExtractor.Extract(html);

            var tmpFolder = @"C:\Users\Anwender\Desktop\müll\nrj\put\tmp\";
            if (!Directory.Exists(tmpFolder)) Directory.CreateDirectory(tmpFolder);
            var putFolder = @"C:\Users\Anwender\Desktop\müll\nrj\put\";
            if (!Directory.Exists(putFolder)) Directory.CreateDirectory(putFolder);
            var putFolderNORMAL = @"C:\Users\Anwender\Desktop\müll\nrj\put\NORMAL\";
            if (!Directory.Exists(putFolderNORMAL)) Directory.CreateDirectory(putFolderNORMAL);
            var putFolderRU = @"C:\Users\Anwender\Desktop\müll\nrj\put\RU\";
            if (!Directory.Exists(putFolderRU)) Directory.CreateDirectory(putFolderRU);

            //TMP folder Inhalt deleten;https://stackoverflow.com/a/1288747
            foreach (FileInfo file in new DirectoryInfo(tmpFolder).GetFiles()) file.Delete();

            //Hole aktuellste ID
            int currentID = 0; foreach (var link in list) if (link.Contains("?a=track&id")) { currentID = Convert.ToInt32(link.Split('=').Last()); break; }

            string last = putFolder + "last.txt";
            if (!File.Exists(last)) File.WriteAllText(last, "0");

            //for (int i = currentID; i >= 0; i--)
            for (int i = Convert.ToInt32(File.ReadAllText(last)); i <= currentID; i++)
            {
                Thread.Sleep(5000);
                int _id = i;
                string[] existingFiles = System.IO.Directory.GetFiles(putFolder, (_id + ".*.mp3"), SearchOption.TopDirectoryOnly);
                if (existingFiles.Length == 0)
                {
  
                    Console.Title = "Checking" + " " + _id + " / " + currentID;

                    using (WebClient wc = new WebClient()) {
                        wc.Encoding = System.Text.Encoding.UTF8;

                        try { string songHTML = wc.DownloadString("http://radioenergymusic.com/index.php?a=track&id=" + _id);

                            var results = SongExtractor.Extract(songHTML);
                            if (results.Count != 0)
                            {
                                string song = results[0];
                                song = song.Replace("â€“", "–");

                                var putTmp = @tmpFolder + _id + "." + song;
                                var putOutNORMAL = @putFolderNORMAL + _id + "." + song;
                                var putOutRU = @putFolderRU + _id + "." + song;

                                Thread.Sleep(5000);

                                Console.WriteLine("Downloading" + " " + _id + "...");
                                new WebClient().DownloadFile(@"http://www.radioenergymusic.com/uploads/tracks/" + WebUtility.UrlEncode(song), putTmp);

                                Console.WriteLine("Downloaded" + " " + _id + "...");
                                if(isRu(results[0]))
                                    File.Move(putTmp, putOutRU);
                                else File.Move(putTmp, putOutNORMAL);
                                File.WriteAllText(putFolder + "last.txt", _id.ToString());
                                //Thread.CurrentThread.Abort();
                            }
                        } catch (WebException ex) { } }


                }
                else Console.Title = "Skipping" + " " + _id;
            }

            Console.ReadLine();
        }
    }
}
