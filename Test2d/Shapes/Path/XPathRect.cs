﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Test2d
{
    /// <summary>
    /// 
    /// </summary>
    public class XPathRect
    {
        /// <summary>
        /// 
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static XPathRect Create(
            double x = 0.0,
            double y = 0.0,
            double width = 0.0,
            double height = 0.0)
        {
            return new XPathRect()
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
            };
        }
    }
}
