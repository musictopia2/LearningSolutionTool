namespace LearningSolutionTool;
internal static class CustomFactory
{
    public static CustomArgs CreateCustomArgs(string[] args)
    {
        string temp = cc1.GetValue("Command"); //must be done this way.
        if (string.IsNullOrWhiteSpace(temp))
        {
            Console.WriteLine("Command is required.");
            Environment.Exit(1);
        }
        EnumCustomCommand command = EnumCustomCommand.FromName(temp, true);
        int exerciseCount = 0;
        temp = cc1.GetValue("ExerciseCount");
        if (string.IsNullOrWhiteSpace(temp))
        {
            Console.WriteLine("ExerciseCount is required.");
            Environment.Exit(1);
        }
        if (int.TryParse(temp, out exerciseCount) == false)
        {
            Console.WriteLine("ExerciseCount must be a valid integer.");
            Environment.Exit(1);
        }
        if (exerciseCount <= 0)
        {
            Console.WriteLine("ExerciseCount must be a positive integer.");
            Environment.Exit(1);
        }
        return new(command, exerciseCount);
    }
}
