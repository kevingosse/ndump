# TODO

- [ ] **ProxyCompiler: remove dependency on locally installed SDK for reference assemblies.**
  `GetReferenceAssemblyDirectory()` currently locates ref assemblies from the .NET targeting pack on disk (`dotnet/packs/Microsoft.NETCore.App.Ref/...`). This breaks if no SDK is installed (e.g. runtime-only deployments, containers). Consider bundling reference assemblies via the `Microsoft.NETCore.App.Ref` NuGet package or using `Basic.Reference.Assemblies`.

- [ ] **Support `Nullable<T>` value type fields.**
  ClrMD reports nullable struct fields with unresolved type parameters (e.g., `System.Nullable<T1>` instead of `System.Nullable<System.DateTime>`). Need to resolve the concrete type argument — possibly by reading the field's type info from the parent object at runtime, or by matching against known specializations. Currently these emit a `// no proxy available` comment.

- [ ] **Support value type fields where ClrMD reports `System.Void`.**
  Some struct fields (e.g., `_onExited`, `_registeredWaitHandle`, `<SynchronizingObject>k__BackingField` on `Process`) have their type reported as `System.Void` by ClrMD, likely because the actual type (delegates, `RegisteredWaitHandle`, etc.) was never instantiated on the heap. Consider resolving these via module metadata or exposing the raw interior address as a fallback.
