// See chapter section: Ch 4. - Getting to Know the Data
#I @"..\packages\"
#r @"FSharp.Data.2.3.1\lib\net40\FSharp.Data.dll"

open FSharp.Data
type Data = CsvProvider<"datasamples\day.csv">
let dataset = Data.Load("datasamples\day.csv")
let data = dataset.Rows

#load @"FSharp.Charting.0.90.14\FSharp.Charting.fsx"
open FSharp.Charting

// Figure 4-2. Day-by-day bicycle usage over time since the first observation
let dataseries = [ for obs in data -> float obs.Cnt ]
let all = Chart.Line dataseries

// See chapter section: Ch 4. - Spotting Trends with Moving Averages

// This chunks the dataseries 1 .. 10 into blocks of three consecutive values: [[|1; 2; 3|]; [|2; 3; 4|];
// [|3; 4; 5|]; [|4; 5; 6|]; [|5; 6; 7|]; [|6; 7; 8|]; [|7; 8; 9|]; [|8; 9; 10|]]. From there,
// generating the moving average (ma) is only a matter of computing the average of each of the chunks:
let windowedExample =
    [ for i in 1 .. 10 -> float i ]
    |> Seq.windowed 3
    |> Seq.toList

let ma n (dataseries:float list) =
    dataseries
    |> Seq.windowed n
    |> Seq.map (fun xs -> xs |> Seq.average)
    |> Seq.toList

Chart.Combine   [
    Chart.Line dataseries
    Chart.Line (ma 7 dataseries)
    Chart.Line (ma 30 dataseries)]

// See chapter section: Ch 4. - Fitting a Model to the Data
let baseline =
    let avg = data |> Seq.averageBy (fun x -> float x.Cnt)
    data |> Seq.averageBy (fun x -> abs (float x.Cnt - avg))

// Defining a Basic Straight-Line (i.e. Linear Regression) Model
type Obs = Data.Row
let model (theta0, theta1) (observation:Obs) =
    theta0 + theta1 * (float observation.Instant)


let model0 = model (4504., 0.)
let model1 = model (6000., -4.5)

// Figure 4-4. Visualizing two demo linear regression models
Chart.Combine   [
    Chart.Line dataseries
    Chart.Line [ for obs in data -> model0 obs ]
    Chart.Line [ for obs in data -> model1 obs ]]

//  See chapter section: Ch 4. - Finding the Lowest-Cost Model

