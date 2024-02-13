using Purger.Api;

var builder = WebApplication.CreateBuilder(args);

var eventHandler = new Purger.Api.EventHandler(EventHandlers.Handle, EventHandlers.Handle);
var commandHandler = new CommandHandler(CommandHandlers.Handle);
var workflow = new Workflow(commandHandler, eventHandler);
var application = new Application(workflow, Router.Route);

var app = builder.Build();

application.Handle(new ProductChangedEvent("sku", "operatingchain"));
application.Handle(new CmsPageEvent("sku"));



// Ignore
app.MapGet("/", () => "Hello World!");

app.Run();

