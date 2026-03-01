using System;
using System.Collections.Generic;
using System.Linq;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace LeKatsuMNL.Services
{
    public class LstmForecastingService : ILstmForecastingService
    {
        public List<double> PredictFutureDemand(List<double> historicalData, int stepsToPredict = 7, int lookbackWindow = 3)
        {
            if (historicalData == null || historicalData.Count <= lookbackWindow)
            {
                // Not enough data to create sequences for LSTM training
                return new List<double>(new double[stepsToPredict]); // Return zeros as fallback
            }

            // 1. Prepare Data (Min-Max Scaling for Neural Network stability)
            double min = historicalData.Min();
            double max = historicalData.Max();
            double range = max - min == 0 ? 1 : max - min; // Prevent division by zero

            var scaledData = historicalData.Select(x => (x - min) / range).ToList();

            // 2. Create Sequences (X) and Targets (Y)
            var (xTrain, yTrain) = CreateSequences(scaledData, lookbackWindow);

            // 3. Build the LSTM Model using Keras API
            var model = BuildLstmModel(lookbackWindow);

            // 4. Train the Model
            model.fit(xTrain, yTrain, batch_size: 1, epochs: 50, verbose: 0);

            // 5. Predict Future Steps iteratively
            var predictions = new List<double>();
            
            // Start with the last known sequence from our historical data
            var currentSequence = scaledData.Skip(scaledData.Count - lookbackWindow).Take(lookbackWindow).ToList();

            for (int i = 0; i < stepsToPredict; i++)
            {
                // Reshape to [1, lookbackWindow, 1] for the model
                var inputNd = np.array(currentSequence.ToArray()).reshape(new Shape(1, lookbackWindow, 1));
                
                // Predict next single value
                var predictionNd = model.predict(inputNd);
                float predictedScaled = predictionNd.numpy().ToArray<float>()[0];

                // Inverse scale the prediction
                double predictedActual = (predictedScaled * range) + min;
                // Ensure no negative sales predictions
                predictions.Add(Math.Max(0, predictedActual));

                // Shift sequence forward (Drop oldest, add new prediction)
                currentSequence.RemoveAt(0);
                currentSequence.Add(predictedScaled);
            }

            return predictions;
        }

        private (NDArray xTrain, NDArray yTrain) CreateSequences(List<double> data, int lookback)
        {
            var xList = new List<float[]>();
            var yList = new List<float>();

            for (int i = 0; i < data.Count - lookback; i++)
            {
                var seq = data.Skip(i).Take(lookback).Select(d => (float)d).ToArray();
                xList.Add(seq);
                yList.Add((float)data[i + lookback]);
            }

            // TensorFlow expects 3D shape for RNNs: [samples, time steps, features]
            var xNd = np.array(xList.ToArray()).reshape(new Shape(xList.Count, lookback, 1));
            var yNd = np.array(yList.ToArray());

            return (xNd, yNd);
        }

        private Tensorflow.Keras.Engine.Sequential BuildLstmModel(int lookback)
        {
            var model = keras.Sequential();

            // Define input layer explicitly
            model.add(keras.Input(shape: new Shape(lookback, 1)));

            // Add LSTM layer
            model.add(keras.layers.LSTM(units: 50));

            // Add Dense output layer (1 neuron for the single next-step prediction)
            model.add(keras.layers.Dense(units: 1));

            model.compile(optimizer: keras.optimizers.Adam(learning_rate: 0.01f), loss: keras.losses.MeanSquaredError());

            return model;
        }
    }
}
