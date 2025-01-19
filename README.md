# Banking Application

## Overview
This project is a console-based banking application built using modern technologies and design principles. It supports basic banking operations like managing transactions, defining interest rules, and generating account statements.

---

## Technologies and Architecture

1. **Technologies Used**:
   - **.NET Core 7**: Framework for building the application.
   - **PostgreSQL**: Relational database for storing account, transaction, and interest rule data.
   - **Entity Framework Core**: Object-relational mapping (ORM) tool for database interactions.
   - **MediatR**: Implements the CQRS pattern by separating queries and commands.
   - **FluentValidation**: Validates inputs with robust rules.
   - **In-Memory Database** (for testing): Enables fast, database-independent unit tests.
   - **xUnit**: Unit testing framework.
   - **FluentAssertions**: Provides a rich set of assertions for unit tests.

2. **Architectural Patterns**:
   - **CQRS**: Segregates query and command responsibilities.
   - **SOLID Principles**: Ensures code maintainability and extensibility.
   - **Dependency Injection**: Manages dependencies throughout the application.
   - **Layered Architecture**: Separates concerns across `Application`, `Domain`, `Infrastructure`, and `Presentation`.

---

## Setup Instructions

### Prerequisites

1. **Install Required Software**:
   - .NET SDK 7.0 or later.
   - PostgreSQL (local instance).
   - Command-line tools (PowerShell, Command Prompt, or Bash).

2. **Database Setup**:
   - Create a new database in your local PostgreSQL instance named `bankingapp`.
   - Ensure the `public` schema exists.
   - Update the connection details in the `appsettings.json` file located in the `ConsoleApp` project:
     ```json
     "Database": {
       "Host": "localhost",
       "Port": "5432",
       "Username": "postgres",
       "Password": "1234",
       "Database": "bankingapp",
       "Schema": "public"
     }
     ```

---

### Running the Application

1. **Restore Dependencies**:
   Open a terminal in the project root directory and run:
   ```bash
   dotnet restore
2. **Run Database Migrations**:
   To apply database migrations and initialize the schema, run:
```bash
   dotnet ef database update --project Infrastructure --startup-project ConsoleApp
```
  
3. **Run the Console Application**:
   Start the application by running the following command in the terminal:
```bash
  dotnet run --project ConsoleApp
```

### Running Unit Tests

The project includes unit tests for validating the logic of services like transactions, interest rules, and statements.

1. Navigate to the Test Project:
```cmd
cd BankingApplication.Tests
```
2. Run All Tests:
```cmd
dotnet test
```
3. Run a Specific Test: You can specify the method name to run only a single test:
```cmd
dotnet test --filter "MethodName"
```
4. Test Output:
- Test results will be displayed in the console.
- In case of failure, the console will indicate the failed assertions.
