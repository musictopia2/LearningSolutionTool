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
            return;
        }
        if (custom.Command == EnumCustomCommand.Section)
        {
            await ProcessSectionAsync(testPath, libraryPath, custom.ExerciseCount);
            //implies you need the first lesson now.
            await ProcessLessonAsync(testPath, libraryPath, custom.ExerciseCount);

            return;
        }
        Console.WriteLine("No command");
        Environment.Exit(1);
    }
    private static async Task ProcessLessonAsync(string testPath, string libraryPath, int exerciseCount)
    {
        //Console.WriteLine($"Processing lesson for test path of {testPath}, library path of {libraryPath} and has {exerciseCount} exercises");

        BasicList<string> firstTest = await ff1.DirectoryListAsync(testPath);
        BasicList<string> sectionTests = firstTest.Select(ff1.FileName).ToBasicList();
        sectionTests.RemoveAllAndObtain(x => x.StartsWith("Section") == false);

        BasicList<string> firstLibrary = await ff1.DirectoryListAsync(libraryPath);
        BasicList<string> sectionLibrary = firstLibrary.Select(ff1.FileName).ToBasicList();
        sectionLibrary.RemoveAllAndObtain(x => x.StartsWith("Section") == false);

        string lastTest = sectionTests.Last();
        string lastLibrary = sectionLibrary.Last();

        string currentTest = GetCurrentSection(lastTest);
        string currentLibrary = GetCurrentSection(lastLibrary);

        if (currentTest != currentLibrary)
        {
            Console.WriteLine($"The current test of {currentTest} does not match {currentLibrary}.  the last test was {lastTest} and last library was {lastLibrary}");
            Environment.Exit(1);
            return;
        }

        currentTest = firstTest.Last();
        currentLibrary =  firstLibrary.Last();
        firstTest = await ff1.DirectoryListAsync(currentTest);
        firstLibrary = await ff1.DirectoryListAsync(currentLibrary);
        if (firstTest.Count != firstLibrary.Count)
        {
            Console.WriteLine($"Counts for lessons don't match.   Tests had {firstTest.Count} and library had {firstLibrary.Count}");
            Environment.Exit(1);
            return;
        }
        string? currentLibraryLesson = firstTest.LastOrDefault();
        string? currentTestLesson = firstLibrary.LastOrDefault();
        if (currentLibraryLesson is not null)
        {
            currentLibraryLesson = ff1.FileName(currentLibraryLesson);
        }
        if (currentTestLesson is not null)
        {
            currentTestLesson = ff1.FileName(currentTestLesson);
        }
        string nextLibraryLesson = GetNextLesson(currentTestLesson);
        string nextTestLesson = GetNextLesson(currentLibraryLesson);
        if (nextTestLesson != nextLibraryLesson)
        {
            Console.WriteLine($"Next test lesson of {nextTestLesson} does not match next library {nextLibraryLesson} ");
            Environment.Exit(1);
            return;
        }
        //Console.WriteLine(nextTestLesson);
        string newTestLessonPath = Path.Combine(testPath, lastTest);
        string newLibraryLessonPath = Path.Combine(libraryPath, lastTest);
        //Console.WriteLine(testPath);
        //Console.WriteLine(libraryPath);

        string libraryName = ff1.FileName(libraryPath);
        string testName = ff1.FileName(testPath);
       

        Console.Write("Enter New Lesson Name: ");
        string newLessonName = Console.ReadLine()!;




        await CreateNewLessonLibraryAsync(libraryName, nextTestLesson, lastLibrary, newLibraryLessonPath, newLessonName, exerciseCount);
        await CreateNewLessonTestsAsync(testName, nextTestLesson, lastTest, newTestLessonPath, newLessonName);
    }
    private static async Task CreateNewLessonLibraryAsync(string projectName, string nextNumber, string currentSection, string libraryBasePath, string lessonName, int exerciseCount)
    {
        string realName = $"Lesson{nextNumber}{lessonName}";
        string newPath = Path.Combine(libraryBasePath, realName);
        await ff1.CreateFolderAsync(newPath);
        await exerciseCount.TimesAsync(async x =>
        {
            string newItem = x.ToString("D2"); //for now, use this until i find a better way to handle this.
            string exercisePath = Path.Combine(newPath, $"Exercise{newItem}");
            await ff1.CreateFolderAsync(exercisePath);
            //namespace CSharpPracticeLibrary.Section01HelloWorld.Lesson01ConsolePrinting.Exercise01;
            string text = $$"""
            namespace {{projectName}}.{{currentSection}}.{{realName}}.Exercise{{newItem}};
            public static class MainClass
            {
            
            }
            """;
            string finalPath = Path.Combine(exercisePath, "MainClass.cs");
            await ff1.WriteAllTextAsync(finalPath, text);
        });
    }
    private static async Task CreateNewLessonTestsAsync(string projectName, string nextNumber, string currentSection, string testBasePath, string lessonName)
    {
        string realName = $"Lesson{nextNumber}{lessonName}";
        string newPath = Path.Combine(testBasePath, realName);
        await ff1.CreateFolderAsync(newPath);
        string text = $$"""
            namespace {{projectName}}.{{currentSection}}.{{realName}};
            [Trait("Section", "{{currentSection}}")]
            public class ExercisesClass
            {
            
            }
            """;
        string finalPath = Path.Combine(newPath, "ExercisesClass.cs");
        await ff1.WriteAllTextAsync(finalPath, text);
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
        string? lastTest = sectionTests.LastOrDefault();
        string? lastLibrary = sectionLibrary.LastOrDefault();



        string nextTest = GetNextSection(lastTest);
        string nextLibrary = GetNextSection(lastLibrary);

        if (nextTest != nextLibrary)
        {
            Console.WriteLine($"The next test of {nextTest} does not match {nextLibrary}.  the last test was {lastTest} and last library was {lastLibrary}");
            Environment.Exit(1);
            return;
        }
        //Console.WriteLine($"The next test section detected was {nextTest} and next library section detected was {nextLibrary}");
        Console.Write("Enter New Section Name:  ");
        string newSection = Console.ReadLine()!;
        string newName = $"Section{nextLibrary}{newSection}";
        string latestLibrary = Path.Combine(libraryPath, newName);
        string latestTest = Path.Combine(testPath, newName);
        //Console.WriteLine($"The New Name Will {newName} and the library path will be {latestLibrary} and the test path will be {latestTest}");

        await ff1.CreateFolderAsync(latestLibrary);
        await ff1.CreateFolderAsync(latestTest);


    }

    private static string GetCurrentSection(string directoryName)
    {
        if (directoryName is null)
        {
            return "01";
        }
        const int startIndex = 7; // "Section".Length
        const int numberLength = 2;

        string numberText = directoryName.Substring(startIndex, numberLength);
        int currentSection = int.Parse(numberText);

        return currentSection.ToString("D2");
    }

    private static string GetNextLesson(string? directoryName)
    {
        if (directoryName is null)
        {
            return "01";
        }
        const int startIndex = 6; // "Lesson".Length
        const int numberLength = 2;

        string numberText = directoryName.Substring(startIndex, numberLength);
        int nextSection = int.Parse(numberText) + 1;

        return nextSection.ToString("D2");
    }
    private static string GetNextSection(string? directoryName)
    {
        if (directoryName is null)
        {
            return "01";
        }
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