#r @"..\packages\FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"
open FSharp.Data
type Titanic = CsvProvider<"dataset\\titanic.csv">
type Passenger = Titanic.Row
let dataset = Titanic.GetSample ()

dataset.Rows
    |> Seq.countBy (fun passenger -> passenger.Survived)
    |> Seq.iter (printfn "%A")

dataset.Rows
|> Seq.averageBy (fun passenger ->
    if passenger.Survived then 1.0 else 0.0)
|> printfn "Chances of survival: %.3f"

// leftoff: Taking a Look at Features