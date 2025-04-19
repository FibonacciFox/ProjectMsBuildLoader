// MainWindow.axaml.cs

using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AvaloniaDllLoader;

public partial class MainWindow : Window
{
    private Assembly? _loadedAssembly;
    private string _projectPath = "/home/deck/Desktop/Projects/ProjectMsBuildLoader/ProjectMsBuildLoader/Demo/Demo.csproj";

    public MainWindow()
    {
        InitializeComponent();

        RefreshButton.Click += RefreshButton_Click;
        LoadButton.Click += LoadButton_Click;
    }

    private void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        ControlSelector.ItemsSource = null;
        RefreshControlsList();
    }

    private void LoadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_loadedAssembly == null || ControlSelector.SelectedItem is not string selectedType)
            return;

        var type = _loadedAssembly.GetType(selectedType);
        if (type == null)
            return;

        var instance = Activator.CreateInstance(type);

        switch (instance)
        {
            case Window window:
                window.Show();
                break;
            case Control control:
                PreviewHost.Content = control;
                break;
            default:
                Console.WriteLine($" Тип {selectedType} не является ни Window, ни Control.");
                break;
        }
    }

    private void RefreshControlsList()
    {
        var loader = new ProjectMsBuildLoader(_projectPath);

        if (!loader.LoadAndBuildProject())
        {
            Console.WriteLine(" Не удалось загрузить и собрать проект.");
            return;
        }

        var outputDll = Path.Combine(
            Path.GetDirectoryName(loader.ProjectPath)!,
            "bin", "Debug", "net9.0", "Demo.dll");

        if (!File.Exists(outputDll))
        {
            Console.WriteLine(" Сборка не найдена: " + outputDll);
            return;
        }

        _loadedAssembly = Assembly.LoadFrom(outputDll);

        var controls = _loadedAssembly.GetTypes()
            .Where(t => typeof(Control).IsAssignableFrom(t) && t.IsPublic && !t.IsAbstract)
            .Select(t => t.FullName)
            .Where(n => n != null)
            .ToList();

        ControlSelector.ItemsSource = controls;

        if (controls.Count > 0)
        {
            ControlSelector.SelectedIndex = 0;
        }
    }
}
