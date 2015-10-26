namespace NaiveBayes
open System.IO

module DataLoader = 
    type RawDataSource = { Path:string; includeHeaders: bool }
    type DocType =
        | Ham
        | Spam

    let ParseDocType (label:string) =
        match label with
        | "ham" -> Ham
        | "spam" -> Spam
        | _ -> failwith "Unknown label"

    let ParseLine (line:string) =
        let split = line.Split('\t')
        let label = split.[0] |> ParseDocType
        let message = split.[1]
        (label, message)

    // Custom ReadLine(s) function for more efficient parsing of large files
    // using sequences to lazily read (i.e yield as needed) each line of a file.
    let ReadLines (filePath:string) = seq {
        use sr = new StreamReader (filePath)
        while not sr.EndOfStream do
            yield sr.ReadLine ()
    }

    let IsFile (_source:string) =
        match File.Exists(_source) with
        | true -> true
        | false -> false

    let GenericFileReader (_source:RawDataSource) =
        match _source.includeHeaders with
        | true -> ReadLines _source.Path |> Seq.toArray // headers
        | false -> ReadLines _source.Path |> Seq.skip 1 |> Seq.toArray // or no headers