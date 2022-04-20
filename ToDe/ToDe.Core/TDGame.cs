﻿using Microsoft.Xna.Framework;
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
        List<Dira> diry;
        List<PolozkaNabidky> nabidka;
        List<Vez> veze;
        protected override void Initialize()
        {
            nepratele = new List<Nepritel>();
            diry = new List<Dira>();
            nabidka = new List<PolozkaNabidky>();
            veze = new List<Vez>();

            base.Initialize();
        }

        ushort pocetVeziKluomet, pocetVeziRaketa;
        float zivotu;
        PolozkaNabidky textPocetVeziKluomet, textPocetVeziRaketa, textZivotu;
        Mapa aktualniMapa;
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Mapa.Textura = Content.Load<Texture2D>(@"Sprites/Basic");
            Mapa.Pismo = Content.Load<SpriteFont>(@"Fonts/Pismo");

            aktualniMapa = Mapa.NactiMapu(1);

            zivotu = 1;
            pocetVeziKluomet = 10;
            pocetVeziRaketa = 5;

            nabidka.Add(new PolozkaNabidky(0, TypPolozkyNabidky.VezKulomet));
            nabidka.Add(textPocetVeziKluomet = new PolozkaNabidky(1, TypPolozkyNabidky.Text) { Text = pocetVeziKluomet.ToString() });
            nabidka.Add(new PolozkaNabidky(3, TypPolozkyNabidky.VezRaketa));
            nabidka.Add(textPocetVeziRaketa = new PolozkaNabidky(4, TypPolozkyNabidky.Text) { Text = pocetVeziRaketa.ToString() });
            nabidka.Add(textZivotu = new PolozkaNabidky(-1, TypPolozkyNabidky.Text) { Text = (zivotu*100) + "%" });

            nepratele.Add(new Nepritel());
            diry.Add(new Dira()
            {
                Pozice = new Vector2(3 * 128 + 64, 4 * 128 + 64),
                RychlostMizeni = 0.075f,
            }); ;


            base.LoadContent();
        }

        bool byloKliknutoMinule = false;
        protected override void Update(GameTime gameTime)
        {
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

            // Umístění věže
            if (poziceKliknuti != Vector2.Zero && 
                poziceKliknuti.X < Mapa.Aktualni.Sloupcu * Mapa.VelikostDlazdice &&
                poziceKliknuti.Y < Mapa.Aktualni.Radku * Mapa.VelikostDlazdice)
            {
                Point souradniceNaMape = new Point(
                        (int)poziceKliknuti.X / Mapa.VelikostDlazdice,
                        (int)poziceKliknuti.Y / Mapa.VelikostDlazdice
                    );
                if (Mapa.Aktualni.Pozadi[souradniceNaMape.Y, souradniceNaMape.X] == TypDlazdice.Ground &&
                    !veze.Any(x => x.SouradniceNaMape == souradniceNaMape))
                {
                    var typ = nabidka.FirstOrDefault(x => x.Oznacen)?.TypPolozky;

                    if (typ == TypPolozkyNabidky.VezKulomet && pocetVeziKluomet > 0)
                    {
                        veze.Add((new VezKulomet()).UmistiVez(souradniceNaMape));
                        pocetVeziKluomet--;
                        textPocetVeziKluomet.Text = pocetVeziKluomet.ToString();
                    }
                    else if (typ == TypPolozkyNabidky.VezRaketa && pocetVeziRaketa > 0)
                    {
                        veze.Add((new VezRaketa()).UmistiVez(souradniceNaMape));
                        pocetVeziRaketa--;
                        textPocetVeziRaketa.Text = pocetVeziRaketa.ToString();
                    }
                }
            }

            // Aktualizace herních objektů
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            nepratele.ForEach(x => x.Update(seconds));
            diry.ForEach(x => x.Update(seconds));
            veze.ForEach(x => x.Update(seconds));
            nabidka.ForEach(x => x.Update(seconds, poziceKliknuti));

            // Odstraňování smazaných objektů ze seznamů
            nepratele.RemoveAll(x => x.Smazat);
            diry.RemoveAll(x => x.Smazat);
            base.Update(gameTime);
        }

        float globalniMeritko = 1;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black); // Barva podkladu na pozadí

            // Matice pro měřítko (zoom) vykreslování, aby se vše vešlo do okna
            var scaleX = (float)GraphicsDevice.Viewport.Width / (float)(aktualniMapa.Sloupcu* Mapa.VelikostDlazdice);
            var scaleY = (float)GraphicsDevice.Viewport.Height / (float)((aktualniMapa.Radku + 1) * Mapa.VelikostDlazdice);
            globalniMeritko = MathHelper.Min(scaleX, scaleY);
            var screenScale = new Vector3(new Vector2(globalniMeritko), 1.0f);
            var scaleMatrix = Matrix.CreateScale(screenScale);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, transformMatrix: scaleMatrix); // Začátek vykreslování


            // Vykreslení pozadí mapy
            for (int i = 0; i < aktualniMapa.Radku; i++)
                for (int j = 0; j < aktualniMapa.Sloupcu; j++)
                {
                    spriteBatch.Kresli(aktualniMapa.CilDlazdice(i, j), aktualniMapa.MezeDlazdice(i, j), Vector2.Zero);
                }

            // Vykreslování seznamů
            nepratele.ForEach(x => x.Draw(spriteBatch));
            diry.ForEach(x => x.Draw(spriteBatch));
            veze.ForEach(x => x.Draw(spriteBatch));
            nabidka.ForEach(x => x.Draw(spriteBatch));
            nabidka.FirstOrDefault().DrawVyber(spriteBatch); // Vykreslení označovacího rámečku v nabídce

            // Konec vykreslování
            spriteBatch.End(); 
            base.Draw(gameTime);
        }
    }
}
