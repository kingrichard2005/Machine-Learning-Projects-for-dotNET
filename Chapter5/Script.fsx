// Listing 5-1. Reading the dataset in memory
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

// Listing 5-2. Basic dataset statistics
printfn "%16s %8s %8s %8s" "Tag Name" "Avg" "Min" "Max"
headers
    |> Array.iteri (fun i name ->
    let col = observations |> Array.map (fun obs -> obs.[i])
    let avg = col |> Array.average
    let min = col |> Array.min
    let max = col |> Array.max
    printfn "%16s %8.1f %8.1f %8.1f" name avg min max)

// Listing 5-3. Plotting average usage by tag
#r @"..\packages\FSharp.Charting.0.90.14\lib\net40\FSharp.Charting.dll"
#load @"..\packages\FSharp.Charting.0.90.14\FSharp.Charting.fsx"
open FSharp.Charting

let labels = ChartTypes.LabelStyle(Interval=0.25)

headers
    |> Seq.mapi (fun i name ->
    name,
    observations
    |> Seq.averageBy (fun obs -> obs.[i]))
    |> Chart.Bar
    |> fun chart -> chart.WithXAxis(LabelStyle=labels)


//  Listing 5-7. Defining (Euclidean) distance and reduction for clustering
#load "KMeans.fs"
open Unsupervised.KMeans

type Observation = float []
let features = headers.Length

//  Euclidean distance function
let distance (obs1:Observation) (obs2:Observation) =
    (obs1, obs2)
    ||> Seq.map2 (fun u1 u2 -> pown (u1 - u2) 2)
    |> Seq.sum

let centroidOf (cluster:Observation seq) =
    Array.init features (fun f ->
        cluster
        |> Seq.averageBy (fun user -> user.[f]))

//  Listing 5-8. Example clustering the dataset
// i.e. with an arbitrary value of '5' for k, i.e. the total number of clusters to derive
let observations1 =
    observations
    |> Array.map (Array.map float)
    |> Array.filter (fun x -> Array.sum x > 0.)

let (clusters1, classifier1) =
    let clustering = clusterize distance centroidOf
    let k = 5
    clustering observations1 k

//  TODO: Leftoff on section "Analyzing the Results"