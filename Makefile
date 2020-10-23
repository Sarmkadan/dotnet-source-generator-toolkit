# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help clean build test run restore format lint docker-build docker-run docker-clean release install uninstall

DOTNET := dotnet
CONFIGURATION := Release
FRAMEWORK := net10.0
PROJECT := dotnet-source-generator-toolkit
VERSION := 1.2.0

help:
	@echo "$(PROJECT) - Makefile targets"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  %-20s %s\n", $$1, $$2}'

clean: ## Clean build artifacts
	$(DOTNET) clean --configuration $(CONFIGURATION)
	rm -rf bin obj publish nupkg *.nupkg
	find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true

restore: ## Restore NuGet dependencies
	$(DOTNET) restore

build: restore ## Build project in Release configuration
	$(DOTNET) build --configuration $(CONFIGURATION) --no-restore

test: build ## Run unit tests
	$(DOTNET) test --configuration $(CONFIGURATION) --no-build --verbosity normal

run: build ## Run the application
	$(DOTNET) run --configuration $(CONFIGURATION) -- --path . --format Json

run-verbose: build ## Run with verbose output
	$(DOTNET) run --configuration $(CONFIGURATION) -- --path . --format Json --verbose

run-dry: build ## Run in dry-run mode (preview without writing)
	$(DOTNET) run --configuration $(CONFIGURATION) -- --path . --dry-run --verbose

publish: build ## Publish for current platform
	$(DOTNET) publish --configuration $(CONFIGURATION) --output publish

publish-linux: build ## Publish for Linux x64
	$(DOTNET) publish --configuration $(CONFIGURATION) -r linux-x64 --self-contained --output publish/linux-x64

publish-windows: build ## Publish for Windows x64
	$(DOTNET) publish --configuration $(CONFIGURATION) -r win-x64 --self-contained --output publish/win-x64

publish-macos: build ## Publish for macOS x64
	$(DOTNET) publish --configuration $(CONFIGURATION) -r osx-x64 --self-contained --output publish/osx-x64

format: ## Format code with EditorConfig settings
	$(DOTNET) format

lint: ## Run code quality checks
	$(DOTNET) build --configuration $(CONFIGURATION) /p:EnforceCodeStyleInBuild=true

pack: clean build ## Create NuGet package
	$(DOTNET) pack --configuration $(CONFIGURATION) --output nupkg --version-suffix "" /p:Version=$(VERSION)

docker-build: ## Build Docker image
	docker build -t $(PROJECT):$(VERSION) -t $(PROJECT):latest .

docker-run: docker-build ## Run in Docker container
	docker run --rm -v $(PWD):/workspace -v $(PWD)/Generated:/workspace/Generated \
		$(PROJECT):latest \
		--path /workspace/src \
		--output /workspace/Generated \
		--format Json

docker-run-compose: ## Run using docker-compose
	docker-compose up --build

docker-clean: ## Clean Docker images and containers
	docker-compose down --remove-orphans 2>/dev/null || true
	docker rmi $(PROJECT):$(VERSION) $(PROJECT):latest 2>/dev/null || true
	docker system prune -f

install-tool: pack ## Install as global .NET tool
	$(DOTNET) tool install --global $(PROJECT) --add-source ./nupkg --version $(VERSION)

uninstall-tool: ## Uninstall global .NET tool
	$(DOTNET) tool uninstall --global $(PROJECT)

update-tool: pack install-tool ## Update global .NET tool

release: clean lint test pack ## Perform full release build
	@echo "Release build complete: nupkg/$(PROJECT).$(VERSION).nupkg"

benchmark: build ## Run performance benchmarks
	@echo "Running benchmarks..."
	time $(DOTNET) run --configuration $(CONFIGURATION) -- --path . --format Json

coverage: ## Generate code coverage report
	$(DOTNET) test --configuration $(CONFIGURATION) /p:CollectCoverage=true /p:CoverageFormat=opencover

docs: ## Generate documentation
	@echo "Documentation is in docs/ directory"
	@echo "  - Getting Started: docs/getting-started.md"
	@echo "  - Architecture: docs/architecture.md"
	@echo "  - API Reference: docs/api-reference.md"
	@echo "  - Deployment: docs/deployment.md"
	@echo "  - FAQ: docs/faq.md"

info: ## Display project information
	@echo "Project: $(PROJECT)"
	@echo "Version: $(VERSION)"
	@echo "Framework: $(FRAMEWORK)"
	@echo ".NET SDK: $(shell dotnet --version)"
	@echo "dotnet CLI version: $(shell dotnet --version)"

setup: restore ## Setup development environment
	@echo "Development environment setup complete"
	@echo "Next steps:"
	@echo "  make build    - Build the project"
	@echo "  make test     - Run tests"
	@echo "  make run      - Run the application"

all: clean restore build test lint ## Run full build pipeline

.DEFAULT_GOAL := help
