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
let all = Chart.Line [ for obs in data -> obs.Cnt ]

// TODO: See chapter section: Ch 4. - Spotting Trends with Moving Averages