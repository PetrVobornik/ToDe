﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ToDe
{
    internal class Nepritel : HerniObjekt
    {
        public float Zdravi { get; set; } = 1;
        public int IndexPoziceNaTrase { get; set; }
        public Vector2 SouradniceCile { get; set; }
        public float VzdalenostNaCeste { get; private set; }
        public float SilaUtoku { get; private set; }
        public bool DosahlCile { get; private set; } = false;

        public Nepritel(LevelVlnaJednotka jednotka)
        {
            Dlazdice = jednotka.Dlazdice(); // new[] { new DlazdiceUrceni(17, 10, 0.1f) };
            UhelKorkceObrazku = 0;

            Zdravi = jednotka.Zdravi;
            SilaUtoku = jednotka.Sila;
            RychlostPohybu = Zdroje.VelikostDlazdice * jednotka.Rychlost;

            RychlostRotace = 90 * RychlostPohybu / (Zdroje.VelikostDlazdice / 2); // Za dobu ujití poloviny dlaždice se otočí o 90°
            Pozice = Zdroje.Aktualni.Level.Mapa.PoziceNaTrase(0);
            SouradniceCile = Zdroje.Aktualni.Level.Mapa.PoziceNaTrase(1);
            IndexPoziceNaTrase = 0;
            UhelOtoceni = (int)Zdroje.Aktualni.Level.Mapa.SmerDalsiTrasy(Zdroje.Aktualni.Level.Mapa.TrasaPochodu[0], Zdroje.Aktualni.Level.Mapa.TrasaPochodu[1]);
        }

        public override void Update(float elapsedSeconds)
        {
            DosahlCile = false;

            if (Smazat) return;

            UhelOtoceni = TDUtils.OtacejSeKCili(elapsedSeconds, Pozice, SouradniceCile, UhelOtoceni, RychlostRotace, out _);
            Pozice += TDUtils.PosunPoUhlu(UhelOtoceni, RychlostPohybu * elapsedSeconds);

            // Dosažení cíle (další dlaždice)?
            var novaPozice = Pozice + TDUtils.PosunPoUhlu(UhelOtoceni, RychlostPohybu * elapsedSeconds);
            var vzdalenostDoCile = Vector2.Distance(novaPozice, SouradniceCile);
            //var vzdalenostDoCile = Vector2.Distance(Pozice, SouradniceCile);
            VzdalenostNaCeste = IndexPoziceNaTrase + vzdalenostDoCile / Zdroje.VelikostDlazdice; // Pozice pro účely řazení


            if (vzdalenostDoCile < 2f || vzdalenostDoCile > Vector2.Distance(Pozice, SouradniceCile))
            {
                IndexPoziceNaTrase++;
                if (IndexPoziceNaTrase < Zdroje.Aktualni.Level.Mapa.TrasaPochodu.Count - 1)
                    SouradniceCile = Zdroje.Aktualni.Level.Mapa.PoziceNaTrase(IndexPoziceNaTrase + 1);
                else
                {
                    DosahlCile = true;
                    Smazat = true;
                }
            }
            Pozice = novaPozice;

            if (Zdravi < 0)
                Smazat = true;

            base.Update(elapsedSeconds);
        }

    }

}