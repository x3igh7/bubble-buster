using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using BubbleBuster.Models;
using Newtonsoft.Json;
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
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(ILambdaContext context)
        {
            var consumerKey = Environment.GetEnvironmentVariable("ConsumerKey");
            var consumerSecret = Environment.GetEnvironmentVariable("ConsumerSecret");
            var accessToken = Environment.GetEnvironmentVariable("AccessToken");
            var accessTokenSecret = Environment.GetEnvironmentVariable("AccessTokenSecret");

            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            //var currentUser = User.GetAuthenticatedUser();
            //var users = currentUser.GetUsersYouRequestedToFollow();
            //return JsonConvert.SerializeObject(currentUser);

            var timelines = new TweetCollection();
            var followed = new List<string> {"FoxNews", "CNN", "MSNBC"};
            foreach (var screename in followed)
            {
                var recentTweets = Timeline.GetUserTimeline(screename, 5);
                timelines.LatestTweets.Add(new TweetCollection.TweetsByScreenName {ScreenName = screename, Tweets = JsonConvert.SerializeObject(recentTweets)});
            }

            var client = new AmazonDynamoDBClient();
            var dynamoDb = new DynamoDBContext(client);

            var result = dynamoDb.SaveAsync(timelines);
            result.Wait();
            return JsonConvert.SerializeObject(result);

            //return "result";
        }
    }
}