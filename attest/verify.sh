#!/bin/bash
#
# Verification Script for Coverage Proof Demo
# This script verifies the integrity of coverage artifacts against the attestation.
#
# Usage: ./verify.sh <path-to-artifacts-directory>
#
# The artifacts directory should contain:
#   - attestation.json
#   - csharp/coverage.cobertura.xml
#   - javascript/coverage.cobertura.xml
#   - javascript/coverage.json
#

set -e

ARTIFACTS_DIR="${1:-.}"

echo "=== Coverage Proof Verification ==="
echo ""

# Check if attestation exists
if [ ! -f "$ARTIFACTS_DIR/attestation.json" ]; then
    echo "ERROR: attestation.json not found in $ARTIFACTS_DIR"
    exit 1
fi

echo "Reading attestation from: $ARTIFACTS_DIR/attestation.json"
echo ""

# Extract expected hashes from attestation
EXPECTED_CSHARP_HASH=$(jq -r '.predicate.coverage_artifacts.csharp.sha256' "$ARTIFACTS_DIR/attestation.json")
EXPECTED_JS_XML_HASH=$(jq -r '.predicate.coverage_artifacts.javascript.files[0].sha256' "$ARTIFACTS_DIR/attestation.json")
EXPECTED_JS_JSON_HASH=$(jq -r '.predicate.coverage_artifacts.javascript.files[1].sha256' "$ARTIFACTS_DIR/attestation.json")

echo "Expected hashes from attestation:"
echo "  C# coverage:         $EXPECTED_CSHARP_HASH"
echo "  JS coverage (XML):   $EXPECTED_JS_XML_HASH"
echo "  JS coverage (JSON):  $EXPECTED_JS_JSON_HASH"
echo ""

# Compute actual hashes
echo "Computing actual hashes..."
ACTUAL_CSHARP_HASH=$(sha256sum "$ARTIFACTS_DIR/csharp/coverage.cobertura.xml" 2>/dev/null | cut -d ' ' -f 1 || echo "FILE_NOT_FOUND")
ACTUAL_JS_XML_HASH=$(sha256sum "$ARTIFACTS_DIR/javascript/coverage.cobertura.xml" 2>/dev/null | cut -d ' ' -f 1 || echo "FILE_NOT_FOUND")
ACTUAL_JS_JSON_HASH=$(sha256sum "$ARTIFACTS_DIR/javascript/coverage.json" 2>/dev/null | cut -d ' ' -f 1 || echo "FILE_NOT_FOUND")

echo "Actual computed hashes:"
echo "  C# coverage:         $ACTUAL_CSHARP_HASH"
echo "  JS coverage (XML):   $ACTUAL_JS_XML_HASH"
echo "  JS coverage (JSON):  $ACTUAL_JS_JSON_HASH"
echo ""

# Verify each hash
VERIFICATION_PASSED=true

echo "=== Verification Results ==="
echo ""

if [ "$EXPECTED_CSHARP_HASH" == "$ACTUAL_CSHARP_HASH" ]; then
    echo "[PASS] C# coverage.cobertura.xml"
else
    echo "[FAIL] C# coverage.cobertura.xml - hash mismatch!"
    VERIFICATION_PASSED=false
fi

if [ "$EXPECTED_JS_XML_HASH" == "$ACTUAL_JS_XML_HASH" ]; then
    echo "[PASS] JavaScript coverage.cobertura.xml"
else
    echo "[FAIL] JavaScript coverage.cobertura.xml - hash mismatch!"
    VERIFICATION_PASSED=false
fi

if [ "$EXPECTED_JS_JSON_HASH" == "$ACTUAL_JS_JSON_HASH" ]; then
    echo "[PASS] JavaScript coverage.json"
else
    echo "[FAIL] JavaScript coverage.json - hash mismatch!"
    VERIFICATION_PASSED=false
fi

echo ""

if [ "$VERIFICATION_PASSED" = true ]; then
    echo "=== ALL VERIFICATIONS PASSED ==="
    echo "The coverage artifacts match the attestation."

    # Display attestation metadata
    echo ""
    echo "Attestation Metadata:"
    echo "  Repository: $(jq -r '.subject.repository' "$ARTIFACTS_DIR/attestation.json")"
    echo "  Commit:     $(jq -r '.subject.commit' "$ARTIFACTS_DIR/attestation.json")"
    echo "  Generated:  $(jq -r '.predicate.generated_at' "$ARTIFACTS_DIR/attestation.json")"
    exit 0
else
    echo "=== VERIFICATION FAILED ==="
    echo "One or more artifacts do not match the attestation."
    echo "This could indicate tampering or corruption."
    exit 1
fi
