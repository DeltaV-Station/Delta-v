// Dependencies
import fs from "node:fs/promises";
import path from "node:path";
import { exit } from "node:process";
import yaml from "js-yaml";

const MAIN_CATEGORY = "Main";

const PartialHeaderRegex = /^\s*(?::cl:|🆑) *.*/im; // :cl: or 🆑 [0] followed by optional author name [1]
const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9_\- ,&]+)?\s*$/im; // :cl: or 🆑 [0] followed by optional author name [1]
const ChangeRegex =
    /^\s*[\*\-]? *(add|remove|tweak|fix|bug|bugfix): *([^\n\r]+)\r?$/i; // * or - followed by change type [0] and change message [1]
const CategoryRegex = /^\s*([a-z]+):\s*$/gi;
const CommentRegex = /(?<!\\)<!--([^>]+)(?<!\\)-->/gs; // HTML comments

/**
 * @param {Parameters<typeof console.error>} optionalParams
 * @returns {ReturnType<typeof exit>}
 */
function exitError(...optionalParams) {
    if (optionalParams.length > 0) {
        console.error(...optionalParams);
    }
    exit(1);
}

if (process.env.GITHUB_REPOSITORY === undefined) {
    exitError("GITHUB_REPOSITORY not set");
}
if (process.env.PR_NUMBER === undefined) {
    exitError("PR_NUMBER not set");
}
if (process.env.CHANGELOG_DIR === undefined) {
    exitError("CHANGELOG_DIR not set");
}
const changelogDir = path.resolve(
    path.join("../../", process.env.CHANGELOG_DIR),
);
try {
    const stat = await fs.stat(changelogDir);
    if (!stat.isDirectory()) {
        exitError("CHANGELOG_DIR is not a directory", changelogDir);
    }
} catch {
    exitError("CHANGELOG_DIR is missing", changelogDir);
}
const changelogFilePrefix = process.env.CHANGELOG_FILE_PREFIX ?? "";
const maxEntries = process.env.CHANGELOG_MAX_ENTRIES ? Number.parseInt(process.env.CHANGELOG_MAX_ENTRIES, 10) : undefined;

/**
 * @param {string} entryType
 * @returns {string|undefined}
 */
function normalizeEntryType(entryType) {
    switch (entryType.toLowerCase()) {
        case "add":
            return "add";
        case "remove":
            return "remove";
        case "bug":
        case "fix":
        case "bugfix":
            return "fix";
        case "tweak":
            return "tweak";
        default:
            return undefined;
    }
}

/**
 * @param {string} body
 * @param {string} user
 */
async function parsePRBody(body, user) {
    const allCategories = new Set([MAIN_CATEGORY]);
    if (process.env.CHANGELOG_EXTRA_CATEGORIES) {
        for (const category of process.env.CHANGELOG_EXTRA_CATEGORIES.split(",")) {
            allCategories.add(category);
        }
    }

    // Get author
    const partialHeader = PartialHeaderRegex.test(body);
    if (!partialHeader) {
        console.log("No changelog entry found, skipping", PartialHeaderRegex, body);
        exit(0);
    }

    const headerMatch = HeaderRegex.exec(body);
    if (!headerMatch) {
        exitError("Header found, but couldn't be parsed");
    }

    let author = headerMatch[1];
    if (!author) {
        console.log("No author found, setting it to author of the PR");
        author = user;
    }

    body = body.substring(headerMatch.index + headerMatch[0].length);

    /** @type {{[category: string]: {type: string, message: string}[]}} */
    let currentCategory = MAIN_CATEGORY;
    const changes = { [currentCategory]: [] };

    for (const line of body.split(/\r?\n/g)) {
        const categoryMatch = CategoryRegex.exec(line);
        if (categoryMatch) {
            if (allCategories.has(categoryMatch[1])) {
                currentCategory = categoryMatch[1];
            }
            changes[currentCategory] ??= [];
            continue;
        }

        const entryMatch = ChangeRegex.exec(line);
        if (!entryMatch) {
            continue
        };

        const type = normalizeEntryType(entryMatch[1]);
        if (!type) continue;
        const message = entryMatch[2].trim();

        changes[currentCategory].push({ message, type, });
    }
    return {
        author,
        changes,
    };
}

const url = `https://api.github.com/repos/${process.env.GITHUB_REPOSITORY}/pulls/${process.env.PR_NUMBER}`;
console.debug("fetching", url);

// Get PR details
// Use GitHub token if available
const pr = await fetch(url, { headers: { Authorization: process.env.GITHUB_TOKEN ? `Bearer ${process.env.GITHUB_TOKEN}` : undefined } });
if (pr.status !== 200) {
    exitError(pr.status, pr.statusText, await pr?.text());
}

const { merged_at, body, user } = await pr.json();

if (!merged_at) {
    console.log("PR not merged, skipping")
    exit(0);
}

// Remove comments from the body
const commentlessBody = body.replace(CommentRegex, "");

const changelogData = await parsePRBody(commentlessBody, user.login);

const author = changelogData.author;
for (const [category, changes] of Object.entries(changelogData.changes)) {
    const fileName = path.join(
        changelogDir,
        changelogFilePrefix +
        (category === MAIN_CATEGORY ? "Changelog" : (category[0].toUpperCase() +
            category.toLowerCase().substring(1))) +
        ".yml",
    );

    const changelogData =
        yaml.load(await fs.readFile(fileName), {
            filename: fileName,
            json: false,
        }) ?? {};
    changelogData.Entries.push({
        author,
        changes,
        id: (changelogData.Entries?.at(-1)?.id ?? 0) + 1,
        time: merged_at.replace(/z$/i, ".0000000+00:00"),
        url: `https://github.com/${process.env.GITHUB_REPOSITORY}/pull/${process.env.PR_NUMBER}`
    });
    if (maxEntries)
        changelogData.Entries = changelogData.Entries.slice(-maxEntries)

    const newChangelog = yaml.dump(changelogData, {
        indent: 2,
        noArrayIndent: true,
        noRefs: true,
        lineWidth: 90
    });

    await fs.writeFile(fileName, newChangelog);
}
