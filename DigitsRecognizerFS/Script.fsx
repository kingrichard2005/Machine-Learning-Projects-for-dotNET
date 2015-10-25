open System.IO
open System

// Define your library scripting code here
// Testing
// useful resource:
(*
    - http://dungpa.github.io/fsharp-cheatsheet/
    - http://fsharpforfunandprofit.com/troubleshooting-fsharp/
    - http://fsharpforfunandprofit.com/posts/overview-of-types-in-fsharp/
    - http://fsharpforfunandprofit.com/posts/defining-functions/
    - http://fsharpforfunandprofit.com/posts/match-expression/
    - http://fsharpforfunandprofit.com/posts/function-values-and-simple-values/
    - http://blogs.msdn.com/b/chrsmith/archive/2008/06/14/function-composition.aspx
*)
// text book example types and funcs
type Observation = { Label:string; Pixels: int[] }
type Distance = int[] * int[] -> int
let toObservation (csvData:string) =
    let columns = csvData.Split(',')
    let label = columns.[0]
    let pixels = columns.[1..] |> Array.map int
    { Label = label; Pixels = pixels }

let reader path =
    let data = File.ReadAllLines path
    data.[1..]
    |> Array.map toObservation

let manhattanDistance (pixels1,pixels2) =
     Array.zip pixels1 pixels2
     |> Array.map (fun (x,y) -> abs (x-y))
     |> Array.sum

let euclideanDistance (pixels1,pixels2) =
    Array.zip pixels1 pixels2
    |> Array.map (fun (x,y) -> pown (x-y) 2)
    |> Array.sum

let train (trainingset:Observation[])(dist:Distance) =
    let classify (pixels:int[]) =
        trainingset 
        |> Array.minBy (fun x -> dist (x.Pixels , pixels))
        |> fun x -> x.Label
    classify

// Source files
let baseDirectory = __SOURCE_DIRECTORY__
let baseDirectory' = Directory.GetParent(baseDirectory)
let trainingPath = @"DigitsRecognizerTests\datasamples\digits\trainingsample.csv"
let validationPath = @"DigitsRecognizerTests\datasamples\digits\validationsample.csv"
let trainingFile = Path.Combine(baseDirectory'.FullName, trainingPath)
let validationFile = Path.Combine(baseDirectory'.FullName, validationPath)

// text book example
let training = reader trainingFile
let manhattanClassifierModel = train training manhattanDistance
let euclideanClassifierModel = train training euclideanDistance
let validationData = reader validationFile

let evaluate validationData model = 
    validationData
    |> Array.averageBy (fun x -> if model x.Pixels = x.Label then 1. else 0.)
    |> printfn "Correct: %.3f"    

printfn "Manhattan"
evaluate validationData manhattanClassifierModel
printfn "Euclidean"
evaluate validationData euclideanClassifierModel

// Sandbox
type RawDataSource = { Path:string; includeHeaders: bool }

let to2DIntArray (aaInt:int [] []) =
    let dim1 = Array.length aaInt
    let dim2 = Array.length aaInt.[0]
    Array2D.init dim1 dim2 (fun i j -> aaInt.[i].[j]) 

let CsvStringtoIntArray (csvData:string) =
    let columns = csvData.Split(',')
    columns.[0..] |> Array.map int

let genericFileReader (_source:RawDataSource) =
    (* path includeHeaders <- previously distinct input params were rolled up into custom type RawDataSource*) 
    // playing with pattern matching
    match _source.includeHeaders with
    | false -> File.ReadAllLines _source.Path |> Seq.skip 1 |> Seq.toArray
    | _ -> File.ReadAllLines _source.Path |> Seq.toArray // all else take headers

let ``Read Csv Data To Int Array Then To 2D Array (from pipelined functions)`` (_context:RawDataSource) =
    _context
    |> genericFileReader
    |> Array.map CsvStringtoIntArray
    |> to2DIntArray

let ``Read Csv Data To Int Array Then To 2D Array (from composed functions)`` = 
    genericFileReader 
    >> Array.map CsvStringtoIntArray 
    >> to2DIntArray

let options = { Path = trainingFile; includeHeaders = false }
let rawData = ``Read Csv Data To Int Array Then To 2D Array (from composed functions)`` options
rawData.[0,0..]
//let sample = rawData.[0..1] |> Array.map CsvStringtoIntArray
//to2DIntArray sample
