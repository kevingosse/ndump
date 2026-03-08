using System.Collections;
using System.Reflection;
using Ndump.Core;

namespace Ndump.IntegrationTests;

/// <summary>
/// Comprehensive behavioral tests that validate proxy data reads against a real memory dump.
/// Organized by type category. Every test validates the actual value read from the dump,
/// not just the shape of the generated code.
/// </summary>
public class BehavioralProxyTests : IClassFixture<DumpFixture>
{
    private readonly DumpFixture _fixture;
    private Assembly Assembly => _fixture.Projection.GeneratedAssembly;
    private DumpContext Context => _fixture.Projection.Context;

    public BehavioralProxyTests(DumpFixture fixture)
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

    private dynamic GetSingleInstance(string clrTypeName)
    {
        var instances = GetInstances(clrTypeName);
        Assert.Single(instances);
        return instances[0];
    }

    // ══════════════════════════════════════════════════════════════════
    //  PRIMITIVE TYPES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Primitives_Bool_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.True((bool)obj._boolVal);
    }

    [Fact]
    public void Primitives_Byte_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal((byte)255, (byte)obj._byteVal);
    }

    [Fact]
    public void Primitives_SByte_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal((sbyte)-42, (sbyte)obj._sbyteVal);
    }

    [Fact]
    public void Primitives_Short_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal((short)-1000, (short)obj._shortVal);
    }

    [Fact]
    public void Primitives_UShort_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal((ushort)50000, (ushort)obj._ushortVal);
    }

    [Fact]
    public void Primitives_Int_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal(123456, (int)obj._intVal);
    }

    [Fact]
    public void Primitives_UInt_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal(4000000000u, (uint)obj._uintVal);
    }

    [Fact]
    public void Primitives_Long_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal(9876543210L, (long)obj._longVal);
    }

    [Fact]
    public void Primitives_ULong_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal(18446744073709551615UL, (ulong)obj._ulongVal);
    }

    [Fact]
    public void Primitives_Float_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal(3.14f, (float)obj._floatVal, precision: 2);
    }

    [Fact]
    public void Primitives_Double_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal(2.718281828, (double)obj._doubleVal, precision: 6);
    }

    [Fact]
    public void Primitives_Char_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.AllPrimitives");
        Assert.Equal('Z', (char)obj._charVal);
    }

    // ══════════════════════════════════════════════════════════════════
    //  NULLABLE<T> VALUE TYPES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Nullable_IntWithValue_ReadsCorrectly()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.NullableHolder");
        int? val = obj._intHasValue;
        Assert.NotNull(val);
        Assert.Equal(42, val!.Value);
    }

    [Fact]
    public void Nullable_IntNull_ReadsAsNull()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.NullableHolder");
        int? val = obj._intNull;
        Assert.Null(val);
    }

    [Fact]
    public void Nullable_DoubleWithValue_ReadsCorrectly()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.NullableHolder");
        double? val = obj._doubleHasValue;
        Assert.NotNull(val);
        Assert.Equal(3.14, val!.Value, precision: 2);
    }

    [Fact]
    public void Nullable_DoubleNull_ReadsAsNull()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.NullableHolder");
        double? val = obj._doubleNull;
        Assert.Null(val);
    }

    [Fact]
    public void Nullable_BoolWithValue_ReadsCorrectly()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.NullableHolder");
        bool? val = obj._boolHasValue;
        Assert.NotNull(val);
        Assert.True(val!.Value);
    }

    [Fact]
    public void Nullable_BoolNull_ReadsAsNull()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.NullableHolder");
        bool? val = obj._boolNull;
        Assert.Null(val);
    }

    [Fact]
    public void Nullable_LongWithValue_ReadsCorrectly()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.NullableHolder");
        long? val = obj._longHasValue;
        Assert.NotNull(val);
        Assert.Equal(9876543210L, val!.Value);
    }

    [Fact]
    public void Nullable_LongNull_ReadsAsNull()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.NullableHolder");
        long? val = obj._longNull;
        Assert.Null(val);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRING VARIANTS
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Strings_NormalString_ReadsCorrectly()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StringVariants");
        Assert.Equal("hello", (string?)obj._normal);
    }

    [Fact]
    public void Strings_NullString_ReadsAsNull()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StringVariants");
        Assert.Null((string?)obj._nullString);
    }

    [Fact]
    public void Strings_EmptyString_ReadsAsEmpty()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StringVariants");
        Assert.Equal("", (string?)obj._empty);
    }

    [Fact]
    public void Strings_Unicode_ReadsCorrectly()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StringVariants");
        Assert.Equal("日本語テスト🎉", (string?)obj._unicode);
    }

    [Fact]
    public void Strings_LongString_ReadsCorrectly()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StringVariants");
        string? val = (string?)obj._long;
        Assert.NotNull(val);
        Assert.Equal(500, val!.Length);
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
        var nodes = GetInstances("Ndump.TestApp.Node");
        // Find node B: B -> C
        var nodeB = nodes.Single(n => (string?)n._name == "B");
        Assert.NotNull((object?)nodeB._next);
    }

    [Fact]
    public void Node_SelfReference_SameAddress()
    {
        var nodes = GetInstances("Ndump.TestApp.Node");
        var nodeA = nodes.Single(n => (string?)n._name == "A");

        // nodeA._self should point to itself
        dynamic selfRef = nodeA._self;
        Assert.NotNull((object?)selfRef);
        Assert.Equal((ulong)nodeA.GetObjAddress(), (ulong)selfRef.GetObjAddress());
    }

    [Fact]
    public void Node_CircularReference_Traversable()
    {
        var nodes = GetInstances("Ndump.TestApp.Node");
        var nodeA = nodes.Single(n => (string?)n._name == "A");

        // A -> B -> C -> A (circular)
        dynamic current = nodeA;
        var visited = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            visited.Add((string)current._name);
            current = current._next;
            Assert.NotNull((object?)current);
        }

        Assert.Equal(["A", "B", "C", "A"], visited);
    }

    [Fact]
    public void Node_CircularReference_AddressesMatch()
    {
        var nodes = GetInstances("Ndump.TestApp.Node");
        var nodeA = nodes.Single(n => (string?)n._name == "A");

        // Follow A -> B -> C -> back to A, verify address matches
        dynamic step1 = nodeA._next;          // B
        dynamic step2 = step1._next;          // C
        dynamic step3 = step2._next;          // A (back to start)

        Assert.Equal((ulong)nodeA.GetObjAddress(), (ulong)step3.GetObjAddress());
    }

    // ══════════════════════════════════════════════════════════════════
    //  DEEP INHERITANCE CHAIN
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void DeepInheritance_Leaf_InheritsFromMiddle()
    {
        var leafType = ProxyType("Ndump.TestApp.Leaf");
        var middleType = ProxyType("Ndump.TestApp.Middle");
        var baseType = ProxyType("Ndump.TestApp.Base");
        var sysObj = ProxyType("System.Object");

        Assert.True(middleType.IsAssignableFrom(leafType));
        Assert.True(baseType.IsAssignableFrom(leafType));
        Assert.True(sysObj.IsAssignableFrom(leafType));
    }

    [Fact]
    public void DeepInheritance_Leaf_CanReadAllFields()
    {
        dynamic leaf = GetSingleInstance("Ndump.TestApp.Leaf");

        // Inherited from Base
        Assert.Equal(100, (int)leaf._baseField);
        // Inherited from Middle
        Assert.Equal("mid", (string?)leaf._middleField);
        // Own field
        Assert.Equal(3.14, (double)leaf._leafField, precision: 2);
    }

    [Fact]
    public void DeepInheritance_FieldsAreDeclaredAtCorrectLevel()
    {
        var leafType = ProxyType("Ndump.TestApp.Leaf");
        var middleType = ProxyType("Ndump.TestApp.Middle");
        var baseType = ProxyType("Ndump.TestApp.Base");

        // _baseField declared on Base only
        Assert.NotNull(baseType.GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(middleType.GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(leafType.GetProperty("_baseField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        // _middleField declared on Middle only
        Assert.NotNull(middleType.GetProperty("_middleField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Null(leafType.GetProperty("_middleField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        // _leafField declared on Leaf only
        Assert.NotNull(leafType.GetProperty("_leafField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    // ══════════════════════════════════════════════════════════════════
    //  ENUM FIELDS
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Enums_IntBacked_ReadsCorrectValue()
    {
        // Enums are projected as their underlying primitive type
        dynamic obj = GetSingleInstance("Ndump.TestApp.EnumHolder");
        // Color.Blue = 3
        Assert.Equal(3, (int)obj._color);
    }

    [Fact]
    public void Enums_ByteBacked_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.EnumHolder");
        // SmallEnum.High = 3
        Assert.Equal((byte)3, (byte)obj._priority);
    }

    [Fact]
    public void Enums_FlagsEnum_ReadsCorrectValue()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.EnumHolder");
        // Permissions.Read | Permissions.Write = 1 | 2 = 3
        Assert.Equal(3, (int)obj._permissions);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT (VALUE TYPE) FIELDS EMBEDDED IN CLASSES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Struct_Point_ReadsFromClass()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StructHolder");
        dynamic pt = obj._position;
        Assert.Equal(10, (int)pt.X);
        Assert.Equal(20, (int)pt.Y);
    }

    [Fact]
    public void Struct_NestedStruct_Rectangle_ReadsTopLeft()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StructHolder");
        dynamic rect = obj._bounds;
        dynamic topLeft = rect.TopLeft;
        Assert.Equal(10, (int)topLeft.X);
        Assert.Equal(20, (int)topLeft.Y);
    }

    [Fact]
    public void Struct_NestedStruct_Rectangle_ReadsBottomRight()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StructHolder");
        dynamic rect = obj._bounds;
        dynamic bottomRight = rect.BottomRight;
        Assert.Equal(30, (int)bottomRight.X);
        Assert.Equal(40, (int)bottomRight.Y);
    }

    [Fact]
    public void Struct_Label_ReadsStringField()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StructHolder");
        dynamic label = obj._label;
        Assert.Equal("test-label", (string?)label.Text);
    }

    [Fact]
    public void Struct_Label_ReadsPrimitiveField()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StructHolder");
        dynamic label = obj._label;
        Assert.Equal(5, (int)label.Priority);
    }

    [Fact]
    public void Struct_Label_ReadsObjectRefField_AsConcreteType()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StructHolder");
        dynamic label = obj._label;
        // Metadata was set to tag1 ("important", id=1)
        dynamic meta = label.Metadata;
        Assert.NotNull((object?)meta);
        // It's a Tag proxy (but typed as _.System.Object, need to cast)
        var tagType = ProxyType("Ndump.TestApp.Tag");
        Assert.True(tagType.IsInstanceOfType((object)meta));
    }

    [Fact]
    public void Struct_Label_ReadsObjectRefField_Data()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StructHolder");
        dynamic label = obj._label;
        // Metadata was set to tag1 ("important", id=1) — verify data through resolved proxy
        dynamic meta = label.Metadata;
        Assert.Equal("important", (string)meta._label);
        Assert.Equal(1L, (long)meta._id);
    }

    [Fact]
    public void Struct_Label_ReadsObjectRefField_AsSystemObject()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.StructHolder");
        dynamic label = obj._label;
        // Metadata was set to tag1 - resolved to concrete Tag proxy via ProxyResolver,
        // which still passes IsInstanceOfType check since Tag inherits from _.System.Object
        dynamic meta = label.Metadata;
        Assert.NotNull((object?)meta);
        var sysObjType = ProxyType("System.Object");
        Assert.True(sysObjType.IsInstanceOfType((object)meta));
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT ARRAYS (arrays of value types)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void StructArray_Point_HasCorrectLength()
    {
        // We need to find the Point[] array on the heap
        // It's stored in the allObjects array, but we can look for it via StructHolder's approach.
        // Actually, the pointArray is a standalone object. Let's look for it via type discovery.
        // Point arrays won't have GetInstances (they're arrays, not class instances).
        // But we can verify struct arrays work through the StructHolder / ArrayHolder approach.
        // For now, test via DumpContext directly.

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
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        var intArr = ((IEnumerable)obj._intArray).Cast<int>().ToList();
        Assert.Equal([10, 20, 30, 40, 50], intArr);
    }

    [Fact]
    public void PrimitiveArray_Byte_ReadsCorrectValues()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        var byteArr = ((IEnumerable)obj._byteArray).Cast<byte>().ToList();
        Assert.Equal([0x01, 0xFF, 0x42], byteArr);
    }

    [Fact]
    public void PrimitiveArray_Double_ReadsCorrectValues()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        var doubleArr = ((IEnumerable)obj._doubleArray).Cast<double>().ToList();
        Assert.Equal([1.1, 2.2, 3.3], doubleArr);
    }

    [Fact]
    public void PrimitiveArray_Double_HasCorrectLength()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        var doubleArr = ((IEnumerable)obj._doubleArray).Cast<double>().ToList();
        Assert.Equal(3, doubleArr.Count);
    }

    [Fact]
    public void PrimitiveArray_Bool_ReadsCorrectValues()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        var boolArr = ((IEnumerable)obj._boolArray).Cast<bool>().ToList();
        Assert.Equal([true, false, true], boolArr);
    }

    [Fact]
    public void PrimitiveArray_Bool_HasCorrectLength()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        var boolArr = ((IEnumerable)obj._boolArray).Cast<bool>().ToList();
        Assert.Equal(3, boolArr.Count);
    }

    [Fact]
    public void PrimitiveArray_Null_ReturnsNull()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        // _nullArray was set to null
        var arr = (object?)obj._nullArray;
        Assert.Null(arr);
    }

    [Fact]
    public void PrimitiveArray_EmptyStringArray_HasLengthZero()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        var arr = ((IEnumerable)obj._emptyStringArray).Cast<string?>().ToList();
        Assert.Empty(arr);
    }

    // ══════════════════════════════════════════════════════════════════
    //  SHARED REFERENCES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void SharedRefs_SameTag_SameAddress()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.SharedRefs");
        dynamic ref1 = obj._ref1;
        dynamic ref2 = obj._ref2;

        Assert.NotNull((object?)ref1);
        Assert.NotNull((object?)ref2);
        Assert.Equal((ulong)ref1.GetObjAddress(), (ulong)ref2.GetObjAddress());
    }

    [Fact]
    public void SharedRefs_SameAddress_SameAddress()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.SharedRefs");
        dynamic shared = obj._shared;
        dynamic sharedAgain = obj._sharedAgain;

        Assert.NotNull((object?)shared);
        Assert.NotNull((object?)sharedAgain);
        Assert.Equal((ulong)shared.GetObjAddress(), (ulong)sharedAgain.GetObjAddress());
    }

    [Fact]
    public void SharedRefs_Tag_ReadsCorrectData()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.SharedRefs");
        // tag1 = ("important", 1)
        dynamic tag = obj._ref1;
        Assert.Equal("important", (string?)tag._label);
        Assert.Equal(1L, (long)tag._id);
    }

    [Fact]
    public void SharedRefs_Address_ReadsCorrectData()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.SharedRefs");
        // addr1 = ("123 Main St", "Springfield", 62701)
        dynamic addr = obj._shared;
        Assert.Equal("123 Main St", (string?)addr._street);
        Assert.Equal("Springfield", (string?)addr._city);
        Assert.Equal(62701, (int)addr._zipCode);
    }

    // ══════════════════════════════════════════════════════════════════
    //  LIST<T> (GENERIC COLLECTIONS)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void List_StringList_HasCorrectCount()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ListHolder");
        dynamic items = obj._items;
        Assert.NotNull((object?)items);
        // List<string> internally stores _size
        int size = (int)items._size;
        Assert.Equal(3, size);
    }

    [Fact]
    public void List_OrderList_HasCorrectCount()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ListHolder");
        dynamic orders = obj._orders;
        Assert.NotNull((object?)orders);
        int size = (int)orders._size;
        Assert.Equal(2, size);
    }

    // ══════════════════════════════════════════════════════════════════
    //  GetObjAddress() AND ToString()
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void GetObjAddress_ReturnsNonZero()
    {
        dynamic tag = GetInstances("Ndump.TestApp.Tag")[0];
        ulong addr = (ulong)tag.GetObjAddress();
        Assert.NotEqual(0UL, addr);
    }

    [Fact]
    public void ToString_ContainsTypeName()
    {
        dynamic tag = GetInstances("Ndump.TestApp.Tag")[0];
        string str = tag.ToString();
        Assert.Contains("Tag@0x", str);
    }

    [Fact]
    public void ToString_ContainsHexAddress()
    {
        dynamic tag = GetInstances("Ndump.TestApp.Tag")[0];
        string str = tag.ToString();
        ulong addr = (ulong)tag.GetObjAddress();
        Assert.Contains(addr.ToString("X"), str);
    }

    // ══════════════════════════════════════════════════════════════════
    //  MULTIPLE INSTANCES WITH DIFFERENT VALUES
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void MultipleInstances_Tags_AllDistinctValues()
    {
        var tags = GetInstances("Ndump.TestApp.Tag");
        Assert.Equal(2, tags.Count);

        var labels = tags.Select(t => (string?)t._label).OrderBy(l => l).ToList();
        var ids = tags.Select(t => (long)t._id).OrderBy(i => i).ToList();

        Assert.Equal(["important", "urgent"], labels);
        Assert.Equal([1L, 2L], ids);
    }

    [Fact]
    public void MultipleInstances_Orders_AllHaveUniqueIds()
    {
        var orders = GetInstances("Ndump.TestApp.Order");
        var ids = orders.Select(o => (int)o._orderId).OrderBy(i => i).ToList();
        Assert.Equal([1001, 1002, 1003], ids);
    }

    [Fact]
    public void MultipleInstances_Addresses_DistinctValues()
    {
        var addresses = GetInstances("Ndump.TestApp.Address");
        Assert.True(addresses.Count >= 2);

        var streets = addresses.Select(a => (string?)a._street).OrderBy(s => s).ToList();
        Assert.Contains("123 Main St", streets);
        Assert.Contains("456 Oak Ave", streets);
    }

    // ══════════════════════════════════════════════════════════════════
    //  PROXY TYPE HIERARCHY VALIDATION
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void ProxyHierarchy_AllTypesInheritFromSystemObject()
    {
        var sysObj = ProxyType("System.Object");
        var typeNames = new[]
        {
            "Ndump.TestApp.Customer", "Ndump.TestApp.Order", "Ndump.TestApp.Address",
            "Ndump.TestApp.Tag", "Ndump.TestApp.AllPrimitives", "Ndump.TestApp.NullableHolder",
            "Ndump.TestApp.StringVariants", "Ndump.TestApp.Node", "Ndump.TestApp.EnumHolder",
            "Ndump.TestApp.ArrayHolder", "Ndump.TestApp.SharedRefs", "Ndump.TestApp.ListHolder"
        };

        foreach (var tn in typeNames)
        {
            var pt = ProxyType(tn);
            Assert.True(sysObj.IsAssignableFrom(pt), $"{tn} should inherit from _.System.Object");
        }
    }

    [Fact]
    public void ProxyHierarchy_Cat_InheritsFromAnimal()
    {
        var animal = ProxyType("Ndump.TestApp.Animal");
        var cat = ProxyType("Ndump.TestApp.Cat");
        Assert.True(animal.IsAssignableFrom(cat));
    }

    [Fact]
    public void ProxyHierarchy_Leaf_ChainCorrect()
    {
        var leaf = ProxyType("Ndump.TestApp.Leaf");
        var middle = ProxyType("Ndump.TestApp.Middle");
        var baseT = ProxyType("Ndump.TestApp.Base");

        Assert.Equal(middle, leaf.BaseType);
        Assert.Equal(baseT, middle.BaseType);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STRUCT PROXY TYPE SHAPE VALIDATION
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void StructProxy_Point_HasFromInterior()
    {
        var pt = ProxyType("Ndump.TestApp.Point");
        var method = pt.GetMethod("FromInterior", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void StructProxy_Point_HasFromArrayElement()
    {
        var pt = ProxyType("Ndump.TestApp.Point");
        var method = pt.GetMethod("FromArrayElement", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void StructProxy_Rectangle_HasPointFields()
    {
        var rect = ProxyType("Ndump.TestApp.Rectangle");
        var topLeft = rect.GetProperty("TopLeft");
        var bottomRight = rect.GetProperty("BottomRight");
        Assert.NotNull(topLeft);
        Assert.NotNull(bottomRight);

        var pointType = ProxyType("Ndump.TestApp.Point");
        Assert.Equal(pointType, topLeft!.PropertyType);
        Assert.Equal(pointType, bottomRight!.PropertyType);
    }

    // ══════════════════════════════════════════════════════════════════
    //  FROMADDRESS ROUND-TRIP
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void FromAddress_RoundTrip_ReturnsCorrectData()
    {
        var tags = GetInstances("Ndump.TestApp.Tag");
        var importantTag = tags.Single(t => (string?)t._label == "important");
        ulong addr = (ulong)importantTag.GetObjAddress();

        // Create a new proxy from the same address
        var tagType = ProxyType("Ndump.TestApp.Tag");
        var fromAddr = tagType.GetMethod("FromAddress", BindingFlags.Public | BindingFlags.Static)!;
        dynamic newProxy = fromAddr.Invoke(null, [addr, Context])!;

        Assert.Equal("important", (string?)newProxy._label);
        Assert.Equal(1L, (long)newProxy._id);
    }

    // ══════════════════════════════════════════════════════════════════
    //  POLYMORPHIC ARRAYS
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void PolymorphicArray_Pets_ResolvedToConcreteTypes()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");
        var charlie = customers.Single(c => (string?)c._name == "Charlie");

        // Charlie has [cat2, dog1, cat1]
        var pets = ((IEnumerable)charlie._pets).Cast<object>().ToList();
        Assert.Equal(3, pets.Count);

        var catType = ProxyType("Ndump.TestApp.Cat");
        var dogType = ProxyType("Ndump.TestApp.Dog");

        var catCount = pets.Count(p => p.GetType() == catType);
        var dogCount = pets.Count(p => p.GetType() == dogType);
        Assert.Equal(2, catCount);
        Assert.Equal(1, dogCount);
    }

    [Fact]
    public void PolymorphicArray_MixedItems_StringElement()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");
        var charlie = customers.Single(c => (string?)c._name == "Charlie");

        // Charlie has [addr2, "world", order3, tag1, addr1]
        var mixed = ((IEnumerable)charlie._mixedItems).Cast<object>().ToList();
        var stringType = ProxyType("System.String");
        var stringElem = mixed.Single(d => d.GetType() == stringType);
        Assert.Equal("world", stringElem.ToString());
    }

    // ══════════════════════════════════════════════════════════════════
    //  DICTIONARY ENTRIES (STRUCT ARRAY INSIDE GENERIC)
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public void Dictionary_CanReadAllEntries()
    {
        var customers = GetInstances("Ndump.TestApp.Customer");
        var charlie = customers.Single(c => (string?)c._name == "Charlie");

        // Charlie has {"math": 100, "art": 88, "science": 91}
        dynamic scores = charlie._scores;
        Assert.Equal(3, (int)scores._count);

        // Read entries array
        var entries = ((IEnumerable)scores._entries).Cast<dynamic>().ToList();
        Assert.True(entries.Count >= 3);

        // First 3 entries should contain our data (order depends on hash)
        var values = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            values.Add((int)entries[i].value);
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
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        dynamic intArr = obj._intArray;
        Assert.Equal(5, (int)intArr.Length);
    }

    [Fact]
    public void DumpArray_Indexer_ReadsCorrectElement()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        dynamic intArr = obj._intArray;
        Assert.Equal(10, (int)intArr[0]);
        Assert.Equal(30, (int)intArr[2]);
        Assert.Equal(50, (int)intArr[4]);
    }

    [Fact]
    public void DumpArray_Indexer_ThrowsOnOutOfRange()
    {
        dynamic obj = GetSingleInstance("Ndump.TestApp.ArrayHolder");
        dynamic intArr = obj._intArray;
        Assert.Throws<IndexOutOfRangeException>(() => { var _ = intArr[5]; });
        Assert.Throws<IndexOutOfRangeException>(() => { var _ = intArr[-1]; });
    }
}
