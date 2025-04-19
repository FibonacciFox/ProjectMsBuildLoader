using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;

namespace AvaloniaDllLoader
{
    /// <summary>
    /// Представляет информацию о файле, входящем в проект.
    /// </summary>
    public class ProjectFileInfo
    {
        public string ItemType { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public string FullPath { get; set; } = "";
    }

    /// <summary>
    /// Класс для загрузки и сборки проекта MSBuild с возможностью логирования в UI.
    /// </summary>
    public class ProjectMsBuildLoader
    {
        private readonly string _projectPath;
        private Project? _project;

        /// <summary>
        /// Путь к .csproj файлу проекта.
        /// </summary>
        public string ProjectPath => _projectPath;

        /// <summary>
        /// Позволяет указать кастомный логгер для вывода логов сборки.
        /// </summary>
        public ILogger? CustomLogger { get; set; }

        /// <summary>
        /// Создаёт загрузчик MSBuild-проекта.
        /// </summary>
        /// <param name="projectPath">Полный путь к .csproj файлу.</param>
        public ProjectMsBuildLoader(string projectPath)
        {
            _projectPath = projectPath;

            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }

        /// <summary>
        /// Загружает и собирает проект, используя MSBuild API. Возвращает true при успешной сборке.
        /// </summary>
        /// <returns>true, если сборка успешна; иначе — false.</returns>
        public bool LoadAndBuildProject()
        {
            try
            {
                var projectCollection = new ProjectCollection();
                _project = new Project(_projectPath, null, null, projectCollection);

                var buildRequest = new BuildRequestData(_project.CreateProjectInstance(), new[] { "Build" });

                var parameters = new BuildParameters(projectCollection)
                {
                    Loggers = CustomLogger != null ? new List<ILogger> { CustomLogger } : null,
                    EnableNodeReuse = true,
                    UseSynchronousLogging = false
                };

                var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);
                return result.OverallResult == BuildResultCode.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сборки проекта: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Возвращает список .axaml и .cs файлов, входящих в проект.
        /// </summary>
        /// <returns>Список ProjectFileInfo.</returns>
        public List<ProjectFileInfo> GetProjectFiles()
        {
            if (_project == null)
                throw new InvalidOperationException("Проект не загружен. Вызовите LoadAndBuildProject сначала.");

            var result = new List<ProjectFileInfo>();

            foreach (var item in _project.Items)
            {
                var path = item.EvaluatedInclude;

                if (path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    var fullPath = Path.Combine(_project.DirectoryPath, path);
                    result.Add(new ProjectFileInfo
                    {
                        ItemType = item.ItemType,
                        RelativePath = path,
                        FullPath = Path.GetFullPath(fullPath)
                    });
                }
            }

            return result;
        }
    }
}
