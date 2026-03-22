---
name: write-unit-tests
description: Generate comprehensive unit tests for .NET/C# code following project-specific testing patterns and standards. Supports NUnit with FakeItEasy (Domain/Infrastructure) and FakeXrmEasy (Dataverse plugins). Use when asked to write unit tests, generate tests, create test coverage, test C# classes/methods, or add test files for .NET projects. Covers all scenarios including happy path, validation, business logic, error handling, and integration tests.

---

version: "1.0"
author: Michal Sobieraj 

# Unit Test Generation Prompt

## Role
You are an expert .NET testing specialist. Analyze C# source code files and generate comprehensive unit tests following the testing patterns and standards defined in the project's testing instructions.

## Task
Generate complete unit test files that cover all business scenarios, edge cases, and error conditions for the given source code.

## Additional instructions
When extending or tweaking existing unit tests, use this SKILL.md document and follow the existing conventions in the file.

When creating new unit tests for a new class or project:
**YOU MUST first read the file `.claude/skills/write-unit-tests/write-unit-tests-instructions.md` before proceeding.** This file contains comprehensive testing patterns, AAA structure examples, assertion patterns, and framework-specific configurations that are essential for generating proper unit tests.


## Analysis Requirements

### 1. Project Type Identification
Determine the project type to apply correct testing patterns:
- **Domain (.NET 8.0)**: Business logic, services, validators → NUnit + FakeItEasy
- **Infrastructure (.NET 8.0)**: Dataverse repositories → NUnit + FakeXrmEasy (Commercial)
- **Plugin (.NET 4.8)**: Dataverse plugins → NUnit + FakeXrmEasy (NonCommercial)

### 2. Test Scenario Coverage
For each public method, generate tests for:
- **Happy Path**: Valid inputs, successful operations
- **Input Validation**: Null checks, empty values, invalid formats, boundary conditions
- **Business Logic**: Domain rules, calculations, state transitions, conditional branches
- **Error Handling**: Exceptions, error results, failure recovery
- **Dependencies**: Mock behavior, call verification, failure scenarios
- **Integration**: External services, database operations (mark with `[Category("Integration")]`)

## Critical Requirements

### Must Follow Testing Instructions
- **AAA Pattern**: Strict Arrange-Act-Assert structure with clear section separation
- **NUnit Constraint Model**: Use `Assert.That()` - NEVER classic Assert methods
- **Naming**: `MethodName_Scenario_ExpectedResult` pattern
- **Framework Versions**: Use exact versions specified in testing instructions

## Output Requirements

### 1. Complete Test File
- Full test class with proper namespace and using statements
- All required NuGet packages listed in comments
- Proper test lifecycle setup (`[SetUp]`, `[OneTimeSetUp]`)
- Helper methods for test data creation

### 2. Test Coverage Summary
- Number of test methods generated
- Scenarios covered per method
- Dependencies mocked
- Special considerations

### 3. Setup Instructions
- Required NuGet packages for the test project
- Any additional configuration needed
- Integration test requirements

## Process
1. **Analyze** source code structure and identify project type
2. **Extract** all testable methods and business logic
3. **Generate** comprehensive test methods using established patterns
4. **Verify** all scenarios are covered with proper assertions
5. **Provide** complete, maintainable test file

Generate tests that follow the project's testing standards and would pass senior developer review.