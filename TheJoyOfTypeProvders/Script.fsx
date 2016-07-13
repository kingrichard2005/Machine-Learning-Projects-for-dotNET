// See chapter section: Ch 3. - "Using the JSON type provider"

//Listing 3-1. Retrieving the latest StackOverflow C# questions with the JSON type provider
#I @"..\packages\"
#r @"FSharp.Data.2.3.1\lib\net40\FSharp.Data.dll"

open FSharp.Data

type Questions = JsonProvider<"""https://api.stackexchange.com/2.2/questions?site=stackoverflow""">
let csQuestions = """https://api.stackexchange.com/2.2/questions?site=stackoverflow&
tagged=C%23"""

Questions.Load(csQuestions).Items |> Seq.iter (fun q -> printfn "%s" q.Title)

//Listing 3-2. Creating a type from a local JSON sample
[<Literal>]
let sample = """{"items":[
{"tags":["java","arrays"],"owner": "// SNIPPED FOR BREVITY"},
{"tags":["javascript","jquery","html"],"owner": "// SNIPPED FOR BREVITY"}],
"has_more":true,"quota_max":300,"quota_remaining":299}"""

type HardCodedQuestions = JsonProvider<sample>

[<Literal>]
let javaQuery = "https://api.stackexchange.com/2.2/questions?site=stackoverflow&tagged=java"
let javaQuestions = HardCodedQuestions.Load(javaQuery)

// TODO: Building a Minimal DSL to Query Questions