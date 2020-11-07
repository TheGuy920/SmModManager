// Copyright © 2013 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using CefSharp;

namespace SmModManager.Core.Handlers
{

    public class DownloadHandler : IDownloadHandler
    {

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            OnBeforeDownloadFired?.Invoke(this, downloadItem);
            Debug.WriteLine("Download Triggered: " + downloadItem);
            if (App.Settings.NewFileName != null)
                downloadItem.SuggestedFileName = App.Settings.NewFileName;
            else
                Dispatcher.CurrentDispatcher.Invoke(() => { App.PageStore.PgDownloading(); });
            if (!callback.IsDisposed)
                using (callback)
                {
                    downloadItem.FullPath = Path.Combine(Constants.CachePath, downloadItem.SuggestedFileName);
                    Debug.WriteLine("Download Full Path: " + downloadItem.FullPath);
                    callback.Continue(downloadItem.FullPath, false);
                }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            OnDownloadUpdatedFired?.Invoke(this, downloadItem);
            if (downloadItem.IsComplete && App.Settings.NewFileName == null)
            {
                App.GetApp.NewModAdded(chromiumWebBrowser);
            }
            else if (downloadItem.IsComplete)
            {
                App.Settings.LatestDownloadComplete = true;
                App.Settings.NewFileName = null;
                App.Settings.Save();
            }
        }

        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

    }

}