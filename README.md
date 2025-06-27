# 🧱 CoreApp (.NET 9 Clean Architecture Boilerplate)

**CoreApp** is a reusable boilerplate project built with .NET 9 and Clean Architecture principles.  
It provides a solid foundation for developing secure, scalable, and maintainable enterprise-grade applications with modern practices such as CQRS, MediatR, and pipeline behaviors.

---

## 🚀 Features

- ✅ Clean Architecture (Domain → Application → Infrastructure → WebAPI)
- ✅ CQRS with MediatR
- ✅ FluentValidation integration
- ✅ MediatR Pipeline Behaviors (Validation, Logging, Exception, Performance, Authorization, Transaction)
- ✅ Authentication-ready structure (`IAuthService`, `User`, `Role`, `RefreshToken`)
- ✅ Fully testable, layered, extensible project structure
- ✅ Modular Git-based development flow with feature branches

---

## 📁 Project Structure

src/
├── CoreApp.Domain # Entities and domain contracts (no dependencies)
├── CoreApp.Application # CQRS, DTOs, interfaces, behaviors
├── CoreApp.Infrastructure # To be implemented: EF Core, AuthService, Logging, etc.
├── CoreApp.WebAPI # REST API setup 
├── CoreApp.Tests # Unit and integration tests 


---

## 🧩 Implemented Modules

### 📌 Domain Layer
- `BaseEntity`, `IEntity`
- `User`, `Role`, `RefreshToken` entities

### 📌 Application Layer
- Auth DTOs: `RegisterRequest`, `LoginRequest`, `AuthResponse`
- CQRS: `RegisterCommand`, `LoginCommand` + their handlers & validators
- Interface: `IAuthService`
- ✅ MediatR Pipeline Behaviors:
  - `ValidationBehavior`
  - `UnhandledExceptionBehavior`
  - `PerformanceBehavior`
  - `LoggingBehavior`
  - `AuthorizationBehavior`
  - `TransactionBehavior`

---

## 📦 Tech Stack

| Area              | Technology                                      |
|-------------------|--------------------------------------------------|
| Language/Runtime  | .NET 9                                           |
| CQRS / Mediator   | MediatR                                          |
| Validation        | FluentValidation                                 |
| Auth (planned)    | JWT, Refresh Token, Claims-based Authorization   |
| ORM (planned)     | EF Core + PostgreSQL or SQL Server               |
| Logging (planned) | Serilog                                          |

---

## 🔁 Git Branching Strategy

| Branch                          | Description                                  |
|----------------------------------|----------------------------------------------|
| `develop`                       | Main development branch                      |
| `feature/domain-model`          | Entities: User, Role, Token                  |
| `feature/application-auth`      | Auth CQRS, DTOs, Handlers                    |
| `feature/application-pipeline-behaviors` | Core MediatR behaviors                     |
| `feature/application-extra-behaviors`    | Logging, Authorization, Transaction         |

All branches were merged via PRs using semantic commit messages.

---

## 🛠️ How to Use

```bash
# clone and switch to develop branch
git clone https://github.com/your-username/CoreApp.git
cd CoreApp
git checkout develop

# build the project
dotnet build

# run tests
dotnet test
