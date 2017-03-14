﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using Core2D.Editor;
using Core2D.Editor.Input;
using Core2D.Editor.Views.Interfaces;
using Core2D.Project;
using Core2D.Renderer;
using Core2D.Renderer.Presenters;
using Core2D.Shapes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Renderer.Win2D;
using Utilities.Uwp;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Core2D.Universal
{
    public sealed partial class MainPage : Page
    {
        private IContainer _componentContainer;
        private IServiceProvider _serviceProvider;
        private ContainerPresenter _presenter;
        private ProjectEditor _projectEditor;
        private InputProcessor _inputProcessor;

        public MainPage()
        {
            InitializeComponent();

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(MainPage).GetTypeInfo().Assembly);

            // View
            builder.RegisterAssemblyTypes(typeof(App).GetTypeInfo().Assembly).AssignableTo<ICommand>().AsImplementedInterfaces().AsSelf().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(App).GetTypeInfo().Assembly).As<IView>().InstancePerLifetimeScope();
            builder.Register(c => this).As<MainPage>().InstancePerLifetimeScope();

            _componentContainer = builder.Build();

            _serviceProvider = _componentContainer.Resolve<IServiceProvider>();
            _projectEditor = _serviceProvider.GetService<ProjectEditor>();

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            _projectEditor.CurrentTool = _projectEditor.Tools.FirstOrDefault(t => t.Name == "Selection");
            _projectEditor.CurrentPathTool = _projectEditor.PathTools.FirstOrDefault(t => t.Name == "Line");
            _projectEditor.OnNewProject();
            _projectEditor.Invalidate = () => canvas.Invalidate();

            _presenter = new EditorPresenter();

            _inputProcessor = new InputProcessor(
                new UwpInputSource(
                    canvas,
                    canvas,
                    FixPointOffset),
                _projectEditor);

            canvas.Draw += CanvasControl_Draw;

            DataContext = _projectEditor;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            canvas.Focus(FocusState.Programmatic);
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _projectEditor.Log?.Dispose();
            _componentContainer?.Dispose();
        }

        private async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var state = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            bool isControl = state.HasFlag(CoreVirtualKeyStates.Down);
            if (isControl)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.N:
                        NewProject();
                        break;
                    case VirtualKey.O:
                        await OpenProject();
                        break;
                    case VirtualKey.S:
                        await SaveProject();
                        break;
                    case VirtualKey.Z:
                        _projectEditor.OnUndo();
                        break;
                    case VirtualKey.Y:
                        _projectEditor.OnRedo();
                        break;
                    case VirtualKey.X:
                        _projectEditor.OnCut();
                        break;
                    case VirtualKey.C:
                        _projectEditor.OnCopy();
                        break;
                    case VirtualKey.V:
                        _projectEditor.OnPaste();
                        break;
                    case VirtualKey.A:
                        _projectEditor.OnSelectAll();
                        break;
                    case VirtualKey.G:
                        _projectEditor.OnGroupSelected();
                        break;
                    case VirtualKey.U:
                        _projectEditor.OnUngroupSelected();
                        break;
                }
            }
            else
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.N:
                        _projectEditor.OnToolNone();
                        break;
                    case VirtualKey.S:
                        _projectEditor.OnToolSelection();
                        break;
                    case VirtualKey.P:
                        _projectEditor.OnToolPoint();
                        break;
                    case VirtualKey.L:
                        _projectEditor.OnToolLine();
                        break;
                    case VirtualKey.A:
                        _projectEditor.OnToolArc();
                        break;
                    case VirtualKey.B:
                        _projectEditor.OnToolCubicBezier();
                        break;
                    case VirtualKey.Q:
                        _projectEditor.OnToolQuadraticBezier();
                        break;
                    case VirtualKey.H:
                        _projectEditor.OnToolPath();
                        break;
                    case VirtualKey.M:
                        _projectEditor.OnToolMove();
                        break;
                    case VirtualKey.R:
                        _projectEditor.OnToolRectangle();
                        break;
                    case VirtualKey.E:
                        _projectEditor.OnToolEllipse();
                        break;
                    case VirtualKey.T:
                        _projectEditor.OnToolText();
                        break;
                    case VirtualKey.I:
                        _projectEditor.OnToolImage();
                        break;
                    case VirtualKey.K:
                        _projectEditor.OnToggleDefaultIsStroked();
                        break;
                    case VirtualKey.F:
                        _projectEditor.OnToggleDefaultIsFilled();
                        break;
                    case VirtualKey.D:
                        _projectEditor.OnToggleDefaultIsClosed();
                        break;
                    case VirtualKey.J:
                        _projectEditor.OnToggleDefaultIsSmoothJoin();
                        break;
                    case VirtualKey.G:
                        _projectEditor.OnToggleSnapToGrid();
                        break;
                    case VirtualKey.C:
                        _projectEditor.OnToggleTryToConnect();
                        break;
                    case VirtualKey.Y:
                        _projectEditor.OnToggleCloneStyle();
                        break;
                    case VirtualKey.Delete:
                        _projectEditor.OnDeleteSelected();
                        break;
                    case VirtualKey.Escape:
                        _projectEditor.OnDeselectAll();
                        break;
                }
            }
        }

        public Point FixPointOffset(Point point)
        {
            var container = _projectEditor.Project.CurrentContainer;
            if (container != null)
            {
                double offsetX = (this.canvas.ActualWidth * this.canvas.DpiScale - container.Width) / 2.0;
                double offsetY = (this.canvas.ActualHeight * this.canvas.DpiScale - container.Height) / 2.0;
                return new Point(point.X - offsetX, point.Y - offsetY);
            }
            return point;
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            Draw(args.DrawingSession);
        }

        private void Draw(CanvasDrawingSession ds)
        {
            var renderer = _projectEditor.Renderers[0];
            var container = _projectEditor.Project.CurrentContainer;
            if (container != null)
            {
                ds.Antialiasing = CanvasAntialiasing.Aliased;
                ds.TextAntialiasing = CanvasTextAntialiasing.Auto;
                ds.Clear(Windows.UI.Colors.White);

                var t = Matrix3x2.CreateTranslation(0.0f, 0.0f);
                var s = Matrix3x2.CreateScale(1.0f);
                var old = ds.Transform;
                ds.Transform = s * t;

                double offsetX = (this.canvas.ActualWidth * ds.Transform.M11 - container.Width) / 2.0;
                double offsetY = (this.canvas.ActualHeight * ds.Transform.M22 - container.Height) / 2.0;
                _presenter?.Render(ds, _projectEditor.Renderers[0], container, offsetX, offsetY);

                ds.Transform = old;
            }
        }

        public async Task CacheImage(string key)
        {
            await (_projectEditor.Renderers[0] as Win2dRenderer).CacheImage(key, canvas);
        }

        public async Task CacheImages(XProject project)
        {
            var images = XProject.GetAllShapes<XImage>(project);
            if (images != null)
            {
                foreach (var image in images)
                {
                    await CacheImage(image.Key);
                }
            }
        }

        private async Task<IStorageFile> GetOpenProjectPathAsync()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".project");
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                return file;
            }
            return null;
        }

        private async Task<IStorageFile> GetSaveProjectPathAsync(string name)
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("Project", new List<string>() { ".project" });
            picker.SuggestedFileName = name;
            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                return file;
            }
            return null;
        }

        private void ContainersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _projectEditor.Invalidate();
        }

        private void NewProject()
        {
            _projectEditor.OnNewProject();
            _projectEditor.Invalidate();
        }

        public async Task OpenProject()
        {
            var file = await GetOpenProjectPathAsync();
            if (file != null)
            {
                var project = default(XProject);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    project = await Task.Run(() =>
                    {
                        return XProject.Open(stream, _projectEditor.FileIO, _projectEditor.JsonSerializer);
                    });
                }

                _projectEditor.OnOpen(project, file.Path);
                await CacheImages(project);
                _projectEditor.Invalidate();
            }

            await Task.Run(() => { });
        }

        public async Task SaveProject()
        {
            var file = await GetSaveProjectPathAsync(_projectEditor.Project.Name);
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    await Task.Run(() =>
                    {
                        XProject.Save(_projectEditor.Project, stream, _projectEditor.FileIO, _projectEditor.JsonSerializer);
                    });
                }

                await CachedFileManager.CompleteUpdatesAsync(file);
            }

            await Task.Run(() => { });
        }
    }
}
