# Architecture Baseline

## Layering

- `DislocationFilter.Domain`: domain model, invariants, domain abstractions.
- `DislocationFilter.Application`: use-case contracts, ports, result types.
- `DislocationFilter.Infrastructure`: implementation details for application ports.
- `DislocationFilter.Presentation.Wpf`: UI and composition root.

## Dependency Rule

- `Presentation -> Application`
- `Presentation -> Infrastructure`
- `Infrastructure -> Application`
- `Application -> Domain`
- `Domain` has no project dependencies.

## Technical Conventions

- Business flow in `Application` should return `Result<TError, TValue>` instead of throwing for expected errors.
- Use immutable request/response models (`record`) in use-cases.
- Keep side effects behind application ports (`Abstractions/Persistence` etc.).
- `App.xaml.cs` is the composition root: windows and view-models are resolved only through DI.
- MVVM base primitives live in `Presentation.Wpf` (`ViewModelBase`, commands, navigation abstraction).
