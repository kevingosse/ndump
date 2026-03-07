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

    public Order(int orderId, double total, string description)
    {
        _orderId = orderId;
        _total = total;
        _description = description;
        _createdAt = DateTime.UtcNow;
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

        var order1 = new Order(1001, 29.99, "Widget order");
        var order2 = new Order(1002, 149.50, "Gadget bulk order");
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

        var cust1 = new Customer("Alice", 30, true, order1, addr1, [order1, order2],
            [order1, addr1, tag1, "hello"], [cat1, dog1], ["vip", "early-adopter"], scores1);
        var cust2 = new Customer("Bob", 45, false, order2, addr2, [order2],
            [tag2, order2], [dog2], ["regular"], scores2);
        var cust3 = new Customer("Charlie", 28, true, order3, addr1, [order1, order2, order3],
            [addr2, "world", order3, tag1, addr1], [cat2, dog1, cat1], ["vip", "premium", "newsletter"], scores3);

        // Keep references alive so GC doesn't collect them
        var allObjects = new object[] { addr1, addr2, order1, order2, order3, cust1, cust2, cust3, tag1, tag2, cat1, cat2, dog1, dog2 };

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
