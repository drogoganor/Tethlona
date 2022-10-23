using Autofac;
using Tethlona;

var container = DependencyInjection.Build();
var startup = container.Resolve<Startup>();
startup.Run();
