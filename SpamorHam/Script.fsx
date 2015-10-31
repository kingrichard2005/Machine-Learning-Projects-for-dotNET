// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "DataLoader.fs"
#load "NaiveBayes.fs"
open NaiveBayes.DataLoader
open NaiveBayes.Classifier
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

// Using Naive bayes classifier
let training = Seq.skip 1000 dataset |> Seq.toArray
let validation = Seq.take 1000 dataset |> Seq.toArray

// First attempt, train a simple model with a single token vocab., i.e. "txt"
let txtClassifier = train training tokens (["txt"] |> set)
validation
|> Seq.averageBy (fun (docType,sms) ->
    if docType = txtClassifier sms then 1.0 else 0.0)
|> printfn "Based on 'txt', correctly classified: %.3f"

// Second attempt uses all tokens from the training set
// Extracts every token from a string
let vocabulary (tokenizer:Tokenizer) (corpus:string seq) =
    corpus
    |> Seq.map tokenizer
    |> Set.unionMany

// Get all tokens in training set
let allTokens =
    training
    |> Seq.map snd
    |> vocabulary tokens

let fullClassifier = train training tokens allTokens

validation
|> Seq.averageBy (fun (docType,sms) ->
if docType = fullClassifier sms then 1.0 else 0.0)
|> printfn "Based on all tokens, correctly classified: %.3f"
