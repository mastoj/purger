namespace Purger.Api;

public interface Message { }
public interface Event : Message { }
public interface Command : Message { }

public delegate IEnumerable<Event> HandleCommand<T>(T command) where T : Command;
public delegate IEnumerable<Command> HandleEvent<T>(T @event) where T : Event;

public record ProductChangedEvent(string productId, string operatingChain) : Event;
public record CmsPageEvent(string url) : Event;

public record PurgeNextecomProductCommand(string productId, string operatingChain) : Command;
public record PurgeCmsPageCommand(string url) : Command;

public static class CommandHandlers
{
  public static IEnumerable<Event> Handle(PurgeNextecomProductCommand command)
  {
    Console.WriteLine($"Purging product {command.productId} from {command.operatingChain}");
    return Enumerable.Empty<Event>();
  }
}

public class CommandHandler
{
  private readonly HandleCommand<PurgeNextecomProductCommand> _purgeNextecomHandler;
  public CommandHandler(HandleCommand<PurgeNextecomProductCommand> purgeNextecomHandler)
  {
    _purgeNextecomHandler = purgeNextecomHandler;
  }

  public IEnumerable<Event> Handle(Command command)
  {
    return command switch
    {
      PurgeNextecomProductCommand c => _purgeNextecomHandler(c),
      _ => throw new InvalidOperationException($"Command {command} not supported")
    };
  }
}

public static class EventHandlers
{
  public static IEnumerable<Command> Handle(ProductChangedEvent @event)
  {
    Console.WriteLine($"Handling product changed event {@event.productId} from {@event.operatingChain}");
    yield return new PurgeNextecomProductCommand(@event.productId, @event.operatingChain);
  }

  public static IEnumerable<Command> Handle(CmsPageEvent @event)
  {
    Console.WriteLine($"Handling CMS page event {@event.url}");
    yield return new PurgeCmsPageCommand(@event.url);
  }

}

public class EventHandler
{
  private readonly HandleEvent<ProductChangedEvent> _productChangedHandler;
  private readonly HandleEvent<CmsPageEvent> _cmsPageHandler;
  public EventHandler(
    HandleEvent<ProductChangedEvent> productChangedHandler,
    HandleEvent<CmsPageEvent> cmsPageHandler)
  {
    _productChangedHandler = productChangedHandler;
    _cmsPageHandler = cmsPageHandler;
  }

  public IEnumerable<Command> Handle(Event @event)
  {
    return @event switch
    {
      ProductChangedEvent e => _productChangedHandler(e),
      CmsPageEvent e => _cmsPageHandler(e),
      _ => throw new InvalidOperationException($"Event {@event} not supported")
    };
  }
}

public class Workflow
{
  private readonly CommandHandler _commandHandler;
  private readonly EventHandler _eventHandler;
  public Workflow(CommandHandler commandHandler, EventHandler eventHandler)
  {
    _commandHandler = commandHandler;
    _eventHandler = eventHandler;
  }

  public IEnumerable<Event> Handle(Command command)
  {
    return _commandHandler.Handle(command);
  }

  public IEnumerable<Command> Handle(Event @event)
  {
    return _eventHandler.Handle(@event);
  }
}

public delegate string RouterDelegate(IEnumerable<Message> message);
public static class Router
{
  public static string Route(IEnumerable<Message> messages)
  {
    return string.Join("\n", messages.Select(m => m.ToString()));
  }
}
public class Application
{
  private Workflow _workflow;
  private RouterDelegate _router;

  public Application(Workflow workflow, RouterDelegate router)
  {
    _workflow = workflow;
    _router = router;
  }

  public void Handle(Message message)
  {
    var result = message switch
    {
      Command c => _router(_workflow.Handle(c)),
      Event e => _router(_workflow.Handle(e)),
      _ => throw new InvalidOperationException($"Message {message} not supported")
    };
    Console.WriteLine(result);
  }
}