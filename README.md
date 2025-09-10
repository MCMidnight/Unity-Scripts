# Midnight's Unity Scripts

A curated collection of production-ready C# scripts for the Unity Engine. This repository is dedicated to sharing robust, scalable, and highly maintainable code that follows modern best practices for game development.

## Core Principles

Every script in this collection is designed with the following principles in mind:

-   **✅ Performant:** Optimized for efficiency, often using event-driven architectures to avoid unnecessary work in `Update()` loops.
-   **✅ Decoupled:** Scripts are designed to be independent, minimizing dependencies and making them easy to integrate into any project.
-   **✅ Scalable:** Built with future growth in mind, allowing for easy extension and modification.
-   **✅ Maintainable:** Clean, well-commented, and self-documenting code that is easy to read, understand, and debug.
-   **✅ Robust:** Handles its own lifecycle (e.g., subscribing/unsubscribing to events) correctly and safely.

---

## Documentation Standard

Every script includes a detailed XML summary header that provides an at-a-glance assessment of its qualities and serves as a quick reference for its purpose and design principles.

**Example**
```
/// <summary>
/// Input Handler Assessment  - Midnight 10/09/2025
/// Performant: It won't cause any framerate drops.
/// Decoupled: It keeps your game logic clean and independent.
/// Scalable: It's easy to add new actions as your game evolves.
/// Maintainable: It's easy to read, understand, and debug.
/// Robust: It handles its own lifecycle correctly and safely.
/// </summary>
```
---
