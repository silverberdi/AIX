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

## Suggested Structure

apps/
  axioma-web/

libs/
  shared/
  ui/
  data-access/
  features/

## State Management

- @ngrx/signals
- Signal stores
- No BehaviorSubject for local component state
