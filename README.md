# Provable Coverage Demo

**Cryptographic proof that your tests actually cover your code.**

This repository demonstrates how to create *verifiable*, *tamper-evident* code coverage reports that anyone can independently verify. No screenshots. No trust required. Just math.

---

## Table of Contents

1. [The Problem](#the-problem)
2. [The Solution](#the-solution)
3. [Architecture Overview](#architecture-overview)
4. [Step-by-Step Walkthrough](#step-by-step-walkthrough)
5. [Security Properties](#security-properties)
6. [Third-Party Verification Guide](#third-party-verification-guide)
7. [Repository Structure](#repository-structure)
8. [Running Locally](#running-locally)
9. [Extending This Demo](#extending-this-demo)

---

## The Problem

Traditional coverage reports are **easy to fake**:

```
❌ Screenshot of "100% coverage" → Can be edited in 30 seconds
❌ Badge showing "95% coverage" → Points to self-reported data
❌ CI log saying "All tests pass" → No proof tests actually ran
❌ Coverage XML uploaded → Could be hand-edited before upload
```

**Why this matters:**
- Plugin marketplaces can't verify quality claims
- Auditors must trust developers' word
- No way to prove coverage wasn't gamed

---

## The Solution

Create an **unbreakable chain of evidence**:

```
┌─────────────────────────────────────────────────────────────────┐
│                    PROVABLE COVERAGE CHAIN                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   SOURCE CODE ──► TESTS ──► RAW TRACES ──► REPORTS ──► PROOF   │
│        │            │           │             │           │     │
│     SHA-256      SHA-256     SHA-256      SHA-256     SIGNED    │
│                                                                 │
│   Every step is hashed. Any tampering breaks the chain.        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Architecture Overview

### What Gets Measured

```
┌─────────────────────────────────────────────────────────────────┐
│                         INPUTS (Hashed)                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Source Files │  │ Dependencies │  │   Commands   │          │
│  │              │  │              │  │              │          │
│  │ prime.js     │  │ package-lock │  │ npm test     │          │
│  │ PrimeLib.cs  │  │ *.csproj     │  │ dotnet test  │          │
│  │              │  │              │  │              │          │
│  │   SHA-256    │  │   SHA-256    │  │  Captured    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        OUTPUTS (Hashed)                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Raw Traces  │  │   Reports    │  │ Attestation  │          │
│  │              │  │              │  │              │          │
│  │ .nyc_output  │  │ cobertura.xml│  │    JSON      │          │
│  │ coverlet     │  │ coverage.json│  │   SIGNED     │          │
│  │              │  │              │  │              │          │
│  │   SHA-256    │  │   SHA-256    │  │  Sigstore    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### CI Pipeline Jobs

```
┌─────────────────────────────────────────────────────────────────┐
│                      CI PIPELINE (4 Jobs)                       │
└─────────────────────────────────────────────────────────────────┘

     Job 1                    Job 2
 ┌───────────┐           ┌───────────┐
 │   C#      │           │JavaScript │
 │ Coverage  │           │ Coverage  │
 │           │           │           │
 │ • Hash    │           │ • Hash    │
 │   sources │           │   sources │
 │ • Hash    │           │ • Hash    │
 │   deps    │           │   lock    │
 │ • Run     │           │ • Run     │
 │   tests   │           │   tests   │
 │ • Capture │           │ • Capture │
 │   traces  │           │   traces  │
 └─────┬─────┘           └─────┬─────┘
       │                       │
       └───────────┬───────────┘
                   ▼
            ┌─────────────┐
            │   Job 3     │
            │  VERIFIER   │ ◄── Independent job
            │             │     recomputes coverage
            │ • Download  │     from raw traces
            │   traces    │
            │ • Recompute │
            │   coverage  │
            │ • Compare   │
            │   hashes    │
            └──────┬──────┘
                   ▼
            ┌─────────────┐
            │   Job 4     │
            │ ATTESTATION │
            │             │
            │ • Combine   │
            │   all data  │
            │ • Create    │
            │   JSON      │
            │ • SIGN      │ ◄── Sigstore signature
            └─────────────┘
```

---

## Step-by-Step Walkthrough

### Step 1: Hash the Source Files

**Goal:** Prove exactly which files were measured (prevent silent file dropping)

**Pre-state:**
```
csharp/PrimeLib/PrimeLib/
└── PrimeChecker.cs    ◄── This file should be covered
```

**What happens:**
```bash
# List all source files (excluding tests)
find . -name "*.cs" ! -name "*Test*.cs" | sort > measured-files.txt

# Hash the file list (prevents adding/removing files)
sha256sum measured-files.txt
# Output: a1b2c3d4... (file list hash)

# Hash the actual content (proves exact code version)
cat $(cat measured-files.txt) | sha256sum
# Output: e5f6g7h8... (content hash)
```

**Post-state:**
```json
{
  "source_files": {
    "list_hash": "a1b2c3d4...",
    "content_hash": "e5f6g7h8..."
  }
}
```

**Why this matters:**
```
❌ WITHOUT: Attacker removes untested file from coverage → 100% achieved!
✅ WITH: File list hash changes → Tampering detected
```

---

### Step 2: Hash the Dependencies

**Goal:** Lock coverage results to exact dependency versions

**Pre-state:**
```
js/
├── package.json        ◄── Declares dependencies
└── package-lock.json   ◄── Locks exact versions
```

**What happens:**
```bash
# Hash the lock file (exact dependency tree)
sha256sum package-lock.json
# Output: i9j0k1l2... (lock hash)
```

**Post-state:**
```json
{
  "dependencies": {
    "lockfile_hash": "i9j0k1l2..."
  }
}
```

**Why this matters:**
```
❌ WITHOUT: Attacker uses different NYC version that reports higher coverage
✅ WITH: Lock hash changes → Different dependencies detected
```

---

### Step 3: Run Tests and Capture Raw Traces

**Goal:** Generate unforgeable instrumentation data

**Pre-state:**
```
js/
├── prime.js           ◄── Code to test
└── prime.test.js      ◄── Test file
```

**What happens:**
```bash
# Run tests with coverage (exact command captured)
npm run coverage

# This generates raw instrumentation traces
.nyc_output/
├── abc123.json    ◄── Raw execution data
└── def456.json    ◄── Per-process traces
```

**Post-state:**
```
coverage/
├── cobertura-coverage.xml   ◄── Human-readable report
├── coverage-final.json      ◄── Processed metrics
└── lcov.info                ◄── Alternative format

.nyc_output/                  ◄── RAW TRACES (the real proof)
├── abc123.json
└── processinfo/
```

**Why this matters:**
```
❌ WITHOUT: Attacker edits cobertura.xml to show 100%
✅ WITH: Raw traces allow recomputation → Edited report detected
```

---

### Step 4: Hash Everything

**Goal:** Create tamper-evident fingerprints

**What happens:**
```bash
# Hash processed reports
sha256sum coverage.cobertura.xml > coverage.cobertura.xml.sha256
sha256sum coverage.json > coverage.json.sha256

# Hash raw traces (archive first for single hash)
tar -cf raw-traces.tar .nyc_output/
sha256sum raw-traces.tar > raw-traces.tar.sha256
```

**Post-state:**
```
coverage-output/
├── coverage.cobertura.xml
├── coverage.cobertura.xml.sha256   ◄── m3n4o5p6...
├── coverage.json
├── coverage.json.sha256            ◄── q7r8s9t0...
├── raw-traces.tar
└── raw-traces.tar.sha256           ◄── u1v2w3x4...
```

**Why this matters:**
```
❌ WITHOUT: No way to know if artifacts were modified after generation
✅ WITH: Any byte change → Completely different hash
```

---

### Step 5: Independent Verification (Verifier Job)

**Goal:** Prove reports match raw traces (second opinion)

**Pre-state:**
```
Downloaded from Job 2:
├── coverage.json           ◄── Claims 100% coverage
├── coverage.json.sha256    ◄── Hash of above
└── raw-traces.tar          ◄── Actual execution data
```

**What happens:**
```bash
# Extract raw traces
tar -xf raw-traces.tar

# Recompute coverage INDEPENDENTLY from raw traces
nyc report --temp-dir ./raw-traces --reporter=json --report-dir=./recomputed

# Compare: Do raw traces support the claimed coverage?
sha256sum ./recomputed/coverage-final.json
# Should produce same metrics as original
```

**Post-state:**
```json
{
  "verification": {
    "original_hash": "q7r8s9t0...",
    "recomputed_hash": "q7r8s9t0...",  ◄── MUST MATCH
    "status": "verified"
  }
}
```

**Why this matters:**
```
❌ WITHOUT: Must trust that coverage.json wasn't hand-edited
✅ WITH: Independent recomputation proves data integrity
```

---

### Step 6: Create Signed Attestation

**Goal:** Cryptographically bind everything together

**What happens:**
```bash
# Combine all hashes and metadata into attestation
{
  "subject": {
    "repository": "user/repo",
    "commit": "abc123..."
  },
  "predicate": {
    "source_files": { "hash": "..." },
    "dependencies": { "hash": "..." },
    "coverage": { "hash": "..." },
    "raw_traces": { "hash": "..." },
    "verification": { "status": "verified" }
  }
}

# Sign with Sigstore (GitHub's artifact attestation)
actions/attest-build-provenance@v1
```

**Post-state:**
```
attestation.json          ◄── All data combined
attestation.json.sha256   ◄── Hash of attestation
[Sigstore signature]      ◄── Cryptographic proof from GitHub
```

**Why this matters:**
```
❌ WITHOUT: Attestation could be created by anyone
✅ WITH: Signature proves it came from THIS GitHub Actions run
```

---

## Security Properties

### What Each Property Prevents

| Property | Attack Prevented | How |
|----------|-----------------|-----|
| **Measured file list hash** | Silent file dropping | Can't remove untested file without changing hash |
| **Dependency lock hash** | Tool version manipulation | Can't use different coverage tool version |
| **Commands captured** | Process substitution | Exact commands recorded, can be replayed |
| **Raw traces hashed** | Report editing | Reports must match raw instrumentation data |
| **Verifier job** | Pipeline manipulation | Independent job recomputes from raw data |
| **Signed attestation** | Attestation forgery | Sigstore proves GitHub Actions origin |

### The Cheating Taxonomy

```
┌─────────────────────────────────────────────────────────────────┐
│                    COMMON COVERAGE CHEATS                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  CHEAT                          │  DEFENSE                      │
│  ──────────────────────────────────────────────────────────     │
│  Edit coverage report           │  Raw traces → recompute       │
│  Remove untested files          │  Measured file list hash      │
│  Use lenient tool version       │  Dependency lock hash         │
│  Mock coverage tool             │  Commands captured + verify   │
│  Modify pipeline                │  Signed attestation           │
│  Generate fake attestation      │  Sigstore signature           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Third-Party Verification Guide

### You received a coverage claim. Here's how to verify it.

#### Step 1: Download Artifacts

```bash
# From GitHub Actions → Artifacts → Download
unzip coverage-attestation.zip -d ./verify
cd ./verify
```

#### Step 2: Verify Attestation Signature

```bash
# Verify the Sigstore signature (proves GitHub Actions origin)
gh attestation verify attestation.json --owner REPO_OWNER
```

#### Step 3: Verify File Hashes

```bash
# Recompute hashes and compare
echo "=== Verifying Hashes ==="

# Check coverage report hash
EXPECTED=$(cat csharp-coverage/coverage.cobertura.xml.sha256)
ACTUAL=$(sha256sum csharp-coverage/coverage.cobertura.xml | cut -d ' ' -f 1)
[ "$EXPECTED" = "$ACTUAL" ] && echo "✅ C# report hash matches" || echo "❌ MISMATCH"

# Check raw traces hash
EXPECTED=$(cat javascript-coverage/raw-traces.tar.sha256)
ACTUAL=$(sha256sum javascript-coverage/raw-traces.tar | cut -d ' ' -f 1)
[ "$EXPECTED" = "$ACTUAL" ] && echo "✅ JS raw traces hash matches" || echo "❌ MISMATCH"
```

#### Step 4: Recompute Coverage from Raw Traces

```bash
# Install NYC
npm install -g nyc

# Extract raw traces
cd javascript-coverage
tar -xf raw-traces.tar

# Recompute coverage independently
nyc report --temp-dir ./raw-traces --reporter=text

# You should see the same percentages as claimed
```

#### Step 5: Verify Measured Files

```bash
# Check that all source files were included
cat measured-files.txt

# Compare with actual source files in repo
# (clone the repo at the attested commit)
git clone REPO_URL --branch COMMIT_SHA
find . -name "*.js" ! -name "*.test.js" | sort

# Lists should match
```

### Verification Checklist

```
□ Attestation signature valid (Sigstore)
□ All file hashes match
□ Raw traces recompute to same coverage
□ Measured file list includes all source files
□ Dependency versions match lock file
□ Commit SHA matches claimed commit
```

---

## Repository Structure

```
coverage-proof-demo/
├── .github/workflows/
│   └── coverage-proof.yml      # CI pipeline (4 jobs)
│
├── csharp/
│   ├── coverage.runsettings    # EXCLUSIONS MANIFEST (explicit)
│   └── PrimeLib/
│       ├── PrimeLib/
│       │   └── PrimeChecker.cs # Prime number library
│       └── PrimeLib.Tests/
│           └── PrimeCheckerTests.cs  # xUnit tests
│
├── js/
│   ├── .nycrc.json             # EXCLUSIONS MANIFEST (explicit)
│   ├── package.json
│   ├── package-lock.json       # Locked dependencies
│   ├── prime.js                # Prime number library
│   └── prime.test.js           # Mocha tests
│
├── attest/
│   ├── verify.sh               # Local verification script
│   └── attestation-schema.json # Schema documentation
│
├── README.md                   # This file
└── LICENSE                     # MIT
```

---

## Running Locally

### Prerequisites

- .NET 8.0+ SDK
- Node.js 20+
- npm

### C# Tests

```bash
cd csharp/PrimeLib
dotnet test --collect:"XPlat Code Coverage"
```

### JavaScript Tests

```bash
cd js
npm install
npm run coverage
```

Expected output:
```
----------|---------|----------|---------|---------|
File      | % Stmts | % Branch | % Funcs | % Lines |
----------|---------|----------|---------|---------|
All files |   100   |   100    |   100   |   100   |
 prime.js |   100   |   100    |   100   |   100   |
----------|---------|----------|---------|---------|
```

---

## Extending This Demo

### Add More Languages

The pattern works for any language with coverage tooling:
- **Python**: pytest-cov → .coverage (SQLite) → recompute with `coverage report`
- **Go**: `go test -coverprofile` → cover.out → recompute with `go tool cover`
- **Rust**: cargo-tarpaulin → cobertura.xml → same verification

### Add Mutation Testing

Coverage doesn't prove test quality. Add mutation testing:

```bash
# JavaScript
npm install -g stryker-cli
stryker run

# C#
dotnet tool install -g dotnet-stryker
dotnet stryker
```

Mutation score + coverage = stronger proof.

### Integrate with Plugin Marketplace

```
┌─────────────────────────────────────────────────────────────────┐
│                    MARKETPLACE INTEGRATION                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Plugin Author                  Marketplace                     │
│  ─────────────                  ───────────                     │
│  1. Run CI pipeline             3. Download attestation         │
│  2. Generate attestation        4. Verify signature             │
│                                 5. Recompute coverage           │
│                                 6. Display verified badge       │
│                                                                 │
│           ┌─────────────────────────────┐                       │
│           │  ✅ Verified Coverage: 100% │                       │
│           │  Commit: abc123...          │                       │
│           │  Verified: 2025-01-19       │                       │
│           └─────────────────────────────┘                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Related Work

- [in-toto](https://in-toto.io/) - Software supply chain integrity
- [SLSA](https://slsa.dev/) - Supply chain Levels for Software Artifacts
- [Sigstore](https://sigstore.dev/) - Keyless signing for open source

---

## License

MIT License - See [LICENSE](LICENSE)

## Author

[Ivan Assenov](https://patents.google.com/patent/US12288046B2/) - No-Code Software Development Platform (US Patent 12,288,046)

---

## Summary

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│   Traditional Coverage          Provable Coverage               │
│   ────────────────────          ─────────────────               │
│   "Trust me, it's 100%"    →    "Here's the math"              │
│   Screenshot proof         →    Cryptographic proof             │
│   Edit the report          →    Recompute from traces           │
│   Badge from nowhere       →    Signed attestation              │
│                                                                 │
│   The difference: VERIFIABLE WITHOUT TRUST                      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```
