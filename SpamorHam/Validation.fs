namespace NaiveBayes

module Validation = 

    let validator (validationSet:(DocType * string)[]) 
                (trainedClassifier:(string -> DocType)) = 
        validationSet
        |> Seq.averageBy (fun (docType,sms) ->
            if docType = trainedClassifier sms then 1.0 else 0.0)