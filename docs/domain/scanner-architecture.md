# Scanner Architecture

## Principle

Scanner integrations are isolated from the document governance core.

## Architecture

Scanner Bridge
↓
Scanner Service
↓
AIX Backend API

## Scanner Bridge

The bridge:
- runs close to scanner hardware
- captures images/PDFs
- communicates with Scanner Service
- does not contain governance logic

## Scanner Service

The service:
- validates device/session
- receives scanned files
- applies technical validations
- uploads documents/files
- emits technical events

## AIX Backend

The backend:
- creates documents
- associates files
- applies policies
- governs evidence

## MVP Recommendation

The MVP should prioritize:
- upload endpoints
- watch folders
- bridge-based scanning

Direct TWAIN/WIA integrations may arrive later.
