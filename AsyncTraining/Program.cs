
// I/O bound operation

using System.Runtime.CompilerServices;

var path = Path.Combine(Directory.GetCurrentDirectory(), "../AsyncTraining/Resources/SomeImportantData.txt");
var result = await ReadFileWithImportantInfoAsync("C:\\Users\\kyrylo.sokyrka\\Repositories\\Work\\AsyncTraining\\AsyncTraining\\Resources\\SomeImportantData.txt");

Console.WriteLine(result);

async Task<int> ReadFileWithImportantInfoAsync(string filePath)
{
    var lines = await File.ReadAllLinesAsync(filePath);
    return lines.Aggregate(0, (total, next) => total += next.Length);
}

// CPU bound operation
result = await Task.Run(CalculateSomeThingImportant);

int CalculateSomeThingImportant() => 1000 - 7;

Console.WriteLine(result);

// LiveLock example
var bob = new Worker("Bob");
var jack = new Worker("Jack");

var door = new DoorHandle(jack);

await Task.WhenAll(
    Task.Run(() => bob.HandleDoor(door, jack)),
    Task.Run(() => jack.HandleDoor(door, bob))
);

// DeadLock Example
var lock1 = new object();
var lock2 = new object();
Console.WriteLine("Starting...");
var task1 = Task.Run(() =>
{
    lock (lock2)
    {
        Task.Delay(1000);

        lock (lock1)
        {
            Console.WriteLine("Finished Thread 1");
        }
    }
});

var task2 = Task.Run(() =>
{
    lock (lock1)
    {
        Task.Delay(1000);

        lock (lock2)
        {
            Console.WriteLine("Finished Thread 2");
        }
    }
});

await Task.WhenAll(task1, task2);
Console.WriteLine("Finished...");

public class DoorHandle
{
    public Worker Owner { get; private set; }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetUser(Worker d) { Owner = d; }

    public DoorHandle(Worker owner)
    {
        Owner = owner;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Use()
    {
        Console.WriteLine($"{Owner.Name} opened a door!");
    }
}

public class Worker
{
    public Worker(string n)
    {
        Name = n;
        WentFirst = true;
    }

    public string Name { get; private set; }
    private bool WentFirst { get; set; }

    public async Task HandleDoor(DoorHandle door, Worker anotherWorker)
    {
        while (WentFirst)
        {
            if (door.Owner != this)
            {
                await Task.Delay(100);
                
                continue;
            }

            if (anotherWorker.WentFirst)
            {
                Console.WriteLine("{0}: You go first {1}!", Name, anotherWorker.Name);
                door.SetUser(anotherWorker);
                continue;
            }

            door.Use();
            WentFirst = false;
            Console.WriteLine("{0}: Thank you, Have a nice day {1}!", Name, anotherWorker.Name);
            door.SetUser(anotherWorker);
        }
    }
}