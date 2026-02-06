# SpaceLens — TASKS (Codex Execution Plan)

This file defines the **only approved implementation tasks** for SpaceLens MVP.

Each task is:
- Sequential
- Strictly scoped
- Independently verifiable

Codex or any agent **must not skip, merge, or reorder tasks**.

---

## TASK A — Solution Scaffold & UI Shell

### Objective
Create the solution, projects, and a functioning WPF UI shell with no real filesystem logic.

### Inputs
- docs/PRD.md
- docs/ARCHITECTURE.md
- docs/DEFINITION_OF_DONE.md
- docs/RULES.md
- README.md (Codex Execution README)

### Instructions
- Create `SpaceLens.sln`
- Create projects:
  - SpaceLens.App (WPF .NET 8)
  - SpaceLens.Core (class library)
  - SpaceLens.Data (class library)
  - SpaceLens.Core.Tests (test project)
- Implement a WPF window containing:
  - Sidebar: Drive picker + Folder picker + Scan button + Snapshot list placeholder
  - Main area: placeholders for Treemap / Tree / Top Files
  - Right-side Inspector panel placeholder
  - Top toolbar with Search and Filters placeholders
- Implement a **fake scan** that simulates progress and produces mock results
- UI must remain responsive during fake scan

### Definition of Done
- `dotnet build` succeeds from repo root
- App launches
- Clicking Scan runs fake scan and updates UI
- No real filesystem access exists

---

## TASK B — Real Filesystem Scanner (Incremental)

### Objective
Replace fake scan with a real incremental filesystem scanner.

### Instructions
- Implement recursive traversal for:
  - Full drives
  - Selected folders
- Scanner must:
  - Run off UI thread
  - Stream results incrementally
  - Handle access-denied paths gracefully
  - Prevent symlink loops
- Capture metadata:
  - Path
  - Size
  - File or folder
  - Extension (files only)
  - Last modified

### Definition of Done
- Scanning a real folder completes without UI freeze
- Tree and Top Files populate with real data
- Folder sizes equal sum of children

---

## TASK C — Snapshot Persistence

### Objective
Persist scan results locally as immutable snapshots.

### Instructions
- Create snapshot storage in SpaceLens.Data
- Store:
  - Snapshot metadata (id, timestamp, root path)
  - File/folder records
- Snapshots must be immutable once created
- App must load existing snapshots on startup

### Definition of Done
- Restarting the app shows previous snapshots
- Loading a snapshot does not rescan filesystem

---

## TASK D — Rule Engine + Inspector Panel

### Objective
Explain *why* files exist and assess risk conservatively.

### Instructions
- Implement deterministic rule engine per docs/RULES.md
- Produce:
  - Category
  - Risk level (Safe / Caution / Do Not Touch)
  - Confidence (Low / Medium / High)
- Populate Inspector panel with:
  - File/folder info
  - Rule explanation
  - Risk and confidence labels
- Unknown items remain Unknown

### Definition of Done
- Selecting any item shows explanation data
- No overconfident or speculative labels exist

---

## TASK E — Review Queue & Recycle Bin Cleanup

### Objective
Enable safe cleanup with explicit user intent.

### Instructions
- Implement Review Queue
- Allow add/remove items
- Display total reclaimable size
- On confirm:
  - Delete items to **Recycle Bin only**
- No permanent delete exists

### Definition of Done
- Deleted test files appear in Windows Recycle Bin
- Files can be restored manually

---

## TASK F — Snapshot Comparison

### Objective
Show how storage usage changes over time.

### Instructions
- Allow user to select Snapshot A and Snapshot B
- Compute and display:
  - Total size delta
  - Fastest-growing folders
  - New large files
  - File type growth

### Definition of Done
- Comparison produces correct deltas
- No rescanning occurs during compare

---

## Global Rules (All Tasks)

- Do not exceed task scope
- Stop immediately when Definition of Done is met
- Do not implement non-goals
- Default to safety
- Produce runnable code at each step

---

## Explicitly Forbidden

- Task skipping
- Task merging
- Feature creep
- Background jobs
- Permanent delete
- Cloud sync

---

End of TASKS.md