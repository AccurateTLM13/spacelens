# SpaceLens — Definition of Done (MVP)

This document defines when SpaceLens MVP is considered **complete and acceptable**.

---

## Build & Run
- Application builds without errors
- Application launches on Windows
- README contains clear build and run instructions

---

## Scanning
- User can scan:
  - An entire drive
  - A specific folder
- Scan runs incrementally with visible progress
- App remains responsive during scan
- Access-denied paths do not crash the app

---

## Data Accuracy
- File and folder sizes are accurate
- Folder sizes equal the sum of children
- Symlink loops are avoided

---

## Views
- Treemap renders proportional areas correctly
- Tree view shows hierarchical folder structure
- Top Files view lists largest files correctly
- Views update as scan progresses

---

## Inspector Panel
- Selecting any item shows:
  - Name
  - Full path
  - Size
  - Last modified date
  - Category
  - Risk level
  - Confidence level

Unknown items are explicitly labeled as unknown.

---

## Rule Engine
- Categories are assigned deterministically
- Risk labels are conservative
- System-critical locations are never labeled Safe
- No explanation overclaims certainty

---

## Review Queue
- Items can be added and removed
- Total reclaimable size updates correctly
- Cleanup requires explicit confirmation

---

## Cleanup Behavior
- All deletes go to Recycle Bin
- No permanent delete option exists
- Deleted files are recoverable via Recycle Bin

---

## Snapshots
- Snapshot is created per scan
- Snapshots persist after app restart
- Snapshots are immutable

---

## Snapshot Comparison
- User can select two snapshots
- App displays:
  - Total size delta
  - Largest growth folders
  - New large files
  - File type growth

---

## Privacy & Safety
- App works offline
- No data is uploaded
- No background scanning occurs
- No elevated privileges required for normal use

---

## Explicitly Not Required for MVP
- Scheduled scans
- Auto cleanup
- Cloud sync
- AI-based classification
- Background monitoring

