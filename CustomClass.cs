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
            await ProcessSectionAsync(testPath, libraryPath);
            return;
        }
        Console.WriteLine("No command");
        Environment.Exit(1);
    }
    private static async Task ProcessLessonAsync(string testPath, string libraryPath, int lessonCount)
    {
        Console.WriteLine($"Processing lesson for test path of {testPath}, library path of {libraryPath} and has {lessonCount} lessons");
    }
    private static async Task ProcessSectionAsync(string testPath, string libraryPath)
    {
        Console.WriteLine($"Processing new section for {testPath}, library path of {libraryPath}");
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