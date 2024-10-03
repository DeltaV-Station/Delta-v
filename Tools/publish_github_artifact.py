#!/usr/bin/env python3

import requests
import os
import subprocess
import re

GITHUB_TOKEN = os.environ["GITHUB_TOKEN"]
PUBLISH_TOKEN = os.environ["PUBLISH_TOKEN"]
ARTIFACT_ID = os.environ["ARTIFACT_ID"]
GITHUB_REPOSITORY = os.environ["GITHUB_REPOSITORY"]
VERSION = os.environ['GITHUB_SHA']

#
# CONFIGURATION PARAMETERS
# Forks should change these to publish to their own infrastructure.
#
ROBUST_CDN_URL = "https://cdn.networkgamez.com/"
FORK_ID = "ebengrad"

def main():
    print("Fetching artifact URL from API...")
    artifact_url = get_artifact_url()
    print(f"Artifact URL is {artifact_url}, publishing to Robust.Cdn")

    data = {
        "version": VERSION,
        "engineVersion": get_engine_version(),
        "archive": artifact_url
    }
    headers = {
        "Authorization": f"Bearer {PUBLISH_TOKEN}",
        "Content-Type": "application/json"
    }
    resp = requests.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish", json=data, headers=headers)
    resp.raise_for_status()
    print("Publish succeeded!")

def get_artifact_url() -> str:
    headers = {
        "Authorization": f"Bearer {GITHUB_TOKEN}",
        "X-GitHub-Api-Version": "2022-11-28"
    }
    resp = requests.get(f"https://api.github.com/repos/{GITHUB_REPOSITORY}/actions/artifacts/{ARTIFACT_ID}/zip", allow_redirects=False, headers=headers)
    resp.raise_for_status()

    return resp.headers["Location"]

def get_engine_version() -> str:
    try:
        # Try to get the latest tag
        proc = subprocess.run(["git", "describe", "--tags", "--abbrev=0"], 
                              stdout=subprocess.PIPE, cwd="RobustToolbox", check=True, encoding="UTF-8")
        tag = proc.stdout.strip()
        assert tag.startswith("v")
        return tag[1:]  # Return tag without 'v'
    except subprocess.CalledProcessError:
        # If no tag is found, check the latest commit message for a version number
        proc = subprocess.run(["git", "log", "-1", "--pretty=%B"], 
                              stdout=subprocess.PIPE, cwd="RobustToolbox", check=True, encoding="UTF-8")
        commit_message = proc.stdout.strip()
        # Use regex to find "Version: X.X.X" in the commit message
        version_match = re.search(r"Version:\s*([\d\.]+)", commit_message)
        if version_match:
            return version_match.group(1)  # Return the version found in the commit message
        else:
            # Fallback to returning the commit hash if no version is found
            proc = subprocess.run(["git", "rev-parse", "HEAD"], 
                                  stdout=subprocess.PIPE, cwd="RobustToolbox", check=True, encoding="UTF-8")
            return proc.stdout.strip()

if __name__ == '__main__':
    main()
