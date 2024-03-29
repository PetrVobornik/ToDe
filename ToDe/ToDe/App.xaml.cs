﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ToDe
{
    public delegate Task<Stream> OpenImportedFileDelegate();

    public partial class App : Application
    {
        // #GM
        //public class PrepniHruEventArgs : EventArgs
        //{
        //    public bool UplnyKonec { get; set; } // TRUE = Hra se zcela vypíná, FALSE = hra se jen přepíná (třeba na její nastavení), ale bude se do ní vracet
        //}
        //public delegate void PrepniHruEventHandler(object sender, PrepniHruEventArgs e);

        public event EventHandler PrepnoutNaHru; // XF volá na nativy, aby se přeply na hru
        //public event PrepniHruEventHandler PrepnoutNaXF; // Volá nativ, že ukončuje hru a vrací řízení zpět XF

        internal void SpustPrepnoutNaHru() => PrepnoutNaHru?.Invoke(this, EventArgs.Empty); // Spouští XF

        //public void SpustPrepnoutNaXF(bool uplnyKonec)
        //    => PrepnoutNaXF?.Invoke(this, new PrepniHruEventArgs() { UplnyKonec = uplnyKonec }); // Spouští nativy


        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        public App(string todeFileName, OpenImportedFileDelegate todeOpen)
        {
            InitializeComponent();

            var mainPage = new MainPage();
            mainPage.TodeImportedFileName = todeFileName;
            mainPage.OpenImportedFile = todeOpen;
            MainPage = new NavigationPage(mainPage);
        }


        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
