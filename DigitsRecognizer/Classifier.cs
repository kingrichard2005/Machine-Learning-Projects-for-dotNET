using System;
using System.Collections.Generic;

namespace DigitsRecognizer
{
    public interface IClassifier
    {
        void Train(IEnumerable<Observation> trainingSet);
        string Predict(int[] pixels);
    }

    public class BasicClassifier : IClassifier
    {
        private IEnumerable<Observation> data;
        private readonly IDistance distance;
        public BasicClassifier(IDistance distance)
        {
            this.distance = distance;
        }

        public void Train(IEnumerable<Observation> trainingSet)
        {
            this.data = trainingSet;
        }

        public string Predict(int[] pixels)
        {
            Observation currentBest = null;
            var shortest = Double.MaxValue;
            foreach (Observation obs in this.data)
            {
                var dist = this.distance.Between(obs.Pixels, pixels);
                if (dist < shortest)
                {
                    shortest = dist;
                    currentBest = obs;
                }
            }
            return (currentBest == null ? "unknown" : currentBest.Label);
        }
    }
}
