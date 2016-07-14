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

// See chapter section: Ch 3. - Building a Minimal DSL to Query Questions
// Listing 3-3. Building a minimal query DSL
let questionQuery = """https://api.stackexchange.com/2.2/questions?site=stackoverflow"""

let tagged tags query =
    // join the tags in a ; separated string
    let joinedTags = tags |> String.concat ";"
    sprintf "%s&tagged=%s" query joinedTags

let page p query = sprintf "%s&page=%i" query p

let pageSize s query = sprintf "%s&pagesize=%i" query s

let extractQuestions (query:string) = Questions.Load(query).Items

//Listing 3-4. Using our DSL to extract questions tagged C# and F#
let ``C#`` = "C%23"
let ``F#`` = "F%23"
let fsSample =
    questionQuery
    |> tagged [``F#``]
    |> pageSize 100
    |> extractQuestions

let csSample =
    questionQuery
    |> tagged [``C#``]
    |> pageSize 100
    |> extractQuestions

//Listing 3-5. Comparing tags by language
let analyzeTags (qs:Questions.Item seq) =
    qs
    |> Seq.collect (fun question -> question.Tags)
    |> Seq.countBy id
    |> Seq.filter (fun (_,count) -> count > 2)
    |> Seq.sortBy (fun (_,count) -> -count)
    |> Seq.iter (fun (tag,count) -> printfn "%s,%i" tag count)

analyzeTags fsSample
analyzeTags csSample

// See chapter section: Ch 3. - The World Bank Type Provider
// Listing 3-6. Using the World Bank type provider
let wb = WorldBankData.GetDataContext()
wb.Countries.Japan.CapitalCity

let countries = wb.Countries
let pop2000 = [ for c in countries -> c.Indicators.``Population, total``.[2000]]
let pop2010 = [ for c in countries -> c.Indicators.``Population, total``.[2010]]

// See chapter section: Ch 3. - The R Type Provider
// note: R programming language required on local development system
#r @"R.NET.Community.1.6.5\lib\net40\RDotNet.dll"
#r @"RProvider.1.1.20\lib\net40\RProvider.Runtime.dll"
#r @"RProvider.1.1.20\lib\net40\RProvider.dll"
open RProvider
open RProvider.``base``
open RProvider.graphics

// Listing 3-7. Basic summary statistics from R
// Retrieve an (F#) list of country surfaces
let surface = [ for c in countries -> c.Indicators.``Surface area (sq. km)``.[2010]]
// Produce summary statistics
R.summary(surface) |> R.print

// Figure 3-6. Basic histogram of country surfaces using R graphics
R.hist(surface)

// Figure 3-7. Log-transformed country surface areas using R graphics
// Can use F# code...
R.hist(surface |> List.map log)
// ...or R functions
R.hist(surface |> R.log)

// Figure 3-8. Basic scatterplot using R.plot
R.plot(surface, pop2010) 

// See chapter section: Ch 3. - Analyzing Data Together with R Data Frames
// Listing 3-8. Creating and plotting an R data frame
let pollution = [ for c in countries -> c.Indicators.``CO2 emissions (kt)``.[2000]]
let education = [ for c in countries -> c.Indicators.``School enrollment, secondary (gross), gender parity index (GPI)``.[2000]]

let rdf = 
    [   "Pop2000", box pop2000
        "Pop2010" , box pop2010
        "Surface" , box surface
        "Pollution", box pollution
        "Education", box education ]
    |> namedParams
    |> R.data_frame

// Scatterplot of all features
rdf |> R.plot

// Summary of all features
rdf |> R.summary |> R.print

// TODO: See chapter section: Ch 3. Deedle, a .NET Data Frame