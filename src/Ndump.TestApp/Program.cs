using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ndump.TestApp;

// Simple types with various field kinds for testing proxy generation

public class Order
{
    private int _orderId;
    private double _total;
    private string _description;
    private DateTime _createdAt;
    private DateTime? _shippedAt;
    private int? _rating;

    public Order(int orderId, double total, string description, DateTime? shippedAt = null, int? rating = null)
    {
        _orderId = orderId;
        _total = total;
        _description = description;
        _createdAt = DateTime.UtcNow;
        _shippedAt = shippedAt;
        _rating = rating;
    }
}

public class Address
{
    private string _street;
    private string _city;
    private int _zipCode;

    public Address(string street, string city, int zipCode)
    {
        _street = street;
        _city = city;
        _zipCode = zipCode;
    }
}

public class Customer
{
    private string _name;
    private int _age;
    private bool _isActive;
    private Order _lastOrder;
    private Address _address;
    private Order[] _orderHistory;
    private object[] _mixedItems;
    private Animal[] _pets;
    private string[] _tags;
    private Dictionary<string, int> _scores;

    public Customer(string name, int age, bool isActive, Order lastOrder, Address address, Order[] orderHistory, object[] mixedItems, Animal[] pets, string[] tags, Dictionary<string, int> scores)
    {
        _name = name;
        _age = age;
        _isActive = isActive;
        _lastOrder = lastOrder;
        _address = address;
        _orderHistory = orderHistory;
        _mixedItems = mixedItems;
        _pets = pets;
        _tags = tags;
        _scores = scores;
    }
}

// Test inheritance hierarchy with polymorphic arrays
public abstract class Animal
{
    private string _name;
    private int _age;

    protected Animal(string name, int age)
    {
        _name = name;
        _age = age;
    }
}

public class Cat : Animal
{
    private bool _isIndoor;

    public Cat(string name, int age, bool isIndoor) : base(name, age)
    {
        _isIndoor = isIndoor;
    }
}

public class Dog : Animal
{
    private string _breed;

    public Dog(string name, int age, string breed) : base(name, age)
    {
        _breed = breed;
    }
}

// Test that we handle types with no references
public class Tag
{
    private string _label;
    private long _id;

    public Tag(string label, long id)
    {
        _label = label;
        _id = id;
    }
}

// ── Additional types for exhaustive testing ──────────────────────────

// Enum types
public enum Color : int { Red = 1, Green = 2, Blue = 3 }
public enum SmallEnum : byte { Low = 1, Medium = 2, High = 3 }

[Flags]
public enum Permissions : int { None = 0, Read = 1, Write = 2, Execute = 4, All = 7 }

// Struct with only primitive fields
public struct Point
{
    public int X;
    public int Y;

    public Point(int x, int y) { X = x; Y = y; }
}

// Struct with a string field and an object reference
public struct Label
{
    public string Text;
    public int Priority;
    public object? Metadata;

    public Label(string text, int priority, object? metadata = null)
    {
        Text = text;
        Priority = priority;
        Metadata = metadata;
    }
}

// Struct nested in struct
public struct Rectangle
{
    public Point TopLeft;
    public Point BottomRight;

    public Rectangle(Point topLeft, Point bottomRight)
    {
        TopLeft = topLeft;
        BottomRight = bottomRight;
    }
}

// Class to hold all primitive types
public class AllPrimitives
{
    private bool _boolVal;
    private byte _byteVal;
    private sbyte _sbyteVal;
    private short _shortVal;
    private ushort _ushortVal;
    private int _intVal;
    private uint _uintVal;
    private long _longVal;
    private ulong _ulongVal;
    private float _floatVal;
    private double _doubleVal;
    private char _charVal;

    public AllPrimitives(bool b, byte by, sbyte sb, short s, ushort us, int i, uint ui, long l, ulong ul, float f, double d, char c)
    {
        _boolVal = b; _byteVal = by; _sbyteVal = sb; _shortVal = s; _ushortVal = us;
        _intVal = i; _uintVal = ui; _longVal = l; _ulongVal = ul;
        _floatVal = f; _doubleVal = d; _charVal = c;
    }
}

// Class with nullable variants
public class NullableHolder
{
    private int? _intHasValue;
    private int? _intNull;
    private double? _doubleHasValue;
    private double? _doubleNull;
    private bool? _boolHasValue;
    private bool? _boolNull;
    private long? _longHasValue;
    private long? _longNull;

    public NullableHolder()
    {
        _intHasValue = 42;
        _intNull = null;
        _doubleHasValue = 3.14;
        _doubleNull = null;
        _boolHasValue = true;
        _boolNull = null;
        _longHasValue = 9876543210L;
        _longNull = null;
    }
}

// Class with null / empty / special string fields
public class StringVariants
{
    private string _normal;
    private string? _nullString;
    private string _empty;
    private string _unicode;
    private string _long;

    public StringVariants()
    {
        _normal = "hello";
        _nullString = null;
        _empty = "";
        _unicode = "日本語テスト🎉";
        _long = new string('x', 500);
    }
}

// Class with null references and self-references
public class Node
{
    private string _name;
    private Node? _next;
    private Node? _self;

    public Node(string name, Node? next = null)
    {
        _name = name;
        _next = next;
        _self = null;
    }

    public void SetSelf() { _self = this; }
    public void SetNext(Node? n) { _next = n; }
}

// Deep inheritance chain
public class Base
{
    private int _baseField;
    public Base(int baseField) { _baseField = baseField; }
}

public class Middle : Base
{
    private string _middleField;
    public Middle(int baseField, string middleField) : base(baseField) { _middleField = middleField; }
}

public class Leaf : Middle
{
    private double _leafField;
    public Leaf(int baseField, string middleField, double leafField) : base(baseField, middleField) { _leafField = leafField; }
}

// Class with struct fields (embedded value types)
public class StructHolder
{
    private Point _position;
    private Rectangle _bounds;
    private Label _label;

    public StructHolder(Point position, Rectangle bounds, Label label)
    {
        _position = position;
        _bounds = bounds;
        _label = label;
    }
}

// Class with enum fields
public class EnumHolder
{
    private Color _color;
    private SmallEnum _priority;
    private Permissions _permissions;

    public EnumHolder(Color color, SmallEnum priority, Permissions permissions)
    {
        _color = color;
        _priority = priority;
        _permissions = permissions;
    }
}

// Class with primitive arrays
public class ArrayHolder
{
    private int[] _intArray;
    private byte[] _byteArray;
    private double[] _doubleArray;
    private bool[] _boolArray;
    private int[]? _nullArray;
    private string[] _emptyStringArray;

    public ArrayHolder()
    {
        _intArray = [10, 20, 30, 40, 50];
        _byteArray = [0x01, 0xFF, 0x42];
        _doubleArray = [1.1, 2.2, 3.3];
        _boolArray = [true, false, true];
        _nullArray = null;
        _emptyStringArray = [];
    }
}

// Test shared references (same object from multiple fields)
public class SharedRefs
{
    private Tag _ref1;
    private Tag _ref2;
    private Address _shared;
    private Address _sharedAgain;

    public SharedRefs(Tag tag, Address addr)
    {
        _ref1 = tag;
        _ref2 = tag;
        _shared = addr;
        _sharedAgain = addr;
    }
}

// Class with a List<string> field
public class ListHolder
{
    private List<string> _items;
    private List<Order> _orders;

    public ListHolder(List<string> items, List<Order> orders)
    {
        _items = items;
        _orders = orders;
    }
}

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: Ndump.TestApp <dump-path>");
            return 1;
        }

        var dumpPath = args[0];

        // Create objects that will be on the heap when the dump is taken
        var addr1 = new Address("123 Main St", "Springfield", 62701);
        var addr2 = new Address("456 Oak Ave", "Shelbyville", 62702);

        var order1 = new Order(1001, 29.99, "Widget order", shippedAt: new DateTime(2025, 6, 15), rating: 5);
        var order2 = new Order(1002, 149.50, "Gadget bulk order", shippedAt: new DateTime(2025, 7, 1));
        var order3 = new Order(1003, 5.00, "Small item");

        var tag1 = new Tag("important", 1);
        var tag2 = new Tag("urgent", 2);

        var cat1 = new Cat("Whiskers", 3, true);
        var cat2 = new Cat("Mittens", 5, false);
        var dog1 = new Dog("Rex", 4, "German Shepherd");
        var dog2 = new Dog("Buddy", 2, "Golden Retriever");

        var scores1 = new Dictionary<string, int> { ["math"] = 95, ["science"] = 87 };
        var scores2 = new Dictionary<string, int> { ["art"] = 72 };
        var scores3 = new Dictionary<string, int> { ["math"] = 100, ["art"] = 88, ["science"] = 91 };

        // Force the KeyCollection type to be loaded
        _ = scores1.Keys.ToArray();

        var cust1 = new Customer("Alice", 30, true, order1, addr1, [order1, order2],
            [order1, addr1, tag1, "hello"], [cat1, dog1], ["vip", "early-adopter"], scores1);
        var cust2 = new Customer("Bob", 45, false, order2, addr2, [order2],
            [tag2, order2], [dog2], ["regular"], scores2);
        var cust3 = new Customer("Charlie", 28, true, order3, addr1, [order1, order2, order3],
            [addr2, "world", order3, tag1, addr1], [cat2, dog1, cat1], ["vip", "premium", "newsletter"], scores3);

        // ── New test objects ──────────────────────────────────────────

        // All primitives
        var allPrimitives = new AllPrimitives(true, 255, -42, -1000, 50000, 123456, 4000000000u, 9876543210L, 18446744073709551615UL, 3.14f, 2.718281828, 'Z');

        // Nullable holder
        var nullableHolder = new NullableHolder();

        // String variants
        var stringVariants = new StringVariants();

        // Linked list with self-reference and circular reference
        var nodeC = new Node("C");
        var nodeB = new Node("B", nodeC);
        var nodeA = new Node("A", nodeB);
        nodeA.SetSelf();
        nodeC.SetNext(nodeA); // circular: A -> B -> C -> A

        // Deep inheritance
        var leaf = new Leaf(100, "mid", 3.14);

        // Struct instances — need to be boxed/stored as fields to appear on heap
        var pt1 = new Point(10, 20);
        var pt2 = new Point(30, 40);
        var rect = new Rectangle(pt1, pt2);
        var label = new Label("test-label", 5, tag1);
        var structHolder = new StructHolder(pt1, rect, label);

        // Enum holder
        var enumHolder = new EnumHolder(Color.Blue, SmallEnum.High, Permissions.Read | Permissions.Write);

        // Array holder
        var arrayHolder = new ArrayHolder();

        // Shared references
        var sharedRefs = new SharedRefs(tag1, addr1);

        // List holder
        var listHolder = new ListHolder(
            ["alpha", "beta", "gamma"],
            [order1, order2]);

        // Force Point[] to appear on the heap for struct array testing
        var pointArray = new Point[] { new(1, 2), new(3, 4), new(5, 6) };

        // Keep references alive so GC doesn't collect them
        var allObjects = new object[]
        {
            addr1, addr2, order1, order2, order3, cust1, cust2, cust3, tag1, tag2,
            cat1, cat2, dog1, dog2,
            allPrimitives, nullableHolder, stringVariants,
            nodeA, nodeB, nodeC,
            leaf,
            structHolder, enumHolder, arrayHolder, sharedRefs, listHolder,
            pointArray
        };

        // Find createdump from the currently executing runtime
        var createdumpPath = FindCreatedump();
        if (createdumpPath is null)
        {
            Console.Error.WriteLine("Could not find createdump executable in the .NET runtime directory");
            return 2;
        }

        // Dump ourselves — createdump without a PID argument dumps its parent process
        var psi = new ProcessStartInfo
        {
            FileName = createdumpPath,
            Arguments = $"--full --name \"{dumpPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var proc = Process.Start(psi)!;
        proc.WaitForExit();

        if (proc.ExitCode != 0)
        {
            var stderr = proc.StandardError.ReadToEnd();
            Console.Error.WriteLine($"createdump failed (exit {proc.ExitCode}): {stderr}");
            return 3;
        }

        Console.WriteLine($"DUMP:{dumpPath}");

        // Use allObjects to prevent optimization
        GC.KeepAlive(allObjects);
        return 0;
    }

    static string? FindCreatedump()
    {
        // The runtime directory contains createdump(.exe)
        var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
        var name = OperatingSystem.IsWindows() ? "createdump.exe" : "createdump";
        var path = Path.Combine(runtimeDir, name);
        return File.Exists(path) ? path : null;
    }
}
