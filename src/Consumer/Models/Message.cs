namespace Consumer.Models;

public class Message<T>
{
    public Payload<T> payload { get; set; }
}
