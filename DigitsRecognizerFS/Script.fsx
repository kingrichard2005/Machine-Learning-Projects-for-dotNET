#load "DataLoader.fs"
open DigitsRecognizer.DataLoader
open System.IO
open System

// Define your library scripting code here
// Testing

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
let ``Read Csv Data To Int Array Then To 2D Array (from pipelined functions)`` (_context:RawDataSource) =
    _context
    |> CsvFileReader
    |> Array.map CsvStringtoIntArray
    |> to2DIntArray

let ``Read Csv Data To Int Array Then To 2D Array (from composed functions)`` = 
    CsvFileReader 
    >> Array.map CsvStringtoIntArray 
    >> to2DIntArray

let options = { Path = trainingFile; includeHeaders = false }
let rawData = ``Read Csv Data To Int Array Then To 2D Array (from composed functions)`` options
rawData.[0,0..]
//let sample = rawData.[0..1] |> Array.map CsvStringtoIntArray
//to2DIntArray sample
