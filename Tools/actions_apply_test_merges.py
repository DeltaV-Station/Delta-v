#!/usr/bin/env python3

"""
Applies test merge pull requests to the build.
Explodes violently if it fails to do so.
"""

import datetime
import os
import subprocess
import sys

import requests
import yaml

GITHUB_TOKEN = os.environ["GITHUB_TOKEN"]
GITHUB_REPOSITORY = os.environ["GITHUB_REPOSITORY"]
GITHUB_REF_NAME = os.environ["GITHUB_REF_NAME"]
GITHUB_API_URL = os.environ.get("GITHUB_API_URL", "https://api.github.com")

TEST_MERGE_LABEL = "Test Merge"
CHANGELOG_FILE = "Resources/Changelog/DeltaVTestMerges.yml"

session = requests.Session()
session.headers["Authorization"] = f"Bearer {GITHUB_TOKEN}"
session.headers["Accept"] = "application/vnd.github+json"


def get_test_merge_prs():
    prs = []
    url = f"{GITHUB_API_URL}/repos/{GITHUB_REPOSITORY}/issues"
    params = {"labels": TEST_MERGE_LABEL, "state": "open", "per_page": 100}

    while url:
        resp = session.get(url, params=params)
        resp.raise_for_status()
        for item in resp.json():
            if "pull_request" in item:
                prs.append(item)

        url = resp.links.get("next", {}).get("url")
        params = None

    prs.sort(key=lambda item: item["number"])
    return prs


def run(*args):
    return subprocess.run(args, check=False)


def unshallow():
    is_shallow = subprocess.run(
        ["git", "rev-parse", "--is-shallow-repository"],
        check=True, capture_output=True, text=True,
    ).stdout.strip()

    if is_shallow != "true":
        return

    print("Repository is shallow, fetching full (blobless) history")
    result = run(
        "git", "-c", "submodule.recurse=false",
        "fetch", "--unshallow", "--filter=blob:none",
        "origin", GITHUB_REF_NAME,
    )
    if result.returncode != 0:
        print("::error::Failed to unshallow repository")
        sys.exit(1)


def apply_test_merge(pr):
    number = pr["number"]
    branch = f"tm-{number}"

    fetch = run(
        "git", "-c", "submodule.recurse=false",
        "fetch", "origin", f"pull/{number}/head:{branch}",
    )
    if fetch.returncode != 0:
        print(f"::error::Failed to fetch PR #{number} ({pr['html_url']})")
        sys.exit(1)

    merge = run(
        "git", "-c", "submodule.recurse=false",
        "merge", "--no-ff", "--no-edit", branch,
    )
    if merge.returncode != 0:
        print(f"::error::Failed to merge PR #{number} ({pr['html_url']}) - conflicts?")
        if os.path.exists(".git/MERGE_HEAD"):
            run("git", "merge", "--abort")
        sys.exit(1)


def write_changelog(prs):
    now = datetime.datetime.now(datetime.timezone.utc).isoformat()

    entries = [
        {
            "id": pr["number"],
            "author": pr["user"]["login"],
            "time": now,
            "url": pr["html_url"],
            "changes": [{"type": "Add", "message": pr["title"]}],
        }
        for pr in prs
    ]

    data = {"Name": "Test Merges", "Order": 99, "Entries": entries}

    with open(CHANGELOG_FILE, "w", encoding="utf-8-sig") as f:
        yaml.safe_dump(data, f, sort_keys=False)


def main():
    run("git", "config", "user.name", "Test Merge Bot")
    run("git", "config", "user.email", "action@github.com")

    prs = get_test_merge_prs()
    print(f"Found {len(prs)} open PR(s) labelled '{TEST_MERGE_LABEL}'")

    if prs:
        unshallow()

    for pr in prs:
        print(f"Applying PR #{pr['number']}: {pr['title']} ({pr['html_url']})")
        apply_test_merge(pr)

    write_changelog(prs)


main()
