using Autofac;
using Cosmopolis;

var container = DependencyInjection.Build();
var startup = container.Resolve<Startup>();
startup.Run();
