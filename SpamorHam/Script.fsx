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

evaluate tokens (["txt"] |> set) training validation

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

evaluate tokens allTokens training validation