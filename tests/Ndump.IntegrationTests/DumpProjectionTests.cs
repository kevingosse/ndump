using System.Reflection;
using Ndump.Core;
using _.Ndump.TestApp;

namespace Ndump.IntegrationTests;

public class DumpProjectionTests : DumpTests
{
    private DumpContext Context => Projection.Context;

    [Test]
    public void DumpFile_Exists()
    {
        File.Exists(DumpPath).ShouldBeTrue($"Dump file should exist at {DumpPath}");
    }

    [Test]
    public void DumpContext_CanOpen()
    {
        using var ctx = DumpContext.Open(DumpPath);
        ctx.Runtime.ShouldNotBeNull();
        ctx.Heap.ShouldNotBeNull();
    }

    [Test]
    public void TypeInspector_DiscoversTestAppTypes()
    {
        using var ctx = DumpContext.Open(DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var typeNames = types.Select(t => t.FullName).ToHashSet();

        typeNames.ShouldContain("Ndump.TestApp.Customer");
        typeNames.ShouldContain("Ndump.TestApp.Order");
        typeNames.ShouldContain("Ndump.TestApp.Address");
        typeNames.ShouldContain("Ndump.TestApp.Tag");
        typeNames.ShouldContain("Ndump.TestApp.Cat");
        typeNames.ShouldContain("Ndump.TestApp.Dog");
        typeNames.ShouldContain("System.Object");
    }

    [Test]
    public void TypeInspector_CustomerHasExpectedFields()
    {
        using var ctx = DumpContext.Open(DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var customer = types.Single(t => t.FullName == "Ndump.TestApp.Customer");
        var fieldNames = customer.Fields.Select(f => f.Name).ToHashSet();

        fieldNames.ShouldContain("_name");
        fieldNames.ShouldContain("_age");
        fieldNames.ShouldContain("_isActive");
        fieldNames.ShouldContain("_lastOrder");
        fieldNames.ShouldContain("_address");
        fieldNames.ShouldContain("_orderHistory");
        fieldNames.ShouldContain("_mixedItems");
        fieldNames.ShouldContain("_pets");
        fieldNames.ShouldContain("_tags");
    }

    [Test]
    public void FullPipeline_ProjectsDump_AndProxiesWork()
    {
        Projection.GeneratedAssembly.ShouldNotBeNull();
        Projection.DiscoveredTypes.Count.ShouldBeGreaterThan(0);
        Projection.GeneratedFiles.Count.ShouldBeGreaterThan(0);
    }

    [Test]
    public void Proxies_CanEnumerateCustomerInstances()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // We created 3 customers in TestApp
        customers.Count.ShouldBe(3);
    }

    [Test]
    public void Proxies_CanReadCustomerStringField()
    {
        var customers = Customer.GetInstances(Context).ToList();

        var names = customers.Select(c => c._name).OrderBy(n => n).ToList();

        names.ShouldContain("Alice");
        names.ShouldContain("Bob");
        names.ShouldContain("Charlie");
    }

    [Test]
    public void Proxies_CanReadCustomerIntField()
    {
        var customers = Customer.GetInstances(Context).ToList();

        var ages = customers.Select(c => c._age).OrderBy(a => a).ToList();

        ages.ShouldBe([28, 30, 45]);
    }

    [Test]
    public void Proxies_CanReadCustomerBoolField()
    {
        var customers = Customer.GetInstances(Context).ToList();

        var activeValues = customers.Select(c => c._isActive).ToList();

        // Alice=true, Bob=false, Charlie=true
        activeValues.Count(v => v).ShouldBe(2);
        activeValues.Count(v => !v).ShouldBe(1);
    }

    [Test]
    public void Proxies_CanNavigateObjectReferences()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Navigate Customer -> _lastOrder -> _orderId
        var orderIds = new List<int>();

        foreach (var customer in customers)
        {
            var order = customer._lastOrder;
            order.ShouldNotBeNull();
            orderIds.Add(order._orderId);
        }

        orderIds.Sort();
        orderIds.ShouldBe([1001, 1002, 1003]);
    }

    [Test]
    public void Proxies_CanNavigateObjectReference_AndReadFields()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Alice has addr1("123 Main St", "Springfield", 62701)
        var alice = customers.Single(c => c._name == "Alice");
        var aliceAddr = alice._address;
        aliceAddr.ShouldNotBeNull();
        aliceAddr.ShouldBeOfType<Address>();
        aliceAddr._street.ShouldBe("123 Main St");
        aliceAddr._city.ShouldBe("Springfield");
        aliceAddr._zipCode.ShouldBe(62701);

        // Bob has addr2("456 Oak Ave", "Shelbyville", 62702)
        var bob = customers.Single(c => c._name == "Bob");
        var bobAddr = bob._address;
        bobAddr.ShouldNotBeNull();
        bobAddr._street.ShouldBe("456 Oak Ave");
        bobAddr._city.ShouldBe("Shelbyville");
        bobAddr._zipCode.ShouldBe(62702);
    }

    [Test]
    public void Proxies_CanEnumerateTagInstances()
    {
        var tags = Tag.GetInstances(Context).ToList();
        tags.Count.ShouldBe(2);

        var labels = tags.Select(t => t._label).OrderBy(l => l).ToList();
        labels.ShouldBe(new[] { "important", "urgent" });
    }

    [Test]
    public void Proxies_FromAddress_ReturnsValidProxy()
    {
        // Get an address from EnumerateInstances
        var addresses = Context.EnumerateInstances("Ndump.TestApp.Tag").ToList();
        addresses.ShouldNotBeEmpty();

        var proxy = Tag.FromAddress(addresses[0], Context);
        proxy.ShouldNotBeNull();
        proxy.GetObjectAddress().ShouldBe(addresses[0]);
    }

    [Test]
    public void TypeInspector_CustomerOrderHistoryIsArray()
    {
        using var ctx = DumpContext.Open(DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var customer = types.Single(t => t.FullName == "Ndump.TestApp.Customer");
        var field = customer.Fields.Single(f => f.Name == "_orderHistory");

        field.Kind.ShouldBe(FieldKind.Array);
        field.ArrayElementTypeName.ShouldBe("Ndump.TestApp.Order");
        field.ArrayElementKind.ShouldBe(FieldKind.ObjectReference);
    }

    [Test]
    public void Proxies_CanReadArrayField()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Find Alice (has [order1, order2])
        var alice = customers.Single(c => c._name == "Alice");
        var aliceOrders = alice._orderHistory!.ToList();
        aliceOrders.Count.ShouldBe(2);

        var aliceOrderIds = aliceOrders.Select(o => o!._orderId).OrderBy(id => id).ToList();
        aliceOrderIds.ShouldBe(new[] { 1001, 1002 });

        // Find Charlie (has [order1, order2, order3])
        var charlie = customers.Single(c => c._name == "Charlie");
        var charlieOrders = charlie._orderHistory!.ToList();
        charlieOrders.Count.ShouldBe(3);
    }

    [Test]
    public void Proxies_ArrayField_HasLength()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Bob has [order2] — 1 element
        var bob = customers.Single(c => c._name == "Bob");
        bob._orderHistory!.Length.ShouldBe(1);
    }

    [Test]
    public void Proxies_OrderHasDoubleField()
    {
        var orders = Order.GetInstances(Context).ToList();

        var totals = orders.Select(o => o._total).OrderBy(t => t).ToList();

        totals[0].ShouldBe(5.00, Math.Pow(10, -2));
        totals[1].ShouldBe(29.99, Math.Pow(10, -2));
        totals[2].ShouldBe(149.50, Math.Pow(10, -2));
    }

    [Test]
    public void TypeInspector_CustomerMixedItemsIsObjectArray()
    {
        using var ctx = DumpContext.Open(DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var customer = types.Single(t => t.FullName == "Ndump.TestApp.Customer");
        var field = customer.Fields.Single(f => f.Name == "_mixedItems");

        field.Kind.ShouldBe(FieldKind.Array);
        field.ArrayElementTypeName.ShouldBe("System.Object");
        field.ArrayElementKind.ShouldBe(FieldKind.ObjectReference);
    }

    [Test]
    public void Proxies_CanReadObjectArrayField()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Alice has [order1, addr1, tag1, "hello"] — 4 elements
        var alice = customers.Single(c => c._name == "Alice");
        var aliceItems = alice._mixedItems!.ToList();
        aliceItems.Count.ShouldBe(4);
    }

    [Test]
    public void Proxies_ObjectArrayElements_InheritFromSystemObject()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Bob has [tag2, order2] — elements should inherit from _.System.Object
        var bob = customers.Single(c => c._name == "Bob");
        var bobItems = bob._mixedItems!.ToList();
        bobItems.Count.ShouldBe(2);

        foreach (var item in bobItems)
        {
            item.ShouldBeAssignableTo<_.System.Object>();
        }
    }

    [Test]
    public void Proxies_ObjectArrayElements_AreCorrectProxyTypes()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Bob has [tag2, order2] — elements should be actual proxy types
        var bob = customers.Single(c => c._name == "Bob");
        var bobItems = bob._mixedItems!.ToList();

        var proxyTypes = bobItems.Select(d => d!.GetType()).ToList();
        proxyTypes.ShouldContain(typeof(Tag));
        proxyTypes.ShouldContain(typeof(Order));
    }

    [Test]
    public void Proxies_ObjectArrayElements_CanReadFieldsDirectly()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Bob has [tag2, order2] — the Order element is already the right proxy type
        var bob = customers.Single(c => c._name == "Bob");
        var orderElement = bob._mixedItems!.OfType<Order>().Single();

        // Can read fields directly — no cast needed since ResolveProxy returns the right type
        orderElement._orderId.ShouldBe(1002);
    }

    [Test]
    public void Proxies_ObjectArrayElements_HasCorrectProxyTypes()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Alice has [order1, addr1, tag1, "hello"] — check proxy types
        var alice = customers.Single(c => c._name == "Alice");
        var aliceItems = alice._mixedItems!.ToList();

        var proxyTypeNames = aliceItems.Select(d => d!.GetType().FullName).ToList();
        proxyTypeNames.ShouldContain("_.Ndump.TestApp.Order");
        proxyTypeNames.ShouldContain("_.Ndump.TestApp.Address");
        proxyTypeNames.ShouldContain("_.Ndump.TestApp.Tag");
        // String gets a System.String proxy (_.System.String)
        proxyTypeNames.ShouldContain("_.System.String");
    }

    [Test]
    public void Proxies_DiscoversAnimalHierarchy()
    {
        using var ctx = DumpContext.Open(DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var typeNames = types.Select(t => t.FullName).ToHashSet();

        typeNames.ShouldContain("Ndump.TestApp.Cat");
        typeNames.ShouldContain("Ndump.TestApp.Dog");

        // Cat and Dog should have Animal as BaseTypeName
        var cat = types.Single(t => t.FullName == "Ndump.TestApp.Cat");
        cat.BaseTypeName.ShouldBe("Ndump.TestApp.Animal");

        var dog = types.Single(t => t.FullName == "Ndump.TestApp.Dog");
        dog.BaseTypeName.ShouldBe("Ndump.TestApp.Animal");
    }

    [Test]
    public void Proxies_AnimalArray_ContainsCatsAndDogs()
    {
        // Cat and Dog should extend Animal in the proxy hierarchy
        typeof(Animal).IsAssignableFrom(typeof(Cat)).ShouldBeTrue();
        typeof(Animal).IsAssignableFrom(typeof(Dog)).ShouldBeTrue();

        var customers = Customer.GetInstances(Context).ToList();

        // Alice has [cat1, dog1]
        var alice = customers.Single(c => c._name == "Alice");
        var alicePets = alice._pets!.ToList();
        alicePets.Count.ShouldBe(2);

        // Elements should be actual Cat and Dog proxy types, not just Animal
        alicePets.OfType<Cat>().Count().ShouldBe(1);
        alicePets.OfType<Dog>().Count().ShouldBe(1);

        // All pets are assignable to Animal
        foreach (var pet in alicePets)
        {
            pet.ShouldBeAssignableTo<Animal>();
        }
    }

    [Test]
    public void Proxies_AnimalArray_CanReadInheritedAndOwnFields()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Alice has [cat1("Whiskers", 3, indoor=true), dog1("Rex", 4, "German Shepherd")]
        var alice = customers.Single(c => c._name == "Alice");
        var alicePets = alice._pets!.ToList();

        // Read inherited _name field (declared on Animal proxy) from all pets
        var petNames = alicePets.Select(p => p!._name).OrderBy(n => n).ToList();
        petNames.ShouldBe(["Rex", "Whiskers"]);

        // Read Dog-specific _breed field
        var dog = alicePets.OfType<Dog>().Single();
        dog._breed.ShouldBe("German Shepherd");
    }

    [Test]
    public void Proxies_AnimalArray_Charlie_HasMixedPets()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Charlie has [cat2("Mittens"), dog1("Rex"), cat1("Whiskers")]
        var charlie = customers.Single(c => c._name == "Charlie");
        var charliePets = charlie._pets!.ToList();
        charliePets.Count.ShouldBe(3);

        charliePets.OfType<Cat>().Count().ShouldBe(2);
        charliePets.OfType<Dog>().Count().ShouldBe(1);
    }

    [Test]
    public void Proxies_InheritFromSystemObject()
    {
        typeof(_.System.Object).IsAssignableFrom(typeof(Customer)).ShouldBeTrue();
        typeof(_.System.Object).IsAssignableFrom(typeof(Order)).ShouldBeTrue();
    }

    [Test]
    public void Proxies_SystemString_HasImplicitConversionToString()
    {
        // _.System.String should extend _.System.Object
        typeof(_.System.Object).IsAssignableFrom(typeof(_.System.String)).ShouldBeTrue();

        // Should have an implicit operator to string?
        var implicitOp = typeof(_.System.String).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(m => m.Name == "op_Implicit" && m.ReturnType == typeof(string));
        implicitOp.ShouldNotBeNull();
    }

    [Test]
    public void Proxies_SystemString_ToStringReturnsValue()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Alice has [order1, addr1, tag1, "hello"] — the string element should have ToString() == "hello"
        var alice = customers.Single(c => c._name == "Alice");
        var stringElement = alice._mixedItems!.OfType<_.System.String>().Single();
        stringElement.ToString().ShouldBe("hello");
    }

    [Test]
    public void Proxies_SystemString_ImplicitOperatorReturnsValue()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Alice has "hello" in _mixedItems
        var alice = customers.Single(c => c._name == "Alice");
        var stringElement = alice._mixedItems!.OfType<_.System.String>().Single();

        // Use the implicit operator directly
        var result = (string?)stringElement;
        result.ShouldBe("hello");
    }

    [Test]
    public void Proxies_SystemString_ValuePropertyReturnsValue()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Charlie has "world" in _mixedItems
        var charlie = customers.Single(c => c._name == "Charlie");
        var stringElement = charlie._mixedItems!.OfType<_.System.String>().Single();
        stringElement.Value.ShouldBe("world");
    }

    [Test]
    public void Proxies_CustomerHasDictionaryField()
    {
        var scoresProp = typeof(Customer).GetProperty("_scores");
        scoresProp.ShouldNotBeNull();

        // The property type should be a closed generic proxy type
        var scoresType = scoresProp.PropertyType;
        scoresType.IsGenericType.ShouldBeTrue();
        scoresType.Name.ShouldBe("Dictionary`2");
        scoresType.Namespace.ShouldBe("_.System.Collections.Generic");
    }

    [Test]
    public void Proxies_DictionaryProxy_CanReadCount()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Alice has {"math": 95, "science": 87} — count = 2
        var alice = customers.Single(c => c._name == "Alice");
        alice._scores!._count.ShouldBe(2);

        // Bob has {"art": 72} — count = 1
        var bob = customers.Single(c => c._name == "Bob");
        bob._scores!._count.ShouldBe(1);

        // Charlie has {"math": 100, "art": 88, "science": 91} — count = 3
        var charlie = customers.Single(c => c._name == "Charlie");
        charlie._scores!._count.ShouldBe(3);
    }

    [Test]
    public void Proxies_DictionaryProxy_HasEntriesAndBuckets()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Bob has {"art": 72} — 1 entry
        var bob = customers.Single(c => c._name == "Bob");
        var bobScores = bob._scores!;

        // _entries returns a DumpArray<Entry> of struct element proxies
        var entries = bobScores._entries!.ToList();
        entries.Count.ShouldBeGreaterThanOrEqualTo(1);

        // Each entry is a proper Entry proxy with fields
        var entryType = typeof(_.System.Collections.Generic.Dictionary<string, int>.Entry);
        entryType.GetProperty("hashCode").ShouldNotBeNull();
        entryType.GetProperty("next").ShouldNotBeNull();
        entryType.GetProperty("value").ShouldNotBeNull();

        // Read the value from the first entry (Bob has {"art": 72}, count=1)
        entries[0].value.ShouldBe(72);

        // _buckets should also exist
        typeof(_.System.Collections.Generic.Dictionary<string, int>).GetProperty("_buckets").ShouldNotBeNull();
    }

    [Test]
    public void Proxies_StringArrayField_ReturnsStringValues()
    {
        var customers = Customer.GetInstances(Context).ToList();

        // Alice has ["vip", "early-adopter"]
        var alice = customers.Single(c => c._name == "Alice");
        var aliceTags = alice._tags!.ToList();
        aliceTags.Count.ShouldBe(2);
        aliceTags.ShouldContain("vip");
        aliceTags.ShouldContain("early-adopter");

        // Bob has ["regular"]
        var bob = customers.Single(c => c._name == "Bob");
        var bobTags = bob._tags!.ToList();
        bobTags.ShouldBe(["regular"]);

        // Charlie has ["vip", "premium", "newsletter"]
        var charlie = customers.Single(c => c._name == "Charlie");
        var charlieTags = charlie._tags!.ToList();
        charlieTags.Count.ShouldBe(3);
        charlieTags.ShouldContain("premium");
    }

    // ── Nullable<T> field tests ──────────────────────────────────────

    [Test]
    public void TypeInspector_OrderHasNullableFields()
    {
        using var ctx = DumpContext.Open(DumpPath);
        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(ctx);

        var order = types.Single(t => t.FullName == "Ndump.TestApp.Order");

        var shippedAt = order.Fields.Single(f => f.Name == "_shippedAt");
        shippedAt.IsNullableValueType.ShouldBeTrue();
        shippedAt.NullableInnerTypeName.ShouldBe("System.DateTime");

        var rating = order.Fields.Single(f => f.Name == "_rating");
        rating.IsNullableValueType.ShouldBeTrue();
        rating.NullableInnerTypeName.ShouldBe("System.Int32");
    }

    [Test]
    public void Proxies_OrderHasNullableProperties()
    {
        var ratingProp = typeof(Order).GetProperty("_rating");
        ratingProp.ShouldNotBeNull();
        ratingProp.PropertyType.ShouldBe(typeof(int?));

        var shippedAtProp = typeof(Order).GetProperty("_shippedAt");
        shippedAtProp.ShouldNotBeNull();
        // _shippedAt is _.System.DateTime? (nullable proxy struct)
        (Nullable.GetUnderlyingType(shippedAtProp.PropertyType) is not null
                    || shippedAtProp.PropertyType.IsClass).ShouldBeTrue("_shippedAt should be a nullable or class type");
    }

    [Test]
    public void Proxies_NullableIntField_HasValue()
    {
        var orders = Order.GetInstances(Context).ToList();

        // order1 has rating=5
        var order1 = orders.Single(o => o._orderId == 1001);
        int? rating = order1._rating;
        rating.ShouldNotBeNull();
        rating.Value.ShouldBe(5);
    }

    [Test]
    public void Proxies_NullableIntField_IsNull()
    {
        var orders = Order.GetInstances(Context).ToList();

        // order3 has no rating (null)
        var order3 = orders.Single(o => o._orderId == 1003);
        order3._rating.ShouldBeNull();
    }

    [Test]
    public void Proxies_NullableIntField_PartiallySet()
    {
        var orders = Order.GetInstances(Context).ToList();

        // order2 has shippedAt set but rating is null
        var order2 = orders.Single(o => o._orderId == 1002);
        int? rating = order2._rating;
        rating.ShouldBeNull();
    }

    [Test]
    public void Proxies_NullableStructField_HasValue()
    {
        var orders = Order.GetInstances(Context).ToList();

        // order1 has shippedAt=2025-06-15
        var order1 = orders.Single(o => o._orderId == 1001);
        order1._shippedAt.ShouldNotBeNull();
    }

    [Test]
    public void Proxies_NullableStructField_IsNull()
    {
        var orders = Order.GetInstances(Context).ToList();

        // order3 has no shippedAt (null)
        var order3 = orders.Single(o => o._orderId == 1003);
        order3._shippedAt.ShouldBeNull();
    }

    [Test]
    public void Proxies_NullableStructField_PartiallySet()
    {
        var orders = Order.GetInstances(Context).ToList();

        // order2 has shippedAt=2025-07-01 but no rating
        var order2 = orders.Single(o => o._orderId == 1002);
        order2._shippedAt.ShouldNotBeNull();
    }

    [Test]
    public void Proxies_NullableDoesNotGenerateSeparateProxy()
    {
        // System.Nullable<...> should NOT have its own proxy type — verify via runtime-compiled assembly
        Projection.GeneratedAssembly.GetType("_.System.Nullable`1").ShouldBeNull();
    }
}
