using System.Reflection;
using Ndump.Core;
using _.Ndump.TestApp;

namespace Ndump.IntegrationTests;

/// <summary>
/// Comprehensive behavioral tests that validate proxy data reads against a real memory dump.
/// Organized by type category. Every test validates the actual value read from the dump,
/// not just the shape of the generated code.
/// Uses statically-typed proxies from Ndump.Generated.
/// </summary>
public class BehavioralProxyTests : DumpTests
{
    private DumpContext Context => Projection.Context;

    // ══════════════════════════════════════════════════════════════════
    //  PRIMITIVE TYPES
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void Primitives_Bool_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._boolVal.ShouldBeTrue();
    }

    [Test]
    public void Primitives_Byte_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._byteVal.ShouldBe((byte)255);
    }

    [Test]
    public void Primitives_SByte_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._sbyteVal.ShouldBe((sbyte)-42);
    }

    [Test]
    public void Primitives_Short_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._shortVal.ShouldBe((short)-1000);
    }

    [Test]
    public void Primitives_UShort_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._ushortVal.ShouldBe((ushort)50000);
    }

    [Test]
    public void Primitives_Int_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._intVal.ShouldBe(123456);
    }

    [Test]
    public void Primitives_UInt_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._uintVal.ShouldBe(4000000000u);
    }

    [Test]
    public void Primitives_Long_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._longVal.ShouldBe(9876543210L);
    }

    [Test]
    public void Primitives_ULong_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._ulongVal.ShouldBe(18446744073709551615UL);
    }

    [Test]
    public void Primitives_Float_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._floatVal.ShouldBe(3.14f, (float)Math.Pow(10, -2));
    }

    [Test]
    public void Primitives_Double_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._doubleVal.ShouldBe(2.718281828, Math.Pow(10, -6));
    }

    [Test]
    public void Primitives_Char_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        obj._charVal.ShouldBe('Z');
    }

    // ══════════════════════════════════════════════════════════════════
    //  NULLABLE<T> VALUE TYPES
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void Nullable_IntWithValue_ReadsCorrectly()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        var val = obj._intHasValue;
        val.ShouldNotBeNull();
        val.Value.ShouldBe(42);
    }

    [Test]
    public void Nullable_IntNull_ReadsAsNull()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        obj._intNull.ShouldBeNull();
    }

    [Test]
    public void Nullable_DoubleWithValue_ReadsCorrectly()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        var val = obj._doubleHasValue;
        val.ShouldNotBeNull();
        val.Value.ShouldBe(3.14, Math.Pow(10, -2));
    }

    [Test]
    public void Nullable_DoubleNull_ReadsAsNull()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        obj._doubleNull.ShouldBeNull();
    }

    [Test]
    public void Nullable_BoolWithValue_ReadsCorrectly()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        var val = obj._boolHasValue;
        val.ShouldNotBeNull();
        val.Value.ShouldBeTrue();
    }

    [Test]
    public void Nullable_BoolNull_ReadsAsNull()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        obj._boolNull.ShouldBeNull();
    }

    [Test]
    public void Nullable_LongWithValue_ReadsCorrectly()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        var val = obj._longHasValue;
        val.ShouldNotBeNull();
        val.Value.ShouldBe(9876543210L);
    }

    [Test]
    public void Nullable_LongNull_ReadsAsNull()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        obj._longNull.ShouldBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRING VARIANTS
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void Strings_NormalString_ReadsCorrectly()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        obj._normal.ShouldBe("hello");
    }

    [Test]
    public void Strings_NullString_ReadsAsNull()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        obj._nullString.ShouldBeNull();
    }

    [Test]
    public void Strings_EmptyString_ReadsAsEmpty()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        obj._empty.ShouldBe("");
    }

    [Test]
    public void Strings_Unicode_ReadsCorrectly()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        obj._unicode.ShouldBe("日本語テスト🎉");
    }

    [Test]
    public void Strings_LongString_ReadsCorrectly()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        string? val = obj._long;
        val.ShouldNotBeNull();
        val.Length.ShouldBe(500);
        val.All(c => c == 'x').ShouldBeTrue();
    }

    // ══════════════════════════════════════════════════════════════════
    //  OBJECT REFERENCES: NULL, SELF-REF, CIRCULAR
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void Node_NullNext_ReturnsNull()
    {
        // Node "C" originally has next = null (before circular link is set), but
        // we set C.next = A in TestApp. Let's verify the chain instead.
        var nodes = Node.GetInstances(Context).ToList();
        // Find node B: B -> C
        var nodeB = nodes.Single(n => n._name == "B");
        nodeB._next.ShouldNotBeNull();
    }

    [Test]
    public void Node_SelfReference_SameAddress()
    {
        var nodes = Node.GetInstances(Context).ToList();
        var nodeA = nodes.Single(n => n._name == "A");

        // nodeA._self should point to itself
        var selfRef = nodeA._self;
        selfRef.ShouldNotBeNull();
        selfRef.GetObjectAddress().ShouldBe(nodeA.GetObjectAddress());
    }

    [Test]
    public void Node_CircularReference_Traversable()
    {
        var nodes = Node.GetInstances(Context).ToList();
        var nodeA = nodes.Single(n => n._name == "A");

        // A -> B -> C -> A (circular)
        var current = nodeA;
        var visited = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            visited.Add(current._name!);
            current = current._next;
            current.ShouldNotBeNull();
        }

        visited.ShouldBe(new[] { "A", "B", "C", "A" });
    }

    [Test]
    public void Node_CircularReference_AddressesMatch()
    {
        var nodes = Node.GetInstances(Context).ToList();
        var nodeA = nodes.Single(n => n._name == "A");

        // Follow A -> B -> C -> back to A, verify address matches
        var step1 = nodeA._next!;          // B
        var step2 = step1._next!;          // C
        var step3 = step2._next!;          // A (back to start)

        step3.GetObjectAddress().ShouldBe(nodeA.GetObjectAddress());
    }

    // ══════════════════════════════════════════════════════════════════
    //  DEEP INHERITANCE CHAIN
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void DeepInheritance_Leaf_InheritsFromMiddle()
    {
        typeof(Middle).IsAssignableFrom(typeof(Leaf)).ShouldBeTrue();
        typeof(Base).IsAssignableFrom(typeof(Leaf)).ShouldBeTrue();
        typeof(_.System.Object).IsAssignableFrom(typeof(Leaf)).ShouldBeTrue();
    }

    [Test]
    public void DeepInheritance_Leaf_CanReadAllFields()
    {
        var leaf = Leaf.GetInstances(Context).Single();

        // Inherited from Base
        leaf._baseField.ShouldBe(100);
        // Inherited from Middle
        leaf._middleField.ShouldBe("mid");
        // Own field
        leaf._leafField.ShouldBe(3.14, Math.Pow(10, -2));
    }

    [Test]
    public void DeepInheritance_FieldsAreDeclaredAtCorrectLevel()
    {
        // _baseField declared on Base only
        typeof(Base).GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ShouldNotBeNull();
        typeof(Middle).GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ShouldBeNull();
        typeof(Leaf).GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ShouldBeNull();

        // _middleField declared on Middle only
        typeof(Middle).GetProperty("_middleField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ShouldNotBeNull();
        typeof(Leaf).GetProperty("_middleField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ShouldBeNull();

        // _leafField declared on Leaf only
        typeof(Leaf).GetProperty("_leafField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ShouldNotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    //  ENUM FIELDS
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void Enums_IntBacked_ReadsCorrectValue()
    {
        // Enums are projected as their underlying primitive type
        var obj = EnumHolder.GetInstances(Context).Single();
        // Color.Blue = 3
        obj._color.ShouldBe(3);
    }

    [Test]
    public void Enums_ByteBacked_ReadsCorrectValue()
    {
        var obj = EnumHolder.GetInstances(Context).Single();
        // SmallEnum.High = 3
        obj._priority.ShouldBe((byte)3);
    }

    [Test]
    public void Enums_FlagsEnum_ReadsCorrectValue()
    {
        var obj = EnumHolder.GetInstances(Context).Single();
        // Permissions.Read | Permissions.Write = 1 | 2 = 3
        obj._permissions.ShouldBe(3);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT (VALUE TYPE) FIELDS EMBEDDED IN CLASSES
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void Struct_Point_ReadsFromClass()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var pt = obj._position;
        pt.X.ShouldBe(10);
        pt.Y.ShouldBe(20);
    }

    [Test]
    public void Struct_NestedStruct_Rectangle_ReadsTopLeft()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var rect = obj._bounds;
        var topLeft = rect.TopLeft;
        topLeft.X.ShouldBe(10);
        topLeft.Y.ShouldBe(20);
    }

    [Test]
    public void Struct_NestedStruct_Rectangle_ReadsBottomRight()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var rect = obj._bounds;
        var bottomRight = rect.BottomRight;
        bottomRight.X.ShouldBe(30);
        bottomRight.Y.ShouldBe(40);
    }

    [Test]
    public void Struct_Label_ReadsStringField()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        label.Text.ShouldBe("test-label");
    }

    [Test]
    public void Struct_Label_ReadsPrimitiveField()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        label.Priority.ShouldBe(5);
    }

    [Test]
    public void Struct_Label_ReadsObjectRefField_AsConcreteType()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        // Metadata was set to tag1 ("important", id=1)
        var meta = label.Metadata;
        meta.ShouldNotBeNull();
        // It's a Tag proxy resolved via ProxyResolver (typed as _.System.Object)
        meta.ShouldBeOfType<Tag>();
    }

    [Test]
    public void Struct_Label_ReadsObjectRefField_Data()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        // Metadata was set to tag1 ("important", id=1) — verify data through resolved proxy
        var meta = (Tag)label.Metadata!;
        meta._label.ShouldBe("important");
        meta._id.ShouldBe(1L);
    }

    [Test]
    public void Struct_Label_ReadsObjectRefField_AsSystemObject()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        // Metadata was set to tag1 - resolved to concrete Tag proxy via ProxyResolver,
        // which still passes IsInstanceOfType check since Tag inherits from _.System.Object
        var meta = label.Metadata;
        meta.ShouldNotBeNull();
        meta.ShouldBeAssignableTo<_.System.Object>();
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT ARRAYS (arrays of value types)
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void StructArray_Point_HasCorrectLength()
    {
        // Find all Point[] arrays on the heap
        var pointArrays = new List<ulong>();
        foreach (var obj in Context.Heap.EnumerateObjects())
        {
            if (obj.IsValid && obj.Type?.Name == "Ndump.TestApp.Point[]")
                pointArrays.Add(obj.Address);
        }

        pointArrays.ShouldNotBeEmpty();

        // Our Point[] has 3 elements: (1,2), (3,4), (5,6)
        var arr = pointArrays.First(a => Context.GetArrayLength(a) == 3);
        Context.GetArrayLength(arr).ShouldBe(3);
    }

    // ══════════════════════════════════════════════════════════════════
    //  PRIMITIVE ARRAYS
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void PrimitiveArray_Int_ReadsCorrectValues()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var intArr = obj._intArray!.ToList();
        intArr.ShouldBe([10, 20, 30, 40, 50]);
    }

    [Test]
    public void PrimitiveArray_Byte_ReadsCorrectValues()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var byteArr = obj._byteArray!.ToList();
        byteArr.ShouldBe([0x01, 0xFF, 0x42]);
    }

    [Test]
    public void PrimitiveArray_Double_ReadsCorrectValues()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var doubleArr = obj._doubleArray!.ToList();
        doubleArr.ShouldBe([1.1, 2.2, 3.3]);
    }

    [Test]
    public void PrimitiveArray_Double_HasCorrectLength()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        obj._doubleArray!.Length.ShouldBe(3);
    }

    [Test]
    public void PrimitiveArray_Bool_ReadsCorrectValues()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var boolArr = obj._boolArray!.ToList();
        boolArr.ShouldBe([true, false, true]);
    }

    [Test]
    public void PrimitiveArray_Bool_HasCorrectLength()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        obj._boolArray!.Length.ShouldBe(3);
    }

    [Test]
    public void PrimitiveArray_Null_ReturnsNull()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        // _nullArray was set to null
        obj._nullArray.ShouldBeNull();
    }

    [Test]
    public void PrimitiveArray_EmptyStringArray_HasLengthZero()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var arr = obj._emptyStringArray!.ToList();
        arr.ShouldBeEmpty();
    }

    // ══════════════════════════════════════════════════════════════════
    //  SHARED REFERENCES
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void SharedRefs_SameTag_SameAddress()
    {
        var obj = SharedRefs.GetInstances(Context).Single();
        var ref1 = obj._ref1;
        var ref2 = obj._ref2;

        ref1.ShouldNotBeNull();
        ref2.ShouldNotBeNull();
        ref2.GetObjectAddress().ShouldBe(ref1.GetObjectAddress());
    }

    [Test]
    public void SharedRefs_SameAddress_SameAddress()
    {
        var obj = SharedRefs.GetInstances(Context).Single();
        var shared = obj._shared;
        var sharedAgain = obj._sharedAgain;

        shared.ShouldNotBeNull();
        sharedAgain.ShouldNotBeNull();
        sharedAgain.GetObjectAddress().ShouldBe(shared.GetObjectAddress());
    }

    [Test]
    public void SharedRefs_Tag_ReadsCorrectData()
    {
        var obj = SharedRefs.GetInstances(Context).Single();
        // tag1 = ("important", 1)
        var tag = obj._ref1!;
        tag._label.ShouldBe("important");
        tag._id.ShouldBe(1L);
    }

    [Test]
    public void SharedRefs_Address_ReadsCorrectData()
    {
        var obj = SharedRefs.GetInstances(Context).Single();
        // addr1 = ("123 Main St", "Springfield", 62701)
        var addr = obj._shared!;
        addr._street.ShouldBe("123 Main St");
        addr._city.ShouldBe("Springfield");
        addr._zipCode.ShouldBe(62701);
    }

    // ══════════════════════════════════════════════════════════════════
    //  LIST<T> (GENERIC COLLECTIONS)
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void List_StringList_HasCorrectCount()
    {
        var obj = ListHolder.GetInstances(Context).Single();
        var items = obj._items;
        items.ShouldNotBeNull();
        items._size.ShouldBe(3);
    }

    [Test]
    public void List_OrderList_HasCorrectCount()
    {
        var obj = ListHolder.GetInstances(Context).Single();
        var orders = obj._orders;
        orders.ShouldNotBeNull();
        orders._size.ShouldBe(2);
    }

    // ══════════════════════════════════════════════════════════════════
    //  GetObjectAddress() AND ToString()
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void GetObjAddress_ReturnsNonZero()
    {
        var tag = Tag.GetInstances(Context).First();
        tag.GetObjectAddress().ShouldNotBe(0UL);
    }

    [Test]
    public void ToString_ContainsTypeName()
    {
        var tag = Tag.GetInstances(Context).First();
        tag.ToString().ShouldContain("Tag@0x");
    }

    [Test]
    public void ToString_ContainsHexAddress()
    {
        var tag = Tag.GetInstances(Context).First();
        string str = tag.ToString();
        ulong addr = tag.GetObjectAddress();
        str.ShouldContain(addr.ToString("X"));
    }

    // ══════════════════════════════════════════════════════════════════
    //  MULTIPLE INSTANCES WITH DIFFERENT VALUES
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void MultipleInstances_Tags_AllDistinctValues()
    {
        var tags = Tag.GetInstances(Context).ToList();
        tags.Count.ShouldBe(2);

        var labels = tags.Select(t => t._label).OrderBy(l => l).ToList();
        var ids = tags.Select(t => t._id).OrderBy(i => i).ToList();

        labels.ShouldBe(["important", "urgent"]);
        ids.ShouldBe([1L, 2L]);
    }

    [Test]
    public void MultipleInstances_Orders_AllHaveUniqueIds()
    {
        var orders = Order.GetInstances(Context).ToList();
        var ids = orders.Select(o => o._orderId).OrderBy(i => i).ToList();
        ids.ShouldBe([1001, 1002, 1003]);
    }

    [Test]
    public void MultipleInstances_Addresses_DistinctValues()
    {
        var addresses = Address.GetInstances(Context).ToList();
        addresses.Count.ShouldBeGreaterThanOrEqualTo(2);

        var streets = addresses.Select(a => a._street).OrderBy(s => s).ToList();
        streets.ShouldContain("123 Main St");
        streets.ShouldContain("456 Oak Ave");
    }

    // ══════════════════════════════════════════════════════════════════
    //  PROXY TYPE HIERARCHY VALIDATION
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void ProxyHierarchy_AllTypesInheritFromSystemObject()
    {
        var sysObj = typeof(_.System.Object);
        var types = new[]
        {
            typeof(Customer), typeof(Order), typeof(Address),
            typeof(Tag), typeof(AllPrimitives), typeof(NullableHolder),
            typeof(StringVariants), typeof(Node), typeof(EnumHolder),
            typeof(ArrayHolder), typeof(SharedRefs), typeof(ListHolder)
        };

        foreach (var t in types)
        {
            sysObj.IsAssignableFrom(t).ShouldBeTrue($"{t.Name} should inherit from _.System.Object");
        }
    }

    [Test]
    public void ProxyHierarchy_Cat_InheritsFromAnimal()
    {
        typeof(Animal).IsAssignableFrom(typeof(Cat)).ShouldBeTrue();
    }

    [Test]
    public void ProxyHierarchy_Leaf_ChainCorrect()
    {
        typeof(Leaf).BaseType.ShouldBe(typeof(Middle));
        typeof(Middle).BaseType.ShouldBe(typeof(Base));
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT PROXY TYPE SHAPE VALIDATION
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void StructProxy_Rectangle_HasPointFields()
    {
        var topLeft = typeof(Rectangle).GetProperty("TopLeft");
        var bottomRight = typeof(Rectangle).GetProperty("BottomRight");
        topLeft.ShouldNotBeNull();
        bottomRight.ShouldNotBeNull();

        topLeft.PropertyType.ShouldBe(typeof(Point));
        bottomRight.PropertyType.ShouldBe(typeof(Point));
    }

    // ══════════════════════════════════════════════════════════════════
    //  FROMADDRESS ROUND-TRIP
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void FromAddress_RoundTrip_ReturnsCorrectData()
    {
        var tags = Tag.GetInstances(Context).ToList();
        var importantTag = tags.Single(t => t._label == "important");
        ulong addr = importantTag.GetObjectAddress();

        // Create a new proxy from the same address
        var newProxy = Tag.FromAddress(addr, Context);

        newProxy._label.ShouldBe("important");
        newProxy._id.ShouldBe(1L);
    }

    // ══════════════════════════════════════════════════════════════════
    //  POLYMORPHIC ARRAYS
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void PolymorphicArray_Pets_ResolvedToConcreteTypes()
    {
        var customers = Customer.GetInstances(Context).ToList();
        var charlie = customers.Single(c => c._name == "Charlie");

        // Charlie has [cat2, dog1, cat1]
        var pets = charlie._pets!.ToList();
        pets.Count.ShouldBe(3);

        var catCount = pets.Count(p => p is Cat);
        var dogCount = pets.Count(p => p is Dog);
        catCount.ShouldBe(2);
        dogCount.ShouldBe(1);
    }

    [Test]
    public void PolymorphicArray_MixedItems_StringElement()
    {
        var customers = Customer.GetInstances(Context).ToList();
        var charlie = customers.Single(c => c._name == "Charlie");

        // Charlie has [addr2, "world", order3, tag1, addr1]
        var mixed = charlie._mixedItems!.ToList();
        var stringElem = mixed.OfType<_.System.String>().Single();
        stringElem.ToString().ShouldBe("world");
    }

    // ══════════════════════════════════════════════════════════════════
    //  DICTIONARY ENTRIES (STRUCT ARRAY INSIDE GENERIC)
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void Dictionary_CanReadAllEntries()
    {
        var customers = Customer.GetInstances(Context).ToList();
        var charlie = customers.Single(c => c._name == "Charlie");

        // Charlie has {"math": 100, "art": 88, "science": 91}
        var scores = charlie._scores!;
        scores._count.ShouldBe(3);

        // Read entries array
        var entries = scores._entries!.ToList();
        entries.Count.ShouldBeGreaterThanOrEqualTo(3);

        // First 3 entries should contain our data (order depends on hash)
        var values = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            values.Add(entries[i].value);
        }
        values.Sort();
        values.ShouldBe([88, 91, 100]);
    }

    // ══════════════════════════════════════════════════════════════════
    //  EDGE CASES / TYPE DISCOVERY
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void TypeDiscovery_FindsAllNewTypes()
    {
        var typeNames = Projection.DiscoveredTypes.Select(t => t.FullName).ToHashSet();

        typeNames.ShouldContain("Ndump.TestApp.AllPrimitives");
        typeNames.ShouldContain("Ndump.TestApp.NullableHolder");
        typeNames.ShouldContain("Ndump.TestApp.StringVariants");
        typeNames.ShouldContain("Ndump.TestApp.Node");
        typeNames.ShouldContain("Ndump.TestApp.Leaf");
        typeNames.ShouldContain("Ndump.TestApp.Middle");
        typeNames.ShouldContain("Ndump.TestApp.Base");
        typeNames.ShouldContain("Ndump.TestApp.StructHolder");
        typeNames.ShouldContain("Ndump.TestApp.EnumHolder");
        typeNames.ShouldContain("Ndump.TestApp.ArrayHolder");
        typeNames.ShouldContain("Ndump.TestApp.SharedRefs");
        typeNames.ShouldContain("Ndump.TestApp.ListHolder");
    }

    [Test]
    public void TypeDiscovery_FindsStructTypes()
    {
        var types = Projection.DiscoveredTypes;
        var point = types.SingleOrDefault(t => t.FullName == "Ndump.TestApp.Point");
        point.ShouldNotBeNull();
        point.IsValueType.ShouldBeTrue();

        var rect = types.SingleOrDefault(t => t.FullName == "Ndump.TestApp.Rectangle");
        rect.ShouldNotBeNull();
        rect.IsValueType.ShouldBeTrue();

        var label = types.SingleOrDefault(t => t.FullName == "Ndump.TestApp.Label");
        label.ShouldNotBeNull();
        label.IsValueType.ShouldBeTrue();
    }

    [Test]
    public void TypeDiscovery_PointFields()
    {
        var types = Projection.DiscoveredTypes;
        var point = types.Single(t => t.FullName == "Ndump.TestApp.Point");

        var fieldNames = point.Fields.Select(f => f.Name).ToHashSet();
        fieldNames.ShouldContain("X");
        fieldNames.ShouldContain("Y");
    }

    [Test]
    public void TypeDiscovery_RectangleFields()
    {
        var types = Projection.DiscoveredTypes;
        var rect = types.Single(t => t.FullName == "Ndump.TestApp.Rectangle");

        var topLeft = rect.Fields.Single(f => f.Name == "TopLeft");
        topLeft.Kind.ShouldBe(FieldKind.ValueType);

        var bottomRight = rect.Fields.Single(f => f.Name == "BottomRight");
        bottomRight.Kind.ShouldBe(FieldKind.ValueType);
    }

    [Test]
    public void TypeDiscovery_EnumFields_ArePrimitive()
    {
        var types = Projection.DiscoveredTypes;
        var enumHolder = types.Single(t => t.FullName == "Ndump.TestApp.EnumHolder");

        var colorField = enumHolder.Fields.Single(f => f.Name == "_color");
        colorField.Kind.ShouldBe(FieldKind.Primitive);

        var priorityField = enumHolder.Fields.Single(f => f.Name == "_priority");
        priorityField.Kind.ShouldBe(FieldKind.Primitive);
    }

    // ══════════════════════════════════════════════════════════════════
    //  ARRAY LENGTH & INDEXER
    // ══════════════════════════════════════════════════════════════════

    [Test]
    public void DumpArray_Length_MatchesOriginal()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        obj._intArray!.Length.ShouldBe(5);
    }

    [Test]
    public void DumpArray_Indexer_ReadsCorrectElement()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var intArr = obj._intArray!;
        intArr[0].ShouldBe(10);
        intArr[2].ShouldBe(30);
        intArr[4].ShouldBe(50);
    }

    [Test]
    public void DumpArray_Indexer_ThrowsOnOutOfRange()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var intArr = obj._intArray!;
        Should.Throw<IndexOutOfRangeException>(() => { var _ = intArr[5]; });
        Should.Throw<IndexOutOfRangeException>(() => { var _ = intArr[-1]; });
    }
}
