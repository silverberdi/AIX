# Security and Policy Summary

## Security Model
AIX uses a policy-first security model. RBAC is foundational, but not sufficient.

## Rules
- roles grant capabilities
- policies condition capabilities
- user restrictions may reduce inherited permissions
- deny overrides allow
- authorization is enforced in backend
- frontend only adapts the visual experience

## MVP Scope
- roles
- permissions
- taxonomy permissions
- DocumentType permissions
- user allow/deny overrides

## Policy-Driven Product
Policies may control security, retention, immutability, versioning, search visibility, OCR, processing pipelines, file requirements and automation.
