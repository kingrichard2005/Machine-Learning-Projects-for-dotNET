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

//  Section "Analyzing the Results"
clusters1
    |> Seq.iter (fun (id,profile) ->
        printfn "CLUSTER %i" id
        profile
        |> Array.iteri (fun i value -> printfn "%16s %.1f" headers.[i] value))

Chart.Combine [
    for (id,profile) in clusters1 ->
        profile
        |> Seq.mapi (fun i value -> headers.[i], value)
        |> Chart.Bar
    ]
|> fun chart -> chart.WithXAxis(LabelStyle=labels)

//  How big are each of these clusters?
observations1
|> Seq.countBy (fun obs -> classifier1 obs)
|> Seq.iter (fun (clusterID, count) ->
    printfn "Cluster %i: %i elements" clusterID count)

//  Rescaling Our Dataset to Improve Clusters
let rowNormalizer (obs:Observation) : Observation =
    let max = obs |> Seq.max
    obs |> Array.map (fun tagUse -> tagUse / max)

let observations2 =
    observations
    |> Array.filter (fun x -> Array.sum x > 0.)
    |> Array.map (Array.map float)
    |> Array.map rowNormalizer

let (clusters2, classifier2) =
    let clustering = clusterize distance centroidOf
    let k = 5
    clustering observations2 k

observations2
|> Seq.countBy (fun obs -> classifier2 obs)
|> Seq.iter (fun (clusterID, count) ->
    printfn "Cluster %i: %i elements" clusterID count)

//  Section: Identifying How Many Clusters to Search For
//  ===

//  Defining the Akaike Information Criterion, or AIC, cluster quality metric

//  Start by using the 'Rule of Thumb' industry standard method to calibrate the AIC
let ruleOfThumb (n:int) = sqrt (float n / 2.)
let k_ruleOfThumb = ruleOfThumb (observations2.Length)

//  Listing 5-10. Akaike Information Criterion (AIC) definition
let squareError (obs1:Observation) (obs2:Observation) =
    (obs1,obs2)
    ||> Seq.zip
    |> Seq.sumBy (fun (x1,x2) -> pown (x1-x2) 2)

let RSS (dataset:Observation[]) centroids =
    dataset
    |> Seq.sumBy (fun obs ->
        centroids
        |> Seq.map (squareError obs)
        |> Seq.min)

let AIC (dataset:Observation[]) centroids =
    let k = centroids |> Seq.length
    let m = dataset.[0] |> Seq.length
    RSS dataset centroids + float (2 * m * k)

//  In order to figure out a decent value for k, we’ll try out every single possibility between k = 1 
//  and k = 25. Because the clustering itself is going to be unstable for low values of k, we will run the algorithm a couple
//  of times for each value and average out the corresponding AIC so that we can cancel out potential flukes due
//  to, for instance, unfortunate initial value selection:

//  Figure 5-19. Search for k-minimizing AIC
[1..25]
|> Seq.map (fun k ->
    let value =
        [ for _ in 1 .. 10 ->
            let (clusters, classifier) =
                let clustering = clusterize distance centroidOf
                clustering observations2 k
            AIC observations2 (clusters |> Seq.map snd) ]
        |> List.average
    k, value)
|> Chart.Line

//  Listing 5-11. Our final (best) clusters
let (bestClusters, bestClassifier) =
    let clustering = clusterize distance centroidOf
    let k = 10
    seq {
        for _ in 1 .. 20 ->
            clustering observations2 k
    }
    |> Seq.minBy (fun (cs,f) ->
        RSS observations2 (cs |> Seq.map snd))

bestClusters
|> Seq.sortBy fst
|> Seq.iter (fun (id,profile) ->
    printfn "CLUSTER %i" id
    profile
    |> Array.iteri (fun i value ->
        if value > 0.2 then printfn "%16s %.1f" headers.[i] value))

//  Leftoff on section "Detecting How Features Are Related"
//  ===
