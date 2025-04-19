using System.Reflection;
using Avalonia.Controls;

namespace ProjectMsBuildLoader;

class Program
{
    static void Main()
    {
        var loader = new global::ProjectMsBuildLoader.ProjectMsBuildLoader(
            "/home/deck/Desktop/Projects/ProjectMsBuildLoader/ProjectMsBuildLoader/Demo/Demo.csproj");

        if (loader.LoadAndBuildProject())
        {
            var files = loader.GetProjectFiles();

            Console.WriteLine("📄 Найденные .cs и .axaml файлы:");
            foreach (var file in files)
            {
                Console.WriteLine($"[{file.ItemType}] {file.RelativePath} → {file.FullPath}");
            }

            // Определим путь до DLL
            var outputDll = Path.Combine(
                Path.GetDirectoryName(loader.ProjectPath)!,
                "bin", "Debug", "net9.0", "Demo.dll");

            if (!File.Exists(outputDll))
            {
                Console.WriteLine($"❌ DLL не найдена: {outputDll}");
                return;
            }

            Console.WriteLine($"\n📦 Загрузка сборки: {outputDll}");
            var asm = Assembly.LoadFrom(outputDll);

            var types = asm.GetTypes();
            var windows = types.Where(t => typeof(Window).IsAssignableFrom(t) && !t.IsAbstract);
            var userControls = types.Where(t => typeof(UserControl).IsAssignableFrom(t) && !t.IsAbstract);

            Console.WriteLine("\n🪟 Найденные окна:");
            foreach (var w in windows)
                Console.WriteLine($" - {w.FullName}");

            Console.WriteLine("\n📦 Найденные пользовательские контролы:");
            foreach (var uc in userControls)
                Console.WriteLine($" - {uc.FullName}");
        }
        else
        {
            Console.WriteLine("❌ Проект не удалось собрать.");
        }
    }
}