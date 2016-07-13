// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "DomainTypes.fs"
#load "DataLoader.fs"
#load "NaiveBayes.fs"
#load "Validation.fs"
open NaiveBayes.DomainTypes
open NaiveBayes.DataLoader
open NaiveBayes.Classifier
open NaiveBayes.Validation
open System.IO
open System.Text.RegularExpressions

// Source files
let baseDirectory = __SOURCE_DIRECTORY__
let baseDirectory' = Directory.GetParent(baseDirectory)
let trainingPath = @"SpamorHam\datasamples\SMSSpamCollection"
let trainingFile = Path.Combine(baseDirectory'.FullName, trainingPath)
let options = { Path = trainingFile; includeHeaders = false }

// Text book example
let dataset = GenericFileReader options |> Array.map ParseLine
let spamWithFREE =
    dataset
    |> Array.filter (fun (docType,_) -> docType = Spam)
    |> Array.filter (fun (_,sms) -> sms.Contains("FREE"))
    |> Array.length

let hamWithFREE =
    dataset
    |> Array.filter (fun (docType,_) -> docType = Ham)
    |> Array.filter (fun (_,sms) -> sms.Contains("FREE"))
    |> Array.length

let primitiveClassifier (sms:string) =
    if (sms.Contains "FREE")
    then Spam
    else Ham

let (docType,sms) = dataset.[0]
primitiveClassifier sms

// Using a Naive bayes classifier
let training = Seq.skip 1000 dataset |> Seq.toArray
let validation = Seq.take 1000 dataset |> Seq.toArray

// Func: Extracts every token from a string-encoded corpus
let vocabulary (tokenizer:Tokenizer) (corpus:string seq) =
    corpus
    |> Seq.map tokenizer
    |> Set.unionMany

// First classifier modeling attempts to use "txt" keyword as basis for Spam / Ham classification
evaluate lowerCaseTokenizer (["txt"] |> set) training validation

// Second classifier modeling attempts to use all (lowercase variants) of tokens from the training set
let lowercaseTokens =
    training
    |> Seq.map snd
    |> vocabulary lowerCaseTokenizer

evaluate lowerCaseTokenizer lowercaseTokens training validation

// Third classifier modeling attempts to use all (cased variants) of tokens from the training set
let casedTokens =
    training
    |> Seq.map snd
    |> vocabulary casedTokenizer

evaluate casedTokenizer casedTokens training validation

// see chapter section: Ch 2. - "Less is more"
// Fourth classifier modeling eliminates noise by reducing the number of 
// tokens (i.e. removing features) considered for classifying Spam vs Ham 
// down to the top N most frequently used tokens
let top n (tokenizer:Tokenizer) (docs:string []) =
    let tokenized = docs |> Array.map tokenizer
    let tokens = tokenized |> Set.unionMany
    tokens
    |> Seq.sortBy (fun t -> - countIn tokenized t)
    |> Seq.take n
    |> Set.ofSeq

let ham,spam =
    let rawHam,rawSpam =
        training
        |> Array.partition (fun (lbl,_) -> lbl=Ham)
    rawHam |> Array.map snd,
    rawSpam |> Array.map snd

// We extract and count how many tokens we have in each group, 
// pick the top 10%, and merge them into one token set using Set.union:
let hamCount = ham |> vocabulary casedTokenizer |> Set.count
let spamCount = spam |> vocabulary casedTokenizer |> Set.count

let topHam = ham |> top (hamCount / 10) casedTokenizer
let topSpam = spam |> top (spamCount / 10) casedTokenizer

let topTokens = Set.union topHam topSpam

// Integrate tokens into a model and evaluate performance
evaluate casedTokenizer topTokens training validation

// see chapter section: Ch 2. - "Choosing Our Words Carefully"
// Debug: Print (to inspect) the top 20 tokens for Ham and Spam
ham |> top 20 casedTokenizer |> Seq.iter (printfn "%s")
spam |> top 20 casedTokenizer |> Seq.iter (printfn "%s")

// Instead of relying on a list of stop words, we will do something simpler. Our topHam and topSpam sets
// contain the most frequently used tokens in Ham and in Spam. If a token appears in both lists, it is likely
// simply a word that is frequently found in English text messages, and is not particularly specific to either Ham
// or Spam. Let’s identify all these common tokens, which correspond to the intersection of both lists, remove
// them from our tokens selection, and run the analysis again:

let commonTokens = Set.intersect topHam topSpam
let specificTokens = Set.difference topTokens commonTokens

evaluate casedTokenizer specificTokens training validation

// See chapter section: Ch 2. - "Creating New Features"
let rareTokens n (tokenizer:Tokenizer) (docs:string []) =
    let tokenized = docs |> Array.map tokenizer
    let tokens = tokenized |> Set.unionMany
    tokens
    |> Seq.sortBy (fun t -> countIn tokenized t)
    |> Seq.take n
    |> Set.ofSeq

let rareHam = ham |> rareTokens 50 casedTokenizer |> Seq.iter (printfn "%s")
let rareSpam = spam |> rareTokens 50 casedTokenizer |> Seq.iter (printfn "%s")

// Listing 2-11. Recognizing phone numbers
let phoneWords = Regex(@"0[7-9]\d{9}")
let phone (text:string) =
    match (phoneWords.IsMatch text) with
    | true -> "__PHONE__"
    | false -> text

let txtCode = Regex(@"\b\d{5}\b")
let txt (text:string) =
    match (txtCode.IsMatch text) with
    | true -> "__TXT__"
    | false -> text

let smartTokenizer = casedTokenizer >> Set.map phone >> Set.map txt
let smartTokens =
    specificTokens
    |> Set.add "__TXT__"
    |> Set.add "__PHONE__"

evaluate smartTokenizer smartTokens training validation

// See chapter section: Ch 2. - "Dealing with Numeric Values"
// Listing 2-12. Spam probability based on message length
let lengthAnalysis len =
    let long (msg:string) = msg.Length > len
    let ham,spam =
        dataset
        |> Array.partition (fun (docType,_) -> docType = Ham)
    let spamAndLongCount =
        spam
        |> Array.filter (fun (_,sms) -> long sms)
        |> Array.length
    let longCount =
        dataset
        |> Array.filter (fun (_,sms) -> long sms)
        |> Array.length
    let pSpam = (float spam.Length) / (float dataset.Length)
    let pLongIfSpam =
        float spamAndLongCount / float spam.Length
    let pLong =
        float longCount /
        float (dataset.Length)
    let pSpamIfLong = pLongIfSpam * pSpam / pLong
    pSpamIfLong

for l in 10 .. 10 .. 130 do
    printfn "P(Spam if Length > %i) = %.4f" l (lengthAnalysis l)

// See chapter section: Ch 2. - "Understanding Errors"
// Listing 2-13. Error by class
let bestClassifier = train training smartTokenizer smartTokens
validation
|> Seq.filter (fun (docType,_) -> docType = Ham)
|> Seq.averageBy (fun (docType,sms) ->
if docType = bestClassifier sms
then 1.0
else 0.0)
|> printfn "Properly classified Ham: %.5f"
validation
|> Seq.filter (fun (docType,_) -> docType = Spam)
|> Seq.averageBy (fun (docType,sms) ->
if docType = bestClassifier sms
then 1.0
else 0.0)
|> printfn "Properly classified Spam: %.5f"

// End of chapter 2