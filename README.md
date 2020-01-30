# <img src="/design/logo/logo_64.png" height="30px"> LoadAssembliesOnStartup.Fody

[![Chat on Gitter](https://img.shields.io/gitter/room/fody/fody.svg?style=flat)](https://gitter.im/Fody/Fody)
[![NuGet Status](http://img.shields.io/nuget/v/LoadAssembliesOnStartup.Fody.svg?style=flat)](https://www.nuget.org/packages/LoadAssembliesOnStartup.Fody/)

Loads all the references on startup by actually using the types in the module initializer.


## This is an add-in for [Fody](https://github.com/Fody/Fody/)

**It is expected that all developers using Fody either [become a Patron on OpenCollective](https://opencollective.com/fody/contribute/patron-3059), or have a [Tidelift Subscription](https://tidelift.com/subscription/pkg/nuget-fody?utm_source=nuget-fody&utm_medium=referral&utm_campaign=enterprise). [See Licensing/Patron FAQ](https://github.com/Fody/Home/blob/master/pages/licensing-patron-faq.md) for more information.**


### NuGet package

Available here: <a href="http://nuget.org/packages/LoadAssembliesOnStartup.Fody" target="_blank">http://nuget.org/packages/LoadAssembliesOnStartup.Fody</a>

To Install from the Nuget Package Manager Console 
    
    PM> Install-Package LoadAssembliesOnStartup.Fody


## How it works

By default, assemblies are only loaded on-demand. This means that the first time a type is actually used, the .NET runtime will load the assembly.

When using <a href="https://github.com/Fody/ModuleInit" target="_blank">ModuleInit</a>, it is possible to initialize an assembly at startup. For example, to register types in a service locator.

To prevent hacks such as the one displayed below:

	// Note: this is a hack, force loading of external assembly 
	var dummyType = typeof(MyExternalAssemblyType);
	Console.WriteLine(dummyType.FullName); 

it is possible to let this plugin take care of this. It will add the following code for each referenced assembly that contains at least one public class:

	var preloadType = typeof(ReferenceType);

This will ensure that an assembly is actually being loaded into the AppDomain (which is **not** the same as Assembly.LoadFrom).


## Configuration options

All config options are accessible by modifying the `LoadAssembliesOnStartup` node in *FodyWeavers.xml*.


### ExcludeAssemblies

A list of assembly names to exclude from the default action of "embed all Copy Local references".

Do not include `.exe` or `.dll` in the names.

Can not be defined with `IncludeAssemblies`.

Can take two forms. 

As an element with items delimited by a newline.

    <LoadAssembliesOnStartup>
        <ExcludeAssemblies>
            Foo
            Bar
        </ExcludeAssemblies>
    </LoadAssembliesOnStartup>
    
Or as a attribute with items delimited by a pipe `|`.

    <LoadAssembliesOnStartup ExcludeAssemblies='Foo|Bar' />


### IncludeAssemblies

A list of assembly names to include from the default action of "embed all Copy Local references".

Do not include `.exe` or `.dll` in the names.

Can not be defined with `ExcludeAssemblies`.

Can take two forms. 

As an element with items delimited by a newline.

    <LoadAssembliesOnStartup>
        <IncludeAssemblies>
            Foo
            Bar
        </IncludeAssemblies>
    </LoadAssembliesOnStartup>
    
Or as a attribute with items delimited by a pipe `|`.

    <LoadAssembliesOnStartup IncludeAssemblies='Foo|Bar' />


### ExcludePrivateAssemblies

Exclude private assembly references in modern SDK projects, e.g.:

```
  <PackageReference Include="Catel.Core" Version="5.8.0" PrivateAssets="true" />
```

Types can still be excluded using the `ExcludeAssemblies` option when this option is set to `false`.

The default value is `true`.

To include private assemblies, use the option below:

	<LoadAssembliesOnStartup ExcludePrivateAssemblies='false' />


### ExcludeSystemAssemblies

Exclude system assemblies (such as System.Runtime.Serialization). Types can still be excluded using the `ExcludeAssemblies` option when this option is set to `false`.

The default value is `true`.

To include system assemblies, use the option below:

	<LoadAssembliesOnStartup ExcludeSystemAssemblies='false' />

The following wildcards will be used:

- Mono.*
- System.*


### ExcludeOptimizedAssemblies

By default, this weaver will include references that are optimized away by the compiler. This can happen when you only use interfaces from a reference. Types can still be excluded using the `ExcludeAssemblies` option.

To disable all the optimized assemblies (default .NET compiler behavior), use the option below:

	<LoadAssembliesOnStartup ExcludeOptimizedAssemblies='true' />


### WrapInTryCatch

By default, this weaver calls the `typeof(SomeType)` without any exception handling. While in general, this is good, it might happen that an assembly cannot be loaded. This can either be solved by adding it to the `ExcludeAssemblies` list *or* setting the `WrapInTryCatch` property to true:

	<LoadAssembliesOnStartup WrapInTryCatch='true' />

Then the weaved code will look like:

	try
	{
		typeof(FirstTypeFromReference1);
	}
	catch (Exception
	{
	}


	try
	{
		typeof(FirstTypeFromReference2);
	}
	catch (Exception
	{
	}


## Icon

Explosion by Gustav Salomonsson from The Noun Project
