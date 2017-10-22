using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace EpgData
{
    public class EpgData
    {
        const string channelsurl = "http://www.epgdata.com/index.php?action=sendInclude&iOEM=vdr&pin={PIN}&dataType=xml";
        const string programsurl = "http://www.epgdata.com/index.php?action=sendPackage&iOEM=vdr&pin={PIN}&dayOffset={OFFSET}&dataType=xml";
       
        private GuideChannel[] _channels;
        private GuideProgram[] _programs;
        private Dictionary<int, string> _genres = new Dictionary<int, string>();
        private Dictionary<int, string> _categories = new Dictionary<int, string>();

        internal GuideChannel[] Channels
        {
            get
            {
                return _channels;
            }

            set
            {
                _channels = value;
            }
        }

        internal GuideProgram[] Programs
        {
            get
            {
                return _programs;
            }

            set
            {
                _programs = value;
            }
        }

        public delegate void MessageHandler(string message);
        public event MessageHandler Message;
        protected void OnMessage(string message)
        {
            if (Message != null)
            {
                Message?.Invoke(message);
            }
        }
        /// <summary>
        /// Downloads EpgData.com Include Package (contains Channel, Genre and Category Xml Files )
        /// </summary>
        /// <returns>a GuideChannel Array</returns>
        public GuideChannel[] LoadEpgDataInclude(string pin)
        {
            OnMessage("Downloading EpgData.com 's Include Package");     
            List<GuideChannel> retval = new List<GuideChannel>();
            string tempFileName = Path.GetTempFileName();
            Download(channelsurl.Replace("{PIN}", pin), tempFileName);
            try
            {
                ZipFile.ExtractToDirectory(tempFileName, tempFileName + "zip");
            }
            catch(Exception)
            {
                
            }
            File.Delete(tempFileName);
            if (File.Exists(tempFileName + @"zip\channel_y.xml"))
            {
                IEnumerator enumerator = null;
                XmlDocument document = new XmlDocument();
                document.Load(tempFileName + @"zip\channel_y.xml");
                OnMessage("Reading Channels");
                try
                {
                    enumerator = document.SelectSingleNode("channel").ChildNodes.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        XmlNode chaNode = (XmlNode)enumerator.Current;
                        GuideChannel channel = new GuideChannel
                        {
                            ChannelId = chaNode["ch4"].InnerText,
                            DisplayName = chaNode["ch0"].InnerText,
                            Language = chaNode["ch3"].InnerText,
                            CallSign = chaNode["ch11"].InnerText
                        };
                        retval.Add(channel);                        
                    }
                }
                finally
                {
                    if (enumerator is IDisposable)
                    {
                        (enumerator as IDisposable).Dispose();
                    }
                }
            }
            if (File.Exists(tempFileName + @"zip\genre.xml"))
            {
                IEnumerator enumerator2 = null;
                XmlDocument document2 = new XmlDocument();
                document2.Load(tempFileName + @"zip\genre.xml");
                OnMessage("Reading Genres");
                try
                {
                    enumerator2 = document2.SelectSingleNode("genre").ChildNodes.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        XmlNode genNode = (XmlNode)enumerator2.Current;
                        _genres.Add(int.Parse(genNode["g0"].InnerText), genNode["g1"].InnerText);
                    }
                }
                finally
                {
                    if (enumerator2 is IDisposable)
                    {
                        (enumerator2 as IDisposable).Dispose();
                    }
                }
            }
            if (File.Exists(tempFileName + @"zip\category.xml"))
            {
                IEnumerator enumerator3 = null;
                XmlDocument document3 = new XmlDocument();
                document3.Load(tempFileName + @"zip\category.xml");
                OnMessage("Reading Categories");
                try
                {
                    enumerator3 = document3.SelectSingleNode("category").ChildNodes.GetEnumerator();
                    while (enumerator3.MoveNext())
                    {
                        XmlNode catNode = (XmlNode)enumerator3.Current;
                        _categories.Add(int.Parse(catNode["ca0"].InnerText), catNode["ca1"].InnerText);
                    }
                }
                finally
                {
                    if (enumerator3 is IDisposable)
                    {
                        (enumerator3 as IDisposable).Dispose();
                    }
                }
            }
            if (Directory.Exists(tempFileName + "zip"))
            {
                Directory.Delete(tempFileName + "zip", true);
            }
            OnMessage("Download of EpgData.com 's Include Package is Complete and all Files Readed");
            _channels= retval.ToArray();
            return retval.ToArray();
        }
        /// <summary>
        /// Downloads EpgData.com EpgData Package 
        /// </summary>
        /// <returns></returns>
        public GuideProgram[] LoadEpgDataPrograms(string pin ,int days)
        {
            List<GuideProgram> retval = new List<GuideProgram>();
            var offset = 0;
            var index = 0;
            while (index < days)
            {
                OnMessage(string.Format("Download EpgData.com 's Data Package for Day {0} ", index));
                var dataoffset = index + offset;
                string tempFileName = Path.GetTempFileName();
                Download(programsurl.Replace("{PIN}", pin).Replace("{OFFSET}", dataoffset.ToString()), tempFileName);
                try
                {
                    ZipFile.ExtractToDirectory(tempFileName, tempFileName + "zip");
                }
                catch
                {
                }
                File.Delete(tempFileName);
                if (Directory.Exists(tempFileName + "zip"))
                {
                    foreach (var str2 in Directory.GetFiles(tempFileName + "zip", "*.xml"))
                    {
                        IEnumerator enumerator = null;
                        XmlDocument document = new XmlDocument();
                        document.Load(str2);
                        try
                        {
                            enumerator = document.SelectSingleNode("pack").ChildNodes.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                XmlNode current = (XmlNode)enumerator.Current;
                                GuideProgram program = new GuideProgram
                                {
                                    BroadcastId = current["d0"].InnerText,
                                    GuideProgramId = current["d1"].InnerText,
                                    ChannelId = current["d2"].InnerText,
                                    StartTime = DateTime.Parse(current["d4"].InnerText),
                                    StopTime = DateTime.Parse(current["d5"].InnerText),
                                    Duration = current["d7"].InnerText,
                                    Category = GetCategoryByCategoryId(int.Parse(current["d10"].InnerText)),
                                    Rating = current["d16"].InnerText,
                                    Title = current["d19"].InnerText,
                                    EpisodeTitle = current["d20"].InnerText,
                                    Description = current["d21"].InnerText,
                                    Genre = GetGenreByGenreId(int.Parse(current["d25"].InnerText)),
                                    EpisodeNumber = current["d26"].InnerText,
                                    StarRating = current["d30"].InnerText,
                                    Country = current["d32"].InnerText,
                                    Date = current["d33"].InnerText,
                                    Presenter = current["d34"].InnerText,
                                    Guest = current["d35"].InnerText,
                                    Director = current["d36"].InnerText,
                                    Actors = ProcessActors(current["d37"].InnerText)                               
                                };
                                retval.Add(program);
                            }
                        }
                        finally
                        {
                            if (enumerator is IDisposable)
                            {
                                (enumerator as IDisposable).Dispose();
                            }
                        }
                    }
                }
                index++;
            }
            OnMessage(string.Format("Download Epg Data for Days {0} Complete", days));
            _programs = retval.ToArray();
            return retval.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private Actor[] ProcessActors(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                List<Actor> retval = new List<Actor>();
                string[] actorsarray = Regex.Split(input, "\\)\\s-");
                foreach (string actorarray in actorsarray)
                {
                    var something = actorarray.Split('(');
                    Actor actor = new Actor();

                    actor.Name = something[0].ToString();


                    retval.Add(actor);
                }
                return retval.ToArray();
            }
            return (null);
        }
        internal string GetLanguageByChannelId(string channelId)
        {
            var retval = string.Empty;
            foreach (var channel in Channels)
            {
                if (channel.ChannelId == channelId)
                {
                    retval = channel.Language;
                }
            }
            return retval;
        }
        internal string GetGenreByGenreId(int genreId)
        {
            var retval = string.Empty;
            _genres.TryGetValue(genreId, out retval);
            return retval;
        }
        internal string GetCategoryByCategoryId(int categoryId)
        {
            var retval = string.Empty;
            _categories.TryGetValue(categoryId, out retval);
            return retval;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        static void Download(string url, string file)
        {
            new WebClient().DownloadFile(url, file);
        }
    }
}
