# Pull in data from Twitter, Facebook, and Reddit(?)
# Use String Distance function to find similar headlines
# ??
# Profit

import string
from fuzzywuzzy import fuzz, process
 
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

cnn = ["Former Labor Secretary Tom Perez elected DNC chair","Man in Heidelberg drives car into pedestrians, is shot by police"]
fox = ["Tom Perez elected chairman of the DNC","Germany: Man hits 3 with car and flees, is shot by police"]
msnbc = ["DNC selects Tom Perez as chair","man in Germany car flees,is shot by police"]

matches = []

for f in fox:
	for c in cnn:
		for m in msnbc:
			if clean_and_comp(f,c) > 60 and clean_and_comp(c,m) > 60 and clean_and_comp(f,m) > 60:
				matches.append((f,c,m))
			else:
				continue

# http://docs.aws.amazon.com/amazondynamodb/latest/gettingstartedguide/GettingStarted.Python.04.html
# https://console.aws.amazon.com/dynamodb/home?region=us-east-1#tables:selected=bubble-buster-tweet-stream

import boto3, json, decimal
from boto3.dynamodb.conditions import Key, Attr

access_key = 'AKIAIKNAALLTOLBXIAJQ'
secret_access_key = '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'

# Helper class to convert a DynamoDB item to JSON.
class DecimalEncoder(json.JSONEncoder):
    def default(self, o):
        if isinstance(o, decimal.Decimal):
            if o % 1 > 0:
                return float(o)
            else:
                return int(o)
        return super(DecimalEncoder, self).default(o)

dynamodb = boto3.resource('dynamodb', region_name='us-east-1')

table = dynamodb.Table('bubble-buster-tweet-stream')

print("Movies from 1992 - titles A-L, with genres and lead actor")

response = table.query(
    ProjectionExpression="#yr, title, info.genres, info.actors[0]",
    ExpressionAttributeNames={ "#yr": "year" }, # Expression Attribute Names for Projection Expression only.
    KeyConditionExpression=Key('year').eq(1992) & Key('title').between('A', 'L'))

for i in response[u'Items']:
    print(json.dumps(i, cls=DecimalEncoder))