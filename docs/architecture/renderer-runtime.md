# AIX Renderer Runtime Canon

## Principle
The renderer is declarative-first.

Angular is the first runtime implementation, not the conceptual owner of the rendering model.

## Architecture
Declarative Schema -> Angular Renderer -> Future Renderers / Microfrontends / Other UI runtimes

## MVP Runtime
- Angular 21
- PrimeNG Avalon
- Signals-first
- Contract-driven renderer

## Stable Contracts
- DocumentSchema
- LayoutSchema
- FieldSchema
- RuleSchema
- FileRequirementSchema

## Layout-Driven Rendering
Composition defines what data exists.
Layout defines how data is presented.

## Default Layout Generation
When a DocumentType is created or edited, AIX generates a default layout based on standalone keywords, groups, tables and file requirements.

## Fixed Field Catalog
- TEXT
- TEXTAREA
- NUMBER
- DECIMAL
- DATE
- DATETIME
- BOOLEAN
- SELECT
- MULTISELECT
- TABLE
- FILE
- RICH_TEXT

## Boundary
The renderer performs rendering, UX validation, state handling and payload assembly.
The backend performs business validation, policy validation, security and persistence.

## Modes
MVP: CAPTURE_MODE.
Future: READ_MODE, COMPARE_MODE, AUDIT_MODE, APPROVAL_MODE.
