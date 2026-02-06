SpaceLens --- Product Requirements Document (MVP)
===============================================

Product Summary
---------------

**SpaceLens** is a privacy-first Windows desktop app that shows users exactly how their storage is being used, explains *why* files exist, and helps them safely reclaim space without risking system damage.

**Core promise:**

> "See what's using your disk, understand why it's there, and clean it safely."

* * * * *

MVP Constraints (Locked)
------------------------

-   Scan **entire drives and individual folders**

-   All deletes go to **Recycle Bin only**

-   **Manual snapshots only** (no background jobs, no scheduling)

-   Offline-first, no telemetry

* * * * *

Primary User Flows
------------------

### Flow 1: Scan a Drive or Folder

1.  User selects:

    -   Drive (C:, D:, external)

    -   OR specific folder path

2.  User clicks **Scan**

3.  App begins scan:

    -   UI renders incrementally

    -   Partial results visible during scan

4.  Results persist locally as a snapshot

* * * * *

### Flow 2: Investigate Storage Usage

1.  User explores results via:

    -   Treemap view

    -   Tree (folder hierarchy)

    -   Top files list

2.  User clicks any file/folder

3.  Inspector panel explains:

    -   What it is

    -   Why it exists (rule-based)

    -   Risk level

    -   Actions

* * * * *

### Flow 3: Safe Cleanup

1.  User selects items

2.  Items go into **Review Queue**

3.  User confirms cleanup

4.  Files move to Recycle Bin

5.  Snapshot remains unchanged (history integrity)

* * * * *

### Flow 4: Compare Snapshots

1.  User selects two snapshots

2.  App shows:

    -   Total size delta

    -   Fastest-growing folders

    -   New large files

    -   File type growth

* * * * *

Core Features
-------------

### 1\. Scanner

-   Recursive traversal

-   Drive or folder root

-   Captures:

    -   Path

    -   Size

    -   File vs folder

    -   Extension

    -   Last modified

-   Handles:

    -   Access denied gracefully

    -   Symlinks safely (no loops)

* * * * *

### 2\. Views

#### A. Treemap View (Primary)

-   Area = size

-   Color by:

    -   File type

    -   Category (optional toggle)

-   Hover tooltip:

    -   Path

    -   Size

    -   % of scan root

#### B. Tree View

-   Expand/collapse folders

-   Sortable by:

    -   Size

    -   Name

    -   Last modified

#### C. Top Files

-   Flat list of largest files

-   Filters apply globally

* * * * *

### 3\. Inspector Panel (Right Side)

When an item is selected:

**Info**

-   Name

-   Full path (copyable)

-   Size

-   Last modified

-   File type / folder

**Explanation**

-   Category (derived)

-   "Why this exists" summary

-   Confidence level (Low / Medium / High)

**Risk Label**

-   Safe

-   Caution

-   Do Not Touch

**Actions**

-   Add to Review Queue

-   Open location

-   Move to another drive

-   Delete (Recycle Bin only)

* * * * *

### 4\. "Why" Rule Engine (MVP)

Deterministic, conservative rules only.

**Categories**

-   System cache

-   App cache

-   Temp files

-   Downloads

-   Media

-   Archives / backups

-   Developer artifacts

-   Unknown

**Risk Mapping**

-   Safe: caches, temp, duplicate downloads

-   Caution: logs, dumps, large archives

-   Do Not Touch: OS directories, active program files

**Confidence**

-   Based on rule match strength + location certainty

> No AI claims. No guessing. If unknown → label unknown.

* * * * *

### 5\. Review Queue

-   Persistent list per scan

-   Shows total reclaimable size

-   Requires explicit confirmation

-   Clear queue after action

* * * * *

### 6\. Snapshots

-   Created after each scan

-   Stored locally

-   Immutable

-   Metadata:

    -   Root path

    -   Timestamp

    -   Total size

**Compare View**

-   Snapshot A vs Snapshot B

-   Highlights growth and new offenders

* * * * *

UX Structure
------------

### Main Layout

`------------------------------------------------
| Toolbar: Scan | Compare | Filters | Search  |
------------------------------------------------
| Sidebar | Treemap / Tree / Top Files | Info |
|         |                             Panel |
------------------------------------------------`

### Sidebar

-   Scan root selector

-   Snapshot history list

### Toolbar

-   Scan

-   Compare snapshots

-   View toggle

-   Filters

* * * * *

Non-Goals (Explicitly Out)
--------------------------

-   Auto cleanup

-   Scheduled scans

-   Cloud sync

-   Background monitoring

-   System file deletion

-   "One-click optimize" nonsense

* * * * *

Technical Notes (Implementation-Agnostic)
-----------------------------------------

-   Incremental scan updates required

-   UI must remain responsive on large drives

-   Local storage only (SQLite or equivalent)

-   Recycle Bin integration mandatory

-   No elevated privileges in MVP unless required for read access

* * * * *

Acceptance Criteria (MVP Done Means)
------------------------------------

-   Can scan any drive or folder

-   User can visually identify largest storage consumers

-   Every delete goes to Recycle Bin

-   "Why" explanations never overclaim

-   Snapshots can be compared reliably

-   App never deletes system-critical files by suggestion
