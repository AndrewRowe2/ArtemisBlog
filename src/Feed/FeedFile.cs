using System.IO;
using System.ServiceModel.Syndication;

namespace ArtemisBlog.Feed
{
    class FeedFile
    {
        private readonly string fileName;
        private SyndicationFeed feed;

        internal FeedFile(string fileName)
        {
            this.fileName = fileName;
        }

        internal void Save(Stream feedStream)
        {
            if (feedStream != null)
            {
                FileInfo fileInfo = new(fileName);

                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                feedStream.Position = 0;
                using FileStream fileStream = fileInfo.OpenWrite();
                feedStream.CopyTo(fileStream);
                feed = null;
            }
        }

        internal SyndicationFeed GetFeed()
        {
            if (feed == null)
            {
                FileInfo fileInfo = new(fileName);

                if (fileInfo.Exists)
                {
                    using FileStream fileStream = fileInfo.OpenRead();
                    feed = new StreamFeedReader(fileStream).GetFeed();
                }
            }

            return feed;
        }

        internal DateTime GetLastModified()
        {
            GetFeed();

            if (feed == null)
            {
                return DateTime.MinValue;
            }
            else
            {
                return feed.LastUpdatedTime.ToLocalTime().DateTime;
            }
        }
    }
}
