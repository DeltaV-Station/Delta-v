#!/usr/bin/env python3

import requests
import os
import subprocess
import re
from typing import Iterable

PUBLISH_TOKEN = os.environ["PUBLISH_TOKEN"]
VERSION = os.environ["GITHUB_SHA"]

RELEASE_DIR = "release"

# CONFIGURATION PARAMETERS
# Forks should change these to publish to their own infrastructure.
ROBUST_CDN_URL = "https://cdn.simplestation.org/"
FORK_ID = "ebengrad"

def main():
    session = requests.Session()
    session.headers = {
        "Authorization": f"Bearer {PUBLISH_TOKEN}",
    }

    print(f"Starting publish on Robust.Cdn for version {VERSION}")

    data = {
        "version": VERSION,
        "engineVersion": get_engine_version(),
    }
    headers = {
        "Content-Type": "application/json"
    }
    resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/start", json=data, headers=headers)
    resp.raise_for_status()
    print("Publish successfully started, adding files...")

    for file in get_files_to_publish():
        print(f"Publishing {file}")
        with open(file, "rb") as f:
            headers = {
                "Content-Type": "application/octet-stream",
                "Robust-Cdn-Publish-File": os.path.basename(file),
                "Robust-Cdn-Publish-Version": VERSION
            }
            resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/file", data=f, headers=headers)

        resp.raise_for_status()

    print("Successfully pushed files, finishing publish...")

    data = {
        "version": VERSION
    }
    headers = {
        "Content-Type": "application/json"
    }
    resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/finish", json=data, headers=headers)
    resp.raise_for_status()

    print("SUCCESS!")


def get_files_to_publish() -> Iterable[str]:
    for file in os.listdir(RELEASE_DIR):
        yield os.path.join(RELEASE_DIR, file)


def get_engine_version() -> str:
    # Regular expression to match version numbers in commit messages in the format `Version: X.Y.Z`
    version_pattern = re.compile(r"Version:\s*(\d+\.\d+\.\d+)")

    try:
        # Try to get the latest tag
        proc = subprocess.run(["git", "describe", "--tags", "--abbrev=0"], stdout=subprocess.PIPE, cwd="RobustToolbox", check=True, encoding="UTF-8")
        tag = proc.stdout.strip()
        if tag.startswith("v"):
            return tag[1:]  # Remove the 'v' prefix if it exists
        return tag
    except subprocess.CalledProcessError:
        # If no tags are found, search the commit history for a version
        print("No tags found; searching commit history for version.")
        log_proc = subprocess.run(
            ["git", "log", "--pretty=%B"],
            stdout=subprocess.PIPE,
            cwd="RobustToolbox",
            check=True,
            encoding="UTF-8"
        )
        
        # Search each commit message for a version number
        for line in log_proc.stdout.splitlines():
            match = version_pattern.search(line)
            if match:
                return match.group(1)  # Return the version number if found

        # If no version is found in commit messages, fall back to commit ID
        print("No version found in commit history; using commit ID as version.")
        id_proc = subprocess.run(["git", "rev-parse", "HEAD"], stdout=subprocess.PIPE, cwd="RobustToolbox", check=True, encoding="UTF-8")
        return id_proc.stdout.strip()


if __name__ == '__main__':
    main()
