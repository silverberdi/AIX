# Document Origin and Processing Pipelines

## Principle
Every document should have an origin_type.

## Origin Types
- DIGITAL_NATIVE
- SCANNED
- EMAIL_CAPTURED
- ERP_GENERATED
- API_RECEIVED
- SYSTEM_GENERATED

## Pipelines
### Scan Pipeline
scan -> store -> OCR -> extraction -> validation -> indexing

### Digital Upload Pipeline
upload -> validate -> store -> index

### ERP Pipeline
integration -> metadata mapping -> store evidence -> index

### E-Form Pipeline
render -> capture -> validate -> persist -> generate optional PDF -> index

Pipeline behavior is policy-driven.
