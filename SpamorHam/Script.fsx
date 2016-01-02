// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "DomainTypes.fs"
#load "DataLoader.fs"
#load "NaiveBayes.fs"
#load "Validation.fs"
open NaiveBayes.DomainTypes
open NaiveBayes.DataLoader
open NaiveBayes.Classifier
open NaiveBayes.Validation
open System.IO

// Source files
let baseDirectory = __SOURCE_DIRECTORY__
let baseDirectory' = Directory.GetParent(baseDirectory)
let trainingPath = @"SpamorHam\datasamples\SMSSpamCollection"
let trainingFile = Path.Combine(baseDirectory'.FullName, trainingPath)
let options = { Path = trainingFile; includeHeaders = false }

// Text book example
let dataset = GenericFileReader options |> Array.map ParseLine
let spamWithFREE =
    dataset
    |> Array.filter (fun (docType,_) -> docType = Spam)
    |> Array.filter (fun (_,sms) -> sms.Contains("FREE"))
    |> Array.length

let hamWithFREE =
    dataset
    |> Array.filter (fun (docType,_) -> docType = Ham)
    |> Array.filter (fun (_,sms) -> sms.Contains("FREE"))
    |> Array.length

let primitiveClassifier (sms:string) =
    if (sms.Contains "FREE")
    then Spam
    else Ham

let (docType,sms) = dataset.[0]
primitiveClassifier sms

// Using a Naive bayes classifier
let training = Seq.skip 1000 dataset |> Seq.toArray
let validation = Seq.take 1000 dataset |> Seq.toArray

// Func: Extracts every token from a string-encoded corpus
let vocabulary (tokenizer:Tokenizer) (corpus:string seq) =
    corpus
    |> Seq.map tokenizer
    |> Set.unionMany

// First classifier modeling attempts to use "txt" keyword as basis for Spam / Ham classification
evaluate lowerCaseTokenizer (["txt"] |> set) training validation

// Second classifier modeling attempts to use all (lowercase variants) of tokens from the training set
let lowercaseTokens =
    training
    |> Seq.map snd
    |> vocabulary lowerCaseTokenizer

evaluate lowerCaseTokenizer lowercaseTokens training validation

// Third classifier modeling attempts to use all (cased variants) of tokens from the training set
let casedTokens =
    training
    |> Seq.map snd
    |> vocabulary casedTokenizer

evaluate casedTokenizer casedTokens training validation

// Fourth classifier modeling eliminates noise by reducing the number of 
// tokens (i.e. removing features) considered for classifying Spam vs Ham 
// down to the top N most frequently used tokens
let top n (tokenizer:Tokenizer) (docs:string []) =
    let tokenized = docs |> Array.map tokenizer
    let tokens = tokenized |> Set.unionMany
    tokens
    |> Seq.sortBy (fun t -> - countIn tokenized t)
    |> Seq.take n
    |> Set.ofSeq

let ham,spam =
    let rawHam,rawSpam =
        training
        |> Array.partition (fun (lbl,_) -> lbl=Ham)
    rawHam |> Array.map snd,
    rawSpam |> Array.map snd

// We extract and count how many tokens we have in each group, 
// pick the top 10%, and merge them into one token set using Set.union:
let hamCount = ham |> vocabulary casedTokenizer |> Set.count
let spamCount = spam |> vocabulary casedTokenizer |> Set.count

let topHam = ham |> top (hamCount / 10) casedTokenizer
let topSpam = spam |> top (spamCount / 10) casedTokenizer

let topTokens = Set.union topHam topSpam

// Integrate tokens into a model and evaluate performance
evaluate casedTokenizer topTokens training validation