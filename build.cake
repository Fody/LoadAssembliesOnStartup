var projectName = "LoadAssembliesOnStartup.Fody";
var projectsToPackage = new [] { "LoadAssembliesOnStartup.Fody" };
var company = "Fody";
var startYear = 2010;
var defaultRepositoryUrl = string.Format("https://github.com/{0}/{1}", company, projectName);

#l "./deployment/cake/variables.cake"
#l "./deployment/cake/tasks.cake"
