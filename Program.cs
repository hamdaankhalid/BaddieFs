namespace BaddieFs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BaddieFsService service = new BaddieFsService();
            Environment.ExitCode = service.Run();
        }
    }
}
