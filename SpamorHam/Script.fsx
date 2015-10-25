// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

//#load "NaiveBayes.fs"
//open NaiveBayes
open System.IO
type DocType =
    | Ham
    | Spam

let parseDocType (label:string) =
    match label with
    | "ham" -> Ham
    | "spam" -> Spam
    | _ -> failwith "Unknown label"

let parseLine (line:string) =
    let split = line.Split('\t')
    let label = split.[0] |> parseDocType
    let message = split.[1]
    (label, message)

// Source files
let baseDirectory = __SOURCE_DIRECTORY__
let baseDirectory' = Directory.GetParent(baseDirectory)
let trainingPath = @"SpamorHam\datasamples\SMSSpamCollection"
let trainingFile = Path.Combine(baseDirectory'.FullName, trainingPath)
type RawDataSource = { Path:string; includeHeaders: bool }

// Custom ReadLine(s) function for more efficient parsing of large files
// using sequences to lazily read (i.e yield as needed) each line of a file.
let readLines (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let isFile (_source:string) =
    match File.Exists(_source) with
    | true -> true
    | false -> false

let genericFileReader (_source:RawDataSource) =
    match _source.includeHeaders with
    | true -> readLines _source.Path |> Seq.toArray // headers
    | false -> readLines _source.Path |> Seq.skip 1 |> Seq.toArray // or no headers

let options = { Path = trainingFile; includeHeaders = false }

// Text book example
let dataset = genericFileReader options |> Array.map parseLine
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
