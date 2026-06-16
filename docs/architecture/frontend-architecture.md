# Frontend Architecture

## Stack

- Angular 21
- Nx
- PrimeNG Avalon
- Signals
- Standalone Components

## Principles

- Signals-first
- Dumb components
- Feature-based architecture
- Lazy loading
- No business logic inside UI components

## Structure (current)

apps/
  aix-ui/
  aix-ui-e2e/

libs/
  shared-core/
  shared-data-access/
  shared-ui/

## State Management

- @ngrx/signals
- Signal stores
- No BehaviorSubject for local component state
