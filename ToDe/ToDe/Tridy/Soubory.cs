﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ToDe
{
    internal static class Soubory
    {
        static string slozkaLevelu;
        public static string SlozkaLevelu
        {
            get
            {
                if (String.IsNullOrEmpty(slozkaLevelu))
                {
                    slozkaLevelu = Path.Combine(FileSystem.AppDataDirectory, "Levels");
                    if (!Directory.Exists(slozkaLevelu))
                        Directory.CreateDirectory(slozkaLevelu);
                }
                return slozkaLevelu;
            }
        }

        public static string CestaSouboruLevelu(string nazevLevelu)
            => Path.Combine(SlozkaLevelu, nazevLevelu + ".xml");

        public static string[] SeznamLevelu()
            => Directory.GetFiles(SlozkaLevelu).Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();


        public static async Task<string> ZeptejSeNaNovyNazevSoboru(Page strnaka, 
            string titulekDialogu, string vychoziNazev = "")
        {
            string nazev = await strnaka.DisplayPromptAsync(titulekDialogu, "Zadejte název pro nový level", "OK", "Zrušit", "Nový level", 120, Keyboard.Plain, vychoziNazev);
            if (String.IsNullOrEmpty(nazev))
                return String.Empty;
            string soubor = Soubory.CestaSouboruLevelu(nazev);
            if (File.Exists(soubor))
                if (!(await strnaka.DisplayAlert(titulekDialogu,
                    String.Format("Level s názvem '{0}' již existuje. Přejete si jej přepsat?", nazev),
                    "Přepsat", "Zrušit")))
                return String.Empty;
            return nazev;
        }
    }
}
