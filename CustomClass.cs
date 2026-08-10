using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using System.Net.Http.Headers;

namespace LearningSolutionTool;
internal static class CustomClass
{
    public static async Task RunAsync(SolutionHookArgs solution, CustomArgs custom)
    {

        if (solution.SolutionFileName.EndsWith("slnx") == false)
        {
            Console.WriteLine("Requires the new slnx format");
            Environment.Exit(1);
            return;
        }
        if (solution.SolutionFileName.EndsWith("Solution.slnx") == false)
        {
            Console.WriteLine("Requires this project to end with Solution");
            Environment.Exit(1);
            return;
        }
        string projectName = solution.SolutionFileName.Replace("Solution.slnx", "");
        BasicList<string> projects = ProjectsInSolution(solution);
        if (projects.Count < 2)
        {
            Console.WriteLine("Requires at least 2 projects in solution in order to do practice because needs at least a library and a test");
            Environment.Exit(1);
        }

        string? testPath = projects.SingleOrDefault(x =>
    Path.GetFileName(x) == $"{projectName}Tests");

        string? libraryPath = projects.SingleOrDefault(x =>
            Path.GetFileName(x) == $"{projectName}Library");


        if (string.IsNullOrWhiteSpace(testPath))
        {
            Console.WriteLine("Requires one unit test project with same name as solution name");
            Environment.Exit(1);
        }
        if (string.IsNullOrWhiteSpace(libraryPath))
        {
            Console.WriteLine("Requires one library path with same name as solution name");
            Environment.Exit(1);
        }

        
        if (custom.Command == EnumCustomCommand.Lesson)
        {
            await ProcessLessonAsync(testPath, libraryPath, custom.ExerciseCount);
            //do section part alone before running this test again.
            await ProcessSectionAsync(testPath, libraryPath, custom.ExerciseCount); //implies you need the first lesson now.
            return;
        }
        if (custom.Command == EnumCustomCommand.Section)
        {
            await ProcessSectionAsync(testPath, libraryPath, custom.ExerciseCount);
            return;
        }
        Console.WriteLine("No command");
        Environment.Exit(1);
    }
    private static async Task ProcessLessonAsync(string testPath, string libraryPath, int lessonCount)
    {
        Console.WriteLine($"Processing lesson for test path of {testPath}, library path of {libraryPath} and has {lessonCount} lessons");





    }
    private static async Task ProcessSectionAsync(string testPath, string libraryPath, int lessonCount)
    {
        //Console.WriteLine($"Processing new section for {testPath}, library path of {libraryPath}");

        BasicList<string> firsts = await ff1.DirectoryListAsync(testPath);
        BasicList<string> sectionTests = firsts.Select(ff1.FileName).ToBasicList();
        sectionTests.RemoveAllAndObtain(x => x.StartsWith("Section") == false);

        firsts = await ff1.DirectoryListAsync(libraryPath);
        BasicList<string> sectionLibrary = firsts.Select(ff1.FileName).ToBasicList();
        sectionLibrary.RemoveAllAndObtain(x => x.StartsWith("Section") == false);
        if (sectionLibrary.Count != sectionTests.Count)
        {
            Console.WriteLine($"Test Path Of {testPath} had {sectionTests.Count} and library path of {libraryPath} had {sectionLibrary.Count} which does not reconcile");
            Environment.Exit(1);
            return;
        }
        string lastTest = sectionTests.Last();
        string lastLibrary = sectionLibrary.Last();



        string nextTest = GetNextSection(lastTest);
        string nextLibrary = GetNextSection(lastLibrary);

        if (nextTest != nextLibrary)
        {
            Console.WriteLine($"The next test of {nextTest} does not match {nextLibrary}.  the last test was {lastTest} and last library was {lastLibrary}");
            Environment.Exit(1);
            return;
        }
        //Console.WriteLine($"The next test section detected was {nextTest} and next library section detected was {nextLibrary}");
        Console.Write("Enter New Section:  ");
        string newSection = Console.ReadLine()!;
        string newName = $"Section{nextLibrary}{newSection}";
        string latestLibrary = Path.Combine(libraryPath, newName);
        string latestTest = Path.Combine(testPath, newName);
        //Console.WriteLine($"The New Name Will {newName} and the library path will be {latestLibrary} and the test path will be {latestTest}");

        await ff1.CreateFolderAsync(latestLibrary);
        await ff1.CreateFolderAsync(latestTest);
        Console.WriteLine("Created the expected folders.  Check to make sure it worked");


    }

    private static string GetNextSection(string directoryName)
    {
        const int startIndex = 7; // "Section".Length
        const int numberLength = 2;

        string numberText = directoryName.Substring(startIndex, numberLength);
        int nextSection = int.Parse(numberText) + 1;

        return nextSection.ToString("D2");
    }

    private static BasicList<string> ProjectsInSolution(SolutionHookArgs solution)
    {
        string path = Path.Combine(
            solution.SolutionDir,
            solution.SolutionFileName);

        XElement source = XElement.Load(path);

        BasicList<string> output = [];

        foreach (XElement project in source.Elements("Project"))
        {
            string? projectPath = project.Attribute("Path")?.Value;

            if (string.IsNullOrWhiteSpace(projectPath))
            {
                Console.WriteLine("blank project path");
                return [];
            }

            string? directory = Path.GetDirectoryName(projectPath);

            if (string.IsNullOrWhiteSpace(directory))
            {
                Console.WriteLine($"No directory for {projectPath}");
                return [];
            }

            string finalPath = Path.Combine(solution.SolutionDir, projectPath);
            // This tool only supports projects directly below the solution.
            if (ff1.FileExists(finalPath) == false)
            {
                Console.WriteLine($"Only supports directly below. The final path was {finalPath}");
                return [];
            }
            string fins = Path.GetDirectoryName(finalPath)!;
            output.Add(fins);
        }

        return output;
    }

}