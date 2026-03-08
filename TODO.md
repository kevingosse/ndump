# TODO

- [ ] **ProxyCompiler: remove dependency on locally installed SDK for reference assemblies.**
  `GetReferenceAssemblyDirectory()` currently locates ref assemblies from the .NET targeting pack on disk (`dotnet/packs/Microsoft.NETCore.App.Ref/...`). This breaks if no SDK is installed (e.g. runtime-only deployments, containers). Consider bundling reference assemblies via the `Microsoft.NETCore.App.Ref` NuGet package or using `Basic.Reference.Assemblies`.

- [ ] **ProxyEmitter: `double[]` arrays generate unsupported-element-type comment instead of a working property.**
  When the dump contains a `System.Double[]`, `ClassifyComponentType` fails to resolve the element type and ProxyEmitter emits a comment (`// ... element type not supported`) instead of an array property. `int[]` and `byte[]` work correctly. Likely a gap in the `ClrElementType` → component-type mapping.
  *Skipped test:* `BehavioralProxyTests.PrimitiveArray_Double_ReadsCorrectValues`

- [ ] **ProxyEmitter: `bool[]` arrays generate unsupported-element-type comment instead of a working property.**
  Same root cause as `double[]`. `System.Boolean[]` component type is not resolved, so no array property is emitted.
  *Skipped test:* `BehavioralProxyTests.PrimitiveArray_Bool_ReadsCorrectValues`

- [ ] **Interior struct proxies: object reference fields don't use ProxyResolver for polymorphic type resolution.**
  When a struct (accessed via interior pointer) has an object reference field, `Field<_.System.Object>()` creates a `_.System.Object` proxy directly via `CreateProxy(typeof(T))`. It does not consult the `ProxyResolver` to resolve the actual CLR type at that address. As a result, object ref fields in structs always return `_.System.Object` instead of the concrete proxy type. Heap-allocated object fields work correctly because they go through the resolver.
  *Skipped test:* `BehavioralProxyTests.Struct_Label_ReadsObjectRefField_AsConcreteType`
