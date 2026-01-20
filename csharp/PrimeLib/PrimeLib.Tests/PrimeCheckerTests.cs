using Xunit;

namespace PrimeLib.Tests;

/// <summary>
/// Tests for PrimeChecker that achieve 100% C1 (statement) and C2 (branch) coverage.
/// Each test targets specific branches in the IsPrime method.
/// </summary>
public class PrimeCheckerTests
{
    // ===========================================
    // Branch 1: n < 2 (not prime)
    // ===========================================

    [Theory]
    [InlineData(-10)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void IsPrime_NumbersLessThanTwo_ReturnsFalse(int n)
    {
        // Tests Branch 1: Numbers less than 2 are not prime
        Assert.False(PrimeChecker.IsPrime(n));
    }

    // ===========================================
    // Branch 2: n == 2 (prime)
    // ===========================================

    [Fact]
    public void IsPrime_Two_ReturnsTrue()
    {
        // Tests Branch 2: 2 is the only even prime
        Assert.True(PrimeChecker.IsPrime(2));
    }

    // ===========================================
    // Branch 3: Even numbers > 2 (not prime)
    // ===========================================

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(100)]
    [InlineData(1000)]
    public void IsPrime_EvenNumbersGreaterThanTwo_ReturnsFalse(int n)
    {
        // Tests Branch 3: Even numbers greater than 2 are not prime
        Assert.False(PrimeChecker.IsPrime(n));
    }

    // ===========================================
    // Branch 4 & 5: Odd composite numbers (loop finds divisor)
    // ===========================================

    [Theory]
    [InlineData(9)]    // 3 * 3
    [InlineData(15)]   // 3 * 5
    [InlineData(21)]   // 3 * 7
    [InlineData(25)]   // 5 * 5
    [InlineData(49)]   // 7 * 7
    [InlineData(81)]   // 9 * 9
    public void IsPrime_OddCompositeNumbers_ReturnsFalse(int n)
    {
        // Tests Branch 4 (loop entry) and Branch 5 (divisor found)
        Assert.False(PrimeChecker.IsPrime(n));
    }

    // ===========================================
    // Branch 4 & 6: Odd prime numbers (loop completes without finding divisor)
    // ===========================================

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(11)]
    [InlineData(13)]
    [InlineData(17)]
    [InlineData(19)]
    [InlineData(23)]
    [InlineData(97)]
    [InlineData(101)]
    public void IsPrime_OddPrimeNumbers_ReturnsTrue(int n)
    {
        // Tests Branch 4 (loop entry/exit) and Branch 6 (no divisors found)
        Assert.True(PrimeChecker.IsPrime(n));
    }

    // ===========================================
    // Edge cases for complete coverage
    // ===========================================

    [Fact]
    public void IsPrime_Three_ReturnsTrue()
    {
        // Edge case: smallest odd prime (loop doesn't execute because sqrt(3) < 3)
        Assert.True(PrimeChecker.IsPrime(3));
    }

    [Fact]
    public void IsPrime_LargePrime_ReturnsTrue()
    {
        // Tests with a larger prime to ensure loop works correctly
        Assert.True(PrimeChecker.IsPrime(7919)); // 1000th prime
    }

    [Fact]
    public void IsPrime_LargeComposite_ReturnsFalse()
    {
        // Tests with a larger composite number
        Assert.False(PrimeChecker.IsPrime(7917)); // 3 * 2639
    }
}
