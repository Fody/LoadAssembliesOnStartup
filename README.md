LoadAssembliesOnStartup
=======================

![GitHubLink](design/logo/logo_64.png)

Loads all the references on startup by actually using the types in the module initializer.

By default, assemblies are only loaded on-demand. This means that the first time a type is actually used, the .NET runtime will load the assembly.

When using <a href="https://github.com/Fody/ModuleInit" target="_blank">ModuleInit</a>, it is possible to initialize an assembly at startup. For example, to register types in a service locator.

To prevent hacks such as the one displayed below:

	// Note: this is a hack, force loading of external assembly
	var dummyType = typeof(MyExternalAssemblyType);
	Console.WriteLine(dummyType.FullName); 

it is possible to let this plugin take care of this. It will add the following code for each referenced assembly that contains at least one public class:

	var preloadType = typeof(ReferenceType);


# Icon #

Explosion by Gustav Salomonsson from The Noun Project