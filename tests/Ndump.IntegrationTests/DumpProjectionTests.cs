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

    private Type ProxyType(string clrTypeName) =>
        Assembly.GetType($"_.{clrTypeName}")!;

    private List<dynamic> GetInstances(string clrTypeName)
    {
        var type = ProxyType(clrTypeName);
        var method = type.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static)!;
        return ((IEnumerable)method.Invoke(null, [Context])!).Cast<dynamic>().ToList();
    }

    private dynamic FromAddress(string clrTypeName, ulong address)
    {
        var type = ProxyType(clrTypeName);
        var method = type.GetMethod("FromAddress", BindingFlags.Public | BindingFlags.Static)!;
        return method.Invoke(null, [address, Context])!;
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
        var customers = GetInstances("Ndump.TestApp.Customer");

        // We created 3 customers in TestApp
        Assert.Equal(3, customers.Count);
    }

    [Fact]
    public void Proxies_CanReadCustomerStringField()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        var names = customers.Select(c => (string?)c._name).OrderBy(n => n).ToList();

        Assert.Contains("Alice", names);
        Assert.Contains("Bob", names);
        Assert.Contains("Charlie", names);
    }

    [Fact]
    public void Proxies_CanReadCustomerIntField()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        var ages = customers.Select(c => (int)c._age).OrderBy(a => a).ToList();

        Assert.Equal([28, 30, 45], ages);
    }

    [Fact]
    public void Proxies_CanReadCustomerBoolField()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        var activeValues = customers.Select(c => (bool)c._isActive).ToList();

        // Alice=true, Bob=false, Charlie=true
        Assert.Equal(2, activeValues.Count(v => v));
        Assert.Equal(1, activeValues.Count(v => !v));
    }

    [Fact]
    public void Proxies_CanNavigateObjectReferences()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Navigate Customer -> _lastOrder -> _orderId
        var orderIds = new List<int>();
        foreach (var customer in customers)
        {
            dynamic order = customer._lastOrder;
            Assert.NotNull(order);
            orderIds.Add((int)order._orderId);
        }

        orderIds.Sort();
        Assert.Equal([1001, 1002, 1003], orderIds);
    }

    [Fact]
    public void Proxies_CanNavigateObjectReference_AndReadFields()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has addr1("123 Main St", "Springfield", 62701)
        var alice = customers.Single(c => (string?)c._name == "Alice");
        dynamic aliceAddr = alice._address;
        Assert.NotNull(aliceAddr);
        Assert.IsType(ProxyType("Ndump.TestApp.Address"), (object)aliceAddr);
        Assert.Equal("123 Main St", (string)aliceAddr._street);
        Assert.Equal("Springfield", (string)aliceAddr._city);
        Assert.Equal(62701, (int)aliceAddr._zipCode);

        // Bob has addr2("456 Oak Ave", "Shelbyville", 62702)
        var bob = customers.Single(c => (string?)c._name == "Bob");
        dynamic bobAddr = bob._address;
        Assert.NotNull(bobAddr);
        Assert.Equal("456 Oak Ave", (string)bobAddr._street);
        Assert.Equal("Shelbyville", (string)bobAddr._city);
        Assert.Equal(62702, (int)bobAddr._zipCode);
    }

    [Fact]
    public void Proxies_CanEnumerateTagInstances()
    {
        var tags = GetInstances("Ndump.TestApp.Tag");
        Assert.Equal(2, tags.Count);

        var labels = tags.Select(t => (string?)t._label).OrderBy(l => l).ToList();
        Assert.Equal(["important", "urgent"], labels);
    }

    [Fact]
    public void Proxies_FromAddress_ReturnsValidProxy()
    {
        // Get an address from EnumerateInstances
        var addresses = Context.EnumerateInstances("Ndump.TestApp.Tag").ToList();
        Assert.NotEmpty(addresses);

        dynamic proxy = FromAddress("Ndump.TestApp.Tag", addresses[0]);
        Assert.NotNull(proxy);
        Assert.Equal(addresses[0], (ulong)proxy.GetObjAddress());
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
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Find Alice (has [order1, order2])
        var alice = customers.Single(c => (string?)c._name == "Alice");
        var aliceOrders = ((IEnumerable)alice._orderHistory).Cast<dynamic>().ToList();
        Assert.Equal(2, aliceOrders.Count);

        var aliceOrderIds = aliceOrders.Select(o => (int)o._orderId).OrderBy(id => id).ToList();
        Assert.Equal([1001, 1002], aliceOrderIds);

        // Find Charlie (has [order1, order2, order3])
        var charlie = customers.Single(c => (string?)c._name == "Charlie");
        var charlieOrders = ((IEnumerable)charlie._orderHistory).Cast<dynamic>().ToList();
        Assert.Equal(3, charlieOrders.Count);
    }

    [Fact]
    public void Proxies_ArrayField_HasLength()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Bob has [order2] — 1 element
        var bob = customers.Single(c => (string?)c._name == "Bob");
        dynamic bobHistory = bob._orderHistory;
        Assert.Equal(1, (int)bobHistory.Length);
    }

    [Fact]
    public void Proxies_OrderHasDoubleField()
    {
        var orders = GetInstances("Ndump.TestApp.Order");

        var totals = orders.Select(o => (double)o._total).OrderBy(t => t).ToList();

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
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has [order1, addr1, tag1, "hello"] — 4 elements
        var alice = customers.Single(c => (string?)c._name == "Alice");
        var aliceItems = ((IEnumerable)alice._mixedItems).Cast<object>().ToList();
        Assert.Equal(4, aliceItems.Count);
    }

    [Fact]
    public void Proxies_ObjectArrayElements_InheritFromSystemObject()
    {
        var sysObjType = ProxyType("System.Object");
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Bob has [tag2, order2] — elements should inherit from _.System.Object
        var bob = customers.Single(c => (string?)c._name == "Bob");
        var bobItems = ((IEnumerable)bob._mixedItems).Cast<object>().ToList();
        Assert.Equal(2, bobItems.Count);

        foreach (var item in bobItems)
        {
            Assert.True(sysObjType.IsInstanceOfType(item));
        }
    }

    [Fact]
    public void Proxies_ObjectArrayElements_AreCorrectProxyTypes()
    {
        var tagType = ProxyType("Ndump.TestApp.Tag");
        var orderType = ProxyType("Ndump.TestApp.Order");
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Bob has [tag2, order2] — elements should be actual proxy types
        var bob = customers.Single(c => (string?)c._name == "Bob");
        var bobItems = ((IEnumerable)bob._mixedItems).Cast<object>().ToList();

        var proxyTypes = bobItems.Select(d => d.GetType()).ToList();
        Assert.Contains(tagType, proxyTypes);
        Assert.Contains(orderType, proxyTypes);
    }

    [Fact]
    public void Proxies_ObjectArrayElements_CanReadFieldsDirectly()
    {
        var orderType = ProxyType("Ndump.TestApp.Order");
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Bob has [tag2, order2] — the Order element is already the right proxy type
        var bob = customers.Single(c => (string?)c._name == "Bob");
        var bobItems = ((IEnumerable)bob._mixedItems).Cast<dynamic>().ToList();

        var orderElement = bobItems.Single(d => ((object)d).GetType() == orderType);

        // Can read fields directly — no cast needed since ResolveProxy returns the right type
        Assert.Equal(1002, (int)orderElement._orderId);
    }

    [Fact]
    public void Proxies_ObjectArrayElements_HasCorrectProxyTypes()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has [order1, addr1, tag1, "hello"] — check proxy types
        var alice = customers.Single(c => (string?)c._name == "Alice");
        var aliceItems = ((IEnumerable)alice._mixedItems).Cast<object>().ToList();

        var proxyTypeNames = aliceItems.Select(d => d.GetType().FullName).ToList();
        Assert.Contains("_.Ndump.TestApp.Order", proxyTypeNames);
        Assert.Contains("_.Ndump.TestApp.Address", proxyTypeNames);
        Assert.Contains("_.Ndump.TestApp.Tag", proxyTypeNames);
        // String gets a System.String proxy (_.System.String)
        Assert.Contains("_.System.String", proxyTypeNames);
    }

    [Fact]
    public void Proxies_DiscoversAnimalHierarchy()
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
        var catType = ProxyType("Ndump.TestApp.Cat");
        var dogType = ProxyType("Ndump.TestApp.Dog");
        var animalType = ProxyType("Ndump.TestApp.Animal");

        // Cat and Dog should extend Animal in the proxy hierarchy
        Assert.True(animalType.IsAssignableFrom(catType));
        Assert.True(animalType.IsAssignableFrom(dogType));

        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has [cat1, dog1]
        var alice = customers.Single(c => (string?)c._name == "Alice");
        var alicePets = ((IEnumerable)alice._pets).Cast<object>().ToList();
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
        var dogType = ProxyType("Ndump.TestApp.Dog");
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has [cat1("Whiskers", 3, indoor=true), dog1("Rex", 4, "German Shepherd")]
        var alice = customers.Single(c => (string?)c._name == "Alice");
        var alicePets = ((IEnumerable)alice._pets).Cast<dynamic>().ToList();

        // Read inherited _name field (declared on Animal proxy) from all pets
        var petNames = alicePets.Select(p => (string?)p._name).OrderBy(n => n).ToList();
        Assert.Equal(["Rex", "Whiskers"], petNames);

        // Read Dog-specific _breed field
        var dog = alicePets.Single(p => ((object)p).GetType() == dogType);
        Assert.Equal("German Shepherd", (string)dog._breed);
    }

    [Fact]
    public void Proxies_AnimalArray_Charlie_HasMixedPets()
    {
        var catType = ProxyType("Ndump.TestApp.Cat");
        var dogType = ProxyType("Ndump.TestApp.Dog");
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Charlie has [cat2("Mittens"), dog1("Rex"), cat1("Whiskers")]
        var charlie = customers.Single(c => (string?)c._name == "Charlie");
        var charliePets = ((IEnumerable)charlie._pets).Cast<object>().ToList();
        Assert.Equal(3, charliePets.Count);

        var catCount = charliePets.Count(p => p.GetType() == catType);
        var dogCount = charliePets.Count(p => p.GetType() == dogType);
        Assert.Equal(2, catCount);
        Assert.Equal(1, dogCount);
    }

    [Fact]
    public void Proxies_InheritFromSystemObject()
    {
        var sysObjType = ProxyType("System.Object");

        Assert.True(sysObjType.IsAssignableFrom(ProxyType("Ndump.TestApp.Customer")));
        Assert.True(sysObjType.IsAssignableFrom(ProxyType("Ndump.TestApp.Order")));
    }

    [Fact]
    public void Proxies_SystemString_HasImplicitConversionToString()
    {
        var stringType = ProxyType("System.String");
        var sysObjType = ProxyType("System.Object");

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
        var stringType = ProxyType("System.String");
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has [order1, addr1, tag1, "hello"] — the string element should have ToString() == "hello"
        var alice = customers.Single(c => (string?)c._name == "Alice");
        var aliceMixed = ((IEnumerable)alice._mixedItems).Cast<object>().ToList();

        var stringElement = aliceMixed.Single(d => d.GetType() == stringType);
        Assert.Equal("hello", stringElement.ToString());
    }

    [Fact]
    public void Proxies_SystemString_ImplicitOperatorReturnsValue()
    {
        var stringType = ProxyType("System.String");
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has "hello" in _mixedItems
        var alice = customers.Single(c => (string?)c._name == "Alice");
        var aliceMixed = ((IEnumerable)alice._mixedItems).Cast<object>().ToList();

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
        var stringType = ProxyType("System.String");
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Charlie has "world" in _mixedItems
        var charlie = customers.Single(c => (string?)c._name == "Charlie");
        var charlieMixed = ((IEnumerable)charlie._mixedItems).Cast<dynamic>().ToList();

        var stringElement = charlieMixed.Single(d => ((object)d).GetType() == stringType);
        Assert.Equal("world", (string?)stringElement.Value);
    }

    [Fact]
    public void Proxies_CustomerHasDictionaryField()
    {
        var customerType = ProxyType("Ndump.TestApp.Customer");

        var scoresProp = customerType.GetProperty("_scores");
        Assert.NotNull(scoresProp);

        // The property type should be a closed generic proxy type
        var scoresType = scoresProp.PropertyType;
        Assert.True(scoresType.IsGenericType);
        Assert.Equal("Dictionary`2", scoresType.Name);
        Assert.Equal("_.System.Collections.Generic", scoresType.Namespace);
    }

    [Fact]
    public void Proxies_DictionaryProxy_CanReadCount()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has {"math": 95, "science": 87} — count = 2
        var alice = customers.Single(c => (string?)c._name == "Alice");
        dynamic aliceScores = alice._scores;
        Assert.Equal(2, (int)aliceScores._count);

        // Bob has {"art": 72} — count = 1
        var bob = customers.Single(c => (string?)c._name == "Bob");
        dynamic bobScores = bob._scores;
        Assert.Equal(1, (int)bobScores._count);

        // Charlie has {"math": 100, "art": 88, "science": 91} — count = 3
        var charlie = customers.Single(c => (string?)c._name == "Charlie");
        dynamic charlieScores = charlie._scores;
        Assert.Equal(3, (int)charlieScores._count);
    }

    [Fact]
    public void Proxies_DictionaryProxy_HasEntriesAndBuckets()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Bob has {"art": 72} — 1 entry
        var bob = customers.Single(c => (string?)c._name == "Bob");
        dynamic bobScores = bob._scores;

        // _entries returns a DumpArray<Entry> of struct element proxies
        var entries = ((IEnumerable)bobScores._entries).Cast<dynamic>().ToList();
        Assert.True(entries.Count >= 1);

        // Each entry is a proper Entry proxy with fields
        var firstEntry = entries[0];
        var entryType = ((object)firstEntry).GetType();
        Assert.NotNull(entryType.GetProperty("hashCode"));
        Assert.NotNull(entryType.GetProperty("next"));
        Assert.NotNull(entryType.GetProperty("value"));

        // Read the value from the first entry (Bob has {"art": 72}, count=1)
        Assert.Equal(72, (int)firstEntry.value);

        // _buckets should also exist (int[] array)
        Assert.NotNull(((object)bobScores).GetType().GetProperty("_buckets"));
    }

    [Fact]
    public void Proxies_StringArrayField_ReturnsStringValues()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");

        // Alice has ["vip", "early-adopter"]
        var alice = customers.Single(c => (string?)c._name == "Alice");
        var aliceTags = ((IEnumerable)alice._tags).Cast<string?>().ToList();
        Assert.Equal(2, aliceTags.Count);
        Assert.Contains("vip", aliceTags);
        Assert.Contains("early-adopter", aliceTags);

        // Bob has ["regular"]
        var bob = customers.Single(c => (string?)c._name == "Bob");
        var bobTags = ((IEnumerable)bob._tags).Cast<string?>().ToList();
        Assert.Equal(["regular"], bobTags);

        // Charlie has ["vip", "premium", "newsletter"]
        var charlie = customers.Single(c => (string?)c._name == "Charlie");
        var charlieTags = ((IEnumerable)charlie._tags).Cast<string?>().ToList();
        Assert.Equal(3, charlieTags.Count);
        Assert.Contains("premium", charlieTags);
    }

    // ── Nullable<T> field tests ──────────────────────────────────────

    [Fact]
    public void TypeInspector_OrderHasNullableFields()
    {
        using var ctx = DumpContext.Open(_fixture.DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var order = types.Single(t => t.FullName == "Ndump.TestApp.Order");

        var shippedAt = order.Fields.Single(f => f.Name == "_shippedAt");
        Assert.True(shippedAt.IsNullableValueType);
        Assert.Equal("System.DateTime", shippedAt.NullableInnerTypeName);

        var rating = order.Fields.Single(f => f.Name == "_rating");
        Assert.True(rating.IsNullableValueType);
        Assert.Equal("System.Int32", rating.NullableInnerTypeName);
    }

    [Fact]
    public void Proxies_OrderHasNullableProperties()
    {
        var orderType = ProxyType("Ndump.TestApp.Order");

        var ratingProp = orderType.GetProperty("_rating");
        Assert.NotNull(ratingProp);
        Assert.Equal(typeof(int?), ratingProp.PropertyType);

        var shippedAtProp = orderType.GetProperty("_shippedAt");
        Assert.NotNull(shippedAtProp);
        // _shippedAt is _.System.DateTime? (nullable proxy struct)
        Assert.True(Nullable.GetUnderlyingType(shippedAtProp.PropertyType) is not null
                    || shippedAtProp.PropertyType.IsClass,
            "_shippedAt should be a nullable or class type");
    }

    [Fact]
    public void Proxies_NullableIntField_HasValue()
    {
        var orders = GetInstances("Ndump.TestApp.Order");

        // order1 has rating=5
        var order1 = orders.Single(o => (int)o._orderId == 1001);
        int? rating = order1._rating;
        Assert.NotNull(rating);
        Assert.Equal(5, rating!.Value);
    }

    [Fact]
    public void Proxies_NullableIntField_IsNull()
    {
        var orders = GetInstances("Ndump.TestApp.Order");

        // order3 has no rating (null)
        var order3 = orders.Single(o => (int)o._orderId == 1003);
        int? rating = order3._rating;
        Assert.Null(rating);
    }

    [Fact]
    public void Proxies_NullableIntField_PartiallySet()
    {
        var orders = GetInstances("Ndump.TestApp.Order");

        // order2 has shippedAt set but rating is null
        var order2 = orders.Single(o => (int)o._orderId == 1002);
        int? rating = order2._rating;
        Assert.Null(rating);
    }

    [Fact]
    public void Proxies_NullableStructField_HasValue()
    {
        var orders = GetInstances("Ndump.TestApp.Order");

        // order1 has shippedAt=2025-06-15
        var order1 = orders.Single(o => (int)o._orderId == 1001);
        var shippedAt = order1._shippedAt;
        Assert.NotNull((object?)shippedAt);
    }

    [Fact]
    public void Proxies_NullableStructField_IsNull()
    {
        var orders = GetInstances("Ndump.TestApp.Order");

        // order3 has no shippedAt (null)
        var order3 = orders.Single(o => (int)o._orderId == 1003);
        var shippedAt = order3._shippedAt;
        Assert.Null((object?)shippedAt);
    }

    [Fact]
    public void Proxies_NullableStructField_PartiallySet()
    {
        var orders = GetInstances("Ndump.TestApp.Order");

        // order2 has shippedAt=2025-07-01 but no rating
        var order2 = orders.Single(o => (int)o._orderId == 1002);
        var shippedAt = order2._shippedAt;
        Assert.NotNull((object?)shippedAt);
    }

    [Fact]
    public void Proxies_NullableDoesNotGenerateSeparateProxy()
    {
        // System.Nullable<...> should NOT have its own proxy type
        Assert.Null(Assembly.GetType("_.System.Nullable`1"));
    }
}
