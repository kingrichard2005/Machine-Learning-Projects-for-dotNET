using DigitsRecognizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DigitsRecognizer.Tests
{
    [TestClass()]
    public class DigitRecognizerTester
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void Test_create_single_Observation_with_ObservationFactory()
        {
            //assert
            string data = "8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,56,180,255,254,224,116,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,105,233,250,180,120,157,211,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,80,250,228,44,0,98,110,194,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,48,247,249,54,34,177,229,254,240,60,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,159,254,115,102,239,240,91,39,83,46,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,151,254,189,254,231,58,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,9,229,254,254,222,47,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,198,254,217,55,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,11,102,254,254,115,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,87,250,254,254,120,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,93,254,178,49,234,215,28,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,18,219,186,20,0,89,254,120,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,145,231,16,0,0,75,254,120,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,35,221,80,0,0,0,75,255,120,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,9,221,187,0,0,0,0,98,254,76,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,69,254,105,0,0,0,0,137,252,40,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,90,254,87,0,0,4,130,249,145,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,113,240,21,0,53,188,251,150,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,126,240,127,195,243,238,106,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,73,236,254,246,161,68,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";

            //act
            var result = DataReader.ObservationFactory(data);

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void Test_ReadObservations_can_read_from_dataPath()
        {
            //arrange
            string dataPath = Path.Combine(TestContext.DeploymentDirectory, "digits", "trainingsample.csv");
            //act
            var result = DataReader.ReadObservations(dataPath);
            //assert
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod()]
        public void Test_Manhatten_Distance_Metric()
        {
            //arrange
            int[] pixels1 = { -5, -6, -7 };
            int[] pixels2 = { -1, 8, 7 };
            ManhattanDistance md = new ManhattanDistance();
            //act
            var result = md.Between(pixels1, pixels2);
            //assert
            Assert.AreEqual(result, 32);
        }

        [TestMethod()]
        public void Test_Base_Classifier_training()
        {
            //arrange
            var distance = new ManhattanDistance();
            var classifier = new BasicClassifier(distance);
            string trainingPath = Path.Combine(TestContext.DeploymentDirectory, "digits", "trainingsample.csv");
            var trainingData = DataReader.ReadObservations(trainingPath);
            classifier.Train(trainingData);
            //act
            string labelResult = classifier.Predict(trainingData[0].Pixels);
            //assert
            Assert.IsFalse(string.IsNullOrEmpty(labelResult));
        }

        [TestMethod()]
        public void Test_model_Evaluator_scoring()
        {
            // arrange
            var distance = new ManhattanDistance();
            var classifier = new BasicClassifier(distance);
            string trainingPath = Path.Combine(TestContext.DeploymentDirectory, "digits", "trainingsample.csv");
            var trainingData = DataReader.ReadObservations(trainingPath);
            classifier.Train(trainingData);
            // act
            var result = Evaluator.Score(trainingData[0], classifier);
            // assert
            Assert.AreEqual(result,1.0);
        }

        [TestMethod()]
        public void Test_model_Evaluator_correction_averaging()
        {
            // arrange
            var distance = new ManhattanDistance();
            var classifier = new BasicClassifier(distance);
            string trainingPath = Path.Combine(TestContext.DeploymentDirectory, "digits", "trainingsample.csv");
            string validationPath = Path.Combine(TestContext.DeploymentDirectory, "digits", "validationsample.csv");
            var trainingData = DataReader.ReadObservations(trainingPath);
            var validationData = DataReader.ReadObservations(validationPath);
            classifier.Train(trainingData);
            // act
            var result = Evaluator.Correct(validationData, classifier);
            // assert
            Assert.AreEqual(result, 0.934);
        }
    }
}