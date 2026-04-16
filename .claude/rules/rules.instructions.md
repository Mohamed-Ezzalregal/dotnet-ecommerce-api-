---
description: "Use when: working on the TestApi .NET project — coding, explaining, or reviewing"
applyTo: "TestApi/**"
---

# Project Rules

## Language & Communication
- Always respond in Arabic (Egyptian dialect).
- Always show full code blocks — never use `...existing code...` or hide/collapse code.
- Always show complete file content when providing code.

## Code Explanation
- When explaining code, always explain the differences between what's used and the related alternatives.
- Example: if code uses `Ok()`, explain how it differs from `CreatedAtAction()`, `BadRequest()`, `NotFound()`, etc.
- Example: if code uses `FindAsync()`, explain how it differs from `FirstOrDefaultAsync()`, `SingleOrDefaultAsync()`, etc.
- Write explanations in a separate file inside `TestApi/Notes/` folder.
- Each topic gets its own separate file (e.g., `AuthController-Explained.md`, `RepositoryPattern-Explained.md`).

## Tech Stack
- .NET 8, ASP.NET Core Web API
- Entity Framework Core + MySQL (Pomelo)
- JWT Authentication with BCrypt
- Follow existing project patterns and naming conventions