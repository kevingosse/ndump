using System.Collections;
using System.Reflection;
using Ndump.Core;

namespace Ndump.IntegrationTests;

public class DumpProjectionTests : IClassFixture<DumpFixture>
{
    private readonly DumpFixture _fixture;
    private DumpProjector.ProjectionResult Result => _fixture.Projection;
    private Assembly Assembly => Result.GeneratedAssembly;
    private DumpContext Context => Result.Context;

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
        Assert.Contains("Ndump.TestApp.Cat", typeNames);
        Assert.Contains("Ndump.TestApp.Dog", typeNames);
        Assert.Contains("System.Object", typeNames);
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
        Assert.Contains("_mixedItems", fieldNames);
        Assert.Contains("_pets", fieldNames);
        Assert.Contains("_tags", fieldNames);
    }

    [Fact]
    public void FullPipeline_ProjectsDump_AndProxiesWork()
    {
        Assert.NotNull(Result.GeneratedAssembly);
        Assert.True(Result.DiscoveredTypes.Count > 0);
        Assert.True(Result.GeneratedFiles.Count > 0);
    }

    [Fact]
    public void Proxies_CanEnumerateCustomerInstances()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer");
        Assert.NotNull(customerType);

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(getInstances);

        var instances = getInstances.Invoke(null, [Context]) as IEnumerable;
        Assert.NotNull(instances);

        var customers = instances.Cast<object>().ToList();

        // We created 3 customers in TestApp
        Assert.Equal(3, customers.Count);
    }

    [Fact]
    public void Proxies_CanReadCustomerStringField()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

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
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var ageProp = customerType.GetProperty("_age");
        Assert.NotNull(ageProp);

        var ages = instances.Select(c => (int)ageProp.GetValue(c)!).OrderBy(a => a).ToList();

        Assert.Equal([28, 30, 45], ages);
    }

    [Fact]
    public void Proxies_CanReadCustomerBoolField()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

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
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var orderType = Assembly.GetType("_.Ndump.TestApp.Order")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        // Navigate Customer -> _lastOrder -> _orderId
        var lastOrderProp = customerType.GetProperty("_lastOrder")!;
        var orderIdProp = orderType.GetProperty("_orderId")!;

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
    public void Proxies_CanNavigateObjectReference_AndReadFields()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var addressType = Assembly.GetType("_.Ndump.TestApp.Address")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var addressProp = customerType.GetProperty("_address")!;
        var streetProp = addressType.GetProperty("_street")!;
        var cityProp = addressType.GetProperty("_city")!;
        var zipProp = addressType.GetProperty("_zipCode")!;

        // Alice has addr1("123 Main St", "Springfield", 62701)
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var aliceAddr = addressProp.GetValue(alice);
        Assert.NotNull(aliceAddr);
        Assert.IsType(addressType, aliceAddr);
        Assert.Equal("123 Main St", streetProp.GetValue(aliceAddr));
        Assert.Equal("Springfield", cityProp.GetValue(aliceAddr));
        Assert.Equal(62701, (int)zipProp.GetValue(aliceAddr)!);

        // Bob has addr2("456 Oak Ave", "Shelbyville", 62702)
        var bob = instances.Single(c => (string?)nameProp.GetValue(c) == "Bob");
        var bobAddr = addressProp.GetValue(bob);
        Assert.NotNull(bobAddr);
        Assert.Equal("456 Oak Ave", streetProp.GetValue(bobAddr));
        Assert.Equal("Shelbyville", cityProp.GetValue(bobAddr));
        Assert.Equal(62702, (int)zipProp.GetValue(bobAddr)!);
    }

    [Fact]
    public void Proxies_CanEnumerateTagInstances()
    {
        var tagType = Assembly.GetType("_.Ndump.TestApp.Tag")!;

        var getInstances = tagType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        Assert.Equal(2, instances.Count);

        var labelProp = tagType.GetProperty("_label")!;

        var labels = instances.Select(t => labelProp.GetValue(t) as string).OrderBy(l => l).ToList();
        Assert.Equal(["important", "urgent"], labels);
    }

    [Fact]
    public void Proxies_FromAddress_ReturnsValidProxy()
    {
        var tagType = Assembly.GetType("_.Ndump.TestApp.Tag")!;

        // Get an address from EnumerateInstances
        var addresses = Context.EnumerateInstances("Ndump.TestApp.Tag").ToList();
        Assert.NotEmpty(addresses);

        var fromAddress = tagType.GetMethod("FromAddress", BindingFlags.Public | BindingFlags.Static)!;

        var proxy = fromAddress.Invoke(null, [addresses[0], Context]);
        Assert.NotNull(proxy);

        var addrMethod = tagType.GetMethod("GetObjAddress")!;
        Assert.Equal(addresses[0], (ulong)addrMethod.Invoke(proxy, null)!);
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
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var orderType = Assembly.GetType("_.Ndump.TestApp.Order")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var historyProp = customerType.GetProperty("_orderHistory")!;
        var orderIdProp = orderType.GetProperty("_orderId")!;

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
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var historyProp = customerType.GetProperty("_orderHistory")!;

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
        var orderType = Assembly.GetType("_.Ndump.TestApp.Order")!;

        var getInstances = orderType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var totalProp = orderType.GetProperty("_total")!;

        var totals = instances.Select(o => (double)totalProp.GetValue(o)!).OrderBy(t => t).ToList();

        Assert.Equal(5.00, totals[0], precision: 2);
        Assert.Equal(29.99, totals[1], precision: 2);
        Assert.Equal(149.50, totals[2], precision: 2);
    }

    [Fact]
    public void TypeInspector_CustomerMixedItemsIsObjectArray()
    {
        using var ctx = DumpContext.Open(_fixture.DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var customer = types.Single(t => t.FullName == "Ndump.TestApp.Customer");
        var field = customer.Fields.Single(f => f.Name == "_mixedItems");

        Assert.Equal(FieldKind.Array, field.Kind);
        Assert.Equal("System.Object", field.ArrayElementTypeName);
        Assert.Equal(FieldKind.ObjectReference, field.ArrayElementKind);
    }

    [Fact]
    public void Proxies_CanReadObjectArrayField()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var mixedProp = customerType.GetProperty("_mixedItems")!;

        // Alice has [order1, addr1, tag1, "hello"] — 4 elements
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var aliceMixed = mixedProp.GetValue(alice);
        Assert.NotNull(aliceMixed);

        var aliceItems = ((IEnumerable)aliceMixed).Cast<object>().ToList();
        Assert.Equal(4, aliceItems.Count);
    }

    [Fact]
    public void Proxies_ObjectArrayElements_InheritFromSystemObject()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var sysObjType = Assembly.GetType("_.System.Object")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var mixedProp = customerType.GetProperty("_mixedItems")!;

        // Bob has [tag2, order2] — elements should inherit from _.System.Object
        var bob = instances.Single(c => (string?)nameProp.GetValue(c) == "Bob");
        var bobMixed = mixedProp.GetValue(bob)!;

        var bobItems = ((IEnumerable)bobMixed).Cast<object>().ToList();
        Assert.Equal(2, bobItems.Count);

        foreach (var item in bobItems)
        {
            Assert.True(sysObjType.IsInstanceOfType(item));
        }
    }

    [Fact]
    public void Proxies_ObjectArrayElements_AreCorrectProxyTypes()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var tagType = Assembly.GetType("_.Ndump.TestApp.Tag")!;
        var orderType = Assembly.GetType("_.Ndump.TestApp.Order")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var mixedProp = customerType.GetProperty("_mixedItems")!;

        // Bob has [tag2, order2] — elements should be actual proxy types
        var bob = instances.Single(c => (string?)nameProp.GetValue(c) == "Bob");
        var bobMixed = mixedProp.GetValue(bob)!;
        var bobItems = ((IEnumerable)bobMixed).Cast<object>().ToList();

        var proxyTypes = bobItems.Select(d => d.GetType()).ToList();
        Assert.Contains(tagType, proxyTypes);
        Assert.Contains(orderType, proxyTypes);
    }

    [Fact]
    public void Proxies_ObjectArrayElements_CanReadFieldsDirectly()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var orderType = Assembly.GetType("_.Ndump.TestApp.Order")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var mixedProp = customerType.GetProperty("_mixedItems")!;
        var orderIdProp = orderType.GetProperty("_orderId")!;

        // Bob has [tag2, order2] — the Order element is already the right proxy type
        var bob = instances.Single(c => (string?)nameProp.GetValue(c) == "Bob");
        var bobMixed = mixedProp.GetValue(bob)!;
        var bobItems = ((IEnumerable)bobMixed).Cast<object>().ToList();

        var orderElement = bobItems.Single(d => d.GetType() == orderType);

        // Can read fields directly — no cast needed since ResolveProxy returns the right type
        var orderId = (int)orderIdProp.GetValue(orderElement)!;
        Assert.Equal(1002, orderId);
    }

    [Fact]
    public void Proxies_ObjectArrayElements_HasCorrectProxyTypes()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var mixedProp = customerType.GetProperty("_mixedItems")!;

        // Alice has [order1, addr1, tag1, "hello"] — check proxy types
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var aliceMixed = mixedProp.GetValue(alice)!;
        var aliceItems = ((IEnumerable)aliceMixed).Cast<object>().ToList();

        var proxyTypeNames = aliceItems.Select(d => d.GetType().FullName).ToList();
        Assert.Contains("_.Ndump.TestApp.Order", proxyTypeNames);
        Assert.Contains("_.Ndump.TestApp.Address", proxyTypeNames);
        Assert.Contains("_.Ndump.TestApp.Tag", proxyTypeNames);
        // String gets a System.String proxy (_.System.String)
        Assert.Contains("_.System.String", proxyTypeNames);
    }

    [Fact]
    public void TypeInspector_DiscoversAnimalHierarchy()
    {
        using var ctx = DumpContext.Open(_fixture.DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var typeNames = types.Select(t => t.FullName).ToHashSet();

        Assert.Contains("Ndump.TestApp.Cat", typeNames);
        Assert.Contains("Ndump.TestApp.Dog", typeNames);

        // Cat and Dog should have Animal as BaseTypeName
        var cat = types.Single(t => t.FullName == "Ndump.TestApp.Cat");
        Assert.Equal("Ndump.TestApp.Animal", cat.BaseTypeName);

        var dog = types.Single(t => t.FullName == "Ndump.TestApp.Dog");
        Assert.Equal("Ndump.TestApp.Animal", dog.BaseTypeName);
    }

    [Fact]
    public void Proxies_AnimalArray_ContainsCatsAndDogs()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var catType = Assembly.GetType("_.Ndump.TestApp.Cat")!;
        var dogType = Assembly.GetType("_.Ndump.TestApp.Dog")!;
        var animalType = Assembly.GetType("_.Ndump.TestApp.Animal")!;

        // Cat and Dog should extend Animal in the proxy hierarchy
        Assert.True(animalType.IsAssignableFrom(catType));
        Assert.True(animalType.IsAssignableFrom(dogType));

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var petsProp = customerType.GetProperty("_pets")!;

        // Alice has [cat1, dog1]
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var alicePets = ((IEnumerable)petsProp.GetValue(alice)!).Cast<object>().ToList();
        Assert.Equal(2, alicePets.Count);

        // Elements should be actual Cat and Dog proxy types, not just Animal
        var petTypes = alicePets.Select(p => p.GetType()).ToList();
        Assert.Contains(catType, petTypes);
        Assert.Contains(dogType, petTypes);

        // All pets are assignable to Animal
        foreach (var pet in alicePets)
        {
            Assert.True(animalType.IsInstanceOfType(pet));
        }
    }

    [Fact]
    public void Proxies_AnimalArray_CanReadInheritedAndOwnFields()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var animalType = Assembly.GetType("_.Ndump.TestApp.Animal")!;
        var dogType = Assembly.GetType("_.Ndump.TestApp.Dog")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var petsProp = customerType.GetProperty("_pets")!;

        // Alice has [cat1("Whiskers", 3, indoor=true), dog1("Rex", 4, "German Shepherd")]
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var alicePets = ((IEnumerable)petsProp.GetValue(alice)!).Cast<object>().ToList();

        // Read inherited _name field (declared on Animal proxy) from a Dog
        var animalNameProp = animalType.GetProperty("_name")!;

        var petNames = alicePets.Select(p => (string?)animalNameProp.GetValue(p)).OrderBy(n => n).ToList();
        Assert.Equal(["Rex", "Whiskers"], petNames);

        // Read Dog-specific _breed field
        var breedProp = dogType.GetProperty("_breed")!;

        var dog = alicePets.Single(p => p.GetType() == dogType);
        Assert.Equal("German Shepherd", breedProp.GetValue(dog));
    }

    [Fact]
    public void Proxies_AnimalArray_Charlie_HasMixedPets()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var catType = Assembly.GetType("_.Ndump.TestApp.Cat")!;
        var dogType = Assembly.GetType("_.Ndump.TestApp.Dog")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var petsProp = customerType.GetProperty("_pets")!;

        // Charlie has [cat2("Mittens"), dog1("Rex"), cat1("Whiskers")]
        var charlie = instances.Single(c => (string?)nameProp.GetValue(c) == "Charlie");
        var charliePets = ((IEnumerable)petsProp.GetValue(charlie)!).Cast<object>().ToList();
        Assert.Equal(3, charliePets.Count);

        var catCount = charliePets.Count(p => p.GetType() == catType);
        var dogCount = charliePets.Count(p => p.GetType() == dogType);
        Assert.Equal(2, catCount);
        Assert.Equal(1, dogCount);
    }

    [Fact]
    public void Proxies_InheritFromSystemObject()
    {
        var sysObjType = Assembly.GetType("_.System.Object")!;

        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        Assert.True(sysObjType.IsAssignableFrom(customerType));

        var orderType = Assembly.GetType("_.Ndump.TestApp.Order")!;
        Assert.True(sysObjType.IsAssignableFrom(orderType));
    }

    [Fact]
    public void Proxies_SystemString_HasImplicitConversionToString()
    {
        var stringType = Assembly.GetType("_.System.String")!;
        var sysObjType = Assembly.GetType("_.System.Object")!;

        // _.System.String should extend _.System.Object
        Assert.True(sysObjType.IsAssignableFrom(stringType));

        // Should have an implicit operator to string?
        var implicitOp = stringType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(m => m.Name == "op_Implicit" && m.ReturnType == typeof(string));
        Assert.NotNull(implicitOp);
    }

    [Fact]
    public void Proxies_SystemString_ToStringReturnsValue()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var stringType = Assembly.GetType("_.System.String")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var mixedProp = customerType.GetProperty("_mixedItems")!;

        // Alice has [order1, addr1, tag1, "hello"] — the string element should have ToString() == "hello"
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var aliceMixed = ((IEnumerable)mixedProp.GetValue(alice)!).Cast<object>().ToList();

        var stringElement = aliceMixed.Single(d => d.GetType() == stringType);
        Assert.Equal("hello", stringElement.ToString());
    }

    [Fact]
    public void Proxies_SystemString_ImplicitOperatorReturnsValue()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var stringType = Assembly.GetType("_.System.String")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var mixedProp = customerType.GetProperty("_mixedItems")!;

        // Alice has "hello" in _mixedItems
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var aliceMixed = ((IEnumerable)mixedProp.GetValue(alice)!).Cast<object>().ToList();

        var stringElement = aliceMixed.Single(d => d.GetType() == stringType);

        // Use the implicit operator via reflection
        var implicitOp = stringType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == "op_Implicit" && m.ReturnType == typeof(string));
        var result = (string?)implicitOp.Invoke(null, [stringElement]);
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Proxies_SystemString_ValuePropertyReturnsValue()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;
        var stringType = Assembly.GetType("_.System.String")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var mixedProp = customerType.GetProperty("_mixedItems")!;

        // Charlie has "world" in _mixedItems
        var charlie = instances.Single(c => (string?)nameProp.GetValue(c) == "Charlie");
        var charlieMixed = ((IEnumerable)mixedProp.GetValue(charlie)!).Cast<object>().ToList();

        var stringElement = charlieMixed.Single(d => d.GetType() == stringType);
        var valueProp = stringType.GetProperty("Value")!;
        Assert.Equal("world", valueProp.GetValue(stringElement));
    }

    [Fact]
    public void Proxies_StringArrayField_ReturnsStringValues()
    {
        var customerType = Assembly.GetType("_.Ndump.TestApp.Customer")!;

        var getInstances = customerType.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
        var instances = (getInstances!.Invoke(null, [Context]) as IEnumerable)!.Cast<object>().ToList();

        var nameProp = customerType.GetProperty("_name")!;
        var tagsProp = customerType.GetProperty("_tags")!;

        // Alice has ["vip", "early-adopter"]
        var alice = instances.Single(c => (string?)nameProp.GetValue(c) == "Alice");
        var aliceTags = ((IEnumerable)tagsProp.GetValue(alice)!).Cast<string?>().ToList();
        Assert.Equal(2, aliceTags.Count);
        Assert.Contains("vip", aliceTags);
        Assert.Contains("early-adopter", aliceTags);

        // Bob has ["regular"]
        var bob = instances.Single(c => (string?)nameProp.GetValue(c) == "Bob");
        var bobTags = ((IEnumerable)tagsProp.GetValue(bob)!).Cast<string?>().ToList();
        Assert.Equal(["regular"], bobTags);

        // Charlie has ["vip", "premium", "newsletter"]
        var charlie = instances.Single(c => (string?)nameProp.GetValue(c) == "Charlie");
        var charlieTags = ((IEnumerable)tagsProp.GetValue(charlie)!).Cast<string?>().ToList();
        Assert.Equal(3, charlieTags.Count);
        Assert.Contains("premium", charlieTags);
    }
}
