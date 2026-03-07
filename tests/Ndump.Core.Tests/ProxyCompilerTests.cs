namespace Ndump.Core.Tests;

public class ProxyCompilerTests
{
    private readonly ProxyCompiler _compiler = new();
    private readonly ProxyEmitter _emitter = new();

    private static TypeMetadata SystemObjectType => new()
    {
        FullName = "System.Object",
        Namespace = "System",
        Name = "Object",
        Fields = []
    };

    private string GenerateSystemObjectCode()
        => _emitter.GenerateProxyCode(SystemObjectType);

    [Fact]
    public void Compile_ValidCode_Succeeds()
    {
        var sysObjCode = GenerateSystemObjectCode();
        var source = """
            using Ndump.Core;
            namespace _.Test;

            public sealed class TestType : global::_.System.Object
            {
                TestType(ulong address, DumpContext ctx) : base(address, ctx) { }

                public static TestType FromAddress(ulong address, DumpContext ctx)
                    => new TestType(address, ctx);
            }
            """;

        var result = _compiler.CompileFromSource([sysObjCode, source]);

        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));
        Assert.NotNull(result.Assembly);
    }

    [Fact]
    public void Compile_InvalidCode_ReturnsErrors()
    {
        var source = """
            namespace _.Test;
            public class Broken { this is not valid C# }
            """;

        var result = _compiler.CompileFromSource([source]);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Compile_GeneratedProxyType_HasExpectedMembers()
    {
        var sysObj = SystemObjectType;
        var type = new TypeMetadata
        {
            FullName = "Test.Sample",
            Namespace = "Test",
            Name = "Sample",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo { Name = "_value", TypeName = "int", Kind = FieldKind.Primitive },
                new FieldInfo { Name = "_label", TypeName = "string", Kind = FieldKind.String }
            ]
        };

        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: [sysObj, type]);
        var code = _emitter.GenerateProxyCode(type, allTypes: [sysObj, type]);
        var result = _compiler.CompileFromSource([sysObjCode, code]);

        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));

        var proxyType = result.Assembly!.GetType("_.Test.Sample");
        Assert.NotNull(proxyType);

        // Check that FromAddress exists
        var fromAddr = proxyType.GetMethod("FromAddress");
        Assert.NotNull(fromAddr);

        // Check that GetInstances exists
        var getInstances = proxyType.GetMethod("GetInstances");
        Assert.NotNull(getInstances);

        // Check properties exist
        var valueProp = proxyType.GetProperty("_value");
        Assert.NotNull(valueProp);
        Assert.Equal(typeof(int), valueProp.PropertyType);

        var labelProp = proxyType.GetProperty("_label");
        Assert.NotNull(labelProp);
        Assert.Equal(typeof(string), labelProp.PropertyType);
    }

    [Fact]
    public void Compile_MultipleTypes_WithCrossReferences_Succeeds()
    {
        var sysObj = SystemObjectType;
        var orderType = new TypeMetadata
        {
            FullName = "App.Order",
            Namespace = "App",
            Name = "Order",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo { Name = "_id", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };

        var customerType = new TypeMetadata
        {
            FullName = "App.Customer",
            Namespace = "App",
            Name = "Customer",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo
                {
                    Name = "_order",
                    TypeName = "App.Order",
                    Kind = FieldKind.ObjectReference,
                    ReferenceTypeName = "App.Order"
                }
            ]
        };

        var allTypes = new[] { sysObj, orderType, customerType };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var orderCode = _emitter.GenerateProxyCode(orderType, allTypes: allTypes);
        var customerCode = _emitter.GenerateProxyCode(customerType, allTypes: allTypes);

        var result = _compiler.CompileFromSource([sysObjCode, orderCode, customerCode]);

        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));

        var genOrder = result.Assembly!.GetType("_.App.Order");
        var genCustomer = result.Assembly.GetType("_.App.Customer");
        Assert.NotNull(genOrder);
        Assert.NotNull(genCustomer);

        // Verify cross-reference property type
        var orderProp = genCustomer.GetProperty("_order");
        Assert.NotNull(orderProp);
        Assert.Equal(genOrder, orderProp.PropertyType);
    }

    [Fact]
    public void Compile_MultipleTypes_CrossNamespaceReferences_Succeeds()
    {
        var sysObj = SystemObjectType;
        var orderType = new TypeMetadata
        {
            FullName = "App.Orders.Order",
            Namespace = "App.Orders",
            Name = "Order",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo { Name = "_id", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };

        var customerType = new TypeMetadata
        {
            FullName = "App.Models.Customer",
            Namespace = "App.Models",
            Name = "Customer",
            BaseTypeName = "System.Object",
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

        var allTypes = new[] { sysObj, orderType, customerType };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var orderCode = _emitter.GenerateProxyCode(orderType, allTypes: allTypes);
        var customerCode = _emitter.GenerateProxyCode(customerType, allTypes: allTypes);

        var result = _compiler.CompileFromSource([sysObjCode, orderCode, customerCode]);

        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));

        var genOrder = result.Assembly!.GetType("_.App.Orders.Order");
        var genCustomer = result.Assembly.GetType("_.App.Models.Customer");
        Assert.NotNull(genOrder);
        Assert.NotNull(genCustomer);

        var orderProp = genCustomer.GetProperty("_order");
        Assert.NotNull(orderProp);
        Assert.Equal(genOrder, orderProp.PropertyType);
    }

    [Fact]
    public void Compile_ArrayField_KnownElementType_Succeeds()
    {
        var sysObj = SystemObjectType;
        var orderType = new TypeMetadata
        {
            FullName = "App.Order",
            Namespace = "App",
            Name = "Order",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo { Name = "_id", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };

        var customerType = new TypeMetadata
        {
            FullName = "App.Customer",
            Namespace = "App",
            Name = "Customer",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo
                {
                    Name = "_orderHistory",
                    TypeName = "App.Order[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "App.Order",
                    ArrayElementKind = FieldKind.ObjectReference
                }
            ]
        };

        var allTypes = new[] { sysObj, orderType, customerType };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var orderCode = _emitter.GenerateProxyCode(orderType, allTypes: allTypes);
        var customerCode = _emitter.GenerateProxyCode(customerType, allTypes: allTypes);

        var result = _compiler.CompileFromSource([sysObjCode, orderCode, customerCode]);

        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));

        var genCustomer = result.Assembly!.GetType("_.App.Customer");
        Assert.NotNull(genCustomer);

        var historyProp = genCustomer.GetProperty("_orderHistory");
        Assert.NotNull(historyProp);

        // Should be DumpArray<Order?>?
        Assert.True(historyProp.PropertyType.IsGenericType);
        Assert.Equal(typeof(DumpArray<>), historyProp.PropertyType.GetGenericTypeDefinition());
    }

    [Fact]
    public void Compile_ObjectArrayField_UsesSystemObjectProxy()
    {
        var sysObj = SystemObjectType;
        var orderType = new TypeMetadata
        {
            FullName = "App.Order",
            Namespace = "App",
            Name = "Order",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo { Name = "_id", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };

        var containerType = new TypeMetadata
        {
            FullName = "App.Container",
            Namespace = "App",
            Name = "Container",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo
                {
                    Name = "_items",
                    TypeName = "System.Object[]",
                    Kind = FieldKind.Array,
                    ArrayElementTypeName = "System.Object",
                    ArrayElementKind = FieldKind.ObjectReference
                }
            ]
        };

        var allTypes = new[] { sysObj, orderType, containerType };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var orderCode = _emitter.GenerateProxyCode(orderType, allTypes: allTypes);
        var containerCode = _emitter.GenerateProxyCode(containerType, allTypes: allTypes);

        var result = _compiler.CompileFromSource([sysObjCode, orderCode, containerCode]);

        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));

        var genContainer = result.Assembly!.GetType("_.App.Container");
        Assert.NotNull(genContainer);

        var itemsProp = genContainer.GetProperty("_items");
        Assert.NotNull(itemsProp);

        // Should be DumpArray<_.System.Object?>?
        Assert.True(itemsProp.PropertyType.IsGenericType);
        Assert.Equal(typeof(DumpArray<>), itemsProp.PropertyType.GetGenericTypeDefinition());

        // The generic argument should be the generated _.System.Object proxy
        var elementType = itemsProp.PropertyType.GetGenericArguments()[0];
        var genSysObj = result.Assembly.GetType("_.System.Object");
        Assert.NotNull(genSysObj);
        Assert.Equal(genSysObj, elementType);
    }

    [Fact]
    public void Compile_GeneratedProxy_InheritsFromSystemObject()
    {
        var sysObj = SystemObjectType;
        var type = new TypeMetadata
        {
            FullName = "Test.Sample",
            Namespace = "Test",
            Name = "Sample",
            BaseTypeName = "System.Object",
            Fields = []
        };

        var allTypes = new[] { sysObj, type };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var code = _emitter.GenerateProxyCode(type, allTypes: allTypes);
        var result = _compiler.CompileFromSource([sysObjCode, code]);

        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));

        var proxyType = result.Assembly!.GetType("_.Test.Sample");
        var genSysObj = result.Assembly.GetType("_.System.Object");
        Assert.NotNull(proxyType);
        Assert.NotNull(genSysObj);

        Assert.True(genSysObj.IsAssignableFrom(proxyType));
    }

    [Fact]
    public void Compile_InheritanceHierarchy_Succeeds()
    {
        var sysObj = SystemObjectType;
        var animal = new TypeMetadata
        {
            FullName = "App.Animal",
            Namespace = "App",
            Name = "Animal",
            BaseTypeName = "System.Object",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo { Name = "_age", TypeName = "int", Kind = FieldKind.Primitive }
            ]
        };
        var cat = new TypeMetadata
        {
            FullName = "App.Cat",
            Namespace = "App",
            Name = "Cat",
            BaseTypeName = "App.Animal",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo { Name = "_age", TypeName = "int", Kind = FieldKind.Primitive },
                new FieldInfo { Name = "_isIndoor", TypeName = "bool", Kind = FieldKind.Primitive }
            ]
        };
        var dog = new TypeMetadata
        {
            FullName = "App.Dog",
            Namespace = "App",
            Name = "Dog",
            BaseTypeName = "App.Animal",
            Fields =
            [
                new FieldInfo { Name = "_name", TypeName = "string", Kind = FieldKind.String },
                new FieldInfo { Name = "_age", TypeName = "int", Kind = FieldKind.Primitive },
                new FieldInfo { Name = "_breed", TypeName = "string", Kind = FieldKind.String }
            ]
        };

        var allTypes = new[] { sysObj, animal, cat, dog };
        var sysObjCode = _emitter.GenerateProxyCode(sysObj, allTypes: allTypes);
        var animalCode = _emitter.GenerateProxyCode(animal, allTypes: allTypes);
        var catCode = _emitter.GenerateProxyCode(cat, allTypes: allTypes);
        var dogCode = _emitter.GenerateProxyCode(dog, allTypes: allTypes);

        var result = _compiler.CompileFromSource([sysObjCode, animalCode, catCode, dogCode]);

        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));

        var genSysObj = result.Assembly!.GetType("_.System.Object");
        var genAnimal = result.Assembly.GetType("_.App.Animal");
        var genCat = result.Assembly.GetType("_.App.Cat");
        var genDog = result.Assembly.GetType("_.App.Dog");
        Assert.NotNull(genSysObj);
        Assert.NotNull(genAnimal);
        Assert.NotNull(genCat);
        Assert.NotNull(genDog);

        // Cat and Dog extend Animal
        Assert.True(genAnimal.IsAssignableFrom(genCat));
        Assert.True(genAnimal.IsAssignableFrom(genDog));

        // Animal extends _.System.Object
        Assert.True(genSysObj.IsAssignableFrom(genAnimal));

        // Cat has _isIndoor but NOT _name/_age (inherited)
        Assert.NotNull(genCat.GetProperty("_isIndoor"));
        Assert.Null(genCat.GetProperty("_name", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));

        // Animal has _name and _age
        Assert.NotNull(genAnimal.GetProperty("_name"));
        Assert.NotNull(genAnimal.GetProperty("_age"));
    }

    [Fact]
    public void Compile_ToDisk_ProducesFile()
    {
        var sysObjCode = GenerateSystemObjectCode();
        var source = """
            using Ndump.Core;
            namespace _.Test;
            public sealed class DiskTest : global::_.System.Object
            {
                DiskTest(ulong address, DumpContext ctx) : base(address, ctx) { }
                public static DiskTest FromAddress(ulong address, DumpContext ctx) => new DiskTest(address, ctx);
            }
            """;

        var tempDir = Path.Combine(Path.GetTempPath(), $"ndump_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var outputPath = Path.Combine(tempDir, "test.dll");

            var result = _compiler.CompileFromSource([sysObjCode, source], outputPath);
            Assert.True(result.IsSuccess, string.Join("\n", result.Errors));
            Assert.True(File.Exists(outputPath));
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
