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

        Assert.Contains("public sealed class Customer", code);
        Assert.Contains("private readonly ulong _objAddress;", code);
        Assert.Contains("private readonly DumpContext _ctx;", code);
        Assert.Contains("private Customer(ulong address, DumpContext ctx)", code);
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

        Assert.Contains("public string? _name => _ctx.GetStringField(_objAddress, \"MyApp.Customer\", \"_name\");", code);
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

        Assert.Contains("public int _age => _ctx.GetFieldValue<int>(_objAddress, \"MyApp.Customer\", \"_age\");", code);
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
    public void GenerateProxy_HasAddressMethod()
    {
        var type = new TypeMetadata
        {
            FullName = "MyApp.Tag",
            Namespace = "MyApp",
            Name = "Tag",
            Fields = []
        };

        var code = _emitter.GenerateProxyCode(type);

        Assert.Contains("public ulong GetObjAddress() => _objAddress;", code);
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
