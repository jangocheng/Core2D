﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Core2D.Avalonia.Behaviors
{
    public sealed class DropBehavior : Behavior<Control>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            DragDrop.SetAllowDrop(AssociatedObject, true);
            AssociatedObject.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AssociatedObject.AddHandler(DragDrop.DragOverEvent, DragOver);
            AssociatedObject.AddHandler(DragDrop.DropEvent, Drop);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            DragDrop.SetAllowDrop(AssociatedObject, false);
            AssociatedObject.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
            AssociatedObject.RemoveHandler(DragDrop.DragOverEvent, DragOver);
            AssociatedObject.RemoveHandler(DragDrop.DropEvent, Drop);
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

            //if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.FileNames))
            //    e.DragEffects = DragDropEffects.None;

            Console.WriteLine($"DragOver {sender}");
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

            //if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.FileNames))
            //    e.DragEffects = DragDropEffects.None;

            Console.WriteLine($"DragEnter {sender}");
        }

        private void Drop(object sender, DragEventArgs e)
        {
            Console.WriteLine($"Drop sender {sender}");
            Console.WriteLine($"Drop source {e.Source}");

            foreach (var format in e.Data.GetDataFormats())
            {
                Console.WriteLine($"Drop format: {format}");
                Console.WriteLine($"Drop data: {e.Data.Get(format)}");
            }

            if (e.Data.Contains(DataFormats.Text))
            {
                var text = e.Data.GetText();
                Console.WriteLine($"Drop text: {text}");
                Console.WriteLine(text);
            }

            if (e.Data.Contains(DataFormats.FileNames))
            {
                foreach (var file in e.Data.GetFileNames())
                {
                    Console.WriteLine($"Drop file: {file}");
                }
            }

            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
        }
    }
}