import sys
import time as time
import math
import numpy as numpy
import spacy
import en_core_web_sm as ENGLISH
from spacy.lang.en.stop_words import STOP_WORDS
import boto3

dynamodb = boto3.resource('dynamodb')
table = dynamodb.Table('FilesWithKeyPhrases')

def extractKeyPhrases():
    nlp = ENGLISH.load()
    uniqueKeyPhrases = set()
    with open ('/Users/joyliu/Downloads/testFile1.txt') as reader:
        fileContent = reader.read()
        totalFileLength = len(fileContent)
        numNLPIteration = math.modf(totalFileLength/1000000)
        remainder = numNLPIteration[0]
        numNLPIteration = numNLPIteration[1]
        i = 0
        while (i < numNLPIteration):
            subFileContent = fileContent[(i*1000000):((i+1)*1000000)]
            token = nlp(subFileContent, disable=["parser", "tagger", "ner"]) #simply just tokenize the given text
            for word in token:
                if word.is_stop is False and word.is_punct is False and word.is_quote is False and word.is_bracket is False and word.is_digit is False:
                    uniqueKeyPhrases.add(word.text)
            i += 1     

        if remainder > 0:
            subFileContent = fileContent[(i*1000000):]
            token = nlp(subFileContent, disable=["parser", "tagger", "ner"])
            for word in token:
                if word.is_stop is False and word.is_punct is False and word.is_quote is False and word.is_bracket is False and word.is_digit is False:
                    uniqueKeyPhrases.add(word.text)
    return uniqueKeyPhrases

def putKeyPhrasesInAWS(keyPhrases):
    print(table.creation_date_time)
    table.put_item(Item={
        'FileLink': '/Users/joyliu/Downloads/testFile1.txt',
        'key_phrases': keyPhrases,
    })

def getKeyPhrasesFromDynamo(fileLink):
    response = table.get_item(
    Key={
        'FileLink': fileLink
    })
    # response is a JSON, need to parse further
    item = response['Item']['key_phrases']
    print(item)


if __name__ == "__main__":
    keyPhrases = str()
    keyPhrases = ' '.join(sorted(extractKeyPhrases())).strip().replace('\n', '')
    #print(keyPhrases)
    putKeyPhrasesInAWS(keyPhrases)
    getKeyPhrasesFromDynamo('/Users/joyliu/Downloads/testFile1.txt')
