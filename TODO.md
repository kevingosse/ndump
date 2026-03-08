# TODO

- [ ] **ProxyCompiler: remove dependency on locally installed SDK for reference assemblies.**
  `GetReferenceAssemblyDirectory()` currently locates ref assemblies from the .NET targeting pack on disk (`dotnet/packs/Microsoft.NETCore.App.Ref/...`). This breaks if no SDK is installed (e.g. runtime-only deployments, containers). Consider bundling reference assemblies via the `Microsoft.NETCore.App.Ref` NuGet package or using `Basic.Reference.Assemblies`.

