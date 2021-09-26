# Contributing to Cryengine Converter

:+1: First off, thanks for taking the time to contribute!  :+1:

The following is a set of guidelines for contributing to the project.  These are mostly guidelines and not rules.  Use your best judgment, and feel free to propose changes to this document in a pull request.

## Coding Conventions guidelines

The project uses the Microsoft Coding conventions found at https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions. They are a nice comprehensive set of guidelines that help make the project more maintainable and understandable to new contributors.

For general coding beyond a style guide, please pick up *Clean Code* by Uncle Bob (Robert Martin) (non-referral link for convenience: https://www.amazon.com/Robert-Martins-Clean-Software-Craftmanship/dp/B08X2T3DCZ/).
- Pick clearly understandable names for variables, classes, and methods.
- Keep methods as short as practical.  If it's more than 10 lines, consider extracting some of the functionality to another method.
- Write tests.  This is a complex project that supports many different games, and making a simple change can easily break another game.

Follow [SOLID principles](https://en.wikipedia.org/wiki/SOLID) and use standard [design patterns](https://en.wikipedia.org/wiki/Software_design_pattern).  They help make the code more maintainable and readable.

Remove unneeded code.  If you comment out a block of code, just delete it.  It's always available in the git history.  Vertical whitespace should be considered a scarse resource and protected vigorously (he says as he reviews the Collada.cs file... 😧).

## Testing
### Unit Tests

The project has a serious lack of unit tests.  If you are adding new code, strongly consider adding tests to ensure it works as expected, and any refactoring doesn't break existing functionality.  Pull requests that consist of only unit tests will be highly prioritized.  In the future, pipeline support will be added to run unit tests on submissions to help validate the code.

### Integration Tests

The integration tests rely on a set of game files stored in the cloud, and therefore aren't included as part of the repository. If you have *legal* access to the games in the integration tests, consider making your own version of the testing files.  Breaking an existing integration test can result in a pull request not being accepted, depending on the problem.  For example, if it just changes an XML element to print `1.0` instead of `1.000000`, no problem.  But if it ends up creating invalid Collada files, that's a problem.  The tests do have a method that can validate the Collada XML, so consider making an integration test just to validate your particular model conversions if nothing else.

Thank you again for your support and contributions! 👍
