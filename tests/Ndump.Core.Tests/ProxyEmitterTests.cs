namespace Ndump.Core.Tests;

public class ProxyEmitterTests
{
    private readonly ProxyEmitter _emitter = new();

    [Fact]
    public void GenerateProxy_SimpleType_HasCorrectStructure()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo { Name = "_age", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public sealed class Customer : global::_.System.Object", code);
        Assert.Contains("Customer(ulong address, DumpContext ctx) : base(address, ctx) { }", code);
    }

    [Fact]
    public void GenerateProxy_StringField_EmitsStringProperty()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public string? _name => Field<string>();", code);
    }

    [Fact]
    public void GenerateProxy_PrimitiveField_EmitsValueProperty()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo { Name = "_age", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public int _age => Field<int>();", code);
    }

    [Fact]
    public void GenerateProxy_ObjectReferenceField_KnownType_EmitsProxyProperty()
    {
        var knownTypes = new HashSet<string> { "MyApp.Customer", "MyApp.Order" };
        var type = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_lastOrder",
                    TypeName = "MyApp.Order",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "MyApp.Order"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type, knownTypes);

        Assert.Contains("public _.MyApp.Order? _lastOrder => Field<_.MyApp.Order>();", code);
    }

    [Fact]
    public void GenerateProxy_TypeWithMixedFields_UsesUnifiedFieldOfT()
    {
        var address = new TypeMetadata
        {
            FullName = "MyApp.Address",
            Namespace = "MyApp",
            Name = "Address",
            Fields = []
        };
        var customer = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo { Name = "_age", TypeName = "int", Kind = FieldKind.Primitive },
                new FieldInfo { Name = "<IsActive>k__BackingField", TypeName = "bool", Kind = FieldKind.Primitive },
                new FieldInfo
                {
                    Name = "_address",
                    TypeName = "MyApp.Address",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "MyApp.Address"
                },
                new FieldInfo
                {
                    Name = "_widget",
                    TypeName = "SomeLib.Widget",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "SomeLib.Widget"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(customer, allTypes: [address, customer]);

        // String uses Field<string>
        Assert.Contains("public string? _name => Field<string>();", code);
        // Primitive uses Field<T>
        Assert.Contains("public int _age => Field<int>();", code);
        // Backing field uses Field<T> with explicit name
        Assert.Contains("public bool IsActive => Field<bool>(\"<IsActive>k__BackingField\");", code);
        // Known reference type uses Field<ProxyType>
        Assert.Contains("public _.MyApp.Address? _address => Field<_.MyApp.Address>();", code);
        // Unknown reference type falls back to RefAddress
        Assert.Contains("public ulong _widget => RefAddress();", code);
    }

    [Fact]
    public void GenerateProxy_ObjectReferenceField_UnknownType_EmitsAddressProperty()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_unknown",
                    TypeName = "SomeLib.Widget",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "SomeLib.Widget"
                }
            ]
        };

        // Only MyApp.Customer is known, not SomeLib.Widget
        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public ulong _unknown => RefAddress();", code);
    }

    [Fact]
    public void GenerateProxy_HasFromAddressFactory()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Tag",
            Namespace = "MyApp",
            Name = "Tag",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public static Tag FromAddress(ulong address, DumpContext ctx)", code);
    }

    [Fact]
    public void GenerateProxy_HasGetInstances()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Tag",
            Namespace = "MyApp",
            Name = "Tag",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("GetInstances(DumpContext ctx)", code);
        Assert.Contains("ctx.EnumerateInstances(\"MyApp.Tag\")", code);
    }

    [Fact]
    public void GenerateProxy_InheritsFromSystemObject()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Tag",
            Namespace = "MyApp",
            Name = "Tag",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        // GetObjAddress is inherited from _.System.Object, not emitted directly
        Assert.DoesNotContain("public ulong GetObjAddress()", code);
        Assert.Contains(": global::_.System.Object", code);
    }

    [Fact]
    public void GenerateProxy_BackingField_SanitizesName()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Foo",
            Namespace = "MyApp",
            Name = "Foo",
            Fields =
            [
                new FieldInfo
                {
                    Name = "<Value>k__BackingField",
                    TypeName = "int",
                    Kind = FieldKind.Primitive
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        // Should emit as "Value" not the mangled backing field name
        Assert.Contains("public int Value =>", code);
    }

    [Fact]
    public void GenerateProxy_UsesProxyNamespace()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Tag",
            Namespace = "MyApp",
            Name = "Tag",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("namespace _.MyApp;", code);
    }

    [Fact]
    public void GenerateProxy_SystemType_UsesCorrectNamespace()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Text.StringBuilder",
            Namespace = "System.Text",
            Name = "StringBuilder",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("namespace _.System.Text;", code);
    }

    [Fact]
    public void GenerateProxy_NoNamespace_UsesUnderscoreNamespace()
    {
        var type = new TypeMetadata
        {
            FullName = "GlobalType",
            Namespace = "",
            Name = "GlobalType",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("namespace _;", code);
    }

    [Fact]
    public void GenerateProxy_GenericType_SanitizesBacktick()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Collections.Generic.List`1",
            Namespace = "System.Collections.Generic",
            Name = "List`1",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public sealed class List_1", code);
        Assert.Contains("namespace _.System.Collections.Generic;", code);
    }

    [Fact]
    public void GenerateProxy_CrossNamespaceReference_UsesFullyQualifiedName()
    {
        var knownTypes = new HashSet<string> { "App.Models.Customer", "App.Orders.Order" };
        var type = new TypeMetadata
        {
            FullName = "App.Models.Customer",
            Namespace = "App.Models",
            Name = "Customer",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_order",
                    TypeName = "App.Orders.Order",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "App.Orders.Order"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type, knownTypes);

        Assert.Contains("public _.App.Orders.Order? _order", code);
    }

    [Fact]
    public void GetProxyNamespace_WithNamespace_PrependsDot()
    {
        Assert.Equal("_.System.Text", ProxyEmitter.GetProxyNamespace("System.Text"));
    }

    [Fact]
    public void GetProxyNamespace_Empty_ReturnsUnderscore()
    {
        Assert.Equal("_", ProxyEmitter.GetProxyNamespace(""));
    }

    [Fact]
    public void SanitizeTypeName_HandlesArrayBrackets()
    {
        Assert.Equal("String__", ProxyEmitter.SanitizeTypeName("String[]"));
    }

    [Fact]
    public void SanitizeTypeName_HandlesBacktick()
    {
        Assert.Equal("List_1", ProxyEmitter.SanitizeTypeName("List`1"));
    }

    [Fact]
    public void GenerateProxy_ArrayField_KnownElementType_EmitsDumpArrayProperty()
    {
        var knownTypes = new HashSet<string> { "MyApp.Customer", "MyApp.Order" };
        var type = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_orderHistory",
                    TypeName = "MyApp.Order[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "MyApp.Order",
                    ArrayElementKind = FieldKind.ObjectReference
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type, knownTypes);

        Assert.Contains("public global::Ndump.Core.DumpArray<_.MyApp.Order?>? _orderHistory", code);
        Assert.Contains("RefAddress()", code);
        Assert.Contains("_ctx.GetArrayLength(addr)", code);
        Assert.Contains("_.MyApp.Order.FromAddress(ea, _ctx)", code);
    }

    [Fact]
    public void GenerateProxy_ArrayField_UnknownElementType_EmitsAddressArray()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_items",
                    TypeName = "SomeLib.Widget[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "SomeLib.Widget",
                    ArrayElementKind = FieldKind.ObjectReference
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public global::Ndump.Core.DumpArray<ulong>? _items", code);
        Assert.Contains("_ctx.GetArrayElementAddress(addr, i)", code);
    }

    [Fact]
    public void GenerateProxy_ArrayField_StringElements_EmitsStringArray()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Foo",
            Namespace = "MyApp",
            Name = "Foo",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_names",
                    TypeName = "System.String[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "System.String",
                    ArrayElementKind = FieldKind.String
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public global::Ndump.Core.DumpArray<string?>? _names", code);
        Assert.Contains("_ctx.GetArrayElementString(addr, i)", code);
    }

    [Fact]
    public void GenerateProxy_ArrayField_PrimitiveElements_EmitsPrimitiveArray()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Foo",
            Namespace = "MyApp",
            Name = "Foo",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_scores",
                    TypeName = "System.Int32[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "System.Int32",
                    ArrayElementKind = FieldKind.Primitive
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public global::Ndump.Core.DumpArray<int>? _scores", code);
        Assert.Contains("_ctx.GetArrayElementValue<int>(addr, i)", code);
    }

    [Fact]
    public void GenerateProxy_ArrayField_ObjectElements_UsesSystemObjectProxy()
    {
        var knownTypes = new HashSet<string> { "MyApp.Customer", "MyApp.Order", "System.Object" };
        var type = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_mixedItems",
                    TypeName = "System.Object[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "System.Object",
                    ArrayElementKind = FieldKind.ObjectReference
                }
            ]
        };

        // System.Object is a base type for Customer and Order, so use allTypes to set that up
        var sysObj = new TypeMetadata { FullName = "System.Object", Namespace = "System", Name = "Object", Fields = [] };
        var order = new TypeMetadata { FullName = "MyApp.Order", Namespace = "MyApp", Name = "Order", Fields = [], BaseTypeName = "System.Object" };
        var code = _emitter.GenerateProxyCode(type, allTypes: [sysObj, order, type]);

        // System.Object is a known proxy type and a base type, so should use resolver with fallback
        Assert.Contains("public global::Ndump.Core.DumpArray<_.System.Object?>? _mixedItems", code);
        Assert.Contains("global::_.ProxyResolver.Resolve(ea, _ctx) as _.System.Object ?? _.System.Object.FromAddress(ea, _ctx)", code);
    }

    [Fact]
    public void GenerateProxy_DuplicateFieldNames_SkipsDuplicates()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Text.OSEncoding",
            Namespace = "System.Text",
            Name = "OSEncoding",
            Fields =
            [
                new FieldInfo { Name = "_codePage", TypeName = "int", Kind = FieldKind.Primitive },
                new FieldInfo { Name = "_codePage", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        // Should have exactly one _codePage property, not two
        var count = code.Split("public int _codePage").Length - 1;
        Assert.Equal(1, count);
        Assert.Contains("Duplicate field skipped", code);
    }

    [Fact]
    public void GenerateProxy_GetInstances_UsesGlobalPrefix()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Collections.Generic.List`1",
            Namespace = "System.Collections.Generic",
            Name = "List`1",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        // Must use global:: to avoid resolving to proxy namespace
        Assert.Contains("global::System.Collections.Generic.IEnumerable<", code);
    }

    [Fact]
    public void GenerateProxy_WithKnownBaseType_ExtendsBaseProxy()
    {
        var animal = new TypeMetadata
        {
            FullName = "MyApp.Animal",
            Namespace = "MyApp",
            Name = "Animal",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo { Name = "_age", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };
        var cat = new TypeMetadata
        {
            FullName = "MyApp.Cat",
            Namespace = "MyApp",
            Name = "Cat",
            BaseTypeName = "MyApp.Animal",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo { Name = "_age", TypeName = "int", Kind = FieldKind.Primitive },
                new FieldInfo { Name = "_isIndoor", TypeName = "bool", Kind = FieldKind.Primitive }
            ]
        };

        var code = _emitter.GenerateProxyCode(cat, allTypes: [animal, cat]);

        // Cat should extend Animal proxy, not _.System.Object
        Assert.Contains("class Cat : _.MyApp.Animal", code);
        // Cat should NOT re-emit inherited fields
        Assert.DoesNotContain("_name", code);
        Assert.DoesNotContain("_age", code);
        // Cat SHOULD have its own field
        Assert.Contains("public bool _isIndoor", code);
    }

    [Fact]
    public void GenerateProxy_BaseType_IsNotSealed()
    {
        var animal = new TypeMetadata
        {
            FullName = "MyApp.Animal",
            Namespace = "MyApp",
            Name = "Animal",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String }
            ]
        };
        var cat = new TypeMetadata
        {
            FullName = "MyApp.Cat",
            Namespace = "MyApp",
            Name = "Cat",
            BaseTypeName = "MyApp.Animal",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String }
            ]
        };

        var animalCode = _emitter.GenerateProxyCode(animal, allTypes: [animal, cat]);

        // Animal should NOT be sealed since Cat extends it
        Assert.DoesNotContain("sealed", animalCode);
        Assert.Contains("public class Animal : global::_.System.Object", animalCode);
    }

    [Fact]
    public void GenerateProxy_LeafType_IsSealed()
    {
        var animal = new TypeMetadata
        {
            FullName = "MyApp.Animal",
            Namespace = "MyApp",
            Name = "Animal",
            Fields = []
        };
        var cat = new TypeMetadata
        {
            FullName = "MyApp.Cat",
            Namespace = "MyApp",
            Name = "Cat",
            BaseTypeName = "MyApp.Animal",
            Fields = []
        };

        var catCode = _emitter.GenerateProxyCode(cat, allTypes: [animal, cat]);

        Assert.Contains("public sealed class Cat : _.MyApp.Animal", catCode);
    }

    [Fact]
    public void GenerateProxy_DerivedType_HasNewKeywordOnFactories()
    {
        var animal = new TypeMetadata
        {
            FullName = "MyApp.Animal",
            Namespace = "MyApp",
            Name = "Animal",
            Fields = []
        };
        var cat = new TypeMetadata
        {
            FullName = "MyApp.Cat",
            Namespace = "MyApp",
            Name = "Cat",
            BaseTypeName = "MyApp.Animal",
            Fields = []
        };

        var catCode = _emitter.GenerateProxyCode(cat, allTypes: [animal, cat]);

        Assert.Contains("public static new Cat FromAddress", catCode);
        Assert.Contains("public static new global::System.Collections.Generic.IEnumerable<Cat> GetInstances", catCode);
    }

    [Fact]
    public void GenerateProxy_ArrayField_BaseElementType_UsesResolverWithFallback()
    {
        var animal = new TypeMetadata
        {
            FullName = "MyApp.Animal",
            Namespace = "MyApp",
            Name = "Animal",
            Fields = []
        };
        var cat = new TypeMetadata
        {
            FullName = "MyApp.Cat",
            Namespace = "MyApp",
            Name = "Cat",
            BaseTypeName = "MyApp.Animal",
            Fields = []
        };
        var owner = new TypeMetadata
        {
            FullName = "MyApp.Owner",
            Namespace = "MyApp",
            Name = "Owner",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_pets",
                    TypeName = "MyApp.Animal[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "MyApp.Animal",
                    ArrayElementKind = FieldKind.ObjectReference
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(owner, allTypes: [animal, cat, owner]);

        Assert.Contains("DumpArray<_.MyApp.Animal?>?", code);
        // Uses resolver with fallback for base types
        Assert.Contains("global::_.ProxyResolver.Resolve(ea, _ctx) as _.MyApp.Animal ?? _.MyApp.Animal.FromAddress(ea, _ctx)", code);
    }

    [Fact]
    public void GenerateProxy_ArrayField_LeafElementType_UsesDirectFromAddress()
    {
        var order = new TypeMetadata
        {
            FullName = "MyApp.Order",
            Namespace = "MyApp",
            Name = "Order",
            Fields = []
        };
        var owner = new TypeMetadata
        {
            FullName = "MyApp.Owner",
            Namespace = "MyApp",
            Name = "Owner",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_orders",
                    TypeName = "MyApp.Order[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "MyApp.Order",
                    ArrayElementKind = FieldKind.ObjectReference
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(owner, allTypes: [order, owner]);

        Assert.Contains("DumpArray<_.MyApp.Order?>?", code);
        // Leaf types use direct FromAddress (no resolver)
        Assert.Contains("_.MyApp.Order.FromAddress(ea, _ctx)", code);
        Assert.DoesNotContain("ProxyResolver", code);
    }

    [Fact]
    public void GenerateProxy_ObjectRefField_BaseType_UsesResolverWithFallback()
    {
        var animal = new TypeMetadata
        {
            FullName = "MyApp.Animal",
            Namespace = "MyApp",
            Name = "Animal",
            Fields = []
        };
        var cat = new TypeMetadata
        {
            FullName = "MyApp.Cat",
            Namespace = "MyApp",
            Name = "Cat",
            BaseTypeName = "MyApp.Animal",
            Fields = []
        };
        var owner = new TypeMetadata
        {
            FullName = "MyApp.Owner",
            Namespace = "MyApp",
            Name = "Owner",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_pet",
                    TypeName = "MyApp.Animal",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "MyApp.Animal"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(owner, allTypes: [animal, cat, owner]);

        Assert.Contains("public _.MyApp.Animal? _pet => Field<_.MyApp.Animal>();", code);
    }

    [Fact]
    public void GenerateProxy_SystemObject_IsRootProxy()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("namespace _.System;", code);
        Assert.Contains("public class Object", code);
        // Should declare _objAddress and _ctx fields
        Assert.Contains("protected readonly ulong _objAddress;", code);
        Assert.Contains("protected readonly DumpContext _ctx;", code);
        Assert.Contains("public ulong GetObjAddress() => _objAddress;", code);
        // Should NOT extend any base class
        Assert.DoesNotContain(": global::", code);
        Assert.DoesNotContain("sealed", code);
    }

    [Fact]
    public void GenerateProxy_SystemString_HasImplicitOperatorAndToString()
    {
        var sysObj = new TypeMetadata { FullName = "System.Object", Namespace = "System", Name = "Object", Fields = [] };
        var type = new TypeMetadata
        {
            FullName = "System.String",
            Namespace = "System",
            Name = "String",
            BaseTypeName = "System.Object",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type, allTypes: [sysObj, type]);

        Assert.Contains("namespace _.System;", code);
        Assert.Contains("class String : global::_.System.Object", code);
        Assert.Contains("public string? Value => _ctx.GetStringValue(_objAddress);", code);
        Assert.Contains("public static implicit operator string?(String? proxy)", code);
        Assert.Contains("public override string ToString()", code);
        Assert.Contains("public static new String FromAddress", code);
        Assert.Contains("public static new global::System.Collections.Generic.IEnumerable<String> GetInstances", code);
    }

    [Fact]
    public void GenerateProxy_SystemString_IsSealed_WhenNoSubtypes()
    {
        var type = new TypeMetadata
        {
            FullName = "System.String",
            Namespace = "System",
            Name = "String",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public sealed class String", code);
    }

    [Fact]
    public void GenerateProxy_NestedType_EmitsNestedClass()
    {
        var type = new TypeMetadata
        {
            FullName = "System.RuntimeType+RuntimeTypeCache",
            Namespace = "System",
            Name = "RuntimeType+RuntimeTypeCache",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("namespace _.System;", code);
        // Outer class is partial (shell for nesting)
        Assert.Contains("public partial class RuntimeType", code);
        // Inner class uses the leaf name only
        Assert.Contains("public sealed class RuntimeTypeCache : global::_.System.Object", code);
        // Factories use the leaf name
        Assert.Contains("RuntimeTypeCache FromAddress(ulong address, DumpContext ctx)", code);
        Assert.Contains("new RuntimeTypeCache(addr, ctx)", code);
        // EnumerateInstances still uses the full CLR name
        Assert.Contains("ctx.EnumerateInstances(\"System.RuntimeType+RuntimeTypeCache\")", code);
    }

    [Fact]
    public void GenerateProxy_DeeplyNestedType_EmitsMultipleLevels()
    {
        var type = new TypeMetadata
        {
            FullName = "Interop+Kernel32+ProcessWaitHandle",
            Namespace = "",
            Name = "Interop+Kernel32+ProcessWaitHandle",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("namespace _;", code);
        Assert.Contains("public partial class Interop", code);
        Assert.Contains("public partial class Kernel32", code);
        Assert.Contains("public sealed class ProcessWaitHandle : global::_.System.Object", code);
    }

    [Fact]
    public void GenerateProxy_NestingContainerType_IsPartial()
    {
        var runtimeType = new TypeMetadata
        {
            FullName = "System.RuntimeType",
            Namespace = "System",
            Name = "RuntimeType",
            Fields = []
        };
        var nested = new TypeMetadata
        {
            FullName = "System.RuntimeType+ActivatorCache",
            Namespace = "System",
            Name = "RuntimeType+ActivatorCache",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(runtimeType, allTypes: [runtimeType, nested]);

        // RuntimeType is a nesting container, so it must be partial
        Assert.Contains("partial class RuntimeType", code);
    }

    [Fact]
    public void GenerateProxy_GenericWithNestedTypeArg_DoesNotNest()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Diagnostics.Tracing.EventSource+OverrideEventProvider>",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Diagnostics.Tracing.EventSource+OverrideEventProvider>",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        // The + is inside <>, so this should NOT be nested
        Assert.DoesNotContain("partial class", code);
        Assert.Contains("public sealed class Dictionary_System_String__System_Diagnostics_Tracing_EventSource_OverrideEventProvider_", code);
    }

    [Fact]
    public void GenerateProxy_NestedType_ReferenceUsesNestedSyntax()
    {
        var runtimeType = new TypeMetadata
        {
            FullName = "System.RuntimeType",
            Namespace = "System",
            Name = "RuntimeType",
            Fields = []
        };
        var cache = new TypeMetadata
        {
            FullName = "System.RuntimeType+ActivatorCache",
            Namespace = "System",
            Name = "RuntimeType+ActivatorCache",
            Fields = []
        };
        var owner = new TypeMetadata
        {
            FullName = "MyApp.Foo",
            Namespace = "MyApp",
            Name = "Foo",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_cache",
                    TypeName = "System.RuntimeType+ActivatorCache",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "System.RuntimeType+ActivatorCache"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(owner, allTypes: [runtimeType, cache, owner]);

        // Reference to nested type uses dot-separated C# syntax
        Assert.Contains("_.System.RuntimeType.ActivatorCache", code);
    }

    [Fact]
    public void SplitNestingParts_SimpleType_ReturnsSinglePart()
    {
        Assert.Equal(["Customer"], ProxyEmitter.SplitNestingParts("Customer"));
    }

    [Fact]
    public void SplitNestingParts_NestedType_SplitsOnPlus()
    {
        Assert.Equal(["RuntimeType", "ActivatorCache"], ProxyEmitter.SplitNestingParts("RuntimeType+ActivatorCache"));
    }

    [Fact]
    public void SplitNestingParts_DeeplyNested_SplitsAllLevels()
    {
        Assert.Equal(["Interop", "Kernel32", "ProcessWaitHandle"], ProxyEmitter.SplitNestingParts("Interop+Kernel32+ProcessWaitHandle"));
    }

    [Fact]
    public void SplitNestingParts_PlusInsideGenerics_DoesNotSplit()
    {
        var name = "Dictionary<String, EventSource+OverrideEventProvider>";
        Assert.Equal([name], ProxyEmitter.SplitNestingParts(name));
    }

    [Fact]
    public void SplitNestingParts_NestedWithGenerics_SplitsCorrectly()
    {
        // Task+<>c — the + is before the generic markers
        Assert.Equal(["Task", "<>c"], ProxyEmitter.SplitNestingParts("Task+<>c"));
    }

    [Fact]
    public void GetProxyTypeName_NestedType_UsesPlusSeparator()
    {
        var type = new TypeMetadata
        {
            FullName = "System.RuntimeType+ActivatorCache",
            Namespace = "System",
            Name = "RuntimeType+ActivatorCache",
            Fields = []
        };

        // CLR reflection uses + for nested types
        Assert.Equal("_.System.RuntimeType+ActivatorCache", ProxyEmitter.GetProxyTypeName(type));
    }

    [Fact]
    public void GetProxyTypeName_DeeplyNestedNoNamespace_UsesPlusSeparator()
    {
        var type = new TypeMetadata
        {
            FullName = "Interop+Kernel32+ProcessWaitHandle",
            Namespace = "",
            Name = "Interop+Kernel32+ProcessWaitHandle",
            Fields = []
        };

        Assert.Equal("_.Interop+Kernel32+ProcessWaitHandle", ProxyEmitter.GetProxyTypeName(type));
    }
}
