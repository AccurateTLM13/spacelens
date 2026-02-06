# SpaceLens — Architecture Overview

## Overview
SpaceLens is a Windows desktop application designed to analyze disk usage, explain why files exist, and enable safe cleanup. The architecture is modular, offline-first, and optimized for incremental scanning and responsive UI updates.

---

## High-Level Modules

### 1. Scanner Module
**Responsibility**: Traverse file systems and collect raw storage data.

**Key Functions**:
- Recursive traversal of drives or selected folders
- Incremental progress reporting
- Graceful handling of access-denied paths
- Symlink detection and loop prevention

**Outputs**:
- Stream of file/folder records:
  - path
  - size (bytes)
  - is_file / is_folder
  - extension (if file)
  - last_modified

---

### 2. Snapshot & Persistence Module
**Responsibility**: Store immutable scan results locally.

**Characteristics**:
- One snapshot per scan
- Snapshots are never mutated after creation
- Local-only storage (no sync, no cloud)

**Stored Data**:
- Snapshot metadata (id, timestamp, root path)
- File/folder records
- Derived attributes (category, risk, confidence)

---

### 3. Rule Engine Module
**Responsibility**: Explain *why* files exist and assess deletion risk.

**Inputs**:
- File/folder metadata
- Path patterns
- Known system/app locations

**Outputs**:
- Category
- Risk level (Safe / Caution / Do Not Touch)
- Confidence level (Low / Medium / High)

Rules are deterministic and conservative. Unknowns remain unknown.

---

### 4. View Layer
**Responsibility**: Visualize scan data and enable exploration.

**Primary Views**:
- Treemap view (area proportional to size)
- Tree view (hierarchical folders)
- Top files view (largest files)

**Shared Features**:
- Global filters (size, date, type)
- Search
- Incremental rendering

---

### 5. Inspector Panel
**Responsibility**: Provide detailed context for selected items.

**Displays**:
- File/folder info
- Explanation from Rule Engine
- Risk & confidence labels
- Available actions

---

### 6. Review Queue Module
**Responsibility**: Stage cleanup actions safely.

**Behavior**:
- Items must be explicitly added
- Shows total reclaimable space
- Supports remove-before-confirm

---

### 7. Cleanup / Recycle Module
**Responsibility**: Perform deletion actions safely.

**Constraints**:
- All deletions go to Recycle Bin
- No permanent delete in MVP
- No system-file auto-suggestions

---

### 8. Snapshot Compare Module
**Responsibility**: Analyze changes over time.

**Capabilities**:
- Compare two snapshots
- Identify growth deltas
- Highlight new large files
- Aggregate file-type growth

---

## Cross-Cutting Concerns
- Performance: incremental scanning + UI updates
- Safety: conservative rules, no elevated actions
- Privacy: offline-only, no telemetry

---

## Non-Goals (By Design)
- Background monitoring
- Scheduled scans
- Auto cleanup
- Cloud sync

