﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Reflection;
using Autofac;
using Core2D.Avalonia.Dock.Factories;
using Core2D.Avalonia.Windows;
using Dock.Avalonia.Dock;
using Dock.Model;

namespace Core2D.Avalonia.Modules
{
    /// <summary>
    /// View components module.
    /// </summary>
    public class ViewModule : Autofac.Module
    {
        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(App).GetTypeInfo().Assembly).As<IDock>().InstancePerLifetimeScope();
            builder.RegisterType<EditorDockFactory>().As<IDockFactory>().InstancePerDependency();
            builder.RegisterType<HostWindow>().As<IDockHost>().InstancePerDependency();
            builder.RegisterType<MainWindow>().As<MainWindow>().InstancePerLifetimeScope();
        }
    }
}
