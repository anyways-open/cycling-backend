//The MIT License (MIT)

//Copyright (c) 2017 SharpSoftware

//Permission is hereby granted, free of charge, to any person obtaining a copy 
//of this software and associated documentation files (the "Software"), to deal 
//in the Software without restriction, including without limitation the rights 
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
//copies of the Software, and to permit persons to whom the Software is 
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included 
//in all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
//THE SOFTWARE.

using System;
using System.IO;
using System.Threading;
using Serilog;

namespace rideaway_backend.FileMonitoring
{
    /// <summary>
    /// Represents a file monitor that continually monitors a file for changes.
    /// </summary>
    public class FileMonitor
    {
        private readonly FileInfo _fileInfo; // Holds the fileinfo of the file to monitor.
        private readonly Timer _timer; // Holds the timer to poll file changes.


        /// <summary>
        /// An action to report that the file has changed and is accessible.
        /// </summary>
        private readonly Action _onFileChanged;

        private DateTime _lastChangeTime;
        private readonly object _sync = new object(); // Holds an object that is used to sync the timer.


        /// <summary>
        /// Creates a new file monitor for the given file.
        /// </summary>
        /// <param name="path"></param>
        public FileMonitor(string path, TimeSpan renewEvery, Action onChange)
        {
            _fileInfo = new FileInfo(path);
            _lastChangeTime = _fileInfo.LastWriteTime;

            _timer = new Timer(Tick, null, renewEvery, renewEvery);
            _onFileChanged = onChange;
            Log.Information($"Started monitoring {path}");
        }

        /// <summary>
        /// Stops monitoring.
        /// </summary>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }


        /// <summary>
        /// Called every _timeout_ milliseconds.
        /// Here we check if something has changed.
        /// If a change occured, we call the callback.
        /// We do _not_ call the callback if the file was deleted; we assume that this is part of the update process
        /// </summary>
        private void Tick(object _)
        {
            lock (_sync)
            {
                _fileInfo.Refresh();

                if (!_fileInfo.Exists) return;
                if (!IsAvailable(_fileInfo))
                {
                    Log.Verbose(
                        $"File {_fileInfo.Name} is currently locked, so it might be changed. We'll recheck on the next tick");
                    return; // Nevermind, we'll come back later
                }

                var lastChange = _fileInfo.LastWriteTime;

                // ReSharper disable once InvertIf
                if (!Equals(lastChange, _lastChangeTime))
                {
                    Log.Information($"File {_fileInfo.Name} has changed; calling callback.");
                    try
                    {
                        _onFileChanged();
                        _lastChangeTime = lastChange;
                    }
                    catch (Exception e)
                    {
                        Log.Error(
                            $"Caught an exception in the callback of a filemonitor: {e.Message}. We're gonna retry next tick");
                        Log.Error(e.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a file is locked, e.g. because another process is writing to it
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool IsAvailable(FileInfo file)
        {
            try
            {
                using (var stream = file.Open(FileMode.Open, FileAccess.Write))
                {
                    return true;
                }
            }
            catch
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            }
        }
    }
}