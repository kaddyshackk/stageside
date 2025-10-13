# Custom Instructions for ComedyPull API

## General Principles

- **Read Before Assuming**: Always examine existing code, documentation, and tests before making assumptions about implementation patterns or architecture
- **Follow Established Patterns**: Identify and maintain consistency with existing architectural patterns, naming conventions, and code organization in this repository
- **Documentation First**: Review relevant documentation in the `/wiki` directory before implementing features or making significant changes
- **Test-Driven Approach**: Check for existing tests and maintain or extend test coverage for any modified code

## Coding Standards

- Follow SOLID principles and clean code practices
- Maintain separation of concerns
- Prefer composition over inheritance where appropriate
- Use dependency injection consistently with the existing patterns
- Follow the repository's naming conventions for files, classes, and methods
- Keep methods focused and single-purpose
- Write self-documenting code with clear intent

## Feature Implementation Workflow

When the user says **"Implement this as a feature"**, follow this workflow:

### 1. Research & Read Documentation
- Review relevant documentation in `/wiki` or other docs directories
- Examine existing implementations of similar features
- Read related tests to understand expected behavior
- Identify dependencies and integration points

### 2. Outline Planned Changes
Before applying any changes:
- List all files that will be created or modified
- Describe the high-level approach and architectural decisions
- Identify which patterns you'll follow from existing code
- Note any potential impacts on other parts of the system
- Get user confirmation if the scope is significant

### 3. Be a Good Citizen
When working on code, look for opportunities to improve it:
- Fix obvious bugs or code smells in the immediate area
- Improve readability (better variable names, extract complex logic)
- Add missing error handling or validation
- Update outdated comments or documentation
- Suggest refactoring if you notice duplication or tight coupling
- **Limit improvements to the immediate context** - don't refactor unrelated code

### 4. Implementation
- Follow the outlined plan
- Maintain consistency with existing patterns
- Update or create tests alongside implementation
- Update relevant documentation

### 5. Verification
- Ensure tests pass
- Verify documentation is updated
- Check that the implementation follows the established patterns

## Project-Specific Notes

- This is a .NET/C# project
- The project uses a data pipeline architecture (see `/wiki` for details)
- State handlers and processors follow a registration pattern
- Entity Framework is used for data access with design-time factories
- Git workflow uses feature branches merged to `main`

## Anti-Patterns to Avoid

- Don't skip reading existing documentation
- Don't assume implementation details without verification
- Don't introduce new patterns without understanding existing ones
- Don't refactor large sections of unrelated code
- Don't commit without updating relevant tests and documentation
