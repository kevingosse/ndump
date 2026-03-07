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

        Assert.Contains("public string? _name => _ctx.GetStringField(_objAddress, \"_name\");", code);
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

        Assert.Contains("public int _age => _ctx.GetFieldValue<int>(_objAddress, \"_age\");", code);
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

        Assert.Contains("public _.MyApp.Order? _lastOrder", code);
        // Order is a leaf type (no subtypes), so uses direct FromAddress
        Assert.Contains("_.MyApp.Order.FromAddress(addr, _ctx)", code);
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

        Assert.Contains("public ulong _unknown => _ctx.GetObjectAddress", code);
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
        Assert.Contains("_ctx.GetObjectAddress(_objAddress,", code);
        Assert.Contains("_ctx.GetArrayLength(addr)", code);
        // Order is a leaf type (no subtypes), so uses direct FromAddress
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
        Assert.Contains("_ctx.ResolveProxy(ea) as _.System.Object ?? _.System.Object.FromAddress(ea, _ctx)", code);
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
        Assert.Contains("_ctx.ResolveProxy(ea) as _.MyApp.Animal ?? _.MyApp.Animal.FromAddress(ea, _ctx)", code);
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
        Assert.DoesNotContain("ResolveProxy", code);
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

        Assert.Contains("_ctx.ResolveProxy(addr) as _.MyApp.Animal ?? _.MyApp.Animal.FromAddress(addr, _ctx)", code);
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
    public void GenerateProxy_NestedType_SanitizesName()
    {
        var type = new TypeMetadata
        {
            FullName = "System.RuntimeType+RuntimeTypeCache",
            Namespace = "System",
            Name = "RuntimeType+RuntimeTypeCache",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public sealed class RuntimeType_RuntimeTypeCache", code);
        Assert.Contains("namespace _.System;", code);
    }
}
