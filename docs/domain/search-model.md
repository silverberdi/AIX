# AIX Search Model

## Principle
AIX uses a hybrid search model. Search is a core product differentiator.

## Search Modes
1. Taxonomy navigation
2. Structured search
3. Full-text search
4. Expedient navigation
5. Relation navigation
6. Semantic search
7. AI conversational search

## MVP Search
- structured metadata search
- taxonomy filters
- DocumentType filters
- state filters
- basic full-text when available
- security-filtered results

## Search Visibility
Search visibility is configurable by DocumentType.

Immediate pipeline:
Capture -> Validate -> Store -> Index -> Visible

Deferred pipeline:
Capture -> Store -> OCR -> Extraction -> Classification -> Index -> Visible

## Security Rule
Search results must respect the effective access policies of the user.

## OCR Policy
OCR is configurable by DocumentType or policy and applies mainly to scanned documents, image-based PDFs and physical document capture.
