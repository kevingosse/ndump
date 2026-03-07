using System.Collections;
using System.Reflection;
using Ndump.Core;

namespace Ndump.IntegrationTests;

public class DumpProjectionTests : IClassFixture<DumpFixture>
{
    private readonly DumpFixture _fixture;

    public DumpProjectionTests(DumpFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void DumpFile_Exists()
    {
        Assert.True(File.Exists(_fixture.DumpPath), $"Dump file should exist at {_fixture.DumpPath}");
    }

    [Fact]
    public void DumpContext_CanOpen()
    {
        using var ctx = DumpContext.Open(_fixture.DumpPath);
        Assert.NotNull(ctx.Runtime);
        Assert.NotNull(ctx.Heap);
    }

    [Fact]
    public void TypeInspector_DiscoversTestAppTypes()
    {
        using var ctx = DumpContext.Open(_fixture.DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var typeNames = types.Select(t => t.FullName).ToHashSet();

        Assert.Contains("Ndump.TestApp.Customer", typeNames);
        Assert.Contains("Ndump.TestApp.Order", typeNames);
        Assert.Contains("Ndump.TestApp.Address", typeNames);
        Assert.Contains("Ndump.TestApp.Tag", typeNames);
    }

    [Fact]
    public void TypeInspector_CustomerHasExpectedFields()
    {
        using var ctx = DumpContext.Open(_fixture.DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var customer = types.Single(t => t.FullName == "Ndump.TestApp.Customer");
        var fieldNames = customer.Fields.Select(f => f.Name).ToHashSet();

        Assert.Contains("_name", fieldNames);
        Assert.Contains("_age", fieldNames);
        Assert.Contains("_isActive", fieldNames);
        Assert.Contains("_lastOrder", fieldNames);
        Assert.Contains("_address", fieldNames);
        Assert.Contains("_orderHistory", fieldNames);
    }

    [Fact]
    public void FullPipeline_ProjectsDump_AndProxiesWork()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        Assert.NotNull(result.GeneratedAssembly);
        Assert.True(result.DiscoveredTypes.Count > 0);
        Assert.True(result.GeneratedFiles.Count > 0);

        // Verify we generated files
        foreach (var file in result.GeneratedFiles)
        {
            Assert.True(File.Exists(file), $"Generated file should exist: {file}");
        }
    }

    [Fact]
    public void Proxies_CanEnumerateCustomerInstances()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var customerType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Customer");
        Assert.NotNull(customerType);

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(getInstances);

        var instances = getInstances.Invoke(null, [result.Context]) as IEnumerable;
        Assert.NotNull(instances);

        var customers = instances.Cast<object>().ToList();

        // We created 3 customers in TestApp
        Assert.Equal(3, customers.Count);
    }

    [Fact]
    public void Proxies_CanReadCustomerStringField()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var customerType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Customer");
        Assert.NotNull(customerType);

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [result.Context]) as IEnumerable)!.Cast<object>().ToList();

        // Read the _name property from each customer
        var nameProp = customerType.GetProperty("_name");
        Assert.NotNull(nameProp);

        var names = instances.Select(c => nameProp.GetValue(c) as string).OrderBy(n => n).ToList();

        Assert.Contains("Alice", names);
        Assert.Contains("Bob", names);
        Assert.Contains("Charlie", names);
    }

    [Fact]
    public void Proxies_CanReadCustomerIntField()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var customerType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Customer");
        Assert.NotNull(customerType);

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [result.Context]) as IEnumerable)!.Cast<object>().ToList();

        var ageProp = customerType.GetProperty("_age");
        Assert.NotNull(ageProp);

        var ages = instances.Select(c => (int)ageProp.GetValue(c)!).OrderBy(a => a).ToList();

        Assert.Equal([28, 30, 45], ages);
    }

    [Fact]
    public void Proxies_CanReadCustomerBoolField()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var customerType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Customer");
        Assert.NotNull(customerType);

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [result.Context]) as IEnumerable)!.Cast<object>().ToList();

        var activeProp = customerType.GetProperty("_isActive");
        Assert.NotNull(activeProp);

        var activeValues = instances.Select(c => (bool)activeProp.GetValue(c)!).ToList();

        // Alice=true, Bob=false, Charlie=true
        Assert.Equal(2, activeValues.Count(v => v));
        Assert.Equal(1, activeValues.Count(v => !v));
    }

    [Fact]
    public void Proxies_CanNavigateObjectReferences()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var customerType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Customer");
        var orderType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Order");
        Assert.NotNull(customerType);
        Assert.NotNull(orderType);

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [result.Context]) as IEnumerable)!.Cast<object>().ToList();

        // Navigate Customer -> _lastOrder -> _orderId
        var lastOrderProp = customerType.GetProperty("_lastOrder");
        Assert.NotNull(lastOrderProp);

        var orderIdProp = orderType.GetProperty("_orderId");
        Assert.NotNull(orderIdProp);

        var orderIds = new List<int>();
        foreach (var customer in instances)
        {
            var order = lastOrderProp.GetValue(customer);
            Assert.NotNull(order);
            orderIds.Add((int)orderIdProp.GetValue(order)!);
        }

        orderIds.Sort();
        Assert.Equal([1001, 1002, 1003], orderIds);
    }

    [Fact]
    public void Proxies_CanEnumerateTagInstances()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var tagType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Tag");
        Assert.NotNull(tagType);

        var getInstances = tagType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [result.Context]) as IEnumerable)!.Cast<object>().ToList();

        Assert.Equal(2, instances.Count);

        var labelProp = tagType.GetProperty("_label");
        Assert.NotNull(labelProp);

        var labels = instances.Select(t => labelProp.GetValue(t) as string).OrderBy(l => l).ToList();
        Assert.Equal(["important", "urgent"], labels);
    }

    [Fact]
    public void Proxies_FromAddress_ReturnsValidProxy()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var tagType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Tag");
        Assert.NotNull(tagType);

        // Get an address from EnumerateInstances
        var addresses = result.Context.EnumerateInstances("Ndump.TestApp.Tag").ToList();
        Assert.NotEmpty(addresses);

        var fromAddress = tagType.GetMethod("FromAddress", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(fromAddress);

        var proxy = fromAddress.Invoke(null, [addresses[0], result.Context]);
        Assert.NotNull(proxy);

        var addrProp = tagType.GetMethod("GetObjAddress");
        Assert.NotNull(addrProp);
        Assert.Equal(addresses[0], (ulong)addrProp.Invoke(proxy, null)!);
    }

    [Fact]
    public void TypeInspector_CustomerOrderHistoryIsArray()
    {
        using var ctx = DumpContext.Open(_fixture.DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var customer = types.Single(t => t.FullName == "Ndump.TestApp.Customer");
        var field = customer.Fields.Single(f => f.Name == "_orderHistory");

        Assert.Equal(FieldKind.Array, field.Kind);
        Assert.Equal("Ndump.TestApp.Order", field.ArrayElementTypeName);
        Assert.Equal(FieldKind.ObjectReference, field.ArrayElementKind);
    }

    [Fact]
    public void Proxies_CanReadArrayField()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var customerType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Customer");
        var orderType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Order");
        Assert.NotNull(customerType);
        Assert.NotNull(orderType);

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [result.Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name");
        var historyProp = customerType.GetProperty("_orderHistory");
        var orderIdProp = orderType.GetProperty("_orderId");
        Assert.NotNull(nameProp);
        Assert.NotNull(historyProp);
        Assert.NotNull(orderIdProp);

        // Find Alice (has [order1, order2])
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var aliceHistory = historyProp.GetValue(alice);
        Assert.NotNull(aliceHistory);

        // DumpArray implements IEnumerable, use it to get elements
        var aliceOrders = ((IEnumerable)aliceHistory).Cast<object>().ToList();
        Assert.Equal(2, aliceOrders.Count);

        var aliceOrderIds = aliceOrders.Select(o => (int)orderIdProp.GetValue(o)!).OrderBy(id => id).ToList();
        Assert.Equal([1001, 1002], aliceOrderIds);

        // Find Charlie (has [order1, order2, order3])
        var charlie = instances.Single(c => (string?)nameProp.GetValue(c) == "Charlie");
        var charlieHistory = historyProp.GetValue(charlie);
        Assert.NotNull(charlieHistory);

        var charlieOrders = ((IEnumerable)charlieHistory).Cast<object>().ToList();
        Assert.Equal(3, charlieOrders.Count);
    }

    [Fact]
    public void Proxies_ArrayField_HasLength()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var customerType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Customer");
        Assert.NotNull(customerType);

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [result.Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name");
        var historyProp = customerType.GetProperty("_orderHistory");
        Assert.NotNull(nameProp);
        Assert.NotNull(historyProp);

        // Bob has [order2] — 1 element
        var bob = instances.Single(c => (string?)nameProp.GetValue(c) == "Bob");
        var bobHistory = historyProp.GetValue(bob)!;

        var lengthProp = bobHistory.GetType().GetProperty("Length");
        Assert.NotNull(lengthProp);
        Assert.Equal(1, (int)lengthProp.GetValue(bobHistory)!);
    }

    [Fact]
    public void Proxies_OrderHasDoubleField()
    {
        var projector = new DumpProjector();
        using var result = projector.Project(_fixture.DumpPath);

        var orderType = result.GeneratedAssembly.GetType("_.Ndump.TestApp.Order");
        Assert.NotNull(orderType);

        var getInstances = orderType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [result.Context]) as IEnumerable)!.Cast<object>().ToList();

        var totalProp = orderType.GetProperty("_total");
        Assert.NotNull(totalProp);

        var totals = instances.Select(o => (double)totalProp.GetValue(o)!).OrderBy(t => t).ToList();

        Assert.Equal(5.00, totals[0], precision: 2);
        Assert.Equal(29.99, totals[1], precision: 2);
        Assert.Equal(149.50, totals[2], precision: 2);
    }
}
