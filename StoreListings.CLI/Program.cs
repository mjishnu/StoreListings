// See https://aka.ms/new-console-template for more information
using ConsoleAppFramework;
using StoreListings.CLI;

Console.OutputEncoding = System.Text.Encoding.UTF8;
ConsoleApp.ConsoleAppBuilder builder = ConsoleApp.Create();
builder.Add<Commands>();
await builder.RunAsync(args);
