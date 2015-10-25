using System;
using System.IO;

namespace DigitsRecognizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var distance = new ManhattanDistance();
            var classifier = new BasicClassifier(distance);
            var trainingPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory() + "../../..").ToString(), "DigitsRecognizerTests", "datasamples", "digits", "trainingsample.csv");
            var training = DataReader.ReadObservations(trainingPath);
            classifier.Train(training);
            var validationPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory() + "../../..").ToString(), "DigitsRecognizerTests", "datasamples", "digits", "validationsample.csv");
            var validation = DataReader.ReadObservations(validationPath);
            var correct = Evaluator.Correct(validation, classifier);
            Console.WriteLine("Correctly classified: {0:P2}", correct);
            Console.ReadLine();
        }
    }
}
