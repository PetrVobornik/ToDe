﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace ToDe
{
    internal static class Kresleni
    {
        public static void Kresli(this SpriteBatch sb,
            Vector2 pozice,
            DlazdiceUrceni dlazdice,
            Vector2? stred = null,
            float uhelOtoceni = 0,
            float meritko = 1,
            SpriteEffects efekt = SpriteEffects.None,
            Color? barva = null,
            Point? velikostDlazdice = null,
            Textura textura = null
            )
        {
            if (!dlazdice.Vykreslovat) return;

            Point vd = velikostDlazdice ?? textura?.VelikostDlazdice ?? new Point(Zdroje.VelikostDlazdice);

            sb.Draw(textura?.Grafika ?? Zdroje.Obsah.Zakladni.Grafika,
                  position: pozice,
                  sourceRectangle: textura == null ? dlazdice.VyrezZeZakladniTextury() : dlazdice.VyrezTextury(textura),
                  rotation: MathHelper.ToRadians(uhelOtoceni),
                  origin: stred ?? new Vector2(vd.X, vd.Y) * 0.5f,
                  scale: meritko,
                  effects: efekt,
                  color: barva ?? Color.White,
                  layerDepth: dlazdice.Z);
        }

        public static void Kresli(this SpriteBatch sb,
            Vector2 pozice,
            Rectangle vyrezZTextury,
            Vector2? stred = null,
            float uhelOtoceni = 0,
            float meritko = 1,
            float Z = 0,
            SpriteEffects efekt = SpriteEffects.None,
            Color? barva = null,
            Textura textura = null
            )
        {
            sb.Draw(textura?.Grafika ?? Zdroje.Obsah.Zakladni.Grafika,
                  position: pozice,
                  sourceRectangle: vyrezZTextury,
                  rotation: MathHelper.ToRadians(uhelOtoceni),
                  origin: stred ?? new Vector2(vyrezZTextury.Width * 0.5f, vyrezZTextury.Height * 0.5f),
                  scale: meritko,
                  effects: efekt,
                  color: barva ?? Color.White,
                  layerDepth: Z);
        }

        public static void Kresli(this SpriteBatch sb,
            Rectangle cil,
            DlazdiceUrceni dlazdice,
            Vector2? stred = null,
            float uhelOtoceni = 0,
            SpriteEffects efekt = SpriteEffects.None,
            Color? barva = null,
            Point? velikostDlazdice = null,
            Textura textura = null
            )
        {
            if (!dlazdice.Vykreslovat) return;
           
            Point vd = velikostDlazdice ?? textura?.VelikostDlazdice ?? new Point(Zdroje.VelikostDlazdice);

            sb.Draw(textura?.Grafika ?? Zdroje.Obsah.Zakladni.Grafika,
                  destinationRectangle: cil,
                  sourceRectangle: textura == null ? dlazdice.VyrezZeZakladniTextury() : dlazdice.VyrezTextury(textura), 
                  rotation: MathHelper.ToRadians(uhelOtoceni),
                  origin: stred ?? new Vector2(vd.X, vd.Y) * 0.5f,
                  effects: efekt,
                  color: barva ?? Color.White,
                  layerDepth: dlazdice.Z);
        }

        public static Color Pruhlednost(float nepruhlednost, Color barva)
            => new Color(barva.R, barva.G, barva.B, (int)(255 * nepruhlednost));

        public static void KresliTextDoprostred(this SpriteBatch sb, string text)
        {
            var textSize = Zdroje.Obsah.Pismo.MeasureString(text);
            sb.DrawString(Zdroje.Obsah.Pismo, text,
                new Vector2(Zdroje.Aktualni.Level.Mapa.Sloupcu * Zdroje.VelikostDlazdice * 0.5f,
                            Zdroje.Aktualni.Level.Mapa.Radku * Zdroje.VelikostDlazdice * 0.5f),
                Color.White, 0, new Vector2(textSize.X * 0.5f, textSize.Y * 0.5f), 1, SpriteEffects.None, 1);
        }
    }

    internal struct DlazdiceUrceni
    {
        public int X; // Pozice X dlaždice na textuře
        public int Y; // Pozice Y dlaždice na textuře
        public float Z; // Vrstva pro vykreslení dlaždice
        public bool Otacet;
        public bool Vykreslovat;

        private DlazdiceUrceni(int x, int y, float z = 0, bool otacet = true) 
            => (X, Y, Z, Otacet, Vykreslovat) = (x, y, z, otacet, true);
        private DlazdiceUrceni(Point souradnice, float z = 0, bool otacet = true)
            => (X, Y, Z, Otacet, Vykreslovat) = (souradnice.X, souradnice.Y, z, otacet, true);
        public DlazdiceUrceni(ZakladniDlazdice zd, float z = 0, bool otacet = true)
        {
            var souradnice = Zdroje.Obsah.Zakladni.SouradniceDlazdice(zd);
            (X, Y, Z, Otacet, Vykreslovat) = (souradnice.X, souradnice.Y, z, otacet, true);
        }

        public Rectangle VyrezZeZakladniTextury()
            => new Rectangle(X * (Zdroje.VelikostDlazdice + 2 * Zdroje.Obsah.Zakladni.Okraj) + Zdroje.Obsah.Zakladni.Okraj,
                             Y * (Zdroje.VelikostDlazdice + 2 * Zdroje.Obsah.Zakladni.Okraj) + Zdroje.Obsah.Zakladni.Okraj,
                             Zdroje.VelikostDlazdice, Zdroje.VelikostDlazdice);

        public Rectangle VyrezTextury(Textura textura)
            => new Rectangle(X * (textura.VelikostDlazdice.X + 2 * textura.Okraj) + textura.Okraj,
                             Y * (textura.VelikostDlazdice.Y + 2 * textura.Okraj) + textura.Okraj,
                             textura.VelikostDlazdice.X, textura.VelikostDlazdice.Y);
    }

    internal struct PrekazkaNaMape
    {
        public Point Pozice;
        public char Znak;

        public PrekazkaNaMape(Point pozice, char znak)
            => (Pozice, Znak) = (pozice, znak);
    }

    internal struct MezeryOdOkraju 
    {
        public float Vlevo;
        public float Nahore;
        public float Vpravo;
        public float Dole;

        public float Horizontalne { 
            get => Vlevo + Vpravo;
            set => (Vlevo, Vpravo) = (value, value);
        }

        public float Vertikalne
        {
            get => Nahore + Dole;
            set => (Nahore, Dole) = (value, value);
        }

        public MezeryOdOkraju(float vlevo, float nahore, float vpravo, float dole)
            => (Vlevo, Nahore, Vpravo, Dole) = (vlevo, nahore, vpravo, dole);
        public MezeryOdOkraju(float horizontalne, float vertikalne, float mezi = 0)
            => (Vlevo, Nahore, Vpravo, Dole) = (horizontalne, vertikalne, horizontalne, vertikalne);
        public MezeryOdOkraju(float vse, float mezi = 0)
            => (Vlevo, Nahore, Vpravo, Dole) = (vse, vse, vse, vse);

        public static implicit operator MezeryOdOkraju(float vse) => new MezeryOdOkraju(vse);
        public MezeryOdOkraju(MezeryOdOkraju vzor, float? vlevo = null, float? nahore = null, float? vpravo = null, float? dole = null)
            => (Vlevo, Nahore, Vpravo, Dole) = (vlevo ?? vzor.Vlevo, nahore ?? vzor.Nahore, vpravo ?? vzor.Vpravo, dole ?? vzor.Dole);

        public override string ToString() => $"{Vlevo}, {Nahore}, {Vpravo}, {Dole}";
    }


    internal static class Rozsireni
    {
        public static Rectangle Plus(this Rectangle rec, int plusX=0, int plusY=0, int plusSirka=0, int plusVyska=0)
            => new Rectangle(rec.Left + plusX, rec.Top + plusY, rec.Width + plusSirka, rec.Height + plusVyska);

        public static T GetAtt<T>(this XElement element, XName attributeName, T defaultValue)
            => (T)AttributeValue(typeof(T), element, attributeName, defaultValue);

        public static T AttVal<T>(XElement element, XName attributeName, T defaultValue)
            => (T)AttributeValue(typeof(T), element, attributeName, defaultValue);


        public static object AttributeValue(Type type, XElement element, XName attributeName, object defaultValue)
        {
            XAttribute attribute = null;
            if (element != null)
                attribute = element.Attribute(attributeName);
            if (attribute == null || String.IsNullOrEmpty(attribute.Value))
                return defaultValue;
            if (type == typeof(DateTime))
                return (DateTime)attribute;
            return ConvertXmlValue(type, attribute.Value, defaultValue);
        }

        public static object ConvertXmlValue(Type type, string value, object defaultValue)
        {
            try
            {
                if (String.IsNullOrEmpty(value))
                    return defaultValue;
                if (type == typeof(double))
                    return Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (type == typeof(float))
                    return Convert.ToSingle(value, CultureInfo.InvariantCulture);
                if (type == typeof(decimal))
                    return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                if (type == typeof(ushort))
                    return Convert.ToUInt16(value);
                if (type == typeof(byte))
                    return Convert.ToByte(value);
                if (type == typeof(short))
                    return Convert.ToInt16(value);
                if (type == typeof(int))
                    return Convert.ToInt32(value);
                if (type == typeof(int?))
                    return (int?)Convert.ToInt32(value);
                if (type == typeof(long))
                    return Convert.ToInt64(value);
                if (type == typeof(long?))
                    return (long?)Convert.ToInt64(value);
                if (type == typeof(DateTime))
                    return DateTime.Parse(value);
                //if (type == typeof(Color))
                //    return Color.FromHex(value.TrimStart('#'));
                if (type == typeof(bool) || type == typeof(bool?))
                {
                    bool result = (!String.IsNullOrEmpty(value) &&
                        (value == "1" || value.ToLower() == "true"));
                    if (type == typeof(bool?))
                        return (bool?)result;
                    return result;
                }
                if (type.GetTypeInfo().IsSubclassOf(typeof(Enum)))
                    return Enum.Parse(type, value, true);
                return value;
            }
            catch { }
            return defaultValue;
        }

    }

    internal static class TDUtils
    {
        public static Random RND = new Random();

        public static Vector2 PosunPoUhlu(float uhel, float rychlost)
        {
            var radiany = MathHelper.ToRadians(uhel);
            return new Vector2((float)Math.Cos(radiany) * rychlost,
                               (float)Math.Sin(radiany) * rychlost);
        }

        public static float OtacejSeKCili(float elapsedSeconds, 
            Vector2 poziceObjektu, Vector2 poziceCile,
            float uhelObjektu, float rychlostRotace, out bool muzeStrilet)
        {
            float ang = MathHelper.ToDegrees((float)Math.Atan2(poziceCile.Y - poziceObjektu.Y, 
                                                               poziceCile.X - poziceObjektu.X));
            float rozdilUhlu = RozdilUhlu(uhelObjektu, ang);

            if (Math.Abs(rozdilUhlu) > rychlostRotace * elapsedSeconds)
            {
                muzeStrilet = false;
                return uhelObjektu + Math.Sign(rozdilUhlu) * (rychlostRotace * elapsedSeconds);
            }
            muzeStrilet = true;
            return ang;
        }

        public static float KorekceUhlu(float angle)
        {
            if (angle > 360)
                angle = angle % 360;
            while (angle < 0)
                angle += 360; // TODO: vypočíst bez cyklu
            return angle;
        }

        static float RozdilUhlu(float aktualniUhel, float cilovyUhel)
        {
            aktualniUhel = KorekceUhlu(aktualniUhel);
            cilovyUhel = KorekceUhlu(cilovyUhel);

            float rozdil = cilovyUhel - aktualniUhel;

            if (rozdil > 180)
                return -(360 - rozdil);
            if (rozdil < -180)
                return rozdil + 360;
            return rozdil;
        }

        public static void NactiParametrVeze(LevelVez vez, XElement eVez, XElement eUroven, ushort idUrovne, string nazevVlastnosti, int nasobitel = 1) 
        {
            Type typ = null;
            switch (vez.Typ)
            {
                case TypVeze.Kulomet: typ = typeof(KonfiguraceVezKulomet); break;
                case TypVeze.Raketa:  typ = typeof(KonfiguraceVezRaketa); break;
            }
            var prop = typ.GetProperty(nazevVlastnosti);
            string nazevAtributu = nazevVlastnosti[0].ToString().ToLower() + nazevVlastnosti.Substring(1); // Název atributu = název vlasntosti, ale první písmeno je malé
            var metoda = typ.GetMethod(nameof(KonfiguraceVezKulomet.ParametryVeze));
            var vlastnostiSVychoziHodnotou = metoda.Invoke(null, new object[] { idUrovne }) as KonfiguraceVeze; // KonfiguraceVezKulomet.ParametryVeze(idUrovne)

            //var eUroven = eVez.Elements().FirstOrDefault(x => Convert.ToInt16(x.Attribute("id").Value) == idUrovne);
            float hodnota;
            if (idUrovne == 1)
                hodnota = eUroven.GetAtt(nazevAtributu,
                    (float)prop.GetValue(vlastnostiSVychoziHodnotou)) * nasobitel;
            else
            {
                string val = eUroven.GetAtt(nazevAtributu, "");
                if (String.IsNullOrEmpty(val))
                    hodnota = (float)prop.GetValue(vlastnostiSVychoziHodnotou) * nasobitel;
                else
                {
                    if (val[0] == '+' || val[0] == '-')
                        hodnota = (float)prop.GetValue(vez.Vlasntosti[(ushort)(idUrovne - 1)])
                            * (1 + Convert.ToSingle(val, CultureInfo.InvariantCulture));
                    else
                        hodnota = Convert.ToSingle(val, CultureInfo.InvariantCulture) * nasobitel;
                }
            }

            prop.SetValue(vez.Vlasntosti[idUrovne], hodnota);

        }
    }

    public class NastaveniHry
    {
        public bool PrehravatZvuky { get; set; } = true;
    }


    public struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public RectangleF(float x, float y, float width, float height)
            => (X, Y, Width, Height) = (x, y, width, height);

        public RectangleF(Rectangle rectangle)
            => (X, Y, Width, Height) = (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

        public bool Contains(float x, float y)
            => x >= X && y >= Y && x <= X + Width && y <= Y + Height;

        public static RectangleF Empty => new RectangleF(0, 0, 0, 0);

        public static bool operator ==(RectangleF r1, RectangleF r2)
            => r1.X == r2.X && r1.Y == r2.Y && r1.Width == r2.Width && r1.Height == r2.Height;

        public static bool operator !=(RectangleF r1, RectangleF r2)
            => r1.X != r2.X || r1.Y != r2.Y || r1.Width != r2.Width || r1.Height != r2.Height;

        public override bool Equals(object obj)
            => obj is RectangleF && (RectangleF)obj == this;

        public override int GetHashCode()
            => (Width.GetHashCode()+1) ^ (Y.GetHashCode()+1) ^ (X.GetHashCode()+1) ^ (Height.GetHashCode()+1);

        public Vector2 Size()
            => new Vector2(Width, Height);
    }


    public class TouchData
    {
        public bool ShowCursor;
        public List<OneTouch> Touchs;
        public List<MultiTouch> MultiTouchs;
        public bool BackButton;
        public Vector2 Offset;
        public float Scale;

        public TouchData()
        {
            Touchs = new List<OneTouch>();
            MultiTouchs = new List<MultiTouch>();
        }

        public bool IsGestureTouched(params GestType[] gests)
        {
            if (Touchs.Count > 0)
                foreach (var touch in Touchs)
                    if (gests.Contains(touch.Gesture))
                        return true;
            return false;
        }

        public OneTouch GetTouchByGesture(GestType gest)
        {
            if (Touchs.Count > 0)
                foreach (var touch in Touchs)
                    if (touch.Gesture == gest)
                        return touch;
            return OneTouch.Empty;
        }

        public Vector2 GetMultiTouchInArea(RectangleF area, bool recalcPosition)
        {
            if (MultiTouchs.Count > 0)
                for (int i = MultiTouchs.Count - 1; i >= 0; i--)
                {
                    var touch = MultiTouchs[i];
                    if (recalcPosition && area.Contains((int)(touch.Position.X / Scale + Offset.X), (int)(touch.Position.Y / Scale + Offset.Y)) ||
                        !recalcPosition && area.Contains((int)(touch.Position.X), (int)(touch.Position.Y)))
                    {
                        var t = touch.Position;
                        if (recalcPosition)
                        {
                            t.X = (touch.Position.X / Scale + Offset.X) - area.X;
                            t.Y = (touch.Position.Y / Scale + Offset.Y) - area.Y;
                        }
                        return t;
                    }
                }
            return Vector2.Zero;
        }

        public OneTouch GetTouchInArea(RectangleF area, bool recalcPosition)
        {
            if (Touchs.Count > 0)
                for (int i = Touchs.Count - 1; i >= 0; i--)
                {
                    var touch = Touchs[i];
                    if (touch.Gesture != GestType.Flick && touch.Gesture != GestType.DragComplete)
                        if (recalcPosition && area.Contains((int)(touch.Position.X / Scale + Offset.X), (int)(touch.Position.Y / Scale + Offset.Y)) ||
                            !recalcPosition && area.Contains((int)(touch.Position.X), (int)(touch.Position.Y)))
                        {
                            var t = touch;
                            if (recalcPosition)
                            {
                                t.Position.X = (touch.Position.X / Scale + Offset.X) - area.X;
                                t.Position.Y = (touch.Position.Y / Scale + Offset.Y) - area.Y;
                            }
                            return t;
                        }
                }
            return OneTouch.Empty;
        }

        public GestType IsTouched(RectangleF area)
        {
            //return GetTouchInArea(area).Gesture;
            if (Touchs.Count > 0)
                foreach (var touch in Touchs)
                    if (area.Contains((int)(touch.Position.X / Scale + Offset.X), (int)(touch.Position.Y / Scale + Offset.Y)))
                        return touch.Gesture;
            return GestType.None;
        }

        public GestType IsTouched(Vector2 pos, float size)
        {
            return IsTouched(pos, size, size);
        }

        public GestType IsTouched(Vector2 pos, float width, float height)
        {
            if (Touchs.Count > 0)
                foreach (var touch in Touchs)
                    if (touch.Position.X / Scale + Offset.X >= pos.X && touch.Position.Y / Scale + Offset.Y >= pos.Y &&
                        touch.Position.X / Scale + Offset.X <= pos.X + width && touch.Position.Y / Scale + Offset.Y <= pos.Y + height)
                        return touch.Gesture;
            return GestType.None;
        }

        public bool IsMultiTouched(Vector2 pos, float width, float height)
        {
            if (MultiTouchs.Count > 0)
                foreach (var touch in MultiTouchs)
                    if (touch.Position.X / Scale + Offset.X >= pos.X && touch.Position.Y / Scale + Offset.Y >= pos.Y &&
                        touch.Position.X / Scale + Offset.X <= pos.X + width && touch.Position.Y / Scale + Offset.Y <= pos.Y + height)
                        return true;
            return false;
        }


        // source: http://microngamestudios.com/files/pinchzoom/PinchZoom.cs
        public static float PinchScaleFactor(Vector2 position1, Vector2 position2, Vector2 delta1, Vector2 delta2)
        {
            Vector2 oldPosition1 = position1 - delta1;
            Vector2 oldPosition2 = position2 - delta2;

            float distance = Vector2.Distance(position1, position2);
            float oldDistance = Vector2.Distance(oldPosition1, oldPosition2);

            if (oldDistance == 0 || distance == 0)
                return 1.0f;

            return distance / oldDistance;
        }
    }

    public struct MultiTouch
    {
        public Vector2 Position;
        //public TouchState State;

    }

    public struct OneTouch
    {
        public GestType Gesture;
        public Vector2 Position; // Position u Flicku = Delta
        public bool ByMouse; // Tento dotek byl vyvolán myší

        public static OneTouch Empty = new OneTouch() { Gesture = GestType.None };
    }

    public enum GestType
    {
        None,
        Tap,            // Klik
        DoubleTap,      // Doubleclick
        Hold,           // Zmáčknout a držet
        HorizontalDrag, // Sunutí horizontální
        VerticalDrag,   // Sunutí vertikální
        FreeDrag,       // Sunutí neurčitým směrem
        DragComplete,   // Sunutí puštěno
        Pinch,          // Dva prsty táhnout najednou
        PinchComplete,  // Sun dvou prstů hotov
        Flick,          // Posun na další obrazovku 
    }

    public enum TouchState
    {
        Invalid,
        Pressed,
        Moved,
        Released,
    }




}
