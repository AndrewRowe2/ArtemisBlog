using ArtemisBlog.Resources;

namespace ArtemisBlog.Options
{
    class RefreshInterval
    {
        private readonly string description;

        public RefreshInterval(int minutes, string description)
        {
            this.description = description;
            Minutes = minutes;
        }

        public override string ToString()
        {
            return description;
        }

        internal int Minutes { get; }

        internal static RefreshInterval FiveMinutes { get; } = new RefreshInterval(5, Legends.FiveMinutes);
        internal static RefreshInterval ThirtyMinutes { get; } = new RefreshInterval(30, Legends.ThirtyMinutes);
        internal static RefreshInterval SixtyMinutes { get; } = new RefreshInterval(60, Legends.SixtyMinutes);
        internal static RefreshInterval TwoHours { get; } = new RefreshInterval(120, Legends.TwoHours);
        internal static RefreshInterval FourHours { get; } = new RefreshInterval(240, Legends.FourHours);
    }
}
