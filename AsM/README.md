# AsM - Application Architecture

## Overview
This document outlines the architecture of the AsM application, a Blazor-based web application with authentication and user management features.

## Architecture

The application follows a layered architecture with clear separation of concerns:

1. **Presentation Layer** - Blazor components and pages
2. **API Layer** - Controllers for handling HTTP requests
3. **Service Layer** - Business logic and orchestration
4. **Data Access Layer** - Database interactions
5. **Domain Layer** - Models and entities

### Folder Structure

```
AsM/
├── Api/                  # API controllers
├── Components/           # Blazor components
│   ├── Layout/           # Layout components
│   └── Pages/            # Page components
├── Data/                 # Data access implementations
├── Interfaces/           # Service and repository interfaces
├── Models/               # Domain models
├── Services/             # Service implementations
└── wwwroot/              # Static assets
```

## Dependency Injection

The application uses ASP.NET Core's built-in dependency injection container to manage dependencies. Services are registered in `Program.cs`.

### Key Interfaces

- `ICassandraDbContext` - Interface for Cassandra database operations
- `INeo4jDbContext` - Interface for Neo4j graph database operations
- `IUserService` - Interface for user-related operations
- `IAuthService` - Interface for authentication-related operations
- `IEmailService` - Interface for email-related operations

## Database Access

The application uses two databases:

1. **Cassandra** - For storing user data, credentials, and email verification codes
2. **Neo4j** - For graph-based data (relationships, etc.)

## Authentication

The application uses cookie-based authentication with the following features:

- Username/email and password authentication
- Password hashing using PBKDF2 with SHA-512
- Email verification

## Best Practices

When developing for this application, follow these best practices:

1. **Separation of Concerns** - Keep UI, business logic, and data access separate
2. **Interface-Based Programming** - Program to interfaces, not implementations
3. **Dependency Injection** - Use constructor injection for dependencies
4. **Error Handling** - Use try-catch blocks and log errors appropriately
5. **Async/Await** - Use async/await for all I/O operations
6. **Validation** - Validate input at the service layer
7. **Testing** - Write unit tests for services and repositories

## Migration Guide

The application is currently being migrated from a monolithic architecture to a more modular, layered architecture. During this transition:

1. New code should follow the new architecture
2. Legacy services (`DatabaseService`, `GraphService`) are maintained for backward compatibility
3. Gradually refactor existing components to use the new services

## Future Improvements

1. Implement a repository pattern for data access
2. Add unit tests for all services
3. Implement a CQRS pattern for more complex operations
4. Add client-side validation
5. Improve error handling and user feedback