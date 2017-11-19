using System.IO;
using System.Xml.Linq;

namespace EpgData
{
    public class XmlTv
    {
        const string DateTimeFormat = "yyyyMMddHHmmss zz00";

        public  void CreateXMLTV(EpgData epgData, Stream output)
        {
            var xml = new XDocument();

            var tv = new XElement("tv");
            tv.Add(new XAttribute("source-info-name", "EPGDATA.COM"));
            tv.Add(new XAttribute("generator-info-name", "EPGDATA"));
            tv.Add(new XAttribute("generator-info-url", "EPGDATA.COM"));
            xml.Add(tv);
            foreach (var chan in epgData.Channels)
            {
                var c = new XElement("channel");
                c.Add(new XAttribute("id", chan.ChannelId));               
                if(string.IsNullOrEmpty(chan.CallSign))
                {
                    var dn = new XElement("display-name");
                    c.Add(dn);
                    dn.Value = chan.DisplayName;
                }
                if (chan.CallSign.Contains("|"))
                {
                    var csa = chan.CallSign.Split('|');
                    foreach (var csv in csa)
                    {
                        var cs = new XElement("display-name");
                        c.Add(cs);
                        cs.Value = csv;
                    }                    
                }                
                tv.Add(c);
            }
            foreach (var program in epgData.Programs)
            {
                var programElement = new XElement("programme");
                programElement.Add(new XAttribute("start", program.StartTime.ToString(DateTimeFormat)));
                programElement.Add(new XAttribute("stop", program.StopTime.ToString(DateTimeFormat)));
                programElement.Add(new XAttribute("channel", program.ChannelId));
                var titleElement = new XElement("title") { Value = program.Title };
                titleElement.Add(new XAttribute("lang", epgData.GetLanguageByChannelId(program.ChannelId)));
                programElement.Add(titleElement);
                if (!string.IsNullOrEmpty(program.EpisodeTitle))
                {
                    var subtitleElement = new XElement("sub-title") { Value = program.EpisodeTitle };
                    subtitleElement.Add(new XAttribute("lang", epgData.GetLanguageByChannelId(program.ChannelId)));
                    programElement.Add(subtitleElement);
                }
                if (!string.IsNullOrEmpty(program.Description))
                {
                    var descElement = new XElement("desc") { Value = program.Description };
                    descElement.Add(new XAttribute("lang", epgData.GetLanguageByChannelId(program.ChannelId)));
                    programElement.Add(descElement);
                }

                if ((!string.IsNullOrEmpty(program.Presenter)) || (!string.IsNullOrEmpty(program.Director)))
                {
                    var creditsElement = new XElement("credits");
                    if (!string.IsNullOrEmpty(program.Presenter))
                    {
                        creditsElement.Add(new XElement("presenter") { Value = program.Presenter });
                    }
                    if (!string.IsNullOrEmpty(program.Director))
                    {
                        creditsElement.Add(new XElement("director") { Value = program.Director });
                    }
                    if (!string.IsNullOrEmpty(program.Guest))
                    {
                        creditsElement.Add(new XElement("guest") { Value = program.Guest });
                    }
                    if (program.Actors!= null)
                    {
                        foreach (var act in program.Actors)
                        {
                            creditsElement.Add(new XElement("actor") { Value = act.Name.Trim() });
                        }
                    }
                    programElement.Add(creditsElement);
                }
                if (!string.IsNullOrEmpty(program.Date))
                {
                    var dateElement = new XElement("date") { Value = program.Date };
                    programElement.Add(dateElement);
                }
                if (!string.IsNullOrEmpty(program.Country))
                {
                    var countryElement = new XElement("country") { Value = program.Country };
                    programElement.Add(countryElement);
                }
                if (!string.IsNullOrEmpty(program.Category))
                {
                    var categoryElement = new XElement("category") { Value = program.Category };
                    categoryElement.Add(new XAttribute("lang", epgData.GetLanguageByChannelId(program.ChannelId)));
                    programElement.Add(categoryElement);
                }
                if (!string.IsNullOrEmpty(program.Duration))
                {
                    var lengthElement = new XElement("length") { Value = program.Duration };
                    lengthElement.Add(new XAttribute("lang", "minutes"));
                    programElement.Add(lengthElement);
                }
                if (!string.IsNullOrEmpty(program.EpisodeNumber))
                {
                    var episodeNumberElement = new XElement("episode-num") { Value = program.EpisodeNumber };
                    episodeNumberElement.Add(new XAttribute("system", ""));
                    programElement.Add(episodeNumberElement);
                }
                if (!string.IsNullOrEmpty(program.Rating))
                {
                    var ratingElement = new XElement("rating");
                    ratingElement.Add(new XAttribute("system", ""));
                    ratingElement.Add(new XElement("Value") { Value = program.Rating });
                    programElement.Add(ratingElement);
                }
                if (!string.IsNullOrEmpty(program.StarRating))
                {
                    var starRatingElement = new XElement("star-rating");
                    starRatingElement.Add(new XElement("Value") { Value = program.StarRating });
                    programElement.Add(starRatingElement);
                }

                tv.Add(programElement);
            }
            xml.Save(output);
        }
    }
}
