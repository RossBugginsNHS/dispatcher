#!/bin/bash

set -a # automatically export all variables
source .env
set +a

# Check if the GITHUB_DISPATCHER__PEM environment variable is set
if [ -z "$GITHUB_DISPATCHER__PEM" ]; then
  echo "Error: GITHUB_DISPATCHER__PEM environment variable is not set."
  exit 1
fi

# Check if the file specified in GITHUB_DISPATCHER__PEM exists
if [ ! -f "$GITHUB_DISPATCHER__PEM" ]; then
  echo "Error: File '$GITHUB_DISPATCHER__PEM' does not exist."
  exit 1
fi

# Load the PEM file content into a multiline environment variable
export GITHUB_DISPATCHER__PEMCONTENT=$(<"$GITHUB_DISPATCHER__PEM")

# Print a message to confirm the variable is set
echo "GITHUB_DISPATCHER__PEMCONTENT has been set."

echo $GITHUB_DISPATCHER__PEMCONTENT
