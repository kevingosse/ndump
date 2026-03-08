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

        code.ShouldContain("public _.MyApp.Order? _lastOrder => Field<_.MyApp.Order>();");
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
        code.ShouldContain("public _.MyApp.Address? _address => Field<_.MyApp.Address>();");
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

        code.ShouldContain("public _.App.Orders.Order? _order");
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

        code.ShouldContain("public global::Ndump.Core.DumpArray<_.MyApp.Order?>? _orderHistory => ArrayField<_.MyApp.Order?>()");
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

        code.ShouldContain("public global::Ndump.Core.DumpArray<_.System.Object?>? _mixedItems => ArrayField<_.System.Object?>()");
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
        code.ShouldContain("class Cat : _.MyApp.Animal");
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

        catCode.ShouldContain("public sealed class Cat : _.MyApp.Animal");
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

        code.ShouldContain("DumpArray<_.MyApp.Animal?>? _pets => ArrayField<_.MyApp.Animal?>()");
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

        code.ShouldContain("DumpArray<_.MyApp.Order?>? _orders => ArrayField<_.MyApp.Order?>()");
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

        code.ShouldContain("public _.MyApp.Animal? _pet => Field<_.MyApp.Animal>();");
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
        code.ShouldContain("new RuntimeTypeCache(addr, context)");
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
        code.ShouldContain("_.System.RuntimeType.ActivatorCache");
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
        code.ShouldContain("_.System.Collections.Generic.List<_.MyApp.Order>");
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

        code.ShouldContain("public _.System.DateTime? _shippedAt => NullableStructField<_.System.DateTime>(\"System.DateTime\");");
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
