# Modularization Canon

AIX is organized around bounded contexts and business capabilities.

AIX is an Event-Driven Modular Monolith.

Rules:
- every bounded context starts with `Domain/`, `Application/`, `Infrastructure/`, `Contracts/`, and `Events/`; deeper folders are created only when there is real code requiring them
- no root organization by technical layers (no global `AIX.Application`)
- no internal HTTP between modules
- communication through events/contracts
- shared kernel must remain minimal
- one database per tenant
- bounded contexts own their tables and their `Infrastructure/` folder
- `AIX.Infrastructure` is only for composition/bootstrap and shared infrastructure adapters
- architecture must support future microservice extraction
