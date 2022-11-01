using System.ServiceModel.Syndication;

namespace ArtemisBlog.Feed
{
    enum PublishedPeriod
    {
        Today,
        Yesterday,
        Friday,
        Thursday,
        Wednesday,
        Tuesday,
        Monday,
        LastWeek,
        TwoWeeksAgo,
        ThreeWeeksAgo,
        EarlierThisMonth,
        LastMonth,
        Older
    }

    static class SyndicationItemExtensions
    {
        private static DateTime LastDay(DateTime start, DayOfWeek dayOfWeek)
        {
            int current = (int)start.DayOfWeek;
            int desired = (int)dayOfWeek;
            int offset;

            // We need a negative offset between 1 and 7...
            offset = desired - current;

            if (current <= desired)
                offset -= 7;

            return start.AddDays(offset);
        }

        private static DateTime LastMonday()
        {
            DateTime today = DateTime.Today;

            if (today.DayOfWeek == DayOfWeek.Monday)
            {
                return today;
            }
            else
            {
                return LastDay(today, DayOfWeek.Monday);
            }
        }

        private static DateTime BeginningOfThisMonth()
        {
            DateTime today = DateTime.Today;

            return new DateTime(today.Year, today.Month, 1);
        }
        private static DateTime BeginningOfLastMonth()
        {
            return BeginningOfThisMonth().AddMonths(-1);
        }

        internal static PublishedPeriod PublishedPeriod(this SyndicationItem syndicationItem)
        {
            DateTime published = syndicationItem.PublishDate.ToLocalTime().Date;

            if (published >= DateTime.Today)
            {
                return Feed.PublishedPeriod.Today;
            }
            else if (published >= DateTime.Today.AddDays(-1))
            {
                return Feed.PublishedPeriod.Yesterday;
            }

            DateTime lastMonday = LastMonday();

            if (published >= lastMonday)
            {
                // Relies on the order of the elements of the PublishedPeriod enum
                return (PublishedPeriod)(7 - (int)published.DayOfWeek);
            }
            else if (published >= lastMonday.AddDays(-7))
            {
                return Feed.PublishedPeriod.LastWeek;
            }
            else if (published >= lastMonday.AddDays(-14))
            {
                return Feed.PublishedPeriod.TwoWeeksAgo;
            }
            else if (published >= lastMonday.AddDays(-21))
            {
                return Feed.PublishedPeriod.ThreeWeeksAgo;
            }
            else if (published >= lastMonday.AddDays(-28))
            {
                /* 
                 * More than 21 days ago, but less than 28 days ago may be in the
                 * current month, but will more usually be in the previous month
                 */
                if (published >= BeginningOfThisMonth())
                {
                    return Feed.PublishedPeriod.EarlierThisMonth;
                }

                return Feed.PublishedPeriod.LastMonth;
            }
            else if (published >= BeginningOfLastMonth())
            {
                return Feed.PublishedPeriod.LastMonth;
            }

            return Feed.PublishedPeriod.Older;
        }
    }
}
