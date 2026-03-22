namespace Ndump.Core.Tests;

public class ProxyEmitterTests
{
    private readonly ProxyEmitter _emitter = new();

    [Test]
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

        code.ShouldContain("public sealed class Customer : global::_.System.Object");
        code.ShouldContain("Customer(ulong address, DumpContext context) : base(address, context) { }");
    }

    [Test]
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

        code.ShouldContain("public string? _name => Field<string>();");
    }

    [Test]
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

        code.ShouldContain("public int _age => Field<int>();");
    }

    [Test]
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

        code.ShouldContain("public global::_.MyApp.Order? _lastOrder => Field<global::_.MyApp.Order>();");
    }

    [Test]
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
        code.ShouldContain("public string? _name => Field<string>();");
        // Primitive uses Field<T>
        code.ShouldContain("public int _age => Field<int>();");
        // Backing field uses Field<T> with explicit name
        code.ShouldContain("public bool IsActive => Field<bool>(\"<IsActive>k__BackingField\");");
        // Known reference type uses Field<ProxyType>
        code.ShouldContain("public global::_.MyApp.Address? _address => Field<global::_.MyApp.Address>();");
        // Unknown reference type falls back to _.System.Object
        code.ShouldContain("public global::_.System.Object? _widget => Field<global::_.System.Object>();");
    }

    [Test]
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

        code.ShouldContain("public global::_.System.Object? _unknown => Field<global::_.System.Object>();");
    }

    [Test]
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

        code.ShouldContain("public static new Tag FromAddress(ulong address, DumpContext context)");
    }

    [Test]
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

        code.ShouldContain("public static new global::System.Collections.Generic.IEnumerable<Tag> GetInstances(DumpContext context)");
        code.ShouldContain("context.EnumerateInstances(\"MyApp.Tag\")");
    }

    [Test]
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

        // GetObjectAddress is inherited from _.System.Object, not emitted directly
        code.ShouldNotContain("public ulong GetObjectAddress()");
        code.ShouldContain(": global::_.System.Object");
    }

    [Test]
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
        code.ShouldContain("public int Value =>");
    }

    [Test]
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

        code.ShouldContain("namespace _.MyApp;");
    }

    [Test]
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

        code.ShouldContain("namespace _.System.Text;");
    }

    [Test]
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

        code.ShouldContain("namespace _;");
    }

    [Test]
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

        code.ShouldContain("public sealed class List_1");
        code.ShouldContain("namespace _.System.Collections.Generic;");
    }

    [Test]
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

        code.ShouldContain("public global::_.App.Orders.Order? _order");
    }

    [Test]
    public void GetProxyNamespace_WithNamespace_PrependsDot()
    {
        ProxyEmitter.GetProxyNamespace("System.Text").ShouldBe("_.System.Text");
    }

    [Test]
    public void GetProxyNamespace_Empty_ReturnsUnderscore()
    {
        ProxyEmitter.GetProxyNamespace("").ShouldBe("_");
    }

    [Test]
    public void GetProxyNamespace_SanitizesInvalidCharsInSegments()
    {
        // CLR namespaces from compiled XAML can contain '!' and ':'
        ProxyEmitter.GetProxyNamespace("CompiledAvaloniaXaml.!AvaloniaResources.NamespaceInfo:")
            .ShouldBe("_.CompiledAvaloniaXaml._AvaloniaResources.NamespaceInfo_");
    }

    [Test]
    public void SanitizeTypeName_HandlesArrayBrackets()
    {
        ProxyEmitter.SanitizeTypeName("String[]").ShouldBe("String__");
    }

    [Test]
    public void SanitizeTypeName_HandlesBacktick()
    {
        ProxyEmitter.SanitizeTypeName("List`1").ShouldBe("List_1");
    }

    [Test]
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

        code.ShouldContain("public global::Ndump.Core.DumpArray<global::_.MyApp.Order?>? _orderHistory => ArrayField<global::_.MyApp.Order?>()");
    }

    [Test]
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

        code.ShouldContain("public global::Ndump.Core.DumpArray<ulong>? _items => ArrayAddresses()");
    }

    [Test]
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

        code.ShouldContain("public global::Ndump.Core.DumpArray<string?>? _names => ArrayField<string?>()");
    }

    [Test]
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

        code.ShouldContain("public global::Ndump.Core.DumpArray<int>? _scores => ArrayField<int>()");
    }

    [Test]
    public void GenerateProxy_ArrayField_DoubleElements_EmitsDoubleArray()
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
                    Name = "_values",
                    TypeName = "System.Double[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "System.Double",
                    ArrayElementKind = FieldKind.Primitive
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public global::Ndump.Core.DumpArray<double>? _values => ArrayField<double>()");
    }

    [Test]
    public void GenerateProxy_ArrayField_BoolElements_EmitsBoolArray()
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
                    Name = "_flags",
                    TypeName = "System.Boolean[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "System.Boolean",
                    ArrayElementKind = FieldKind.Primitive
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public global::Ndump.Core.DumpArray<bool>? _flags => ArrayField<bool>()");
    }

    [Test]
    public void GenerateProxy_ArrayField_ObjectElements_UsesSystemObjectProxy()
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

        code.ShouldContain("public global::Ndump.Core.DumpArray<global::_.System.Object?>? _mixedItems => ArrayField<global::_.System.Object?>()");
    }

    [Test]
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
        count.ShouldBe(1);
        code.ShouldContain("Duplicate field skipped");
    }

    [Test]
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
        code.ShouldContain("global::System.Collections.Generic.IEnumerable<");
    }

    [Test]
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
        code.ShouldContain("class Cat : global::_.MyApp.Animal");
        // Cat should NOT re-emit inherited fields
        code.ShouldNotContain("_name");
        code.ShouldNotContain("_age");
        // Cat SHOULD have its own field
        code.ShouldContain("public bool _isIndoor");
    }

    [Test]
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
        animalCode.ShouldNotContain("sealed");
        animalCode.ShouldContain("public class Animal : global::_.System.Object");
    }

    [Test]
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

        catCode.ShouldContain("public sealed class Cat : global::_.MyApp.Animal");
    }

    [Test]
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

        catCode.ShouldContain("public static new Cat FromAddress");
        catCode.ShouldContain("public static new global::System.Collections.Generic.IEnumerable<Cat> GetInstances");
    }

    [Test]
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

        code.ShouldContain("DumpArray<global::_.MyApp.Animal?>? _pets => ArrayField<global::_.MyApp.Animal?>()");
    }

    [Test]
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

        code.ShouldContain("DumpArray<global::_.MyApp.Order?>? _orders => ArrayField<global::_.MyApp.Order?>()");
    }

    [Test]
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

        code.ShouldContain("public global::_.MyApp.Animal? _pet => Field<global::_.MyApp.Animal>();");
    }

    [Test]
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

        code.ShouldContain("namespace _.System;");
        code.ShouldContain("public class Object");
        // Should declare _objAddress and _context fields
        code.ShouldContain("protected readonly ulong _objAddress;");
        code.ShouldContain("protected readonly DumpContext _context;");
        code.ShouldContain("public ulong GetObjectAddress() => _objAddress;");
        // Should NOT extend any base class — class declaration has no base
        System.Text.RegularExpressions.Regex.IsMatch(code, @"public class Object\r?\n").ShouldBeTrue();
        code.ShouldNotContain("sealed");
    }

    [Test]
    public void GenerateProxy_SystemObject_InteriorPath_UsesProxyResolver()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        // Field<T> passes _interiorTypeName as optional parameter to DumpContext methods
        code.ShouldContain("_context.GetFieldValue<T>(_objAddress, fieldName, _interiorTypeName)");
        code.ShouldContain("_context.GetStringField(_objAddress, fieldName, _interiorTypeName)");
        code.ShouldContain("_context.GetObjectAddress(_objAddress, fieldName, _interiorTypeName)");
    }

    [Test]
    public void GenerateProxy_SystemObject_Field_UsesProxyResolver()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        // Field<T> and ReadArrayElement<T> should both use ProxyResolver.Resolve<T>
        CountOccurrences(code, "return global::_.ProxyResolver.Resolve<T>(addr, _context);").ShouldBe(2);
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0, index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    // ───── SanitizeTypeName: reserved characters ─────

    [Test]
    public void SanitizeTypeName_HandlesAngleBrackets()
    {
        ProxyEmitter.SanitizeTypeName("<>c").ShouldBe("__c");
    }

    [Test]
    public void SanitizeTypeName_HandlesDash()
    {
        ProxyEmitter.SanitizeTypeName("my-type").ShouldBe("my_type");
    }

    [Test]
    public void SanitizeTypeName_HandlesPipe()
    {
        ProxyEmitter.SanitizeTypeName("A|B").ShouldBe("A_B");
    }

    [Test]
    public void SanitizeTypeName_HandlesSlashAndBackslash()
    {
        ProxyEmitter.SanitizeTypeName("A/B\\C").ShouldBe("A_B_C");
    }

    [Test]
    public void SanitizeTypeName_HandlesColon()
    {
        ProxyEmitter.SanitizeTypeName("A:B").ShouldBe("A_B");
    }

    [Test]
    public void SanitizeTypeName_HandlesDollarAndAt()
    {
        ProxyEmitter.SanitizeTypeName("$Foo@Bar").ShouldBe("_Foo_Bar");
    }

    [Test]
    public void SanitizeTypeName_HandlesEquals()
    {
        ProxyEmitter.SanitizeTypeName("A=B").ShouldBe("A_B");
    }

    [Test]
    public void SanitizeTypeName_HandlesPlus()
    {
        ProxyEmitter.SanitizeTypeName("Outer+Inner").ShouldBe("Outer_Inner");
    }

    [Test]
    public void SanitizeTypeName_HandlesDot()
    {
        ProxyEmitter.SanitizeTypeName("System.Object").ShouldBe("System_Object");
    }

    [Test]
    public void SanitizeTypeName_HandlesCommaAndSpace()
    {
        ProxyEmitter.SanitizeTypeName("Dictionary<String, Int32>").ShouldBe("Dictionary_String__Int32_");
    }

    [Test]
    public void SanitizeTypeName_HandlesStarAndQuestion()
    {
        ProxyEmitter.SanitizeTypeName("A*B?C").ShouldBe("A_B_C");
    }

    [Test]
    public void SanitizeTypeName_HandlesExclamationMark()
    {
        ProxyEmitter.SanitizeTypeName("CompiledAvaloniaXaml_!AvaloniaResources").ShouldBe("CompiledAvaloniaXaml__AvaloniaResources");
    }

    [Test]
    public void SanitizeTypeName_StripsExoticCharacters()
    {
        // Characters not in the explicit replace list should still be sanitized
        ProxyEmitter.SanitizeTypeName("A~B#C%D").ShouldBe("A_B_C_D");
    }

    // ───── SanitizeNestedSuffix (tested via proxy codegen) ─────

    [Test]
    public void GenerateProxy_NestedType_MultiLevelSuffix_SanitizesWithDots()
    {
        // A nested type like Outer+Mid+Inner where Mid+Inner is the nested suffix
        var type = new TypeMetadata
        {
            FullName = "MyApp.Outer+Mid+Inner",
            Namespace = "MyApp",
            Name = "Outer+Mid+Inner",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        // Should produce nested partial wrappers
        code.ShouldContain("public partial class Outer");
        code.ShouldContain("public partial class Mid");
        code.ShouldContain("public sealed class Inner : global::_.System.Object");
    }

    // ───── TruncateIdentifier ─────

    [Test]
    public void TruncateIdentifier_ShortName_Unchanged()
    {
        ProxyEmitter.TruncateIdentifier("Customer").ShouldBe("Customer");
    }

    [Test]
    public void TruncateIdentifier_ExactlyAtLimit_Unchanged()
    {
        var name = new string('A', 450);
        ProxyEmitter.TruncateIdentifier(name).ShouldBe(name);
    }

    [Test]
    public void TruncateIdentifier_OverLimit_TruncatesWithHash()
    {
        var name = new string('A', 500);
        var result = ProxyEmitter.TruncateIdentifier(name);
        result.Length.ShouldBeLessThanOrEqualTo(450);
        result.ShouldContain("_"); // hash separator
        // Should be deterministic
        ProxyEmitter.TruncateIdentifier(name).ShouldBe(result);
    }

    [Test]
    public void TruncateIdentifier_DifferentLongNames_ProduceDifferentResults()
    {
        var name1 = new string('A', 500);
        var name2 = new string('B', 500);
        ProxyEmitter.TruncateIdentifier(name1).ShouldNotBe(ProxyEmitter.TruncateIdentifier(name2));
    }

    // ───── TruncateFileName ─────

    [Test]
    public void TruncateFileName_ShortName_Unchanged()
    {
        ProxyEmitter.TruncateFileName("MyApp_Customer.g.cs").ShouldBe("MyApp_Customer.g.cs");
    }

    [Test]
    public void TruncateFileName_OverLimit_TruncatesWithHashAndExtension()
    {
        var name = new string('A', 250) + ".g.cs";
        var result = ProxyEmitter.TruncateFileName(name);
        result.Length.ShouldBeLessThanOrEqualTo(200);
        result.ShouldEndWith(".g.cs");
        // Should be deterministic
        ProxyEmitter.TruncateFileName(name).ShouldBe(result);
    }

    [Test]
    public void TruncateFileName_DifferentLongNames_ProduceDifferentResults()
    {
        var name1 = new string('A', 250) + ".g.cs";
        var name2 = new string('B', 250) + ".g.cs";
        ProxyEmitter.TruncateFileName(name1).ShouldNotBe(ProxyEmitter.TruncateFileName(name2));
    }

    // ───── Compiler-generated types with reserved chars ─────

    [Test]
    public void GenerateProxy_CompilerGeneratedType_AngleBracketsAreSanitized()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Task+<>c",
            Namespace = "MyApp",
            Name = "Task+<>c",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("namespace _.MyApp;");
        code.ShouldContain("public partial class Task");
        code.ShouldContain("public sealed class __c : global::_.System.Object");
    }

    [Test]
    public void GenerateProxy_CompilerGeneratedStateMachine_Sanitized()
    {
        // State machine types like <ReadAsync>d__5
        var type = new TypeMetadata
        {
            FullName = "MyApp.Reader+<ReadAsync>d__5",
            Namespace = "MyApp",
            Name = "Reader+<ReadAsync>d__5",
            Fields =
            [
                new FieldInfo { Name = "<>1__state", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("namespace _.MyApp;");
        code.ShouldContain("public partial class Reader");
        // Angle brackets and backtick-like chars should be sanitized
        code.ShouldContain("class _ReadAsync_d__5 : global::_.System.Object");
        // Field name with angle brackets should produce clean property
        code.ShouldContain("public int __1__state =>");
    }

    // ───── Value type proxy: IProxy + FromInterior ─────

    [Test]
    public void GenerateProxy_ValueType_ImplementsIProxy()
    {
        var type = new TypeMetadata
        {
            FullName = "System.DateTime",
            Namespace = "System",
            Name = "DateTime",
            Fields = [],
            IsValueType = true
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("global::Ndump.Core.IProxy<DateTime>");
        code.ShouldContain("FromInterior(ulong address, DumpContext context, string interiorTypeName)");
        // Value types should NOT have GetInstances (they're not heap objects)
        code.ShouldNotContain("GetInstances");
    }

    [Test]
    public void GenerateProxy_ValueType_ViaBaseTypeName_ImplementsIProxy()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.MyStruct",
            Namespace = "MyApp",
            Name = "MyStruct",
            Fields = [],
            BaseTypeName = "System.ValueType"
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("global::Ndump.Core.IProxy<MyStruct>");
    }

    [Test]
    public void GenerateProxy_EnumType_ImplementsIProxy()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Color",
            Namespace = "MyApp",
            Name = "Color",
            Fields = [],
            BaseTypeName = "System.Enum"
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("global::Ndump.Core.IProxy<Color>");
    }

    // ───── Value type discovery from field analysis ─────

    [Test]
    public void GenerateProxy_ValueTypeDiscoveredFromFieldKind_NestedGenericGetsIProxy()
    {
        // A nested type inside a generic parent isn't marked IsValueType=true,
        // but another type's field references it as ValueType. The field-based discovery
        // should cause the nested generic proxy to implement IProxy.
        var dictType = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Int32>",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Int32>",
            Fields = [],
            GenericDefinitionName = "Dictionary",
            GenericDefinitionFullName = "System.Collections.Generic.Dictionary",
            GenericTypeArguments = ["System.String", "System.Int32"]
        };
        var entryType = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Int32>+Entry",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Int32>+Entry",
            Fields = [],
            IsValueType = false // NOT marked as value type by ClrMD heap discovery
        };
        var holder = new TypeMetadata
        {
            FullName = "MyApp.Holder",
            Namespace = "MyApp",
            Name = "Holder",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_entry",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    // Field analysis reveals it's a value type
                    ReferenceTypeName = "System.Collections.Generic.Dictionary<System.String, System.Int32>+Entry"
                }
            ]
        };

        var emitter = new ProxyEmitter();
        var allTypes = new TypeMetadata[] { dictType, entryType, holder };
        var dir = Path.Combine(Path.GetTempPath(), $"ndump_test_{Guid.NewGuid():N}");
        var files = emitter.EmitProxies(allTypes, dir);

        try
        {
            var entryFile = files.FirstOrDefault(f => f.Contains("Entry"));
            entryFile.ShouldNotBeNull();
            var code = File.ReadAllText(entryFile);

            // Entry should implement IProxy because field analysis discovered it as value type
            code.ShouldContain("global::Ndump.Core.IProxy<Entry>");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Test]
    public void GenerateProxy_ValueTypeDiscoveredFromArrayElement_NestedGenericGetsIProxy()
    {
        var dictType = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Int32>",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Int32>",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_entries",
                    TypeName = "System.Collections.Generic.Dictionary<System.String, System.Int32>+Entry[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "System.Collections.Generic.Dictionary<System.String, System.Int32>+Entry",
                    ArrayElementKind = FieldKind.ValueType
                }
            ],
            GenericDefinitionName = "Dictionary",
            GenericDefinitionFullName = "System.Collections.Generic.Dictionary",
            GenericTypeArguments = ["System.String", "System.Int32"]
        };
        var entryType = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Int32>+Entry",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Int32>+Entry",
            Fields = [],
            IsValueType = false // Not marked by ClrMD
        };

        var emitter = new ProxyEmitter();
        var allTypes = new TypeMetadata[] { dictType, entryType };
        var dir = Path.Combine(Path.GetTempPath(), $"ndump_test_{Guid.NewGuid():N}");
        var files = emitter.EmitProxies(allTypes, dir);

        try
        {
            var entryFile = files.FirstOrDefault(f => f.Contains("Entry"));
            entryFile.ShouldNotBeNull();
            var code = File.ReadAllText(entryFile);

            code.ShouldContain("global::Ndump.Core.IProxy<Entry>");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ───── Base type naming conflict (CS0122 avoidance) ─────

    [Test]
    public void GenerateProxy_BaseTypeNamingConflict_FallsBackToObject()
    {
        // If base type "Control" has a nested type "Control+Button", and the derived
        // type is also named "Button", there's a constructor ambiguity. Should fall back to Object.
        var control = new TypeMetadata
        {
            FullName = "UI.Control",
            Namespace = "UI",
            Name = "Control",
            Fields = []
        };
        var controlButton = new TypeMetadata
        {
            FullName = "UI.Control+Button",
            Namespace = "UI",
            Name = "Control+Button",
            Fields = []
        };
        var button = new TypeMetadata
        {
            FullName = "UI.Button",
            Namespace = "UI",
            Name = "Button",
            BaseTypeName = "UI.Control",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(button, allTypes: [control, controlButton, button]);

        // Should NOT extend Control (naming conflict with Control.Button)
        code.ShouldContain("class Button : global::_.System.Object");
        code.ShouldNotContain("class Button : _.UI.Control");
    }

    [Test]
    public void GenerateProxy_BaseTypeNoConflict_ExtendsBaseProxy()
    {
        // When there's no naming conflict, normal inheritance should work
        var control = new TypeMetadata
        {
            FullName = "UI.Control",
            Namespace = "UI",
            Name = "Control",
            Fields = []
        };
        var button = new TypeMetadata
        {
            FullName = "UI.Button",
            Namespace = "UI",
            Name = "Button",
            BaseTypeName = "UI.Control",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(button, allTypes: [control, button]);

        code.ShouldContain("class Button : global::_.UI.Control");
    }

    // ───── __Canon references in generic proxies ─────

    [Test]
    public void GenerateProxy_CanonFieldReference_MapsToSystemObject()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Wrapper<MyApp.Item>",
            Namespace = "MyApp",
            Name = "Wrapper<MyApp.Item>",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_value",
                    TypeName = "System.__Canon",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "System.__Canon"
                }
            ],
            GenericDefinitionName = "Wrapper",
            GenericDefinitionFullName = "MyApp.Wrapper",
            GenericTypeArguments = ["MyApp.Item"]
        };

        var code = _emitter.GenerateProxyCode(type);

        // __Canon should be mapped to _.System.Object, not cause a missing type error
        code.ShouldContain("global::_.System.Object?");
        code.ShouldNotContain("__Canon");
    }

    // ───── Nested generic type proxy ─────

    [Test]
    public void GenerateProxy_NestedInsideGenericParent_WrappedInGenericPartialClass()
    {
        var dictType = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Int32>",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Int32>",
            Fields = [],
            GenericDefinitionName = "Dictionary",
            GenericDefinitionFullName = "System.Collections.Generic.Dictionary",
            GenericTypeArguments = ["System.String", "System.Int32"]
        };
        var entryType = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Int32>+Entry",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Int32>+Entry",
            Fields =
            [
                new FieldInfo { Name = "_hashCode", TypeName = "int", Kind = FieldKind.Primitive }
            ],
            IsValueType = true
        };

        var emitter = new ProxyEmitter();
        var allTypes = new TypeMetadata[] { dictType, entryType };
        var files = emitter.EmitProxies(allTypes, Path.Combine(Path.GetTempPath(), $"ndump_test_{Guid.NewGuid():N}"));

        try
        {
            // Find the Entry proxy file
            var entryFile = files.FirstOrDefault(f => f.Contains("Entry"));
            entryFile.ShouldNotBeNull();
            var code = File.ReadAllText(entryFile);

            // Should be wrapped in generic partial class
            code.ShouldContain("public partial class Dictionary<T1, T2>");
            code.ShouldContain("class Entry : global::_.System.Object");
            code.ShouldContain("int _hashCode");
        }
        finally
        {
            var dir = Path.GetDirectoryName(files[0])!;
            Directory.Delete(dir, true);
        }
    }

    // ───── BuildNestingWrapperDecl: generic parent wrappers ─────

    [Test]
    public void GenerateProxy_NestedInGenericParent_CompilerGeneratedChild_SanitizedCorrectly()
    {
        // Type like FrugalMapBase<System.Int32>+<>c
        var parent = new TypeMetadata
        {
            FullName = "MyApp.FrugalMapBase<System.Int32>",
            Namespace = "MyApp",
            Name = "FrugalMapBase<System.Int32>",
            Fields = [],
            GenericDefinitionName = "FrugalMapBase",
            GenericDefinitionFullName = "MyApp.FrugalMapBase",
            GenericTypeArguments = ["System.Int32"]
        };
        var nested = new TypeMetadata
        {
            FullName = "MyApp.FrugalMapBase<System.Int32>+<>c",
            Namespace = "MyApp",
            Name = "FrugalMapBase<System.Int32>+<>c",
            Fields = []
        };

        var emitter = new ProxyEmitter();
        var allTypes = new TypeMetadata[] { parent, nested };
        var dir = Path.Combine(Path.GetTempPath(), $"ndump_test_{Guid.NewGuid():N}");
        var files = emitter.EmitProxies(allTypes, dir);

        try
        {
            var nestedFile = files.FirstOrDefault(f =>
                Path.GetFileName(f) != "ProxyResolver.g.cs" &&
                !Path.GetFileName(f)!.EndsWith("FrugalMapBase_1.g.cs"));

            nestedFile.ShouldNotBeNull();
            var code = File.ReadAllText(nestedFile);

            // Outer wrapper should be generic partial class
            code.ShouldContain("public partial class FrugalMapBase<T>");
            // Inner class should have sanitized name
            code.ShouldContain("class __c : global::_.System.Object");
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ───── SplitNestingParts: additional edge cases ─────

    [Test]
    public void SplitNestingParts_CompilerGeneratedNested_SplitsCorrectly()
    {
        // Compiler-generated display class
        ProxyEmitter.SplitNestingParts("Reader+<ReadAsync>d__5").ShouldBe(["Reader", "<ReadAsync>d__5"]);
    }

    [Test]
    public void SplitNestingParts_GenericParentWithNestedChild_SplitsCorrectly()
    {
        ProxyEmitter.SplitNestingParts("Dictionary<String, Int32>+Entry").ShouldBe(
            ["Dictionary<String, Int32>", "Entry"]);
    }

    [Test]
    public void SplitNestingParts_MultipleLevels_SplitsAll()
    {
        ProxyEmitter.SplitNestingParts("A+B+C+D").ShouldBe(["A", "B", "C", "D"]);
    }

    // ───── Value type struct field references ─────

    [Test]
    public void GenerateProxy_StructField_KnownValueType_EmitsStructField()
    {
        var dateTime = new TypeMetadata
        {
            FullName = "System.DateTime",
            Namespace = "System",
            Name = "DateTime",
            Fields = [],
            IsValueType = true
        };
        var order = new TypeMetadata
        {
            FullName = "MyApp.Order",
            Namespace = "MyApp",
            Name = "Order",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_created",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.DateTime"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(order, allTypes: [dateTime, order]);

        code.ShouldContain("global::_.System.DateTime _created => StructField<global::_.System.DateTime>(\"System.DateTime\");");
    }

    // ───── Compile tests for new patterns ─────

    [Test]
    public void Compile_ValueTypeProxy_Succeeds()
    {
        var compiler = new ProxyCompiler();
        var sysObj = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };
        var dateTime = new TypeMetadata
        {
            FullName = "System.DateTime",
            Namespace = "System",
            Name = "DateTime",
            Fields =
            [
                new FieldInfo { Name = "_ticks", TypeName = "long", Kind = FieldKind.Primitive }
            ],
            IsValueType = true,
            BaseTypeName = "System.Object"
        };

        var allTypes = new[] { sysObj, dateTime };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var code = _emitter.GenerateProxyCode(dateTime, allTypes: allTypes);
        var resolverCode = ProxyEmitter.GenerateProxyResolver();

        var result = compiler.CompileFromSource([sysObjCode, code, resolverCode]);
        result.IsSuccess.ShouldBeTrue(string.Join("\n", result.Errors));

        var proxyType = result.Assembly!.GetType("_.System.DateTime");
        proxyType.ShouldNotBeNull();

        // Should implement IProxy<DateTime>
        var iproxyType = proxyType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "IProxy`1");
        iproxyType.ShouldNotBeNull();
    }

    [Test]
    public void Compile_CompilerGeneratedType_Succeeds()
    {
        var compiler = new ProxyCompiler();
        var sysObj = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };
        var task = new TypeMetadata
        {
            FullName = "MyApp.Task",
            Namespace = "MyApp",
            Name = "Task",
            Fields = [],
            BaseTypeName = "System.Object"
        };
        var compilerGenerated = new TypeMetadata
        {
            FullName = "MyApp.Task+<>c",
            Namespace = "MyApp",
            Name = "Task+<>c",
            Fields = [],
            BaseTypeName = "System.Object"
        };

        var allTypes = new[] { sysObj, task, compilerGenerated };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var taskCode = _emitter.GenerateProxyCode(task, allTypes: allTypes);
        var genCode = _emitter.GenerateProxyCode(compilerGenerated, allTypes: allTypes);
        var resolverCode = ProxyEmitter.GenerateProxyResolver();

        var result = compiler.CompileFromSource([sysObjCode, taskCode, genCode, resolverCode]);
        result.IsSuccess.ShouldBeTrue(string.Join("\n", result.Errors));

        var proxyType = result.Assembly!.GetType("_.MyApp.Task+__c");
        proxyType.ShouldNotBeNull();
    }

    [Test]
    public void Compile_BaseTypeNamingConflict_Succeeds()
    {
        var compiler = new ProxyCompiler();
        var sysObj = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };
        var control = new TypeMetadata
        {
            FullName = "UI.Control",
            Namespace = "UI",
            Name = "Control",
            Fields = [],
            BaseTypeName = "System.Object"
        };
        var controlButton = new TypeMetadata
        {
            FullName = "UI.Control+Button",
            Namespace = "UI",
            Name = "Control+Button",
            Fields = [],
            BaseTypeName = "System.Object"
        };
        var button = new TypeMetadata
        {
            FullName = "UI.Button",
            Namespace = "UI",
            Name = "Button",
            BaseTypeName = "UI.Control",
            Fields = [],
        };

        var allTypes = new[] { sysObj, control, controlButton, button };
        var codes = allTypes.Select(t => _emitter.GenerateProxyCode(t, allTypes: allTypes)).ToList();
        codes.Add(ProxyEmitter.GenerateProxyResolver());

        var result = compiler.CompileFromSource(codes.ToArray());
        result.IsSuccess.ShouldBeTrue(string.Join("\n", result.Errors));
    }

    [Test]
    public void Compile_LongTypeName_Succeeds()
    {
        var compiler = new ProxyCompiler();
        var sysObj = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };
        var longName = "VeryLong" + new string('X', 500) + "TypeName";
        var longType = new TypeMetadata
        {
            FullName = $"MyApp.{longName}",
            Namespace = "MyApp",
            Name = longName,
            Fields = [],
            BaseTypeName = "System.Object"
        };

        var allTypes = new[] { sysObj, longType };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var code = _emitter.GenerateProxyCode(longType, allTypes: allTypes);
        var resolverCode = ProxyEmitter.GenerateProxyResolver();

        var result = compiler.CompileFromSource([sysObjCode, code, resolverCode]);
        result.IsSuccess.ShouldBeTrue(string.Join("\n", result.Errors));
    }

    [Test]
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

        code.ShouldContain("namespace _.System;");
        code.ShouldContain("class String : global::_.System.Object");
        code.ShouldContain("public string? Value => _context.GetStringValue(_objAddress);");
        code.ShouldContain("public static implicit operator string?(String? proxy)");
        code.ShouldContain("public override string ToString()");
        code.ShouldContain("public static new String FromAddress");
        code.ShouldContain("public static new global::System.Collections.Generic.IEnumerable<String> GetInstances");
    }

    [Test]
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

        code.ShouldContain("public sealed class String");
    }

    [Test]
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

        code.ShouldContain("namespace _.System;");
        // Outer class is partial (shell for nesting)
        code.ShouldContain("public partial class RuntimeType");
        // Inner class uses the leaf name only
        code.ShouldContain("public sealed class RuntimeTypeCache : global::_.System.Object");
        // Factories use the leaf name
        code.ShouldContain("RuntimeTypeCache FromAddress(ulong address, DumpContext context)");
        code.ShouldContain("yield return FromAddress(addr, context)");
        // EnumerateInstances still uses the full CLR name
        code.ShouldContain("context.EnumerateInstances(\"System.RuntimeType+RuntimeTypeCache\")");
    }

    [Test]
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

        code.ShouldContain("namespace _;");
        code.ShouldContain("public partial class Interop");
        code.ShouldContain("public partial class Kernel32");
        code.ShouldContain("public sealed class ProcessWaitHandle : global::_.System.Object");
    }

    [Test]
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
        code.ShouldContain("partial class RuntimeType");
    }

    [Test]
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
        code.ShouldNotContain("partial class");
        code.ShouldContain("public sealed class Dictionary_System_String__System_Diagnostics_Tracing_EventSource_OverrideEventProvider_");
    }

    [Test]
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
        code.ShouldContain("global::_.System.RuntimeType.ActivatorCache");
    }

    [Test]
    public void SplitNestingParts_SimpleType_ReturnsSinglePart()
    {
        ProxyEmitter.SplitNestingParts("Customer").ShouldBe(["Customer"]);
    }

    [Test]
    public void SplitNestingParts_NestedType_SplitsOnPlus()
    {
        ProxyEmitter.SplitNestingParts("RuntimeType+ActivatorCache").ShouldBe(["RuntimeType", "ActivatorCache"]);
    }

    [Test]
    public void SplitNestingParts_DeeplyNested_SplitsAllLevels()
    {
        ProxyEmitter.SplitNestingParts("Interop+Kernel32+ProcessWaitHandle").ShouldBe(["Interop", "Kernel32", "ProcessWaitHandle"
        ]);
    }

    [Test]
    public void SplitNestingParts_PlusInsideGenerics_DoesNotSplit()
    {
        var name = "Dictionary<String, EventSource+OverrideEventProvider>";
        ProxyEmitter.SplitNestingParts(name).ShouldBe([name]);
    }

    [Test]
    public void SplitNestingParts_NestedWithGenerics_SplitsCorrectly()
    {
        // Task+<>c — the + is before the generic markers
        ProxyEmitter.SplitNestingParts("Task+<>c").ShouldBe(["Task", "<>c"]);
    }

    [Test]
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
        ProxyEmitter.GetProxyTypeName(type).ShouldBe("_.System.RuntimeType+ActivatorCache");
    }

    [Test]
    public void GetProxyTypeName_DeeplyNestedNoNamespace_UsesPlusSeparator()
    {
        var type = new TypeMetadata
        {
            FullName = "Interop+Kernel32+ProcessWaitHandle",
            Namespace = "",
            Name = "Interop+Kernel32+ProcessWaitHandle",
            Fields = []
        };

        ProxyEmitter.GetProxyTypeName(type).ShouldBe("_.Interop+Kernel32+ProcessWaitHandle");
    }

    [Test]
    public void GenerateProxy_GenericType_EmitsGenericClass()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Collections.Generic.List<System.String>",
            Namespace = "System.Collections.Generic",
            Name = "List<System.String>",
            Fields =
            [
                new FieldInfo { Name = "_size", TypeName = "int", Kind = FieldKind.Primitive }
            ],
            GenericDefinitionName = "List",
            GenericDefinitionFullName = "System.Collections.Generic.List",
            GenericTypeArguments = ["System.String"]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("namespace _.System.Collections.Generic;");
        code.ShouldContain("public sealed class List<T> : global::_.System.Object");
        code.ShouldContain("private List(ulong address, DumpContext context) : base(address, context) { }");
        code.ShouldContain("public static new List<T> FromAddress(ulong address, DumpContext context)");
        code.ShouldContain("public int _size => Field<int>();");
    }

    [Test]
    public void GenerateProxy_GenericType_SubstitutesTypeArgForObjectRef()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Wrapper<MyApp.Order>",
            Namespace = "MyApp",
            Name = "Wrapper<MyApp.Order>",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_value",
                    TypeName = "MyApp.Order",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "MyApp.Order"
                },
                new FieldInfo { Name = "_count", TypeName = "int", Kind = FieldKind.Primitive }
            ],
            GenericDefinitionName = "Wrapper",
            GenericDefinitionFullName = "MyApp.Wrapper",
            GenericTypeArguments = ["MyApp.Order"]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public sealed class Wrapper<T> : global::_.System.Object");
        // _value type matches type arg MyApp.Order → substituted with T
        code.ShouldContain("public T? _value => Field<T>();");
        // _count is not a type arg → stays as int
        code.ShouldContain("public int _count => Field<int>();");
    }

    [Test]
    public void GenerateProxy_GenericType_MultipleTypeArgs()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Object>",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Object>",
            Fields =
            [
                new FieldInfo { Name = "_count", TypeName = "int", Kind = FieldKind.Primitive }
            ],
            GenericDefinitionName = "Dictionary",
            GenericDefinitionFullName = "System.Collections.Generic.Dictionary",
            GenericTypeArguments = ["System.String", "System.Object"]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public sealed class Dictionary<T1, T2> : global::_.System.Object");
        code.ShouldContain("public static new Dictionary<T1, T2> FromAddress");
    }

    [Test]
    public void GenerateProxy_GenericType_ArrayFieldSubstitutesTypeArg()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Collections.Generic.List<MyApp.Order>",
            Namespace = "System.Collections.Generic",
            Name = "List<MyApp.Order>",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_items",
                    TypeName = "MyApp.Order[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "MyApp.Order",
                    ArrayElementKind = FieldKind.ObjectReference
                },
                new FieldInfo { Name = "_size", TypeName = "int", Kind = FieldKind.Primitive }
            ],
            GenericDefinitionName = "List",
            GenericDefinitionFullName = "System.Collections.Generic.List",
            GenericTypeArguments = ["MyApp.Order"]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public sealed class List<T> : global::_.System.Object");
        // Array element type matches type arg → DumpArray<T>
        code.ShouldContain("DumpArray<T>?");
        code.ShouldContain("DumpArray<T>? _items => ArrayField<T>()");
    }

    [Test]
    public void GenerateProxy_GenericType_StringTypeArgSubstituted()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Box<System.String>",
            Namespace = "MyApp",
            Name = "Box<System.String>",
            Fields =
            [
                new FieldInfo { Name = "_value", TypeName = "string", Kind = FieldKind.String }
            ],
            GenericDefinitionName = "Box",
            GenericDefinitionFullName = "MyApp.Box",
            GenericTypeArguments = ["System.String"]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public sealed class Box<T> : global::_.System.Object");
        // _value is a String field and System.String is the type arg → T
        code.ShouldContain("public T _value => Field<T>();");
    }

    [Test]
    public void GetProxyTypeName_GenericType_UsesBacktickNotation()
    {
        var type = new TypeMetadata
        {
            FullName = "System.Collections.Generic.Dictionary<System.String, System.Object>",
            Namespace = "System.Collections.Generic",
            Name = "Dictionary<System.String, System.Object>",
            Fields = [],
            GenericDefinitionName = "Dictionary",
            GenericDefinitionFullName = "System.Collections.Generic.Dictionary",
            GenericTypeArguments = ["System.String", "System.Object"]
        };

        ProxyEmitter.GetProxyTypeName(type).ShouldBe("_.System.Collections.Generic.Dictionary`2");
    }

    [Test]
    public void GenerateProxy_ReferenceToGenericType_EmitsGenericProxyType()
    {
        var listType = new TypeMetadata
        {
            FullName = "System.Collections.Generic.List<MyApp.Order>",
            Namespace = "System.Collections.Generic",
            Name = "List<MyApp.Order>",
            Fields = [],
            GenericDefinitionName = "List",
            GenericDefinitionFullName = "System.Collections.Generic.List",
            GenericTypeArguments = ["MyApp.Order"]
        };
        var order = new TypeMetadata
        {
            FullName = "MyApp.Order",
            Namespace = "MyApp",
            Name = "Order",
            Fields = []
        };
        var owner = new TypeMetadata
        {
            FullName = "MyApp.Customer",
            Namespace = "MyApp",
            Name = "Customer",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_orders",
                    TypeName = "System.Collections.Generic.List<MyApp.Order>",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "System.Collections.Generic.List<MyApp.Order>"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(owner, allTypes: [listType, order, owner]);

        // Reference to a generic proxy should use proper generic syntax
        code.ShouldContain("global::_.System.Collections.Generic.List<global::_.MyApp.Order>");
    }

    [Test]
    public void ParseGenericName_SimpleGeneric_ExtractsDefinitionAndArgs()
    {
        var (def, args) = TypeInspector.ParseGenericName("Dictionary<System.String, System.Object>");
        def.ShouldBe("Dictionary");
        args.ShouldBe(["System.String", "System.Object"]);
    }

    [Test]
    public void ParseGenericName_NestedGeneric_PreservesInnerGeneric()
    {
        var (def, args) = TypeInspector.ParseGenericName("List<Func<EventSource>>");
        def.ShouldBe("List");
        args.ShouldBe(["Func<EventSource>"]);
    }

    [Test]
    public void ParseGenericName_NonGeneric_ReturnsNull()
    {
        var (def, args) = TypeInspector.ParseGenericName("Customer");
        def.ShouldBeNull();
        args.ShouldBeEmpty();
    }

    [Test]
    public void GenerateProxy_NullablePrimitiveField_EmitsNullableProperty()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Order",
            Namespace = "MyApp",
            Name = "Order",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_rating",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.Nullable<T1>",
                    NullableInnerTypeName = "System.Int32"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public int? _rating => NullableField<int>();");
    }

    [Test]
    public void GenerateProxy_NullablePrimitiveField_BackingField_EmitsNullableWithName()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Order",
            Namespace = "MyApp",
            Name = "Order",
            Fields =
            [
                new FieldInfo
                {
                    Name = "<Rating>k__BackingField",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.Nullable<T1>",
                    NullableInnerTypeName = "System.Int32"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public int? Rating => NullableField<int>(\"<Rating>k__BackingField\");");
    }

    [Test]
    public void GenerateProxy_NullableStructField_WithKnownProxy_EmitsNullableStructProperty()
    {
        var dateTimeType = new TypeMetadata
        {
            FullName = "System.DateTime",
            Namespace = "System",
            Name = "DateTime",
            Fields = [],
            IsValueType = true
        };
        var type = new TypeMetadata
        {
            FullName = "MyApp.Order",
            Namespace = "MyApp",
            Name = "Order",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_shippedAt",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.Nullable<T1>",
                    NullableInnerTypeName = "System.DateTime"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type, allTypes: [dateTimeType, type]);

        code.ShouldContain("public global::_.System.DateTime? _shippedAt => NullableStructField<global::_.System.DateTime>(\"System.DateTime\");");
    }

    [Test]
    public void GenerateProxy_NullableField_UnknownInnerType_EmitsComment()
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
                    Name = "_data",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.Nullable<T1>",
                    NullableInnerTypeName = "SomeLib.UnknownStruct"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("// Nullable<SomeLib.UnknownStruct> field: _data");
    }

    [Test]
    public void GenerateProxy_VoidField_EmitsRawFieldAddress()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Holder",
            Namespace = "MyApp",
            Name = "Holder",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_callback",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.Void"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public ulong _callback => RawFieldAddress();");
    }

    [Test]
    public void GenerateProxy_VoidField_BackingField_EmitsRawFieldAddressWithName()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Holder",
            Namespace = "MyApp",
            Name = "Holder",
            Fields =
            [
                new FieldInfo
                {
                    Name = "<Callback>k__BackingField",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.Void"
                }
            ]
        };

        var code = _emitter.GenerateProxyCode(type);

        code.ShouldContain("public ulong Callback => RawFieldAddress(\"<Callback>k__BackingField\");");
    }

    [Test]
    public void Compile_NullablePrimitiveField_Succeeds()
    {
        var compiler = new ProxyCompiler();
        var sysObj = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };

        var type = new TypeMetadata
        {
            FullName = "MyApp.Order",
            Namespace = "MyApp",
            Name = "Order",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_rating",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.Nullable<T1>",
                    NullableInnerTypeName = "System.Int32"
                }
            ]
        };

        var allTypes = new[] { sysObj, type };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var code = _emitter.GenerateProxyCode(type, allTypes: allTypes);

        var resolverCode = ProxyEmitter.GenerateProxyResolver();
        var result = compiler.CompileFromSource([sysObjCode, code, resolverCode]);
        result.IsSuccess.ShouldBeTrue(string.Join("\n", result.Errors));

        var proxyType = result.Assembly!.GetType("_.MyApp.Order");
        proxyType.ShouldNotBeNull();

        var ratingProp = proxyType.GetProperty("_rating");
        ratingProp.ShouldNotBeNull();
        ratingProp.PropertyType.ShouldBe(typeof(int?));
    }

    [Test]
    public void Compile_VoidField_Succeeds()
    {
        var compiler = new ProxyCompiler();
        var sysObj = new TypeMetadata
        {
            FullName = "System.Object",
            Namespace = "System",
            Name = "Object",
            Fields = []
        };

        var type = new TypeMetadata
        {
            FullName = "MyApp.Holder",
            Namespace = "MyApp",
            Name = "Holder",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_callback",
                    TypeName = "object",
                    Kind = FieldKind.ValueType,
                    ReferenceTypeName = "System.Void"
                }
            ]
        };

        var allTypes = new[] { sysObj, type };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var code = _emitter.GenerateProxyCode(type, allTypes: allTypes);
        var resolverCode = ProxyEmitter.GenerateProxyResolver();

        var result = compiler.CompileFromSource([sysObjCode, code, resolverCode]);
        result.IsSuccess.ShouldBeTrue(string.Join("\n", result.Errors));

        var proxyType = result.Assembly!.GetType("_.MyApp.Holder");
        proxyType.ShouldNotBeNull();

        var callbackProp = proxyType.GetProperty("_callback");
        callbackProp.ShouldNotBeNull();
        callbackProp.PropertyType.ShouldBe(typeof(ulong));
    }
}
