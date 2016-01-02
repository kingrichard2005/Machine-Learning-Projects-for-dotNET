namespace NaiveBayes

module Validation = 
    open NaiveBayes.Classifier

    let validator (validationSet:(DocType * string)[]) 
                (trainedClassifier:(string -> DocType)) = 
        validationSet
        |> Seq.averageBy (fun (docType,sms) ->
            if docType = trainedClassifier sms then 1.0 else 0.0)

    let evaluate (tokenizer:Tokenizer)
                (tokens: Token Set) 
                (training:(DocType * string)[])
                (validation:(DocType * string)[]) = 
        let classifier = train training tokenizer tokens
        printfn "Correctly classified: %.3f" (validator validation classifier)