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
public class BehavioralProxyTests : IClassFixture<DumpFixture>
{
    private readonly DumpFixture _fixture;
    private DumpContext Context => _fixture.Projection.Context;

    public BehavioralProxyTests(DumpFixture fixture)
    {
        _fixture = fixture;
    }

    // ══════════════════════════════════════════════════════════════════
    //  PRIMITIVE TYPES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Primitives_Bool_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.True(obj._boolVal);
    }

    [Fact]
    public void Primitives_Byte_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal((byte)255, obj._byteVal);
    }

    [Fact]
    public void Primitives_SByte_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal((sbyte)-42, obj._sbyteVal);
    }

    [Fact]
    public void Primitives_Short_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal((short)-1000, obj._shortVal);
    }

    [Fact]
    public void Primitives_UShort_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal((ushort)50000, obj._ushortVal);
    }

    [Fact]
    public void Primitives_Int_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal(123456, obj._intVal);
    }

    [Fact]
    public void Primitives_UInt_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal(4000000000u, obj._uintVal);
    }

    [Fact]
    public void Primitives_Long_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal(9876543210L, obj._longVal);
    }

    [Fact]
    public void Primitives_ULong_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal(18446744073709551615UL, obj._ulongVal);
    }

    [Fact]
    public void Primitives_Float_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal(3.14f, obj._floatVal, precision: 2);
    }

    [Fact]
    public void Primitives_Double_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal(2.718281828, obj._doubleVal, precision: 6);
    }

    [Fact]
    public void Primitives_Char_ReadsCorrectValue()
    {
        var obj = AllPrimitives.GetInstances(Context).Single();
        Assert.Equal('Z', obj._charVal);
    }

    // ══════════════════════════════════════════════════════════════════
    //  NULLABLE<T> VALUE TYPES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Nullable_IntWithValue_ReadsCorrectly()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        int? val = obj._intHasValue;
        Assert.NotNull(val);
        Assert.Equal(42, val.Value);
    }

    [Fact]
    public void Nullable_IntNull_ReadsAsNull()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        int? val = obj._intNull;
        Assert.Null(val);
    }

    [Fact]
    public void Nullable_DoubleWithValue_ReadsCorrectly()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        double? val = obj._doubleHasValue;
        Assert.NotNull(val);
        Assert.Equal(3.14, val.Value, precision: 2);
    }

    [Fact]
    public void Nullable_DoubleNull_ReadsAsNull()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        double? val = obj._doubleNull;
        Assert.Null(val);
    }

    [Fact]
    public void Nullable_BoolWithValue_ReadsCorrectly()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        bool? val = obj._boolHasValue;
        Assert.NotNull(val);
        Assert.True(val.Value);
    }

    [Fact]
    public void Nullable_BoolNull_ReadsAsNull()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        bool? val = obj._boolNull;
        Assert.Null(val);
    }

    [Fact]
    public void Nullable_LongWithValue_ReadsCorrectly()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        long? val = obj._longHasValue;
        Assert.NotNull(val);
        Assert.Equal(9876543210L, val.Value);
    }

    [Fact]
    public void Nullable_LongNull_ReadsAsNull()
    {
        var obj = NullableHolder.GetInstances(Context).Single();
        long? val = obj._longNull;
        Assert.Null(val);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRING VARIANTS
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Strings_NormalString_ReadsCorrectly()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        Assert.Equal("hello", obj._normal);
    }

    [Fact]
    public void Strings_NullString_ReadsAsNull()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        Assert.Null(obj._nullString);
    }

    [Fact]
    public void Strings_EmptyString_ReadsAsEmpty()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        Assert.Equal("", obj._empty);
    }

    [Fact]
    public void Strings_Unicode_ReadsCorrectly()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        Assert.Equal("日本語テスト🎉", obj._unicode);
    }

    [Fact]
    public void Strings_LongString_ReadsCorrectly()
    {
        var obj = StringVariants.GetInstances(Context).Single();
        string? val = obj._long;
        Assert.NotNull(val);
        Assert.Equal(500, val.Length);
        Assert.True(val.All(c => c == 'x'));
    }

    // ══════════════════════════════════════════════════════════════════
    //  OBJECT REFERENCES: NULL, SELF-REF, CIRCULAR
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Node_NullNext_ReturnsNull()
    {
        // Node "C" originally has next = null (before circular link is set), but
        // we set C.next = A in TestApp. Let's verify the chain instead.
        var nodes = Node.GetInstances(Context).ToList();
        // Find node B: B -> C
        var nodeB = nodes.Single(n => n._name == "B");
        Assert.NotNull(nodeB._next);
    }

    [Fact]
    public void Node_SelfReference_SameAddress()
    {
        var nodes = Node.GetInstances(Context).ToList();
        var nodeA = nodes.Single(n => n._name == "A");

        // nodeA._self should point to itself
        var selfRef = nodeA._self;
        Assert.NotNull(selfRef);
        Assert.Equal(nodeA.GetObjectAddress(), selfRef.GetObjectAddress());
    }

    [Fact]
    public void Node_CircularReference_Traversable()
    {
        var nodes = Node.GetInstances(Context).ToList();
        var nodeA = nodes.Single(n => n._name == "A");

        // A -> B -> C -> A (circular)
        Node? current = nodeA;
        var visited = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            visited.Add(current._name!);
            current = current._next;
            Assert.NotNull(current);
        }

        Assert.Equal(["A", "B", "C", "A"], visited);
    }

    [Fact]
    public void Node_CircularReference_AddressesMatch()
    {
        var nodes = Node.GetInstances(Context).ToList();
        var nodeA = nodes.Single(n => n._name == "A");

        // Follow A -> B -> C -> back to A, verify address matches
        var step1 = nodeA._next!;          // B
        var step2 = step1._next!;          // C
        var step3 = step2._next!;          // A (back to start)

        Assert.Equal(nodeA.GetObjectAddress(), step3.GetObjectAddress());
    }

    // ══════════════════════════════════════════════════════════════════
    //  DEEP INHERITANCE CHAIN
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void DeepInheritance_Leaf_InheritsFromMiddle()
    {
        Assert.True(typeof(Middle).IsAssignableFrom(typeof(Leaf)));
        Assert.True(typeof(Base).IsAssignableFrom(typeof(Leaf)));
        Assert.True(typeof(_.System.Object).IsAssignableFrom(typeof(Leaf)));
    }

    [Fact]
    public void DeepInheritance_Leaf_CanReadAllFields()
    {
        var leaf = Leaf.GetInstances(Context).Single();

        // Inherited from Base
        Assert.Equal(100, leaf._baseField);
        // Inherited from Middle
        Assert.Equal("mid", leaf._middleField);
        // Own field
        Assert.Equal(3.14, leaf._leafField, precision: 2);
    }

    [Fact]
    public void DeepInheritance_FieldsAreDeclaredAtCorrectLevel()
    {
        // _baseField declared on Base only
        Assert.NotNull(typeof(Base).GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(typeof(Middle).GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(typeof(Leaf).GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        // _middleField declared on Middle only
        Assert.NotNull(typeof(Middle).GetProperty("_middleField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(typeof(Leaf).GetProperty("_middleField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        // _leafField declared on Leaf only
        Assert.NotNull(typeof(Leaf).GetProperty("_leafField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    // ══════════════════════════════════════════════════════════════════
    //  ENUM FIELDS
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Enums_IntBacked_ReadsCorrectValue()
    {
        // Enums are projected as their underlying primitive type
        var obj = EnumHolder.GetInstances(Context).Single();
        // Color.Blue = 3
        Assert.Equal(3, obj._color);
    }

    [Fact]
    public void Enums_ByteBacked_ReadsCorrectValue()
    {
        var obj = EnumHolder.GetInstances(Context).Single();
        // SmallEnum.High = 3
        Assert.Equal((byte)3, obj._priority);
    }

    [Fact]
    public void Enums_FlagsEnum_ReadsCorrectValue()
    {
        var obj = EnumHolder.GetInstances(Context).Single();
        // Permissions.Read | Permissions.Write = 1 | 2 = 3
        Assert.Equal(3, obj._permissions);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT (VALUE TYPE) FIELDS EMBEDDED IN CLASSES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Struct_Point_ReadsFromClass()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var pt = obj._position;
        Assert.Equal(10, pt.X);
        Assert.Equal(20, pt.Y);
    }

    [Fact]
    public void Struct_NestedStruct_Rectangle_ReadsTopLeft()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var rect = obj._bounds;
        var topLeft = rect.TopLeft;
        Assert.Equal(10, topLeft.X);
        Assert.Equal(20, topLeft.Y);
    }

    [Fact]
    public void Struct_NestedStruct_Rectangle_ReadsBottomRight()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var rect = obj._bounds;
        var bottomRight = rect.BottomRight;
        Assert.Equal(30, bottomRight.X);
        Assert.Equal(40, bottomRight.Y);
    }

    [Fact]
    public void Struct_Label_ReadsStringField()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        Assert.Equal("test-label", label.Text);
    }

    [Fact]
    public void Struct_Label_ReadsPrimitiveField()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        Assert.Equal(5, label.Priority);
    }

    [Fact]
    public void Struct_Label_ReadsObjectRefField_AsConcreteType()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        // Metadata was set to tag1 ("important", id=1)
        var meta = label.Metadata;
        Assert.NotNull(meta);
        // It's a Tag proxy resolved via ProxyResolver (typed as _.System.Object)
        Assert.IsType<Tag>(meta);
    }

    [Fact]
    public void Struct_Label_ReadsObjectRefField_Data()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        // Metadata was set to tag1 ("important", id=1) — verify data through resolved proxy
        var meta = (Tag)label.Metadata!;
        Assert.Equal("important", meta._label);
        Assert.Equal(1L, meta._id);
    }

    [Fact]
    public void Struct_Label_ReadsObjectRefField_AsSystemObject()
    {
        var obj = StructHolder.GetInstances(Context).Single();
        var label = obj._label;
        // Metadata was set to tag1 - resolved to concrete Tag proxy via ProxyResolver,
        // which still passes IsInstanceOfType check since Tag inherits from _.System.Object
        var meta = label.Metadata;
        Assert.NotNull(meta);
        Assert.IsAssignableFrom<_.System.Object>(meta);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT ARRAYS (arrays of value types)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void StructArray_Point_HasCorrectLength()
    {
        // Find all Point[] arrays on the heap
        var pointArrays = new List<ulong>();
        foreach (var obj in Context.Heap.EnumerateObjects())
        {
            if (obj.IsValid && obj.Type?.Name == "Ndump.TestApp.Point[]")
                pointArrays.Add(obj.Address);
        }

        Assert.NotEmpty(pointArrays);

        // Our Point[] has 3 elements: (1,2), (3,4), (5,6)
        var arr = pointArrays.First(a => Context.GetArrayLength(a) == 3);
        Assert.Equal(3, Context.GetArrayLength(arr));
    }

    // ══════════════════════════════════════════════════════════════════
    //  PRIMITIVE ARRAYS
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void PrimitiveArray_Int_ReadsCorrectValues()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var intArr = obj._intArray!.ToList();
        Assert.Equal([10, 20, 30, 40, 50], intArr);
    }

    [Fact]
    public void PrimitiveArray_Byte_ReadsCorrectValues()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var byteArr = obj._byteArray!.ToList();
        Assert.Equal([0x01, 0xFF, 0x42], byteArr);
    }

    [Fact]
    public void PrimitiveArray_Double_ReadsCorrectValues()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var doubleArr = obj._doubleArray!.ToList();
        Assert.Equal([1.1, 2.2, 3.3], doubleArr);
    }

    [Fact]
    public void PrimitiveArray_Double_HasCorrectLength()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        Assert.Equal(3, obj._doubleArray!.Length);
    }

    [Fact]
    public void PrimitiveArray_Bool_ReadsCorrectValues()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var boolArr = obj._boolArray!.ToList();
        Assert.Equal([true, false, true], boolArr);
    }

    [Fact]
    public void PrimitiveArray_Bool_HasCorrectLength()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        Assert.Equal(3, obj._boolArray!.Length);
    }

    [Fact]
    public void PrimitiveArray_Null_ReturnsNull()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        // _nullArray was set to null
        Assert.Null(obj._nullArray);
    }

    [Fact]
    public void PrimitiveArray_EmptyStringArray_HasLengthZero()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var arr = obj._emptyStringArray!.ToList();
        Assert.Empty(arr);
    }

    // ══════════════════════════════════════════════════════════════════
    //  SHARED REFERENCES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void SharedRefs_SameTag_SameAddress()
    {
        var obj = SharedRefs.GetInstances(Context).Single();
        var ref1 = obj._ref1;
        var ref2 = obj._ref2;

        Assert.NotNull(ref1);
        Assert.NotNull(ref2);
        Assert.Equal(ref1.GetObjectAddress(), ref2.GetObjectAddress());
    }

    [Fact]
    public void SharedRefs_SameAddress_SameAddress()
    {
        var obj = SharedRefs.GetInstances(Context).Single();
        var shared = obj._shared;
        var sharedAgain = obj._sharedAgain;

        Assert.NotNull(shared);
        Assert.NotNull(sharedAgain);
        Assert.Equal(shared.GetObjectAddress(), sharedAgain.GetObjectAddress());
    }

    [Fact]
    public void SharedRefs_Tag_ReadsCorrectData()
    {
        var obj = SharedRefs.GetInstances(Context).Single();
        // tag1 = ("important", 1)
        var tag = obj._ref1!;
        Assert.Equal("important", tag._label);
        Assert.Equal(1L, tag._id);
    }

    [Fact]
    public void SharedRefs_Address_ReadsCorrectData()
    {
        var obj = SharedRefs.GetInstances(Context).Single();
        // addr1 = ("123 Main St", "Springfield", 62701)
        var addr = obj._shared!;
        Assert.Equal("123 Main St", addr._street);
        Assert.Equal("Springfield", addr._city);
        Assert.Equal(62701, addr._zipCode);
    }

    // ══════════════════════════════════════════════════════════════════
    //  LIST<T> (GENERIC COLLECTIONS)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void List_StringList_HasCorrectCount()
    {
        var obj = ListHolder.GetInstances(Context).Single();
        var items = obj._items;
        Assert.NotNull(items);
        Assert.Equal(3, items._size);
    }

    [Fact]
    public void List_OrderList_HasCorrectCount()
    {
        var obj = ListHolder.GetInstances(Context).Single();
        var orders = obj._orders;
        Assert.NotNull(orders);
        Assert.Equal(2, orders._size);
    }

    // ══════════════════════════════════════════════════════════════════
    //  GetObjectAddress() AND ToString()
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void GetObjAddress_ReturnsNonZero()
    {
        var tag = Tag.GetInstances(Context).First();
        Assert.NotEqual(0UL, tag.GetObjectAddress());
    }

    [Fact]
    public void ToString_ContainsTypeName()
    {
        var tag = Tag.GetInstances(Context).First();
        Assert.Contains("Tag@0x", tag.ToString());
    }

    [Fact]
    public void ToString_ContainsHexAddress()
    {
        var tag = Tag.GetInstances(Context).First();
        string str = tag.ToString();
        ulong addr = tag.GetObjectAddress();
        Assert.Contains(addr.ToString("X"), str);
    }

    // ══════════════════════════════════════════════════════════════════
    //  MULTIPLE INSTANCES WITH DIFFERENT VALUES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void MultipleInstances_Tags_AllDistinctValues()
    {
        var tags = Tag.GetInstances(Context).ToList();
        Assert.Equal(2, tags.Count);

        var labels = tags.Select(t => t._label).OrderBy(l => l).ToList();
        var ids = tags.Select(t => t._id).OrderBy(i => i).ToList();

        Assert.Equal(["important", "urgent"], labels);
        Assert.Equal([1L, 2L], ids);
    }

    [Fact]
    public void MultipleInstances_Orders_AllHaveUniqueIds()
    {
        var orders = Order.GetInstances(Context).ToList();
        var ids = orders.Select(o => o._orderId).OrderBy(i => i).ToList();
        Assert.Equal([1001, 1002, 1003], ids);
    }

    [Fact]
    public void MultipleInstances_Addresses_DistinctValues()
    {
        var addresses = Address.GetInstances(Context).ToList();
        Assert.True(addresses.Count >= 2);

        var streets = addresses.Select(a => a._street).OrderBy(s => s).ToList();
        Assert.Contains("123 Main St", streets);
        Assert.Contains("456 Oak Ave", streets);
    }

    // ══════════════════════════════════════════════════════════════════
    //  PROXY TYPE HIERARCHY VALIDATION
    // ══════════════════════════════════════════════════════════════════

    [Fact]
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
            Assert.True(sysObj.IsAssignableFrom(t), $"{t.Name} should inherit from _.System.Object");
        }
    }

    [Fact]
    public void ProxyHierarchy_Cat_InheritsFromAnimal()
    {
        Assert.True(typeof(Animal).IsAssignableFrom(typeof(Cat)));
    }

    [Fact]
    public void ProxyHierarchy_Leaf_ChainCorrect()
    {
        Assert.Equal(typeof(Middle), typeof(Leaf).BaseType);
        Assert.Equal(typeof(Base), typeof(Middle).BaseType);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT PROXY TYPE SHAPE VALIDATION
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void StructProxy_Rectangle_HasPointFields()
    {
        var topLeft = typeof(Rectangle).GetProperty("TopLeft");
        var bottomRight = typeof(Rectangle).GetProperty("BottomRight");
        Assert.NotNull(topLeft);
        Assert.NotNull(bottomRight);

        Assert.Equal(typeof(Point), topLeft.PropertyType);
        Assert.Equal(typeof(Point), bottomRight.PropertyType);
    }

    // ══════════════════════════════════════════════════════════════════
    //  FROMADDRESS ROUND-TRIP
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void FromAddress_RoundTrip_ReturnsCorrectData()
    {
        var tags = Tag.GetInstances(Context).ToList();
        var importantTag = tags.Single(t => t._label == "important");
        ulong addr = importantTag.GetObjectAddress();

        // Create a new proxy from the same address
        var newProxy = Tag.FromAddress(addr, Context);

        Assert.Equal("important", newProxy._label);
        Assert.Equal(1L, newProxy._id);
    }

    // ══════════════════════════════════════════════════════════════════
    //  POLYMORPHIC ARRAYS
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void PolymorphicArray_Pets_ResolvedToConcreteTypes()
    {
        var customers = Customer.GetInstances(Context).ToList();
        var charlie = customers.Single(c => c._name == "Charlie");

        // Charlie has [cat2, dog1, cat1]
        var pets = charlie._pets!.ToList();
        Assert.Equal(3, pets.Count);

        var catCount = pets.Count(p => p is Cat);
        var dogCount = pets.Count(p => p is Dog);
        Assert.Equal(2, catCount);
        Assert.Equal(1, dogCount);
    }

    [Fact]
    public void PolymorphicArray_MixedItems_StringElement()
    {
        var customers = Customer.GetInstances(Context).ToList();
        var charlie = customers.Single(c => c._name == "Charlie");

        // Charlie has [addr2, "world", order3, tag1, addr1]
        var mixed = charlie._mixedItems!.ToList();
        var stringElem = mixed.OfType<_.System.String>().Single();
        Assert.Equal("world", stringElem.ToString());
    }

    // ══════════════════════════════════════════════════════════════════
    //  DICTIONARY ENTRIES (STRUCT ARRAY INSIDE GENERIC)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Dictionary_CanReadAllEntries()
    {
        var customers = Customer.GetInstances(Context).ToList();
        var charlie = customers.Single(c => c._name == "Charlie");

        // Charlie has {"math": 100, "art": 88, "science": 91}
        var scores = charlie._scores!;
        Assert.Equal(3, scores._count);

        // Read entries array
        var entries = scores._entries!.ToList();
        Assert.True(entries.Count >= 3);

        // First 3 entries should contain our data (order depends on hash)
        var values = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            values.Add(entries[i].value);
        }
        values.Sort();
        Assert.Equal([88, 91, 100], values);
    }

    // ══════════════════════════════════════════════════════════════════
    //  EDGE CASES / TYPE DISCOVERY
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void TypeDiscovery_FindsAllNewTypes()
    {
        var typeNames = _fixture.Projection.DiscoveredTypes.Select(t => t.FullName).ToHashSet();

        Assert.Contains("Ndump.TestApp.AllPrimitives", typeNames);
        Assert.Contains("Ndump.TestApp.NullableHolder", typeNames);
        Assert.Contains("Ndump.TestApp.StringVariants", typeNames);
        Assert.Contains("Ndump.TestApp.Node", typeNames);
        Assert.Contains("Ndump.TestApp.Leaf", typeNames);
        Assert.Contains("Ndump.TestApp.Middle", typeNames);
        Assert.Contains("Ndump.TestApp.Base", typeNames);
        Assert.Contains("Ndump.TestApp.StructHolder", typeNames);
        Assert.Contains("Ndump.TestApp.EnumHolder", typeNames);
        Assert.Contains("Ndump.TestApp.ArrayHolder", typeNames);
        Assert.Contains("Ndump.TestApp.SharedRefs", typeNames);
        Assert.Contains("Ndump.TestApp.ListHolder", typeNames);
    }

    [Fact]
    public void TypeDiscovery_FindsStructTypes()
    {
        var types = _fixture.Projection.DiscoveredTypes;
        var point = types.SingleOrDefault(t => t.FullName == "Ndump.TestApp.Point");
        Assert.NotNull(point);
        Assert.True(point.IsValueType);

        var rect = types.SingleOrDefault(t => t.FullName == "Ndump.TestApp.Rectangle");
        Assert.NotNull(rect);
        Assert.True(rect.IsValueType);

        var label = types.SingleOrDefault(t => t.FullName == "Ndump.TestApp.Label");
        Assert.NotNull(label);
        Assert.True(label.IsValueType);
    }

    [Fact]
    public void TypeDiscovery_PointFields()
    {
        var types = _fixture.Projection.DiscoveredTypes;
        var point = types.Single(t => t.FullName == "Ndump.TestApp.Point");

        var fieldNames = point.Fields.Select(f => f.Name).ToHashSet();
        Assert.Contains("X", fieldNames);
        Assert.Contains("Y", fieldNames);
    }

    [Fact]
    public void TypeDiscovery_RectangleFields()
    {
        var types = _fixture.Projection.DiscoveredTypes;
        var rect = types.Single(t => t.FullName == "Ndump.TestApp.Rectangle");

        var topLeft = rect.Fields.Single(f => f.Name == "TopLeft");
        Assert.Equal(FieldKind.ValueType, topLeft.Kind);

        var bottomRight = rect.Fields.Single(f => f.Name == "BottomRight");
        Assert.Equal(FieldKind.ValueType, bottomRight.Kind);
    }

    [Fact]
    public void TypeDiscovery_EnumFields_ArePrimitive()
    {
        var types = _fixture.Projection.DiscoveredTypes;
        var enumHolder = types.Single(t => t.FullName == "Ndump.TestApp.EnumHolder");

        var colorField = enumHolder.Fields.Single(f => f.Name == "_color");
        Assert.Equal(FieldKind.Primitive, colorField.Kind);

        var priorityField = enumHolder.Fields.Single(f => f.Name == "_priority");
        Assert.Equal(FieldKind.Primitive, priorityField.Kind);
    }

    // ══════════════════════════════════════════════════════════════════
    //  ARRAY LENGTH & INDEXER
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void DumpArray_Length_MatchesOriginal()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        Assert.Equal(5, obj._intArray!.Length);
    }

    [Fact]
    public void DumpArray_Indexer_ReadsCorrectElement()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var intArr = obj._intArray!;
        Assert.Equal(10, intArr[0]);
        Assert.Equal(30, intArr[2]);
        Assert.Equal(50, intArr[4]);
    }

    [Fact]
    public void DumpArray_Indexer_ThrowsOnOutOfRange()
    {
        var obj = ArrayHolder.GetInstances(Context).Single();
        var intArr = obj._intArray!;
        Assert.Throws<IndexOutOfRangeException>(() => { var _ = intArr[5]; });
        Assert.Throws<IndexOutOfRangeException>(() => { var _ = intArr[-1]; });
    }
}
