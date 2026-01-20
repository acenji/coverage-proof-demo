namespace PrimeLib;

/// <summary>
/// A simple prime number checker designed to demonstrate C1/C2 code coverage.
/// Each branch is intentionally clear for coverage verification.
/// </summary>
public static class PrimeChecker
{
    /// <summary>
    /// Determines whether a given integer is a prime number.
    /// </summary>
    /// <param name="n">The integer to check.</param>
    /// <returns>True if n is prime; otherwise, false.</returns>
    public static bool IsPrime(int n)
    {
        // Branch 1: Numbers less than 2 are not prime
        if (n < 2)
        {
            return false;
        }

        // Branch 2: 2 is the only even prime
        if (n == 2)
        {
            return true;
        }

        // Branch 3: Even numbers greater than 2 are not prime
        if (n % 2 == 0)
        {
            return false;
        }

        // Branch 4: Check odd divisors up to sqrt(n)
        int limit = (int)Math.Sqrt(n);
        for (int i = 3; i <= limit; i += 2)
        {
            // Branch 5: Found a divisor - not prime
            if (n % i == 0)
            {
                return false;
            }
        }

        // Branch 6: No divisors found - is prime
        return true;
    }
}
