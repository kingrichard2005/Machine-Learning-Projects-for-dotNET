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

// See chapter section: Ch 4. - Finding the Lowest-Cost Model
type Model = Obs -> float

let cost (data:Obs seq) (m:Model) =
    data
    |> Seq.sumBy (fun x -> pown (float x.Cnt - m x) 2) // Euclidean-distance metric squares differences to penalize larger errors over smaller ones
    |> sqrt

let overallCost = cost data
overallCost model0 |> printfn "Cost model0: %.0f"
overallCost model1 |> printfn "Cost model1: %.0f"

// See chapter section: Ch 4. - Finding the Minimum of a (Cost) Function with Gradient Descent

let update alpha (theta0, theta1) (obs:Obs) =
    let y = float obs.Cnt
    let x = float obs.Instant
    let theta0' = theta0 - 2. * alpha * 1. * (theta0 + theta1 * x - y)
    let theta1' = theta1 - 2. * alpha * x * (theta0 + theta1 * x - y)
    theta0', theta1'

let obs100 = data |> Seq.item 100
let testUpdate = update 0.00001 (0.,0.) obs100
cost [obs100] (model (0.,0.))
cost [obs100] (model testUpdate)

let stochastic rate (theta0,theta1) =
    data
    |> Seq.fold (fun (t0,t1) obs ->
        printfn "%.4f,%.4f" t0 t1
        update rate (t0,t1) obs) (theta0,theta1)

// Toy fold example
let toydata = [0;1;2;3;4]
let sum = toydata |> Seq.fold (fun total x -> total + x) 0

let tune_rate =
    [ for r in 1 .. 20 ->
            (pown 0.1 r), stochastic (pown 0.1 r) (0.,0.) |> model |> overallCost ]


let rate = pown 0.1 8
let model2 = model (stochastic rate (0.0,0.0))

// Figure 4-6. Best-fit curve using stochastic gradient descent
Chart.Combine [
    Chart.Line dataseries
    Chart.Line [ for obs in data -> model2 obs ] ]

let hiRate = 10.0 * rate
let error_eval =
    data
    |> Seq.scan (fun (t0,t1) obs -> update hiRate (t0,t1) obs) (0.0,0.0)
    |> Seq.map (model >> overallCost)
    |> Chart.Line // Figure 4-7. Step-by-step error using stochastic gradient descent


let batchUpdate rate (theta0, theta1) (data:Obs seq) =
    let updates =
        data
        |> Seq.map (update rate (theta0, theta1))
    let theta0' = updates |> Seq.averageBy fst
    let theta1' = updates |> Seq.averageBy snd
    theta0', theta1'

let batch rate iters =
    let rec search (t0,t1) i =
        if i = 0 then (t0,t1)
        else
            search (batchUpdate rate (t0,t1) data) (i-1)
    search (0.0,0.0) iters


let batched_error rate =
    Seq.unfold (fun (t0,t1) ->
        let (t0',t1') = batchUpdate rate (t0,t1) data
        let err = model (t0,t1) |> overallCost
        Some(err, (t0',t1'))) (0.0,0.0)
    |> Seq.take 100
    |> Seq.toList
    |> Chart.Line

// Figure 4-8. Step-by-step error using batch gradient descent
batched_error 0.000001

// Linear Algebra with Math.NET
#r @"MathNet.Numerics.Signed.3.12.0\lib\net40\MathNet.Numerics.dll"
#r @"MathNet.Numerics.FSharp.Signed.3.12.0\lib\net40\MathNet.Numerics.FSharp.dll"

open MathNet
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double

// Toy Math.Net vector and matrix example
let A = vector [ 1.; 2.; 3. ]
let B = matrix [    [ 1.; 2. ]
                    [ 3.; 4. ]
                    [ 5.; 6. ] ]
let C = A * A
let D = A * B
let E = A * B.Column(1)

// ...rewrite our prediction and cost functions, "algebra-style":
type Vec = Vector<float>
type Mat = Matrix<float>

let cost_v2 (theta:Vec) (Y:Vec) (X:Mat) =
    let ps = Y - (theta * X.Transpose())
    ps * ps |> sqrt

let predict (theta:Vec) (v:Vec) = theta * v

let X = matrix [ for obs in data -> [ 1.; float obs.Instant ]]
let Y = vector [ for obs in data -> float obs.Cnt ]

let theta = vector [6000.; -4.5]
predict theta (X.Row(0))
cost_v2 theta Y X

let estimate (Y:Vec) (X:Mat) =
    (X.Transpose() * X).Inverse() * X.Transpose() * Y


// Cross-Validation and Over-Fitting, Again
let seed = 314159
let rng = System.Random(seed)

// Fisher-Yates shuffle
let shuffle (arr:'a []) =
    let arr = Array.copy arr
    let len = arr.Length
    for i in (len - 1) .. -1 .. 1 do
        let temp = arr.[i]
        let j = rng.Next(0, i+1)
        arr.[i] <- arr.[j]
        arr.[j] <- temp
    arr

// Test
let myArray = [| 1 .. 5 |]
myArray |> shuffle


let training,validation =
    let shuffled =
        data
        |> Seq.toArray
        |> shuffle
    let size =
        0.7 * float (Array.length shuffled) |> int
    shuffled.[..size],
    shuffled.[size+1..]

// Simplifying the Creation of Models
type Featurizer = Obs -> float list

let exampleFeaturizer (obs:Obs) =
    [ 1.0;
    float obs.Instant; ]

let predictor (f:Featurizer) (theta:Vec) =
    f >> vector >> (*) theta

let evaluate (model:Model) (data:Obs seq) =
    data
    |> Seq.averageBy (fun obs ->
        abs (model obs - float obs.Cnt))


let model_v2 (f:Featurizer) (data:Obs seq) =
    let Yt, Xt =
        data
        |> Seq.toList
        |> List.map (fun obs -> float obs.Cnt, f obs)
        |> List.unzip
    let theta = estimate (vector Yt) (matrix Xt)
    let predict = predictor f theta
    theta,predict

let featurizer0 (obs:Obs) =
    [ 1.;
        float obs.Instant; ]

let (theta0,model0_v2) = model_v2 featurizer0 training

evaluate model0_v2 training |> printfn "Training: %.0f"
evaluate model0_v2 validation |> printfn "Validation: %.0f"

// Figure 4-9. Visualization of the straight-line regression
Chart.Combine [
    Chart.Line [ for obs in data -> float obs.Cnt ]
    Chart.Line [ for obs in data -> model0_v2 obs ] ]

let featurizer1 (obs:Obs) =
    [   1.
        obs.Instant |> float
        obs.Atemp |> float
        obs.Hum |> float
        obs.Temp |> float
        obs.Windspeed |> float
    ]

let (theta1,model1_v2) = model_v2 featurizer1 training
evaluate model1_v2 training |> printfn "Training: %.0f"
evaluate model1_v2 validation |> printfn "Validation: %.0f"

// Figure 4-11. Predicted versus actual scatterplot
Chart.Point [ for obs in data -> float obs.Cnt, model1_v2 obs ]

let featurizer2 (obs:Obs) =
    [   1.
        obs.Instant |> float
        obs.Hum |> float
        obs.Temp |> float
        obs.Windspeed |> float
        (if obs.Weekday = 0 then 1.0 else 0.0)
        (if obs.Weekday = 1 then 1.0 else 0.0)
        (if obs.Weekday = 2 then 1.0 else 0.0)
        (if obs.Weekday = 3 then 1.0 else 0.0)
        (if obs.Weekday = 4 then 1.0 else 0.0)
        (if obs.Weekday = 5 then 1.0 else 0.0)
        (if obs.Weekday = 6 then 1.0 else 0.0)
    ]

let (theta2_v2,model2_v2) = model_v2 featurizer2 training

let featurizer2_v2 (obs:Obs) =
    [   1.
        obs.Instant |> float
        obs.Hum |> float
        obs.Temp |> float
        obs.Windspeed |> float
        (if obs.Weekday = 1 then 1.0 else 0.0)
        (if obs.Weekday = 2 then 1.0 else 0.0)
        (if obs.Weekday = 3 then 1.0 else 0.0)
        (if obs.Weekday = 4 then 1.0 else 0.0)
        (if obs.Weekday = 5 then 1.0 else 0.0)
        (if obs.Weekday = 6 then 1.0 else 0.0)
    ]

let (theta2,model2_v3) = model_v2 featurizer2_v2 training
evaluate model2 training |> printfn "Training: %.0f"
evaluate model2 validation |> printfn "Validation: %.0f"

// Figure 4-12. Bicycle usage against temperature scatterplot
Chart.Point [ for obs in data -> obs.Temp, obs.Cnt ]

// Higher-order polynomial feature
let squareTempFeaturizer (obs:Obs) =
    [   1.
        obs.Temp |> float
        obs.Temp * obs.Temp |> float
    ]

let (_,squareTempModel) = model_v2 squareTempFeaturizer data

// Figure 4-13. Fitting square temperature
Chart.Combine [
    Chart.Point [ for obs in data -> obs.Temp, obs.Cnt ]
    Chart.Point [ for obs in data -> obs.Temp, squareTempModel obs ] ]

let featurizer3 (obs:Obs) =
    [   1.
        obs.Instant |> float
        obs.Hum |> float
        obs.Temp |> float
        obs.Windspeed |> float
        obs.Temp * obs.Temp |> float
        (if obs.Weekday = 1 then 1.0 else 0.0)
        (if obs.Weekday = 2 then 1.0 else 0.0)
        (if obs.Weekday = 3 then 1.0 else 0.0)
        (if obs.Weekday = 4 then 1.0 else 0.0)
        (if obs.Weekday = 5 then 1.0 else 0.0)
        (if obs.Weekday = 6 then 1.0 else 0.0)
    ]

let (theta3,model3) = model_v2 featurizer3 training
evaluate model3 training |> printfn "Training: %.0f"
evaluate model3 validation |> printfn "Validation: %.0f"

