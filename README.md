# EntityComponentSystem

A lightweight, reusable Entity Component System (ECS) class library for .NET — designed as a foundation for VideoGame projects.

## About

This library provides a minimal ECS architecture that cleanly separates **data** (components) from **identity** (entities). It is intended to be packaged and reused across multiple VideoGame projects, keeping game logic organized and decoupled.

## Features

- **Lightweight `Entity` struct** — entities are simple integer IDs with zero overhead
- **Entity ID recycling** — destroyed entity IDs are reused to keep IDs compact
- **Generic component storage** — attach any `IComponent` to an entity with type-safe access
- **Query support** — iterate over all entities that have a specific component type
- **No external dependencies** — pure .NET 10 class library

## Getting Started

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

### Installation

#### 1. Add as Submodule

In the root directory of your project:

```bash
git submodule add https://github.com/R0binSmit/EntityComponentSystem libs/EntityComponentSystem
git submodule update --init --recursive
```

### 2. Add to your Solution

```bash
dotnet sln add libs/EntityComponentSystem/src/EntityComponentSystem/EntityComponentSystem.csproj
```

## Usage

### Define a component

Components are plain data types that implement `IComponent`.

### Manage entities and components

Example by using the `EntityManager` and `ComponentManager` to create entities, attach components, and query them:

```csharp
using EntityComponentSystem;
using EntityComponentSystem.Manager;

var entities = new EntityManager();
var components = new ComponentManager();

// Create entities
var player = entities.Create();
var enemy = entities.Create();

// Attach components
components.Add(player, new Position { X = 100, Y = 200 });
components.Add(player, new Velocity { Dx = 1, Dy = -1 });
components.Add(enemy, new Position { X = 400, Y = 300 });

// Query a component
var pos = components.Get<Position>(player);

// Check if an entity has a component
if (components.Has<Velocity>(player))
{
    var vel = components.Get<Velocity>(player);
}

// Iterate all entities with a component
foreach (var (entity, vel) in components.GetAll<Velocity>())
{
    var p = components.Get<Position>(entity);
    // update position ...
}

// Destroy an entity and clean up its components
components.RemoveAll(enemy);
entities.Destroy(enemy);
```

## Project Structure

```
src/EntityComponentSystem/
├── Entity.cs                       # Lightweight entity identifier (readonly struct)
├── Interfaces/
│   └── IComponent.cs               # Marker interface for components
└── Manager/
    ├── ComponentManager.cs          # Stores and queries components per entity
    └── EntityManager.cs             # Creates, destroys, and tracks entities
```
