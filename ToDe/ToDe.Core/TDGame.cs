﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToDe
{
    public class TDGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public TDGame()
        {
            IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this);
        }

        List<Nepritel> nepratele;
        List<MizejiciObjekt> mizejiciObjekty;
        List<Vez> veze;
        List<Raketa> rakety;
        List<Exploze> exploze;
        List<Prekazka> prekazky;
        OvladaciPanel ovladaciPanel;
        protected override void Initialize()
        {
            nepratele = new List<Nepritel>();
            mizejiciObjekty = new List<MizejiciObjekt>();
            veze = new List<Vez>();
            rakety = new List<Raketa>();
            exploze = new List<Exploze>();
            prekazky = new List<Prekazka>();

            base.Initialize();
        }

        float zdravi;
        Zdroje aktualniMapa;
        HerniObjekt vybranyObjekt;
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Načtení zdrojů
            Zdroje.Obsah = new Obsah() {
                Pismo = Content.Load<SpriteFont>(@"Fonts/Pismo"),
                ZvukKulomet = new Zvuk(Content.Load<SoundEffect>(@"Sounds/vez_kulomet"), 2),
                ZvukRaketaStart = new Zvuk(Content.Load<SoundEffect>(@"Sounds/raketa_start"), 3, 0.75f),
                ZvukRaketaDopad = new Zvuk(Content.Load<SoundEffect>(@"Sounds/raketa_dopad"), 3, 0.5f),
                ZvukKonecVyhra = new Zvuk(Content.Load<SoundEffect>(@"Sounds/konec_vyhra"), 1),
                ZvukKonecProhra = new Zvuk(Content.Load<SoundEffect>(@"Sounds/konec_prohra"), 1),
            };
            Zdroje.Obsah.Zakladni.Grafika = Content.Load<Texture2D>(@"Sprites/textura-vyber");
            Zdroje.Obsah.Exploze.Grafika = Content.Load<Texture2D>(@"Sprites/exploze");
            Zdroje.Obsah.Ukazatel.Grafika = Content.Load<Texture2D>(@"Sprites/ukazatel");

            SpustitHru();

            ovladaciPanel = new OvladaciPanel();

            NastavTexty(true);

            base.LoadContent();
        }

        private void SpustitHru()
        {
            nepratele.Clear();
            mizejiciObjekty.Clear();
            veze.Clear();
            rakety.Clear();
            exploze.Clear();
            prekazky.Clear();
            vybranyObjekt = null;

            // Načtení levelu
            aktualniMapa = Zdroje.NactiLevel(ref Zdroje.CisloLevelu);

            // Přidání překážek na mapě
            foreach (var prekazka in aktualniMapa.Level.Mapa.Prekzaky)
                prekazky.Add(new Prekazka(prekazka.Znak, prekazka.Pozice));

            // Nastavení hry
            zdravi = 1;

            jeKonecHry = false;
            pauza = false;
        }

        void NastavTexty(bool novyLevel = false)
        {
            // Texty do nabídky
            int zbytekZivota = (int)Math.Round(zdravi * 100);
            if (zdravi < 0)
                zbytekZivota = 0;
            else if (zbytekZivota > 100)
                zbytekZivota = 100;
            else if (zbytekZivota == 0 && zdravi > 0)
                zbytekZivota = 1;

            //ovladaciPanel.TextFinance.Text = "$" + (Math.Floor(Zdroje.Aktualni.Level.Konto / 10.0) * 10).ToString();
            //ovladaciPanel.TextZivotu.Text = zbytekZivota + "%";
            ovladaciPanel.TextFinanceAZivotu.Text = String.Format("{0}%\n${1}", 
                zbytekZivota, Math.Floor(Zdroje.Aktualni.Level.Konto / 10.0) * 10);

            if (novyLevel)
            {
                //PolozkaNabidky.Vyber.Viditelny = false;
                ovladaciPanel.TextCenaVezeKluomet.Text = "$" + KonfiguraceVezKulomet.VychoziParametry.Cena.ToString();
                ovladaciPanel.TextCenaVezeRaketa.Text = "$" + KonfiguraceVezRaketa.VychoziParametry.Cena.ToString();
            }

            ovladaciPanel.TextCenaVezeKluomet.Barva = Zdroje.Aktualni.Level.Konto >= KonfiguraceVezKulomet.VychoziParametry.Cena 
                ? Color.Lime : Color.Red;
            ovladaciPanel.TextCenaVezeRaketa.Barva = Zdroje.Aktualni.Level.Konto >= KonfiguraceVezRaketa.VychoziParametry.Cena
                ? Color.Lime : Color.Red;

            if (vybranyObjekt?.GetType().IsSubclassOf(typeof(Vez)) == true)
            {
                float cena = Zdroje.Aktualni.Level.VezDleTypu(((Vez)vybranyObjekt).TypVeze).PrijemZaDemolici;
                ovladaciPanel.TextCenaDemolice.NastavTextCeny(cena);
                //ovladaciPanel.TextCenaDemolice.Text = (cena < 0 ? "-" : "+") + "$" + Math.Abs(cena).ToString();
                //ovladaciPanel.TextCenaDemolice.Barva = (cena < 0 && Zdroje.Aktualni.Level.Konto + cena < 0)
                //    ? Color.Red : Color.Lime;
            }
            else if (vybranyObjekt is Prekazka)
            {
                float cena = Zdroje.Aktualni.Level.CenikPrekazek[((Prekazka)vybranyObjekt).ZnakPrekazky];
                ovladaciPanel.TextCenaDemolice.NastavTextCeny(cena);
            }
        }

        void AkutalizovatPlanUtoku(double oKolik)
        {
            Zdroje.Aktualni.Level.PlanPosilaniVln.ForEach(x => x.Cas = x.Cas + (float)oKolik);
        }


        // -------------------------------------------- UPDATE ---------------------------------------------
        bool byloKliknutoMinule = false;
        bool pauza = false, jeKonecHry = false;
        double casPriPauznuti;
        protected override void Update(GameTime gameTime)
        {
            VypocetMeritka();

            // Kliknutí
            Vector2 poziceKliknuti = Vector2.Zero;
            // Dotyk
            poziceKliknuti = TouchPanel.GetState().FirstOrDefault(tl => tl.State == TouchLocationState.Pressed).Position;
            // Myš
            var ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed)
                poziceKliknuti = new Vector2(ms.X, ms.Y);
            poziceKliknuti /= globalniMeritko;
            // Jde o nový klik?
            bool kliknutiZahajeno = !byloKliknutoMinule && poziceKliknuti != Vector2.Zero;
            if (byloKliknutoMinule && poziceKliknuti != Vector2.Zero)
            {
                byloKliknutoMinule = true;
                poziceKliknuti = Vector2.Zero;
            }
            else
                byloKliknutoMinule = poziceKliknuti != Vector2.Zero;

            // Přepočet časů na sekundy
            float celkovyHerniCas = (float)gameTime.TotalGameTime.TotalSeconds;
            float casOdMinule = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Konec hry - čekání na klik pro zahájení nové hry
            if (zdravi <= 0) // Jsme mrtví...
            {
                // První detekce, že hra skončila naší prohrou
                if (!jeKonecHry)
                {
                    jeKonecHry = true;
                    //Zdroje.CisloLevelu = 1; // Až kliknem a zahájíme novou hru, tak začne od prvního kola
                    Zdroje.Obsah.ZvukKonecProhra.HrajZvuk(celkovyHerniCas);
                }

                // Kliknutí při konci hry = zahaj novou hru
                if (poziceKliknuti != Vector2.Zero)
                {
                    SpustitHru();
                    NastavTexty(true);
                    AkutalizovatPlanUtoku(gameTime.TotalGameTime.TotalSeconds);
                }
                base.Update(gameTime);
                return;
            }
            // Konec hry výhrou
            else if (nepratele.Count == 0 && Zdroje.Aktualni.Level.PlanPosilaniVln.Count == 0) 
            {
                // První detekce, že hra skončila naší výhrou
                if (!jeKonecHry)
                {
                    jeKonecHry = true;
                    Zdroje.CisloLevelu++;
                    Zdroje.Obsah.ZvukKonecVyhra.HrajZvuk(celkovyHerniCas);
                }
                // Kliknutí na oznámení výhry = jde se na další kolo
                if (poziceKliknuti != Vector2.Zero)
                {
                    SpustitHru();
                    NastavTexty(true);
                    AkutalizovatPlanUtoku(gameTime.TotalGameTime.TotalSeconds);
                }
            }

            // Aktualizace nabídky (ještě před pauzou, aby se dala odpauzovat)
            ovladaciPanel.Update(casOdMinule, poziceKliknuti);

            // Pauzování (zapnout/vyponout pauzu)
            var oznacenaPolozkaNabidky = ovladaciPanel.KliknutoNa?.TypPolozky;
            if (oznacenaPolozkaNabidky == TypPolozkyNabidky.Pauza)
            {
                if (!pauza)
                {
                    pauza = true;
                    casPriPauznuti = gameTime.TotalGameTime.TotalSeconds;
                    ovladaciPanel.Pauza.Dlazdice[0] = new DlazdiceUrceni(ZakladniDlazdice.Nabidka_Pauza_Konec, ovladaciPanel.Pauza.Dlazdice[0].Z, false);
                }
                else 
                {
                    pauza = false;
                    // Přepočítat plán útoku
                    AkutalizovatPlanUtoku(gameTime.TotalGameTime.TotalSeconds - casPriPauznuti);
                    ovladaciPanel.Pauza.Dlazdice[0] = new DlazdiceUrceni(ZakladniDlazdice.Nabidka_Pauza, ovladaciPanel.Pauza.Dlazdice[0].Z, false);
                }
            }

            if (pauza) // Pokud je pauza, čas od minule dáme na 0, tzn. nic se nebude hýbat
                casOdMinule = 0;

            // Navýšení konta o finance, které nám přibývají za sekundu
            Zdroje.Aktualni.Level.Konto += Zdroje.Aktualni.Level.RychlostBohatnuti * casOdMinule;

            // Výběr objektu na mapě 
            if (poziceKliknuti != Vector2.Zero &&
                poziceKliknuti.X < Zdroje.Aktualni.Level.Mapa.Sloupcu * Zdroje.VelikostDlazdice &&
                poziceKliknuti.Y < Zdroje.Aktualni.Level.Mapa.Radku * Zdroje.VelikostDlazdice) // Kliknuto do prostoru mapy
            {
                Point souradniceNaMape = new Point(
                        (int)poziceKliknuti.X / Zdroje.VelikostDlazdice,
                        (int)poziceKliknuti.Y / Zdroje.VelikostDlazdice
                    );
                if (Zdroje.Aktualni.Level.Mapa.Pozadi[souradniceNaMape.Y, souradniceNaMape.X] == TypDlazdice.Plocha) // Kliklo se mimo silnici 
                {
                    // Kliklo se na věž?
                    HerniObjekt kliknutyObjekt = veze.FirstOrDefault(x => x.SouradniceNaMape == souradniceNaMape);
                    // Nebo se kliklo na překážku?
                    if (kliknutyObjekt == null)
                        kliknutyObjekt = prekazky.FirstOrDefault(x => x.SouradniceNaMape == souradniceNaMape);
                    // Kliknuto na dlaždici na mapě, na které nic dalšího není
                    if (kliknutyObjekt == null) 
                        kliknutyObjekt = new VybranaDlazdice(souradniceNaMape);

                    if (vybranyObjekt != null && (vybranyObjekt == kliknutyObjekt ||
                        (vybranyObjekt is VybranaDlazdice && kliknutyObjekt is VybranaDlazdice &&
                            ((VybranaDlazdice)vybranyObjekt).PoziceNaMape == ((VybranaDlazdice)kliknutyObjekt).PoziceNaMape)))
                    {
                        // Kliknutí na objekt, který již byl vybrán před tím
                        if (!(vybranyObjekt is VybranaDlazdice))
                            vybranyObjekt.ObjektJeVybran = false;
                        vybranyObjekt = null;
                    }
                    else
                    {
                        // Kliknutí na nový objekt
                        if (vybranyObjekt != null)
                            vybranyObjekt.ObjektJeVybran = false;
                        vybranyObjekt = kliknutyObjekt;
                        vybranyObjekt.ObjektJeVybran = true;
                    }
                }
            }

            // Kliknutí na něco v ovládacím panelu
            if (ovladaciPanel.KliknutoNa != null)
            {
                // Postavení věže
                if (vybranyObjekt is VybranaDlazdice)
                {
                    var typ = ovladaciPanel.KliknutoNa.TypPolozky; // Vybraný typ věže ve spodní nabídce

                    // Umístnění kulometné věže
                    if (typ == TypPolozkyNabidky.VezKulomet &&
                        KonfiguraceVezKulomet.VychoziParametry.Cena <= Zdroje.Aktualni.Level.Konto)
                    {
                        veze.Add((new VezKulomet()).UmistiVez(((VybranaDlazdice)vybranyObjekt).PoziceNaMape));
                        Zdroje.Aktualni.Level.Konto -= KonfiguraceVezKulomet.VychoziParametry.Cena;
                        vybranyObjekt = null;
                    }
                    // Umístění raketové věže
                    else if (typ == TypPolozkyNabidky.VezRaketa &&
                                KonfiguraceVezRaketa.VychoziParametry.Cena <= Zdroje.Aktualni.Level.Konto)
                    {
                        veze.Add((new VezRaketa()).UmistiVez(((VybranaDlazdice)vybranyObjekt).PoziceNaMape));
                        Zdroje.Aktualni.Level.Konto -= KonfiguraceVezRaketa.VychoziParametry.Cena;
                        vybranyObjekt = null;
                    }
                }

                // Zboření
                if (ovladaciPanel.KliknutoNa?.TypPolozky == TypPolozkyNabidky.Vymazat && vybranyObjekt != null)
                {
                    // Zboření věže
                    if (vybranyObjekt?.GetType().IsSubclassOf(typeof(Vez)) == true)
                    {
                        veze.Remove(vybranyObjekt as Vez);
                        var typVeze = ((Vez)vybranyObjekt).TypVeze;
                        Zdroje.Aktualni.Level.Konto += Zdroje.Aktualni.Level.VezDleTypu(typVeze)?.PrijemZaDemolici ?? 0;
                        vybranyObjekt = null;
                    }
                    // Zboření překážky
                    else if (vybranyObjekt is Prekazka)
                    {
                        prekazky.Remove(vybranyObjekt as Prekazka);
                        Zdroje.Aktualni.Level.Konto += Zdroje.Aktualni.Level.CenikPrekazek[((Prekazka)vybranyObjekt).ZnakPrekazky];
                        vybranyObjekt = null;
                    }

                }
            }

            // Přepnutí nabídky dle typu vybraného objektu na mapě
            if (vybranyObjekt == null)
                ovladaciPanel.PrepniNabidku(TypNabidky.Zakladni);
            else if (vybranyObjekt is VybranaDlazdice)
                ovladaciPanel.PrepniNabidku(TypNabidky.ProDlazdici);
            else if (vybranyObjekt.GetType().IsSubclassOf(typeof(Vez)))
                ovladaciPanel.PrepniNabidku(TypNabidky.ProVez);
            else if (vybranyObjekt is Prekazka)
                ovladaciPanel.PrepniNabidku(TypNabidky.ProPrekazku);


            // Aktualizace ostatních herních objektů
            nepratele.ForEach(x => x.Update(casOdMinule));
            mizejiciObjekty.ForEach(x => x.Update(casOdMinule));
            prekazky.ForEach(x => x.Update(casOdMinule));

            // Otáčení (update) věží (řeší se zvlášť)
            nepratele = nepratele.OrderByDescending(x => x.VzdalenostNaCeste).ToList(); // Seřazení seznamu nepřátel, aby ti nejvíce vepředu byli první
            foreach (var vez in veze)
            {
                vez.Cil = nepratele.FirstOrDefault(x => Vector2.Distance(vez.Pozice, x.Pozice) < vez.DosahStrelby); // Vybrat cíl věži, na který dostane a je nejvíce vepředu
                vez.Update(casOdMinule);
                // Výstřel
                if (vez.Strelba)
                {
                    if (vez is VezKulomet)
                    {
                        Zdroje.Obsah.ZvukKulomet.HrajZvuk(celkovyHerniCas);
                        mizejiciObjekty.Add(new PlamenStrelby((VezKulomet)vez));
                    }
                    else if (vez is VezRaketa)
                    {
                        Zdroje.Obsah.ZvukRaketaStart.HrajZvuk(celkovyHerniCas);
                        rakety.Add(new Raketa((VezRaketa)vez));
                    }
                }
            }

            // Rakety
            foreach (var raketa in rakety)
            {
                raketa.Update(casOdMinule);
                // Dopad (výbuch) rakety
                if (raketa.Dopad)
                {
                    Zdroje.Obsah.ZvukRaketaDopad.HrajZvuk(celkovyHerniCas);
                    mizejiciObjekty.Add(new Dira() { Pozice = raketa.Pozice }); // Vykreslit díru na cestě
                    exploze.Add(new Exploze(raketa));
                    // Ubrání života nepřátelům v okolí
                    var cile = nepratele
                        .Select(x => new { Vzdalenost = Vector2.Distance(raketa.Pozice, x.Pozice), Nepritel = x })
                        .Where(x => x.Vzdalenost < raketa.DosahExploze);
                    foreach (var cil in cile)
                        cil.Nepritel.Zdravi -= raketa.SilaStrely * (raketa.DosahExploze - cil.Vzdalenost) / raketa.DosahExploze;
                }
            }
            exploze.ForEach(x => x.Update(casOdMinule)); // Aktualizace explozí

            // Vypouštění nepřátel (vlny)
            if (Zdroje.Aktualni.Level.PlanPosilaniVln.Count > 0)
            {
                if (Zdroje.Aktualni.Level.PlanPosilaniVln[0].Cas <= celkovyHerniCas && !pauza)
                {
                    nepratele.Add(new Nepritel(Zdroje.Aktualni.Level.PlanPosilaniVln[0].Jednotka));
                    Zdroje.Aktualni.Level.PlanPosilaniVln.RemoveAt(0); // Už byl vyslán, smazat z plánu
                }
            }

            // Kontrola Vojáků, kteří došli do cíle
            foreach (var nepritel in nepratele.Where(x => x.DosahlCile))
            {
                zdravi = Math.Max(zdravi - nepritel.SilaUtoku * nepritel.ProcentoZdravi, 0); // Ubrat hráči zdraví
            }
                
            // Aktualizace textů v nabídce
            NastavTexty();                


            // Odstraňování smazaných objektů ze seznamů
            nepratele.RemoveAll(x => x.Smazat);
            mizejiciObjekty.RemoveAll(x => x.Smazat);
            rakety.RemoveAll(x => x.Smazat);
            exploze.RemoveAll(x => x.Smazat);
            
            base.Update(gameTime);
        }

        float globalniMeritko = 1; // Uložení měřítka grafiky, pro přepočet pozice kliknutí
        void VypocetMeritka()
        {
            Point rozmery;
            if (Window?.ClientBounds != null)
            {
                rozmery = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height); // TODO: otestovat jestli to vrací správné rozměry i na ANdoridu
                if (GraphicsDevice.Viewport.Width != rozmery.X || GraphicsDevice.Viewport.Height != rozmery.Y)
                {
                    GraphicsDevice.Viewport = new Viewport(0, 0, rozmery.X, rozmery.Y);
                    //GraphicsDevice.PresentationParameters.Bounds = Window.ClientBounds; TODO: dořešit spuštění v režimu na výšku
                }
            }
            else
                rozmery = new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // Matice pro měřítko (zoom) vykreslování, aby se vše vešlo do okna
            var scaleX = rozmery.X / (float)(aktualniMapa.Level.Mapa.Sloupcu * Zdroje.VelikostDlazdice);
            var scaleY = rozmery.Y / (float)((aktualniMapa.Level.Mapa.Radku + 1) * Zdroje.VelikostDlazdice);
            globalniMeritko = MathHelper.Min(scaleX, scaleY); // Použijeme měřítko, aby se vše vždy vešlo do obrazovky

            // Zkusit totéž vypočítat, pro transponovanou mapu, nebude-li to náhodou lepší
            scaleX = rozmery.Y / (float)((aktualniMapa.Level.Mapa.Sloupcu + 1) * Zdroje.VelikostDlazdice);
            scaleY = rozmery.X / (float)(aktualniMapa.Level.Mapa.Radku * Zdroje.VelikostDlazdice);
            float globalniMeritko2 = MathHelper.Min(scaleX, scaleY);

            // Výběr lepší varianty
            if (globalniMeritko2 > globalniMeritko)
            {
                globalniMeritko = globalniMeritko2;
                aktualniMapa.Level.Mapa.TranspoziceMapy();
                nepratele.ForEach(x => x.TranspozicePozice());
                mizejiciObjekty.ForEach(x => x.TranspozicePozice());
                veze.ForEach(x => x.TranspozicePozice());
                rakety.ForEach(x => x.TranspozicePozice());
                exploze.ForEach(x => x.TranspozicePozice());
                prekazky.ForEach(x => x.TranspozicePozice());
                ovladaciPanel.TranspozicePozice();
                if (vybranyObjekt is VybranaDlazdice)
                    vybranyObjekt.TranspozicePozice();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black); // Barva podkladu na pozadí

            // Matice pro měřítko (zoom) vykreslování, aby se vše vešlo do okna
            var screenScale = new Vector3(new Vector2(globalniMeritko), 1.0f);
            var scaleMatrix = Matrix.CreateScale(screenScale);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, transformMatrix: scaleMatrix); // Začátek vykreslování

            // Vykreslení pozadí mapy
            for (int i = 0; i < aktualniMapa.Level.Mapa.Radku; i++)
                for (int j = 0; j < aktualniMapa.Level.Mapa.Sloupcu; j++)
                    spriteBatch.Kresli(aktualniMapa.Level.Mapa.CilDlazdice(i, j), 
                                       aktualniMapa.Level.Mapa.MezeDlazdice(i, j), Vector2.Zero, 
                                       barva: (vybranyObjekt is VybranaDlazdice && 
                                               ((VybranaDlazdice)vybranyObjekt).PoziceNaMape.Y == i &&
                                               ((VybranaDlazdice)vybranyObjekt).PoziceNaMape.X == j)
                                               ? Color.LightSalmon : (Color?)null);

            // Vykreslování seznamů herních objektů
            nepratele.ForEach(x => x.Draw(spriteBatch));
            mizejiciObjekty.ForEach(x => x.Draw(spriteBatch));
            veze.ForEach(x => x.Draw(spriteBatch));
            rakety.ForEach(x => x.Draw(spriteBatch));
            exploze.ForEach(x => x.Draw(spriteBatch));
            prekazky.ForEach(x => x.Draw(spriteBatch));
            ovladaciPanel.Draw(spriteBatch);

            // Game Over
            if (zdravi <= 0) // Prohra
            {
                spriteBatch.KresliTextDoprostred("GAME OVER");
            } 
            else if (jeKonecHry) // Výhra
            {
                spriteBatch.KresliTextDoprostred("VICTORY");
            }

            // Konec vykreslování
            spriteBatch.End(); 
            base.Draw(gameTime);
        }
    }
}
