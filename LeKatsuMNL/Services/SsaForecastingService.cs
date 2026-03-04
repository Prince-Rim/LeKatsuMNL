using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeKatsuMNL.Services
{
    public class SsaForecastingService : ISsaForecastingService
    {
        public List<double> ExtractTrend(List<double> timeSeries, int windowLength = 7)
        {
            if (timeSeries == null || timeSeries.Count < windowLength * 2)
            {
                // Not enough data for meaningful SSA. Return original or throw depending on policy.
                // For Capstone MVP, return original if too small, but log a warning.
                return timeSeries ?? new List<double>();
            }

            int N = timeSeries.Count;
            int L = windowLength;
            int K = N - L + 1;

            // 1. Embedding: Create the Trajectory Matrix X
            var X = Matrix<double>.Build.Dense(L, K);
            for (int i = 0; i < L; i++)
            {
                for (int j = 0; j < K; j++)
                {
                    X[i, j] = timeSeries[i + j];
                }
            }

            // 2. Singular Value Decomposition (SVD)
            // Math.NET uses X = U * S * V^T
            var svd = X.Svd(computeVectors: true);
            var U = svd.U;
            var S = svd.W; // Diagonal Matrix of singular values
            var VT = svd.VT;

            // 3. Grouping (Reconstruction from top component representing Trend)
            // For basic trend extraction, we take the 1st principal component (r=1)
            var reconstructedX = Matrix<double>.Build.Dense(L, K);
            for (int i = 0; i < L; i++)
            {
                for (int j = 0; j < K; j++)
                {
                    // Reconstruct using only the first singular value/vector to get the main trend (denoised)
                    reconstructedX[i, j] = S[0, 0] * U[i, 0] * VT[0, j];
                }
            }

            // 4. Diagonal Averaging (Hankelization) to convert matrix back to 1D time series
            var trendSeries = new double[N];
            
            // Reconstruct the 1D array by averaging the anti-diagonals of reconstructedX
            for (int k = 0; k < N; k++)
            {
                double sum = 0;
                int count = 0;

                // k represents the index in the original time series
                for (int m = 0; m < L; m++)
                {
                    int n = k - m;
                    if (n >= 0 && n < K)
                    {
                        sum += reconstructedX[m, n];
                        count++;
                    }
                }
                
                trendSeries[k] = sum / count;
            }

            return trendSeries.ToList();
        }
    }
}
