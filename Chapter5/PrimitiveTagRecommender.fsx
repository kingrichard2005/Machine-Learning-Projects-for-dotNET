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

//Listing 5-20. Preparing (i.e. feature scaling) the dataset
let scale (row:float[]) =
    let min = row |> Array.min
    let max = row |> Array.max
    if min = max
    then row
    else
        row |> Array.map (fun x -> (x - min) / (max - min))

let test = observations.[..99] |> Array.map scale
let train = observations.[100..] |> Array.map scale

// Listing 5-21. Similarity and utility (helper) functions
let distance (row1:float[]) (row2:float[]) =
    (row1,row2)
    ||> Array.map2 (fun x y -> pown (x - y) 2)
    |> Array.sum

let similarity (row1:float[]) (row2:float[]) =
    1. / (1. + distance row1 row2)

let split (row:float[]) =
    row.[..19],row.[20..]

let weights (values:float[]) =
    let total = values |> Array.sum
    values
    |> Array.map (fun x -> x / total)


// Listing 5-22. Computing predictions for a user
let predict (row:float[]) =
    let known,unknown = row |> split
    let similarities =
        train
        |> Array.map (fun example ->
            let common, _ = example |> split
            similarity known common)
        |> weights
    [| for i in 20 .. 29 ->
        let column = train |> Array.map (fun x -> x.[i])
        let prediction =
            (similarities,column)
            ||> Array.map2 (fun s v -> s * v)
            |> Array.sum
        prediction |]

// Apply predictions to last 10 target tags using information from the first 20 known tags
let targetTags = headers.[20..]
predict test.[0] |> Array.zip targetTags

// Listing 5-23. Percentage correct recommendations
let validation =
    test
    |> Array.map (fun obs ->
        let actual = obs |> split |> snd
        let predicted = obs |> predict
        let recommended, observed =
            Array.zip predicted actual
            |> Array.maxBy fst
        if observed > 0. then 1. else 0.)
    |> Array.average
    |> printfn "Correct calls: %f"

// Listing 5-24. Naïve recommendation accuracy
let averages = [|
    for i in 20 .. 29 ->
        train |> Array.averageBy(fun row -> row.[i]) |]

let baseline =
    test
    |> Array.map (fun obs ->
        let actual = obs |> split |> snd
        let predicted = averages
        let recommended, observed =
            Array.zip predicted actual
            |> Array.maxBy fst
        if observed > 0. then 1. else 0.)
    |> Array.average
    |> printfn "Correct calls: %f"

