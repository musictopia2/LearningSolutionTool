namespace LearningSolutionTool;
internal static class CustomClass
{
    public static async Task RunAsync(SolutionHookArgs solution, CustomArgs custom)
    {
        Console.WriteLine(solution.SolutionDir);
        Console.WriteLine(solution.SolutionFileName);
        Console.WriteLine(custom.ExerciseCount);
        Console.WriteLine(custom.Command.Name);
        //just prove i am able to get the proper values.
        //once i prove that, can do the real work.
    }
}