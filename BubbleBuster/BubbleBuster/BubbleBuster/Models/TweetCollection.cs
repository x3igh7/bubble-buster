using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Tweetinvi.Models;

namespace BubbleBuster.Models
{
    [DynamoDBTable("bubble-buster-tweet-stream")]
    public class TweetCollection
    {
        public TweetCollection()
        {
            this.DateCreated = DateTime.Now;
            this.LatestTweets = new List<TweetsByScreenName>();
        }

        [DynamoDBHashKey]
        public DateTime DateCreated { get; set; }

        public List<TweetsByScreenName> LatestTweets { get; set; }

        public class TweetsByScreenName
        {
            public string ScreenName { get; set; }
            public string Tweets { get; set; }
        }
    }
}