//------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BackgroundRemovalBasics
{
    using System;
    using System.Windows;

    using System.ComponentModel.Composition.Primitives;
    using System.ComponentModel.Composition.Hosting;
    using Microsoft.Samples.Kinect.BackgroundRemovalBasics;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /* private AssemblyCatalog catalog;
         private MainWindow windowApp;
         public WaveFrom waveView;
         private bool disposed = false;

         /// <summary>
         /// Managed Entity Framework composition container used to compose the entity graph
         /// </summary>
         private CompositionContainer compositionContainer;

         protected virtual void Dispose(bool disposing)
         {
             if (!this.disposed)
             {
                 if (disposing)
                 {


                 }
             }

             this.disposed = true;
         }

         public void Dispose()
         {
             this.Dispose(true);
             GC.SuppressFinalize(this);
         }

         protected override void OnStartup(StartupEventArgs e)
         {
             base.OnStartup(e);


             // Catalog all exported parts within this assembly
             this.catalog = new AssemblyCatalog(typeof(App).Assembly);
             this.compositionContainer = new CompositionContainer(this.catalog);

             //waveView = new WaveFrom();
             //System.Windows.Forms.Application.EnableVisualStyles();
             //System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
             //waveView.Show();
             //waveView.OnReceiveSuccess += waveView_OnReceiveSuccess;
            
             windowApp = new MainWindow();
             windowApp.Show();
         }

         void waveView_OnReceiveSuccess(object sender, GetOutEventArgs e)
         {
             //throw new NotImplementedException();
             if (e.waveContent != null)
             {
                 windowApp.ReceiveSuccessWaveCode(e.waveContent);
             }
         }*/

      
    }
}
