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
let genericFileReader (_source:RawDataSource) =
    match _source.includeHeaders with
    | true -> File.ReadAllLines _source.Path |> Seq.toArray // headers
    | false -> File.ReadAllLines _source.Path |> Seq.skip 1 |> Seq.toArray // or no headers

let options = { Path = trainingFile; includeHeaders = false }
let dataset = genericFileReader options
