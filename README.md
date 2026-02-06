# SpaceLens — Codex Execution README (MVP)

This repository defines the authoritative scope, constraints, and rules for building **SpaceLens**, a Windows desktop storage analysis tool.

This document is written for **Codex and AI agents**.  
All instructions below are **binding**.

---

## Product Definition

SpaceLens is a **Windows-only** desktop application that:
- Scans drives and folders
- Reports disk usage accurately
- Explains *why* files exist using deterministic rules
- Allows **safe cleanup via Recycle Bin only**

---

## Hard Constraints (Do Not Violate)

These constraints are locked for MVP:

1. Scan **drives and folders**
2. **Recycle Bin only** deletes  
   - No permanent delete
   - No bypass
3. **Manual snapshots only**
   - No background jobs
   - No scheduled scans
4. Offline-only
   - No telemetry
   - No cloud
   - No accounts
5. Windows only
6. .NET 8 + WPF only

If any constraint is unclear, default to **not implementing** the feature.

---

## Source of Truth

The following files are authoritative and must be followed exactly:

- `docs/PRD.md`
- `docs/ARCHITECTURE.md`
- `docs/DEFINITION_OF_DONE.md`
- `docs/RULES.md`

If instructions conflict, resolve in this order:
1. DEFINITION_OF_DONE.md
2. RULES.md
3. ARCHITECTURE.md
4. PRD.md
5. This README

---

## Build Environment

Target platform:
- Windows 10 or 11
- .NET 8 SDK
- WPF (MVVM)

---

## Required Project Structure

The solution **must** follow this structure:
spacelens/
  docs/
    PRD.md
    ARCHITECTURE.md
    DEFINITION_OF_DONE.md
    RULES.md
    Repo-layout.md
  src/
    SpaceLens.App/        # WPF UI (.NET 8)
    SpaceLens.Core/       # Scanner, rules, comparison logic
    SpaceLens.Data/       # Local persistence
  tests/
    SpaceLens.Core.Tests/
  SpaceLens.sln

Do not collapse projects or merge responsibilities.

---

## Scanner Requirements

- Recursive filesystem traversal
- Incremental results streamed to UI
- Folder sizes aggregate progressively
- Access denied paths handled gracefully
- Symlink loops prevented
- UI must remain responsive at all times

---

## Rule Engine Requirements

- Deterministic rules only
- Conservative classification
- Unknown items remain **Unknown**
- Never escalate risk downward
- Never claim certainty without confidence

Rules are defined in `docs/RULES.md`.  
Do not invent new categories.

---

## Cleanup Requirements

- Cleanup actions must be explicit
- Items must enter a Review Queue
- User confirmation required
- All deletes go to Recycle Bin
- No permanent delete functionality exists in MVP

---

## Snapshot Requirements

- One snapshot per scan
- Snapshots are immutable
- Stored locally only
- App must load snapshots without rescanning
- Manual snapshot creation only

---

## Definition of Done

A task is **not complete** unless:
- The app builds successfully
- The app runs
- Behavior matches `docs/DEFINITION_OF_DONE.md`
- No MVP constraints are violated

Partial implementations are not acceptable.

---

## Explicit Non-Goals

The following must NOT be implemented:

- Scheduled scans
- Auto cleanup
- One-click optimize
- Background monitoring
- AI-based classification
- Cloud sync
- User accounts
- Permanent delete

---

## Codex Execution Rules

When executing tasks:
- Read all `/docs` files first
- Implement only the requested task
- Stop when the task’s Definition of Done is met
- Do not add speculative features
- Prove work by producing a runnable build

If a requirement is ambiguous, stop and ask for clarification.

---

End of instructions.


