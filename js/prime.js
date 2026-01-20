/**
 * A simple prime number checker designed to demonstrate C1/C2 code coverage.
 * Mirror implementation of the C# PrimeChecker for multi-language coverage demo.
 */

/**
 * Determines whether a given integer is a prime number.
 * @param {number} n - The integer to check.
 * @returns {boolean} True if n is prime; otherwise, false.
 */
function isPrime(n) {
    // Branch 1: Numbers less than 2 are not prime
    if (n < 2) {
        return false;
    }

    // Branch 2: 2 is the only even prime
    if (n === 2) {
        return true;
    }

    // Branch 3: Even numbers greater than 2 are not prime
    if (n % 2 === 0) {
        return false;
    }

    // Branch 4: Check odd divisors up to sqrt(n)
    const limit = Math.floor(Math.sqrt(n));
    for (let i = 3; i <= limit; i += 2) {
        // Branch 5: Found a divisor - not prime
        if (n % i === 0) {
            return false;
        }
    }

    // Branch 6: No divisors found - is prime
    return true;
}

module.exports = { isPrime };
