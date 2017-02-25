using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Environment = System.Environment;
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

            var timelines = new Dictionary<string, IEnumerable<ITweet>>();

            var followed = new List<string> {"FoxNews", "CNN", "MSNBC"};
            foreach (var screename in followed)
            {
                var recentTweets = Timeline.GetUserTimeline(screename, 40);
                timelines.Add(screename, recentTweets);
            }

            return JsonConvert.SerializeObject(timelines);
        }
    }
}