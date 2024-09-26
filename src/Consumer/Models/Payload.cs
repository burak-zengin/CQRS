namespace Consumer.Models;

public class Payload<T>
{
    public T before { get; set; }

    public T after { get; set; }

    public string op { get; set; }
}
