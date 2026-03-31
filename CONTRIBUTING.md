# Contributing to dotnet-source-generator-toolkit

Thank you for considering contributing to `dotnet-source-generator-toolkit`!

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- A C# IDE (Visual Studio, Rider, or VS Code with the C# extension)

## Building Locally

```bash
git clone https://github.com/sarmkadan/dotnet-source-generator-toolkit.git
cd dotnet-source-generator-toolkit

# Restore packages
dotnet restore

# Build
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release --verbosity normal
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal --logger "trx;LogFileName=test-results.trx"

# Run a specific test project
dotnet test tests/dotnet-source-generator-toolkit.Tests/
```

## Getting Started with a Contribution

1. **Fork** the repository on GitHub.
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/your-username/dotnet-source-generator-toolkit.git
   ```
3. **Create a branch** for your feature or bugfix:
   ```bash
   git checkout -b feature/my-new-feature
   ```
4. Make your changes, add tests, and verify everything passes:
   ```bash
   dotnet build && dotnet test
   ```
5. Push your branch and open a Pull Request against `main`.

## Code Style

- Follow the `.editorconfig` rules present in the repository root.
- Private fields use the `_camelCase` prefix convention.
- Provide XML documentation comments for all public APIs.
- Use `file`-scoped namespaces for new C# files.
- Keep methods focused — prefer small, single-responsibility functions.
- **Preserve all author headers** at the top of existing files; do not remove them.

## Pull Request Guidelines

- Ensure all existing tests pass before submitting.
- Add or update tests for any new behaviour.
- Write a clear PR description explaining what the change does and why.
- Reference any related issues with `Closes #<issue-number>`.
- Keep PRs focused — one feature or fix per PR is preferred.

## Reporting Issues

Use [GitHub Issues](https://github.com/sarmkadan/dotnet-source-generator-toolkit/issues) for bugs and feature requests.

- Provide a clear, descriptive title.
- Include reproduction steps, expected behaviour, and actual behaviour for bugs.
- For feature requests, explain the use case and why it belongs in the toolkit.

## License

By contributing you agree that your contributions will be licensed under the [MIT License](LICENSE).

