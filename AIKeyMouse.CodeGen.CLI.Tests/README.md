# AIKeyMouse.CodeGen.CLI Tests

End-to-end integration tests for the AIKeyMouse Code Generator CLI tool.

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run only fast tests (skip LLM integration tests)
```bash
dotnet test --filter "TestCategory!=Integration"
```

### Run only integration tests
```bash
dotnet test --filter "TestCategory=Integration"
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~PageCommandTests"
```

## Test Structure

- **PageCommandTests** - Tests for `page` command
  - Basic page object generation
  - HTML parsing and element extraction
  - Custom namespace handling
  
- **StepsCommandTests** - Tests for `steps` command
  - Feature file parsing
  - Step definition generation
  - Scenario filtering

- **Helpers** - Test utilities
  - `ProcessRunner` - Execute CLI commands
  - `TestDataHelper` - Manage test files
  - `CodeValidator` - Validate generated C# code

## Test Data

- `TestData/Html/` - Sample HTML files for parsing
- `TestData/Features/` - Sample Gherkin feature files
- `TestOutput/` - Generated files (gitignored, cleaned automatically)

## Notes

- Integration tests require Ollama running locally or GROQ_API_KEY set
- Tests are marked with `[TestCategory("Integration")]` for filtering
- Old test outputs are automatically cleaned (keeps last 3 runs)
- Each test run creates timestamped output directory for debugging
