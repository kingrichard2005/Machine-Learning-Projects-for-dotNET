namespace DigitsRecognizer
open System.IO
    
    module DataLoader = 
        type RawDataSource = { Path:string; includeHeaders: bool }
        
        let to2DIntArray (aaInt:int [] []) =
            let dim1 = Array.length aaInt
            let dim2 = Array.length aaInt.[0]
            Array2D.init dim1 dim2 (fun i j -> aaInt.[i].[j]) 

        let CsvStringtoIntArray (csvData:string) =
            let columns = csvData.Split(',')
            columns.[0..] |> Array.map int

        let IsFile (_source:string) =
            match File.Exists(_source) with
                | true -> true
                | false -> false

        let CsvFileReader (_source:RawDataSource) =
            match IsFile(_source.Path) with
                | true ->
                    match _source.includeHeaders with
                        | false -> File.ReadAllLines _source.Path |> Seq.skip 1 |> Seq.toArray // headers
                        | true -> File.ReadAllLines _source.Path |> Seq.toArray // or no headers
                | false -> failwith "Invalid File"
