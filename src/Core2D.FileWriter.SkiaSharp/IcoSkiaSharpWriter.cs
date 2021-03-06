﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Core2D.Interfaces;
using Core2D.Containers;
using Core2D.Renderer;
using Core2D.Renderer.Presenters;
using Core2D.Shape;
using Core2D.Renderer.SkiaSharp;

namespace Core2D.FileWriter.SkiaSharpIco
{
    /// <summary>
    /// SkiaSharp ico <see cref="IFileWriter"/> implementation.
    /// </summary>
    public sealed class IcoSkiaSharpWriter : IFileWriter
    {
        /// <inheritdoc/>
        string IFileWriter.Name { get; } = "Ico (SkiaSharp)";

        /// <inheritdoc/>
        string IFileWriter.Extension { get; } = "ico";

        /// <inheritdoc/>
        void IFileWriter.Save(string path, object item, object options)
        {
            if (string.IsNullOrEmpty(path) || item == null)
                return;

            var ic = options as IImageCache;
            if (options == null)
                return;

            var renderer = new SkiaSharpRenderer(true, 96.0);
            renderer.State.DrawShapeState.Flags = ShapeStateFlags.Printable;
            renderer.State.ImageCache = ic;

            var presenter = new ExportPresenter();

            IProjectExporter exporter = new IcoSkiaSharpExporter(renderer, presenter);

            if (item is PageContainer)
            {
                exporter.Save(path, item as PageContainer);
            }
            else if (item is DocumentContainer)
            {
                throw new NotSupportedException("Saving documents as ico drawing is not supported.");
            }
            else if (item is ProjectContainer)
            {
                throw new NotSupportedException("Saving projects as ico drawing is not supported.");
            }
        }
    }
}
