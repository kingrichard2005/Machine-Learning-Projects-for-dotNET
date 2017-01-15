#r @"..\packages\MathNet.Numerics.3.13.1\lib\net40\MathNet.Numerics.dll"
#r @"..\packages\MathNet.Numerics.FSharp.3.13.1\lib\net40\MathNet.Numerics.FSharp.dll"

open MathNet
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.Statistics
open System
open System.IO

let folder = __SOURCE_DIRECTORY__
let file = "datasamples\userprofiles-toptags.txt"

let headers,observations =
    let raw =
        Path.Combine(folder, file)
        |> File.ReadAllLines
    
    // first row is headers, first col is user ID
    let headers = (raw.[0].Split ',').[1..]
    
    let observations =
        raw.[1..]
        |> Array.map (fun line -> (line.Split ',').[1..])
        |> Array.map (Array.map float)

    headers,observations

// Applying PCA to the StackOverflow Dataset
// Time to try it out in our script! Using our algorithm is as simple as this:
// Listing 5-16. Running PCA on the StackOverflow dataset
#load "PCA.fs"
open Unsupervised.PCA

let normalized = normalize (headers.Length) observations
let (eValues,eVectors), projector = pca normalized

//Listing 5-17. Feature weight analysis
let total = eValues |> Seq.sumBy (fun x -> x.Magnitude)
eValues
|> Vector.toList
|> List.rev
|> List.scan (fun (percent,cumul) value ->
    let percent = 100. * value.Magnitude / total
    let cumul = cumul + percent
    (percent,cumul)) (0.,0.)
|> List.tail
|> List.iteri (fun i (p,c) -> printfn "Feat %2i: %.2f%% (%.2f%%)" i p c)

#r @"..\packages\FSharp.Charting.0.90.14\lib\net40\FSharp.Charting.dll"
#load @"..\packages\FSharp.Charting.0.90.14\FSharp.Charting.fsx"
open FSharp.Charting

// Listing 5-18. Plotting original features against extracted components
let principalComponent comp1 comp2 =
    let title = sprintf "Component %i vs %i" comp1 comp2
    let features = headers.Length
    let coords = Seq.zip (eVectors.Column(features-comp1)) (eVectors.Column(features-comp2))
    Chart.Point (coords, Title = title, Labels = headers, MarkerSize = 7)
    |> Chart.WithXAxis(Min = -1.0, Max = 1.0,
        MajorGrid = ChartTypes.Grid(Interval = 0.25),
        LabelStyle = ChartTypes.LabelStyle(Interval = 0.25),
        MajorTickMark = ChartTypes.TickMark(Enabled = false))
    |> Chart.WithYAxis(Min = -1.0, Max = 1.0,
        MajorGrid = ChartTypes.Grid(Interval = 0.25),
        LabelStyle = ChartTypes.LabelStyle(Interval = 0.25),
        MajorTickMark = ChartTypes.TickMark(Enabled = false))

principalComponent 1 2;;
principalComponent 3 4;;

// Analyzing the Extracted Features
// Listing 5-19. Plotting observations against the principal components
let projections comp1 comp2 =
    let title = sprintf "Component %i vs %i" comp1 comp2
    let features = headers.Length
    let coords =
        normalized
        |> Seq.map projector
        |> Seq.map (fun obs -> obs.[features-comp1], obs.[features-comp2])
    Chart.Point (coords, Title = title)
    |> Chart.WithXAxis(Min = -200.0, Max = 500.0,
        MajorGrid = ChartTypes.Grid(Interval = 100.),
        LabelStyle = ChartTypes.LabelStyle(Interval = 100.),
        MajorTickMark = ChartTypes.TickMark(Enabled = false))
    |> Chart.WithYAxis(Min = -200.0, Max = 500.0,
        MajorGrid = ChartTypes.Grid(Interval = 100.),
        LabelStyle = ChartTypes.LabelStyle(Interval = 100.),
        MajorTickMark = ChartTypes.TickMark(Enabled = false))

projections 1 2;;
projections 3 4;;

// leftoff: Making Recommendations