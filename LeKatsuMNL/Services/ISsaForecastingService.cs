using System.Collections.Generic;

namespace LeKatsuMNL.Services
{
    public interface ISsaForecastingService
    {
        /// <summary>
        /// Performs Singular Spectrum Analysis (SSA) on a given time series.
        /// Extracts the underlying trend and filters out noise.
        /// </summary>
        /// <param name="timeSeries">The raw sales or demand data</param>
        /// <param name="windowLength">The window length (L) for embedding</param>
        /// <returns>A new list containing the smoothed/reconstructed trend data</returns>
        List<double> ExtractTrend(List<double> timeSeries, int windowLength = 7);
    }
}
