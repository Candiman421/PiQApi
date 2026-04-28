// CertApi.Abstractions/Utilities/Randomization/ICertRandomProvider.cs
namespace CertApi.Abstractions.Utilities.Randomization;

/// <summary>
/// Provides an abstraction for random number generation to improve testability
/// </summary>
public interface ICertRandomProvider
{
    /// <summary>
    /// Returns a non-negative random integer
    /// </summary>
    int NextInt();

    /// <summary>
    /// Returns a non-negative random integer less than the specified maximum
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound</param>
    int NextIntUnderMax(int maxValue);

    /// <summary>
    /// Returns a random integer within the specified range
    /// </summary>
    /// <param name="minValue">The inclusive lower bound</param>
    /// <param name="maxValue">The exclusive upper bound</param>
    int NextIntInRange(int minValue, int maxValue);

    /// <summary>
    /// Fills the elements of a specified array of bytes with random numbers
    /// </summary>
    /// <param name="buffer">The array to be filled with random numbers</param>
    void NextBytes(byte[] buffer);

    /// <summary>
    /// Returns a random floating-point number between 0.0 and 1.0
    /// </summary>
    double NextDouble();
}
