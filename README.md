# Broiler.UI.Shell

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)

Broiler.UI.Shell provides the platform-neutral shell dialog contracts, host ports (`IUiPrintHost`, `IUiScanHost`), and Standard implementations for the Broiler retained-mode UI framework. It layers on top of `Broiler.UI` (Core) and `Broiler.UI.Standard` without introducing native UI handle dependencies.

## Packages

12 packages targeting `net10.0`, organized as contract/implementation pairs:

| Dialog | Contract Package | Standard Implementation |
| --- | --- | --- |
| About | `Broiler.UI.AboutDialog` | `Broiler.UI.AboutDialog.Standard` |
| Color | `Broiler.UI.ColorDialog` | `Broiler.UI.ColorDialog.Standard` |
| File (Open/Save) | `Broiler.UI.FileDialog` | `Broiler.UI.FileDialog.Standard` |
| Font | `Broiler.UI.FontDialog` | `Broiler.UI.FontDialog.Standard` |
| Print | `Broiler.UI.PrintDialog` | `Broiler.UI.PrintDialog.Standard` |
| Scan | `Broiler.UI.ScanDialog` | `Broiler.UI.ScanDialog.Standard` |

`Broiler.UI.PrintDialog` includes `IUiPrintHost` and `UiPrintSettings`; `Broiler.UI.ScanDialog` includes `IUiScanHost` and `UiScanSettings`.

### Dependency Direction

```text
Broiler.UI.<Dialog>.Standard -> Broiler.UI.<Dialog> -> Broiler.UI.Dialog -> Broiler.UI.Window -> Broiler.UI -> Broiler.Graphics
                             -> Broiler.UI.Standard -> Broiler.UI
```

## Repository Layout

```text
src/Abstractions/Shell/              One contract assembly per shell dialog
src/Implementations/Standard/Shell/  One Standard implementation per contract
src/tests/Shell/                     xUnit suites for shell contracts and Standard dialogs
src/samples/Win32/                   Win32 shell dialog gallery sample host
eng/                                 Vendored packaging metadata and icons
Broiler.UI.Shell.slnx                Solution over all Shell libraries, tests, and samples
```

## Building and Testing

Build and run all tests with the .NET 10 SDK (expects sibling `../Broiler.UI` checkout):

```bash
dotnet build Broiler.UI.Shell.slnx -c Debug
dotnet test Broiler.UI.Shell.slnx -c Debug
```

Run the Win32 shell sample host:

```bash
dotnet run --project src/samples/Win32/Broiler.UI.Shell.Win32.Demo -c Debug-Windows
```

## Human Review

See [HUMAN_REVIEW.md](HUMAN_REVIEW.md) for current human-review status (`PENDING`).

## License

Broiler.UI.Shell is licensed under the [Apache License 2.0](LICENSE).
