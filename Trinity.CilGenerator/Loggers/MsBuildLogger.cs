﻿// LICENSE:
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// AUTHORS:
//
//  Moritz Eberl <moritz@semiodesk.com>
//  Sebastian Faubel <sebastian@semiodesk.com>
//
// Copyright (c) Semiodesk GmbH 2015

using Microsoft.Build.Utilities;

namespace Semiodesk.Trinity.CilGenerator.Loggers
{
  public class MsBuildLogger : ILogger
  {
    #region Members

    private readonly TaskLoggingHelper _helper;

    #endregion

    #region Constructors

    public MsBuildLogger(TaskLoggingHelper helper)
    {
      _helper = helper;
    }

    #endregion

    #region Methods

    public void LogMessage(string msg, params object[] args)
    {
      _helper.LogMessage(msg, args);
    }

    public void LogWarning(string msg, params object[] args)
    {
      _helper.LogWarning(msg, args);
    }

    public void LogError(string msg, params object[] args)
    {
      _helper.LogError(msg, args);
    }

    #endregion
  }
}
