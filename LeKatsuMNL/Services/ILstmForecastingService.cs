using System.Collections.Generic;

namespace LeKatsuMNL.Services
{
    public interface ILstmForecastingService
    {
        /// <summary>
        /// Trains an LSTM model on the provided historical demand (ideally denoised by SSA)
        /// and predicts the future demand for a specified number of steps.
        /// </summary>
        /// <param name="historicalData">The denoised historical sales data</param>
        /// <param name="stepsToPredict">How many future periods to forecast (e.g., 7 days)</param>
        /// <param name="lookbackWindow">How many past periods the LSTM sequences together to learn the pattern</param>
        /// <returns>A list containing the predicted future values</returns>
        List<double> PredictFutureDemand(List<double> historicalData, int stepsToPredict = 7, int lookbackWindow = 3);
    }
}
