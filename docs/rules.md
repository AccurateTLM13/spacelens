# SpaceLens — Rule Engine Heuristics (MVP)

This document defines deterministic rules used to explain storage usage and assess deletion risk.

Rules are conservative by design.
If confidence is low, the item remains **Unknown**.

---

## Output Fields
Each evaluated item produces:
- Category
- Risk Level
- Confidence Level

---

## Categories

### System Cache
**Examples**:
- Windows temp directories
- Update download caches

**Risk**: Safe
**Confidence**: High

---

### App Cache
**Examples**:
- Browser cache directories
- Application cache folders

**Risk**: Safe
**Confidence**: Medium–High

---

### Temporary Files
**Examples**:
- .tmp files
- crash dumps (.dmp)

**Risk**: Safe or Caution (context dependent)
**Confidence**: Medium

---

### Downloads
**Examples**:
- User download folders
- Installers, ISOs, archives

**Risk**: Caution
**Confidence**: High

---

### Media
**Examples**:
- Video, audio, image libraries

**Risk**: Caution
**Confidence**: High

---

### Archives / Backups
**Examples**:
- .zip, .rar, .7z
- Backup folders

**Risk**: Caution
**Confidence**: Medium

---

### Developer Artifacts
**Examples**:
- Build output directories
- Dependency folders

**Risk**: Caution
**Confidence**: Medium

---

### Program Files
**Examples**:
- Installed application directories

**Risk**: Do Not Touch
**Confidence**: High

---

### Operating System
**Examples**:
- Windows system directories

**Risk**: Do Not Touch
**Confidence**: High

---

### Unknown
**Criteria**:
- Does not match any rule

**Risk**: Caution
**Confidence**: Low

---

## Risk Level Definitions

### Safe
- Common cleanup candidates
- Deleting unlikely to impact system stability

### Caution
- User should review before deleting
- May be important depending on context

### Do Not Touch
- Deleting may break system or apps
- Never auto-suggested for cleanup

---

## Confidence Levels

### High
- Strong location and pattern match

### Medium
- Partial pattern match

### Low
- Heuristic guess only

---

## Design Principles
- Never escalate risk downward
- Never claim certainty when unknown
- Default to safety over cleanup

