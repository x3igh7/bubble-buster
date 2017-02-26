using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using BubbleBuster.Models;
using Tweetinvi;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace BubbleBuster
{
    public class Function
    {
        /// <summary>
        ///     A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="context">Lambda context.</param>
        /// <returns></returns>
        public async void FunctionHandler(ILambdaContext context)
        {
            var consumerKey = Environment.GetEnvironmentVariable("ConsumerKey");
            var consumerSecret = Environment.GetEnvironmentVariable("ConsumerSecret");
            var accessToken = Environment.GetEnvironmentVariable("AccessToken");
            var accessTokenSecret = Environment.GetEnvironmentVariable("AccessTokenSecret");

            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            var timelines = new TweetCollection();
            var followed = new List<string> {"FoxNews", "CNN", "MSNBC"};
            foreach (var screename in followed)
            {
                var recentTweets = Timeline.GetUserTimeline(screename, 15);

                var slimTweets = recentTweets.Select(
                    tweet => new SlimTweets
                    {
                        Id = tweet.Id,
                        Text = tweet.Text,
                        CreatedDate = tweet.CreatedAt,
                        CreatedByScreenName = tweet.CreatedBy.ScreenName
                    }).ToList();

                timelines.LatestTweets.Add(
                    new TweetCollection.TweetsByScreenName {ScreenName = screename, Tweets = slimTweets});
            }

            var client = new AmazonDynamoDBClient();
            var dynamoDb = new DynamoDBContext(client);

            await dynamoDb.SaveAsync(timelines);
        }
    }
}