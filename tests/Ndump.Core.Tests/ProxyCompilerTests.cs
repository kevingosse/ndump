namespace Ndump.Core.Tests;

public class ProxyCompilerTests
{
    private readonly ProxyCompiler _compiler = new();

    [Fact]
    public void Compile_ValidCode_Succeeds()
    {
        var source = """
            using Ndump.Core;
            namespace _.Test;

            public sealed class TestType
            {
                private readonly ulong _objAddress;
                private readonly DumpContext _ctx;

                private TestType(ulong address, DumpContext ctx)
                {
                    _objAddress = address;
                    _ctx = ctx;
                }

                public static TestType FromAddress(ulong address, DumpContext ctx)
                    => new TestType(address, ctx);

                public ulong ObjAddress => _objAddress;
            }
            """;

        var result = _compiler.CompileFromSource([source]);

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
        var emitter = new ProxyEmitter();
        var type = new TypeMetadata
        {
            FullName = "Test.Sample",
            Namespace = "Test",
            Name = "Sample",
            Fields =
            [
                new FieldInfo { Name = "_value", TypeName = "int", Kind = FieldKind.Primitive },
                new FieldInfo { Name = "_label", TypeName = "string", Kind = FieldKind.String }
            ]
        };

        var code = emitter.GenerateProxyCode(type);
        var result = _compiler.CompileFromSource([code]);

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
        var emitter = new ProxyEmitter();
        var knownTypes = new HashSet<string> { "App.Order", "App.Customer" };

        var orderType = new TypeMetadata
        {
            FullName = "App.Order",
            Namespace = "App",
            Name = "Order",
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

        var orderCode = emitter.GenerateProxyCode(orderType, knownTypes);
        var customerCode = emitter.GenerateProxyCode(customerType, knownTypes);

        var result = _compiler.CompileFromSource([orderCode, customerCode]);

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
        var emitter = new ProxyEmitter();
        var knownTypes = new HashSet<string> { "App.Models.Customer", "App.Orders.Order" };

        var orderType = new TypeMetadata
        {
            FullName = "App.Orders.Order",
            Namespace = "App.Orders",
            Name = "Order",
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

        var orderCode = emitter.GenerateProxyCode(orderType, knownTypes);
        var customerCode = emitter.GenerateProxyCode(customerType, knownTypes);

        var result = _compiler.CompileFromSource([orderCode, customerCode]);

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
    public void Compile_ToDisk_ProducesFile()
    {
        var source = """
            using Ndump.Core;
            namespace _.Test;
            public sealed class DiskTest
            {
                private readonly ulong _objAddress;
                private readonly DumpContext _ctx;
                private DiskTest(ulong address, DumpContext ctx) { _objAddress = address; _ctx = ctx; }
                public static DiskTest FromAddress(ulong address, DumpContext ctx) => new DiskTest(address, ctx);
            }
            """;

        var tempDir = Path.Combine(Path.GetTempPath(), $"ndump_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var outputPath = Path.Combine(tempDir, "test.dll");

        var result = _compiler.CompileFromSource([source], outputPath);
        Assert.True(result.IsSuccess, string.Join("\n", result.Errors));
        Assert.True(File.Exists(outputPath));
    }
}
