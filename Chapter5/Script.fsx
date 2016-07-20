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

// TODO: Listing 5-3. Plotting average usage by tag