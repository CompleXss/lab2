using Lab2_host;
using System.Diagnostics;

var basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName)
	?? AppDomain.CurrentDomain.BaseDirectory;

Directory.SetCurrentDirectory(basePath);

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSystemd();
builder.Services.AddHostedService<FileReceiver>();

var host = builder.Build();
host.Run();
