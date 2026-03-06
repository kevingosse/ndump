# ndump

## Overview

**ndump** is a developer tool for exploring and debugging .NET memory dumps. Its goal is to make post‑mortem debugging of .NET applications significantly more productive by providing a strongly‑typed object model and interactive tooling around memory dump inspection.

Traditional dump debugging tools expose memory structures through low‑level inspection commands. ndump instead projects the runtime objects contained in a memory dump into generated .NET types that can be accessed programmatically, enabling developers to explore the heap using familiar C# constructs.

The toolchain is built around three main components:

- A core library responsible for reading dumps and projecting CLR objects.
- A CLI used to scaffold projects and interact with dumps.
- A GUI debugger providing a rich interactive experience.

The system relies on **ClrMD** to read memory dumps and CLR metadata.

---

# Key Concepts

## Dump Projection

When ndump analyzes a memory dump:

1. The dump is loaded using ClrMD.
2. The CLR metadata and heap structure are analyzed.
3. A set of **proxy types** is generated that mirror the structure of the types found in the dump.
4. These types are emitted into a generated assembly.

The generated assembly allows developers to interact with objects from the dump as if they were regular .NET objects.

Example:

```csharp
var customers = _Generated.Customer.GetInstances();

foreach (var customer in customers)
{
    Console.WriteLine(customer.Name);
}
```

The generated objects expose:

- Fields
- Object references
- Value types

They do **not** contain executable logic from the original program. Only structural information is available.

---

# Project Structure

The repository contains three main projects.

```
ndump
│
├─ Ndump.Core
├─ Ndump.Cli
└─ Ndump.Gui
```

## Ndump.Core

The core engine of the system.

Responsibilities:

- Loading memory dumps
- CLR runtime inspection via ClrMD
- Heap traversal
- Type discovery
- Code generation for projected types
- Emitting the generated assembly
- Providing APIs to query heap objects (in the generated assembly)

---

## Ndump.Cli

Command line interface for interacting with ndump.

The CLI will be distributed as a **.NET global tool**.

Command pattern:

```
ndump <command>
```

Example commands:

```
ndump new dump.dmp
ndump gui dump.dmp
```

Responsibilities:

- Entry point for the tool
- Project scaffolding
- Launching the GUI

---

## Ndump.Gui

A graphical memory dump debugger.

The GUI provides a rich inspection environment similar to an IDE debugger but operating on a memory dump.

Planned features:

- Heap explorer
- Object inspector
- Memory viewer
- Reference graph navigation
- Decompiler integration
- C# scripting engine (based on RoslynPad)

The scripting engine will reference the generated assembly produced by Ndump.Core so developers can write queries directly against dump objects.

Example script:

The GUI should provide:

- IntelliSense
- Autocomplete
- Object visualization

--- 

# Non‑Goals

- Executing code from the original process
- Reproducing the full runtime environment
- Replacing low‑level debugging tools like WinDbg

ndump focuses on **developer productivity when analyzing managed heaps**.

---

# Technology Stack

- .NET
- ClrMD
- RoslynPad (for scripting)
