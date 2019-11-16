import sys
from collections import Counter
import time as time
import numpy as np
import math
import string
import spacy
from spacy.lang.en import English
from spacy.lang.es import Spanish
from spacy.lang.es.examples import sentences

#tokenizer, takes about max 0.5 second just to eliminate stop-words and have unique words for 1MB of text

def main():  
    nlp = Spanish()
    nlp.Defaults.stop_words|= {"-","--","!","'","&","$",}
    inputTextFilePath = sys.argv[1]
    content = open(inputTextFilePath, "r", encoding = 'utf-8')
    entireFileContent = content.read()
    setofuniqueselectedwords = set()
    selected_words = []

    #starting timer
    startingTime = time.time()
    totalFileLength = len(entireFileContent)
    numNLPIteration = math.modf(totalFileLength/1000000)
    remainder = numNLPIteration[0]
    numNLPIteration = numNLPIteration[1]
    i = 0
    while (i < numNLPIteration):
        subFileContent = entireFileContent[(i*1000000):((i+1)*1000000)]
        token = nlp(subFileContent, disable=["parser", "tagger", "ner"]) #simply just tokenize the given text
        for word in token:
            if word.is_stop is False and word.is_punct is False and word.is_quote is False and word.is_bracket is False and word.is_digit is False:
                selected_words.append(word.text)
        i += 1     

    if remainder > 0:
        subFileContent = entireFileContent[(i*1000000):]
        token = nlp(subFileContent, disable=["parser", "tagger", "ner"])
        for word in token:
            if word.is_stop is False and word.is_punct is False and word.is_quote is False and word.is_bracket is False and word.is_digit is False:
                selected_words.append(word.text)

    setofuniqueselectedwords = set(selected_words)
    print(time.time() - startingTime)
    keywords = ' '.join(sorted(setofuniqueselectedwords))
    sys.stdout.buffer.write(keywords.encode('utf-8'))

if __name__=='__main__':
    main()

