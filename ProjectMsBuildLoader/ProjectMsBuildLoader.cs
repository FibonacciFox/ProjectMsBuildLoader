using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.Build.Evaluation;

namespace ProjectMsBuildLoader
{
    public class ProjectFileInfo
    {
        public string ItemType { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public string FullPath { get; set; } = "";
    }

    public class ProjectMsBuildLoader
    {
        private readonly string _projectPath;
        private Project? _project;

        public string ProjectPath => _projectPath;

        public ProjectMsBuildLoader(string projectPath)
        {
            _projectPath = projectPath;

            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }

        public bool LoadAndBuildProject()
        {
            try
            {
                _project = new Project(_projectPath);
                bool buildSuccess = _project.Build();
                return buildSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки или сборки проекта: {ex.Message}");
                return false;
            }
        }

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
