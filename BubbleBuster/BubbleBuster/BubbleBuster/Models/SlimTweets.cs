using System;

namespace BubbleBuster.Models
{
    public class SlimTweets
    {
        public DateTime CreatedDate { get; set; }
        public string CreatedByScreenName { get; set; }
        public long Id { get; set; }
        public string Text { get; set; }
    }
}