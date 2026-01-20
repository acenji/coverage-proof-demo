const assert = require('assert');
const { isPrime } = require('./prime');

/**
 * Tests for isPrime that achieve 100% C1 (statement) and C2 (branch) coverage.
 * Each test targets specific branches in the isPrime function.
 */

describe('isPrime', function() {

    // ===========================================
    // Branch 1: n < 2 (not prime)
    // ===========================================

    describe('numbers less than 2', function() {
        it('should return false for negative numbers', function() {
            assert.strictEqual(isPrime(-10), false);
            assert.strictEqual(isPrime(-1), false);
        });

        it('should return false for 0', function() {
            assert.strictEqual(isPrime(0), false);
        });

        it('should return false for 1', function() {
            assert.strictEqual(isPrime(1), false);
        });
    });

    // ===========================================
    // Branch 2: n === 2 (prime)
    // ===========================================

    describe('the number 2', function() {
        it('should return true for 2 (only even prime)', function() {
            assert.strictEqual(isPrime(2), true);
        });
    });

    // ===========================================
    // Branch 3: Even numbers > 2 (not prime)
    // ===========================================

    describe('even numbers greater than 2', function() {
        it('should return false for small even composites', function() {
            assert.strictEqual(isPrime(4), false);
            assert.strictEqual(isPrime(6), false);
        });

        it('should return false for large even numbers', function() {
            assert.strictEqual(isPrime(100), false);
            assert.strictEqual(isPrime(1000), false);
        });
    });

    // ===========================================
    // Branch 4 & 5: Odd composite numbers (loop finds divisor)
    // ===========================================

    describe('odd composite numbers', function() {
        it('should return false for products of small primes', function() {
            assert.strictEqual(isPrime(9), false);   // 3 * 3
            assert.strictEqual(isPrime(15), false);  // 3 * 5
            assert.strictEqual(isPrime(21), false);  // 3 * 7
        });

        it('should return false for perfect squares of primes', function() {
            assert.strictEqual(isPrime(25), false);  // 5 * 5
            assert.strictEqual(isPrime(49), false);  // 7 * 7
            assert.strictEqual(isPrime(81), false);  // 9 * 9
        });
    });

    // ===========================================
    // Branch 4 & 6: Odd prime numbers (loop completes without finding divisor)
    // ===========================================

    describe('odd prime numbers', function() {
        it('should return true for small primes', function() {
            assert.strictEqual(isPrime(3), true);
            assert.strictEqual(isPrime(5), true);
            assert.strictEqual(isPrime(7), true);
        });

        it('should return true for medium primes', function() {
            assert.strictEqual(isPrime(11), true);
            assert.strictEqual(isPrime(13), true);
            assert.strictEqual(isPrime(17), true);
            assert.strictEqual(isPrime(19), true);
            assert.strictEqual(isPrime(23), true);
        });

        it('should return true for larger primes', function() {
            assert.strictEqual(isPrime(97), true);
            assert.strictEqual(isPrime(101), true);
            assert.strictEqual(isPrime(7919), true);  // 1000th prime
        });
    });

    // ===========================================
    // Edge cases for complete coverage
    // ===========================================

    describe('edge cases', function() {
        it('should handle large composite correctly', function() {
            assert.strictEqual(isPrime(7917), false);  // 3 * 2639
        });
    });
});
