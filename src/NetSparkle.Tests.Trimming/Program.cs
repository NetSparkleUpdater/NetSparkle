using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var checker = new Ed25519Checker(SecurityMode.Strict, "");