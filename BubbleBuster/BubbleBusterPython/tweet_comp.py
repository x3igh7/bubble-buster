# Pull in data from Twitter
# Use String Distance function to find similar headlines
# ??
# Profit

from fuzzywuzzy import fuzz, process
import boto3, json, decimal, string
from boto3.dynamodb.conditions import Key, Attr

def lambda_handler(event, context):
    # might need to use that fancy class at the bottom... maybe not. some of the
    # examples seem not to use it...
    dynamo_records = event['Records'];

    # iterate over records and do what you gotta do
    matching_tweets_ids = {
        'fox': 123,
        'cnn': 456,
        'msnbc': 789
    }

    save_to_dynamo(matching_tweets_ids);
    return dynamo_records

def save_to_dynamo(matching_tweets_ids):
    dynamodb = boto3.resource('dynamodb', region_name='us-east-1')
    table = dynamodb.Table('bubble-buster-tweet-comparison')
    table.put_item(
        Item={
            'fox': matching_tweets_ids['fox'],
            'cnn': matching_tweets_ids['cnn'],
            'msnbc': matching_tweets_ids['msnbc'],
        })

def normalize(s):
    for p in string.punctuation:
        s = s.replace(p, '')
    return s.lower().strip().split(" ")

# https://marcobonzanini.com/2015/02/25/fuzzy-string-matching-in-python/
# https://github.com/seatgeek/fuzzywuzzy
# s1 and s2 are headlines pulled from social media
def clean_and_comp(s1,s2):
	clean_s1 = normalize(s1)
	clean_s2 = normalize(s2)
	return fuzz.token_set_ratio(clean_s1,clean_s2)

# http://docs.aws.amazon.com/amazondynamodb/latest/gettingstartedguide/GettingStarted.Python.04.html
# https://console.aws.amazon.com/dynamodb/home?region=us-east-1#tables:selected=bubble-buster-tweet-stream
# Helper class to convert a DynamoDB item to JSON.
class DecimalEncoder(json.JSONEncoder):
    def default(self, o):
        if isinstance(o, decimal.Decimal):
            if o % 1 > 0:
                return float(o)
            else:
                return int(o)
        return super(DecimalEncoder, self).default(o)

# http://stackoverflow.com/questions/18967696/aws-command-line-interface-unable-to-locate-credentials-special-permissions
dynamodb = boto3.resource('dynamodb', region_name='us-east-1')
table = dynamodb.Table('bubble-buster-tweet-stream')

fox_tweets = []
cnn_tweets = []
msnbc_tweets = []
matches = []

full_data = table.scan(ProjectionExpression="LatestTweets")

for i in full_data[u'Items']:
	for x in i['LatestTweets']:
		for t in x['Tweets']:
			if t['CreatedByScreenName'] == 'FoxNews':
				fox_tweets.append((t['Text'],str(t['Id'])))
			if t['CreatedByScreenName'] == 'MSNBC':
				msnbc_tweets.append((t['Text'],str(t['Id'])))
			if t['CreatedByScreenName'] == 'CNN':
				cnn_tweets.append((t['Text'],str(t['Id'])))
			else:
				continue

# http://docs.aws.amazon.com/amazondynamodb/latest/gettingstartedguide/GettingStarted.Python.04.html
# https://console.aws.amazon.com/dynamodb/home?region=us-east-1#tables:selected=bubble-buster-tweet-stream
# #https://console.aws.amazon.com/lambda/home?region=us-east-1#/functions/BubbleBuster_TweetComparision?tab=code
# https://github.com/x3igh7/bubble-buster/blob/develop/BubbleBuster/BubbleBusterPython/tweet_comp.py
for f in fox_tweets:
	for c in cnn_tweets:
		for m in msnbc_tweets:
			if clean_and_comp(f,c) > 60 and clean_and_comp(c,m) > 60 and clean_and_comp(f,m) > 60:
				matches.append((f,c,m))
			else:
				continue
