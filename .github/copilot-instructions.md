# Copilot Instructions

## Project Overview

This repository contains a Web API and Blazor application for an online minigame platform.

Technology stack:

* Backend: C#
* Frontend: Blazor
* Architecture: Single solution containing both backend and frontend projects.

Minigames are treated as **black-box components**. The platform must remain as independent from individual minigame implementations as possible.

---

# Core Development Principles

## 1. Minigame Independence

Minigames must be treated as external modules.

Rules:

* Do not introduce direct dependencies on specific minigame implementations.
* Use abstractions, interfaces, or contracts when interacting with minigames.
* Platform logic must work with any minigame that follows the expected interface.
* Avoid embedding minigame-specific logic into core platform components.

---

## 2. Backend and Frontend Consistency

Whenever a new feature is implemented:

* The feature must be implemented in **both backend and frontend** when applicable.
* Backend API changes must be reflected in the Blazor frontend.
* UI components must align with backend models and endpoints.

Do not implement features that exist only on one side if both layers require support.

---

## 3. Flexibility in Common Projects

Projects outside the `Minigames` folder are considered **common platform projects**.

When modifying these projects:

* Prefer extensibility over hardcoded logic.
* Use abstractions, interfaces, and extension points.
* Avoid implementing logic that assumes a specific minigame.
* Design systems that allow additional minigames or features to be integrated later.

---

## 4. SOLID Principles

Follow SOLID design principles at all times.

Most important rule:

### Single Responsibility Principle

Each class should have **one clear responsibility**.

Guidelines:

* Avoid large multi-purpose classes.
* Separate API logic, business logic, and UI logic.
* Services should focus on a single domain responsibility.

Other SOLID rules should also be respected:

* Open/Closed Principle
* Liskov Substitution Principle
* Interface Segregation Principle
* Dependency Inversion Principle

---

## 5. Code Structure Guidelines

Prefer the following patterns:

* Use interfaces for services and integrations.
* Use dependency injection for service resolution.
* Keep controllers thin and move business logic into services.
* Avoid tightly coupling UI logic with backend implementation details.

---

## 6. When Generating New Code

Copilot should:

* Reuse existing project patterns and architecture.
* Avoid introducing new architectural styles unless necessary.
* Follow naming conventions already used in the solution.
* Keep new code consistent with the existing structure.

---

## 7. Domain Access and Repository Pattern

All CRUD operations with entities must be performed **only through the Domain project**.

Rules:

* Do not access database entities directly from API controllers, services, or frontend code.
* All entity persistence logic must go through **repositories located in the Domain layer**.
* Follow the **Repository pattern** used in existing implementations.
* Reuse existing repository abstractions and patterns instead of introducing new data-access approaches.

Guidelines:

* Controllers should call services.
* Services should call repositories defined in the Domain project.
* Repositories are responsible for interacting with the database.

When implementing CRUD functionality:

* Follow the structure and patterns used in existing repository implementations.
* Do not bypass repositories with direct database access.
* Keep entity manipulation inside the Domain layer.

Copilot should **analyze existing repository examples in the solution and follow the same patterns when generating new code**.

---

# Summary for Copilot

When generating or modifying code:

* Keep the platform independent from minigame implementations.
* Implement features consistently across backend and frontend.
* Prefer flexible and extensible solutions instead of hardcoding.
* Follow SOLID principles, especially **Single Responsibility**.
* Respect the existing architecture of the solution.
