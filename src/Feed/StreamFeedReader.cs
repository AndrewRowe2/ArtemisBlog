using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;

namespace ArtemisBlog.Feed
{
    class StreamFeedReader
    {
        private readonly Stream feedData;

        internal StreamFeedReader(Stream feedData)
        {
            this.feedData = feedData;
        }

        internal SyndicationFeed GetFeed()
        {
            if (feedData != null)
            {
                feedData.Position = 0;
                using XmlReader xmlReader = XmlReader.Create(feedData);
                
                return SyndicationFeed.Load(xmlReader);
            }

            return null;
        }
    }
}
