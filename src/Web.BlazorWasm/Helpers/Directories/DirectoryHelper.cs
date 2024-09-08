namespace Core.Domain
{
    public class DirectoryHelper
    {
        public static string GetSolutionBasePath()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory?.FullName ?? string.Empty;
        }

        public static string GetHtmlTemplatePath()
        {
            return Path.Combine(GetSolutionBasePath(), @"src\Microservice\Core\Core.Service\Templates\Html\");
        }
    }
}