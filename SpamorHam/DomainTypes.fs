namespace NaiveBayes

// make DomainTypes module content global and visible to all other modules
[<AutoOpen>]
module DomainTypes = 
    type Token = string
    type Tokenizer = string -> Token Set
    type TokenizedDoc = Token Set
    type DocsGroup =
        { Proportion:float
        ;TokenFrequencies:Map<Token,float> }

    type RawDataSource = { Path:string; includeHeaders: bool }
    type DocType =
        | Ham
        | Spam