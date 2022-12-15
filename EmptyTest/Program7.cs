IServiceProvider


//Console.WriteLine("你好f34f34f34f3qwq".AsSpan().ToString());

/*Delegate a = () => Console.WriteLine("114514");
a.DynamicInvoke();*/


/*#nullable disable

using static ISenderFactory;

internal class Program
{


    private static void Main(string[] args)
    {
        Console.WriteLine();
        SenderFactory f = new();
        f.OnMakeSender += F_OnMakeSender;
    }

    private static void F_OnMakeSender(ISender sender)
    {
        Console.WriteLine(sender.Name);
    }

    private static void SomeProcess(ISender sender)
    {
        Console.WriteLine($"Process: {sender.Name}");
        
    }
}

interface ISenderFactory
{
    public delegate void FactoryEventHandler<in T>(T arg);
    //public delegate void OnMakeISenderHandler(ISender sender);
    public event FactoryEventHandler<ISender> OnMakeISender;
    ISender Make();
}

class SenderFactory : ISenderFactory
{
    public delegate void OnMakeSenderHandler(Sender sender);
    public event FactoryEventHandler<Sender> OnMakeSender;
    public Sender Make() => new();
    ISender ISenderFactory.Make() => Make();

    event ISenderFactory.FactoryEventHandler<ISender> ISenderFactory.OnMakeISender { add => OnMakeSender += value; remove => OnMakeSender -= value; }
}

interface ISender { string Name { get; } }
class Sender : ISender { public string Name { get; } = "Test"; }
*/

/*SomeClass s = new();
s.Prop = 1;
Console.WriteLine(s.Prop);

public class SomeClass
{
    private int prop = 0;

    public int Prop
    {
        get => prop;
        set => prop = value;
    }
}*/

/*NormalStruct n = new(2);
ISayable u = n;
ISayable u2 = n;
n.Value = 4;
u.Say();
n.Value = 9;
u2.Say();
MakeSay(u);

void MakeSay(ISayable sayable)
{
    sayable.Say();
}
public class NormalClass : ISayable
{
    public int Value;
    public NormalClass(int value) => Value = value;
    public void Say()
    {
        Console.WriteLine($"I'm NormalClass: {Value}");
    }
}
public struct NormalStruct : ISayable
{
    public int Value;
    public NormalStruct(int value) => Value = value;
    public void Say()
    {
        Console.WriteLine($"I'm NormalStruct: {Value}");
    }
}
public interface ISayable
{
    void Say();
}*/




/*A a = new();
public class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}*/




/*Console.WriteLine("Hello World.");
Console.ReadLine();*/

/*using System.Net.Http.Headers;
HttpClient client = new();
string json = """
    {
        "group_id": 860355679
    }
    """;
StringContent content = new(json);
content.Headers.Clear();
content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
content.Headers.ContentEncoding.Add("utf-8");
//content.Headers.Add("Content-Type", "application/json; charset=utf-8");
var res = await client.PostAsync("http://127.0.0.1:5700/get_group_info", content);
string s = await res.Content.ReadAsStringAsync();
Console.WriteLine(s);*/


/*using System.Net;
using System.Text;

HttpListener listener = new();
listener.Prefixes.Add("http://127.0.0.1:5566/");
listener.Start();

while (true)
{
    var context = listener.GetContext();

    var method = context.Request.HttpMethod;

    HttpListenerResponse response = context.Response;
    response.StatusCode = (int)HttpStatusCode.OK;
    response.ContentType = "application/json;charset=UTF-8";
    response.ContentEncoding = Encoding.UTF8;
    response.AppendHeader("Content-Type", "application/html;charset=UTF-8");

    using StreamWriter writer = new(response.OutputStream);
    writer.WriteLine($"这是回应, 你使用的方法是{method}");
    string body = new StreamReader(context.Request.InputStream).ReadToEnd();
    writer.WriteLine($"你的请求body是:\n{body}");

    Console.WriteLine($"请求来了, url是{context.Request.Url}, body是\n{body}\n------");
}*/

/*{
    Something();
}

{
    GC.Collect();
}
void Something()
{ 
    DisposeableThing thing = new();
}

public class DisposeableThing : IDisposable
{
    public void Dispose()
    {
        Console.WriteLine("disposed");
        GC.SuppressFinalize(this);
    }

    ~DisposeableThing()
    {
        Console.WriteLine("析构");
    }
}*/



/*public class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello world");
        Person p = new("114514", 20);
        Person p2 = new("114514", 20);
        Console.WriteLine(p == p2);
    }
}

public class Person
{
    public Person(string name, int age)
    {
        this.Name = name;
        this.Age = age;
    }

    public string Name { get; set; }

    public int Age { get; set; }

    public override bool Equals(object? obj)
    {
        bool r = obj is Person person &&
               this.Name == person.Name &&
               this.Age == person.Age;
        Console.WriteLine("进行了一次Equals比较");
        return r;
    }

    public override int GetHashCode()
    {
        Console.WriteLine("进行了一次GetHashCode");
        return HashCode.Combine(this.Name, this.Age);
    }

    public static bool operator ==(Person? left, Person? right)
    {
        Console.WriteLine("进行了一次==");
        return EqualityComparer<Person>.Default.Equals(left, right);
    }

    public static bool operator !=(Person? left, Person? right)
    {
        Console.WriteLine("进行了一次!=");
        return !(left == right);
    }
}*/