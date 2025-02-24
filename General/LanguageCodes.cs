using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Shared
{
    [JsonConverter(typeof(Converter))]
    public readonly struct LanguageCodes
    {
        public readonly int Int;

        public LanguageCodes(int code)
        {
            this.Int = code;
        }

        public readonly override string ToString()
        {
            foreach (var m in Map)
            {
                if (m.Value == this.Int && m.Key.Length == 3)
                    return m.Key;
            }
            return "";
        }

        /// <summary>
        /// Returns language string in specific format.<br/>
        /// 2 = 2 letter code<br/>
        /// 3 = 3 letter code (same as <see cref="ToString()"/>)<br/>
        /// n = natural language with spaces
        /// otherwise natural language with underscore
        /// </summary>
        public readonly string ToString(string format)
        {
            if (format is "2")
            {
                foreach (var m in Map)
                {
                    if (m.Value == this.Int && m.Key.Length == 2)
                        return m.Key;
                }
            }
            if (format is not "3")
            {
                foreach (var m in Map)
                {
                    if (m.Value == this.Int && m.Key[0] is > 'A' and < 'Z')
                        if (format is "n")
                            return m.Key.Replace('_', ' ');
                        else
                            return m.Key;
                }
            }
            return ToString();
        }

        public static LanguageCodes Parse(string? name)
        {
            if (name is not null && Map.TryGetValue(name, out int num))
                return new(num);
            return new(0);
        }

        public static bool TryParse(string? name, out LanguageCodes code)
        {
            code = Parse(name);
            return code != 0 || name is "und" or "Undetermined";
        }

        public static implicit operator LanguageCodes(int number)
        {
            return new(number);
        }

        #region converter

        public class Converter : JsonConverter<LanguageCodes>
        {
            public override LanguageCodes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                    return LanguageCodes.Parse(reader.GetString());
                if (reader.TokenType == JsonTokenType.Number)
                    return (LanguageCodes)reader.GetInt32();
                return (LanguageCodes)0;
            }

            public override void Write(Utf8JsonWriter writer, LanguageCodes value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        #endregion

        #region equality

        public static bool operator ==(LanguageCodes value1, LanguageCodes value2)
        {
            return value1.Int == value2.Int;
        }

        public static bool operator !=(LanguageCodes value1, LanguageCodes value2)
        {
            return value1.Int != value2.Int;
        }

        public static bool operator ==(LanguageCodes value1, int value2)
        {
            return value1.Int == value2;
        }

        public static bool operator !=(LanguageCodes value1, int value2)
        {
            return value1.Int != value2;
        }

        public readonly override bool Equals(object? obj)
        {
            if (obj is LanguageCodes code)
                return this.Int == code.Int;
            if (obj is int number)
                return this.Int == number;
            if (obj is string name)
                return this == Parse(name);
            return false;
        }

        public readonly override int GetHashCode()
        {
            return this.Int.GetHashCode();
        }

        #endregion

        #region init

        private static Dictionary<string, int>? _Map;
        public static Dictionary<string, int> Map
        {
            get
            {
                if (_Map == null)
                {
                    _Map = new Dictionary<string, int>();
                    foreach (var fi in typeof(LanguageCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (!fi.IsLiteral || fi.IsInitOnly || fi.FieldType != typeof(int))
                            continue;
                        _Map[fi.Name] = (int)fi.GetValue(null)!;
                    }
                }
                return _Map;
            }
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Useless SuppressMessage", Justification = "Required suppression marked unnecessarily.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "SYSLIB1045:GeneratedRegexAttribute", Justification = "Simplified, just for debug.")]
        internal static void ScrapeFromWeb()
        {
            // copy table from https://www.loc.gov/standards/iso639-2/php/code_list.php

            var text = System.IO.File.ReadAllText(@"E:\Github\AutoCompress\AutoCompress\Resources\iso639-2 from 2017-12-21.txt");
            text = Regex.Replace(text, @"(^.*?\t.*?\t[\w ,-]*(?<! )).*", "$1", RegexOptions.Multiline);
            text = Regex.Replace(text, @" ?\t ?", "\t", RegexOptions.Multiline);
            text = Regex.Replace(text, @" \(\w\)", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"(?>(^\w+)\n)?(^.+?\t)(.*)", "$2$1\t$3", RegexOptions.Multiline);
            text = Regex.Replace(text, @",", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"[ -]", "_", RegexOptions.Multiline);
            var matches = Regex.Matches(text, @"(?<c3>\w{3})\t(?<ca>\w{3})?\t(?<c2>\w{2})?\t(?<cn>[\w]+)", RegexOptions.Multiline);

            int num = -1;
            var set = new HashSet<string>();
            foreach (Match match in matches)
            {
                if (match.Groups["c3"].Value is not "und")
                    continue;
                print(match);
                break;
            }
            foreach (Match match in matches)
            {
                if (match.Groups["c3"].Value is not "mul")
                    continue;
                print(match);
                break;
            }
            foreach (Match match in matches)
            {
                print(match);
            }

            void print(Match match)
            {
                string c3 = match.Groups["c3"].Value;
                if (set.Contains(c3))
                    return;
                num++;
                set.Add(c3);

                if (c3 is "new")
                    c3 = "@new";
                Console.Write($"public const int {c3} = {num}");

                string ca = match.Groups["ca"].Value;
                if (ca != "")
                    Console.Write($", {ca} = {num}");

                string c2 = match.Groups["c2"].Value;
                if (c2 is "is")
                    c2 = "@is";
                if (c2 is "as")
                    c2 = "@as";
                if (c2 != "")
                    Console.Write($", {c2} = {num}");

                string cn = match.Groups["cn"].Value;
                cn = cn.Replace('ü', 'u');
                Console.Write($", {cn} = {num};\n");
            }
        }

        #region constants

        public const int und = 0, Undetermined = 0;
        public const int mul = 1, Multiple_languages = 1;
        public const int aar = 2, aa = 2, Afar = 2;
        public const int abk = 3, ab = 3, Abkhazian = 3;
        public const int ace = 4, Achinese = 4;
        public const int ach = 5, Acoli = 5;
        public const int ada = 6, Adangme = 6;
        public const int ady = 7, Adyghe = 7;
        public const int afa = 8, Afro_Asiatic_languages = 8;
        public const int afh = 9, Afrihili = 9;
        public const int afr = 10, af = 10, Afrikaans = 10;
        public const int ain = 11, Ainu = 11;
        public const int aka = 12, ak = 12, Akan = 12;
        public const int akk = 13, Akkadian = 13;
        public const int sqi = 14, alb = 14, sq = 14, Albanian = 14;
        public const int ale = 15, Aleut = 15;
        public const int alg = 16, Algonquian_languages = 16;
        public const int alt = 17, Southern_Altai = 17;
        public const int amh = 18, am = 18, Amharic = 18;
        public const int ang = 19, English_Old = 19;
        public const int anp = 20, Angika = 20;
        public const int apa = 21, Apache_languages = 21;
        public const int ara = 22, ar = 22, Arabic = 22;
        public const int arc = 23, Official_Aramaic = 23;
        public const int arg = 24, an = 24, Aragonese = 24;
        public const int hye = 25, arm = 25, hy = 25, Armenian = 25;
        public const int arn = 26, Mapudungun = 26;
        public const int arp = 27, Arapaho = 27;
        public const int art = 28, Artificial_languages = 28;
        public const int arw = 29, Arawak = 29;
        public const int asm = 30, @as = 30, Assamese = 30;
        public const int ast = 31, Asturian = 31;
        public const int ath = 32, Athapascan_languages = 32;
        public const int aus = 33, Australian_languages = 33;
        public const int ava = 34, av = 34, Avaric = 34;
        public const int ave = 35, ae = 35, Avestan = 35;
        public const int awa = 36, Awadhi = 36;
        public const int aym = 37, ay = 37, Aymara = 37;
        public const int aze = 38, az = 38, Azerbaijani = 38;
        public const int bad = 39, Banda_languages = 39;
        public const int bai = 40, Bamileke_languages = 40;
        public const int bak = 41, ba = 41, Bashkir = 41;
        public const int bal = 42, Baluchi = 42;
        public const int bam = 43, bm = 43, Bambara = 43;
        public const int ban = 44, Balinese = 44;
        public const int eus = 45, baq = 45, eu = 45, Basque = 45;
        public const int bas = 46, Basa = 46;
        public const int bat = 47, Baltic_languages = 47;
        public const int bej = 48, Beja = 48;
        public const int bel = 49, be = 49, Belarusian = 49;
        public const int bem = 50, Bemba = 50;
        public const int ben = 51, bn = 51, Bengali = 51;
        public const int ber = 52, Berber_languages = 52;
        public const int bho = 53, Bhojpuri = 53;
        public const int bih = 54, Bihari_languages = 54;
        public const int bik = 55, Bikol = 55;
        public const int bin = 56, Bini = 56;
        public const int bis = 57, bi = 57, Bislama = 57;
        public const int bla = 58, Siksika = 58;
        public const int bnt = 59, Bantu_languages = 59;
        public const int bod = 60, tib = 60, bo = 60, Tibetan = 60;
        public const int bos = 61, bs = 61, Bosnian = 61;
        public const int bra = 62, Braj = 62;
        public const int bre = 63, br = 63, Breton = 63;
        public const int btk = 64, Batak_languages = 64;
        public const int bua = 65, Buriat = 65;
        public const int bug = 66, Buginese = 66;
        public const int bul = 67, bg = 67, Bulgarian = 67;
        public const int mya = 68, bur = 68, my = 68, Burmese = 68;
        public const int byn = 69, Blin = 69;
        public const int cad = 70, Caddo = 70;
        public const int cai = 71, Central_American_Indian_languages = 71;
        public const int car = 72, Galibi_Carib = 72;
        public const int cat = 73, ca = 73, Catalan = 73;
        public const int cau = 74, Caucasian_languages = 74;
        public const int ceb = 75, Cebuano = 75;
        public const int cel = 76, Celtic_languages = 76;
        public const int ces = 77, cze = 77, cs = 77, Czech = 77;
        public const int cha = 78, ch = 78, Chamorro = 78;
        public const int chb = 79, Chibcha = 79;
        public const int che = 80, ce = 80, Chechen = 80;
        public const int chg = 81, Chagatai = 81;
        public const int zho = 82, chi = 82, zh = 82, Chinese = 82;
        public const int chk = 83, Chuukese = 83;
        public const int chm = 84, Mari = 84;
        public const int chn = 85, Chinook_jargon = 85;
        public const int cho = 86, Choctaw = 86;
        public const int chp = 87, Chipewyan = 87;
        public const int chr = 88, Cherokee = 88;
        public const int chu = 89, cu = 89, Church_Slavic = 89;
        public const int chv = 90, cv = 90, Chuvash = 90;
        public const int chy = 91, Cheyenne = 91;
        public const int cmc = 92, Chamic_languages = 92;
        public const int cnr = 93, Montenegrin = 93;
        public const int cop = 94, Coptic = 94;
        public const int cor = 95, kw = 95, Cornish = 95;
        public const int cos = 96, co = 96, Corsican = 96;
        public const int cpe = 97, Creoles_and_pidgins_English_based = 97;
        public const int cpf = 98, Creoles_and_pidgins_French_based = 98;
        public const int cpp = 99, Creoles_and_pidgins_Portuguese_based = 99;
        public const int cre = 100, cr = 100, Cree = 100;
        public const int crh = 101, Crimean_Tatar = 101;
        public const int crp = 102, Creoles_and_pidgins = 102;
        public const int csb = 103, Kashubian = 103;
        public const int cus = 104, Cushitic_languages = 104;
        public const int cym = 105, wel = 105, cy = 105, Welsh = 105;
        public const int dak = 106, Dakota = 106;
        public const int dan = 107, da = 107, Danish = 107;
        public const int dar = 108, Dargwa = 108;
        public const int day = 109, Land_Dayak_languages = 109;
        public const int del = 110, Delaware = 110;
        public const int den = 111, Slave = 111;
        public const int deu = 112, ger = 112, de = 112, German = 112;
        public const int dgr = 113, Tlicho = 113;
        public const int din = 114, Dinka = 114;
        public const int div = 115, dv = 115, Divehi = 115;
        public const int doi = 116, Dogri = 116;
        public const int dra = 117, Dravidian_languages = 117;
        public const int dsb = 118, Lower_Sorbian = 118;
        public const int dua = 119, Duala = 119;
        public const int dum = 120, Dutch_Middle = 120;
        public const int nld = 121, dut = 121, nl = 121, Dutch = 121;
        public const int dyu = 122, Dyula = 122;
        public const int dzo = 123, dz = 123, Dzongkha = 123;
        public const int efi = 124, Efik = 124;
        public const int egy = 125, Egyptian = 125;
        public const int eka = 126, Ekajuk = 126;
        public const int ell = 127, gre = 127, el = 127, Greek_Modern = 127;
        public const int elx = 128, Elamite = 128;
        public const int eng = 129, en = 129, English = 129;
        public const int enm = 130, English_Middle = 130;
        public const int epo = 131, eo = 131, Esperanto = 131;
        public const int est = 132, et = 132, Estonian = 132;
        public const int ewe = 133, ee = 133, Ewe = 133;
        public const int ewo = 134, Ewondo = 134;
        public const int fan = 135, Fang = 135;
        public const int fao = 136, fo = 136, Faroese = 136;
        public const int fas = 137, per = 137, fa = 137, Persian = 137;
        public const int fat = 138, Fanti = 138;
        public const int fij = 139, fj = 139, Fijian = 139;
        public const int fil = 140, Filipino = 140;
        public const int fin = 141, fi = 141, Finnish = 141;
        public const int fiu = 142, Finno_Ugrian_languages = 142;
        public const int fon = 143, Fon = 143;
        public const int fra = 144, fre = 144, fr = 144, French = 144;
        public const int frm = 145, French_Middle = 145;
        public const int fro = 146, French_Old = 146;
        public const int frr = 147, Northern_Frisian = 147;
        public const int frs = 148, Eastern_Frisian = 148;
        public const int fry = 149, fy = 149, Western_Frisian = 149;
        public const int ful = 150, ff = 150, Fulah = 150;
        public const int fur = 151, Friulian = 151;
        public const int gaa = 152, Ga = 152;
        public const int gay = 153, Gayo = 153;
        public const int gba = 154, Gbaya = 154;
        public const int gem = 155, Germanic_languages = 155;
        public const int kat = 156, geo = 156, ka = 156, Georgian = 156;
        public const int gez = 157, Geez = 157;
        public const int gil = 158, Gilbertese = 158;
        public const int gla = 159, gd = 159, Gaelic = 159;
        public const int gle = 160, ga = 160, Irish = 160;
        public const int glg = 161, gl = 161, Galician = 161;
        public const int glv = 162, gv = 162, Manx = 162;
        public const int gmh = 163, German_Middle_High = 163;
        public const int goh = 164, German_Old_High = 164;
        public const int gon = 165, Gondi = 165;
        public const int gor = 166, Gorontalo = 166;
        public const int got = 167, Gothic = 167;
        public const int grb = 168, Grebo = 168;
        public const int grc = 169, Greek_Ancient = 169;
        public const int grn = 170, gn = 170, Guarani = 170;
        public const int gsw = 171, Swiss_German = 171;
        public const int guj = 172, gu = 172, Gujarati = 172;
        public const int gwi = 173, Gwich = 173;
        public const int hai = 174, Haida = 174;
        public const int hat = 175, ht = 175, Haitian = 175;
        public const int hau = 176, ha = 176, Hausa = 176;
        public const int haw = 177, Hawaiian = 177;
        public const int heb = 178, he = 178, Hebrew = 178;
        public const int her = 179, hz = 179, Herero = 179;
        public const int hil = 180, Hiligaynon = 180;
        public const int him = 181, Himachali_languages = 181;
        public const int hin = 182, hi = 182, Hindi = 182;
        public const int hit = 183, Hittite = 183;
        public const int hmn = 184, Hmong = 184;
        public const int hmo = 185, ho = 185, Hiri_Motu = 185;
        public const int hrv = 186, hr = 186, Croatian = 186;
        public const int hsb = 187, Upper_Sorbian = 187;
        public const int hun = 188, hu = 188, Hungarian = 188;
        public const int hup = 189, Hupa = 189;
        public const int iba = 190, Iban = 190;
        public const int ibo = 191, ig = 191, Igbo = 191;
        public const int isl = 192, ice = 192, @is = 192, Icelandic = 192;
        public const int ido = 193, io = 193, Ido = 193;
        public const int iii = 194, ii = 194, Sichuan_Yi = 194;
        public const int ijo = 195, Ijo_languages = 195;
        public const int iku = 196, iu = 196, Inuktitut = 196;
        public const int ile = 197, ie = 197, Interlingue = 197;
        public const int ilo = 198, Iloko = 198;
        public const int ina = 199, ia = 199, Interlingua = 199;
        public const int inc = 200, Indic_languages = 200;
        public const int ind = 201, id = 201, Indonesian = 201;
        public const int ine = 202, Indo_European_languages = 202;
        public const int inh = 203, Ingush = 203;
        public const int ipk = 204, ik = 204, Inupiaq = 204;
        public const int ira = 205, Iranian_languages = 205;
        public const int iro = 206, Iroquoian_languages = 206;
        public const int ita = 207, it = 207, Italian = 207;
        public const int jav = 208, jv = 208, Javanese = 208;
        public const int jbo = 209, Lojban = 209;
        public const int jpn = 210, ja = 210, Japanese = 210;
        public const int jpr = 211, Judeo_Persian = 211;
        public const int jrb = 212, Judeo_Arabic = 212;
        public const int kaa = 213, Kara_Kalpak = 213;
        public const int kab = 214, Kabyle = 214;
        public const int kac = 215, Kachin = 215;
        public const int kal = 216, kl = 216, Kalaallisut = 216;
        public const int kam = 217, Kamba = 217;
        public const int kan = 218, kn = 218, Kannada = 218;
        public const int kar = 219, Karen_languages = 219;
        public const int kas = 220, ks = 220, Kashmiri = 220;
        public const int kau = 221, kr = 221, Kanuri = 221;
        public const int kaw = 222, Kawi = 222;
        public const int kaz = 223, kk = 223, Kazakh = 223;
        public const int kbd = 224, Kabardian = 224;
        public const int kha = 225, Khasi = 225;
        public const int khi = 226, Khoisan_languages = 226;
        public const int khm = 227, km = 227, Central_Khmer = 227;
        public const int kho = 228, Khotanese = 228;
        public const int kik = 229, ki = 229, Kikuyu = 229;
        public const int kin = 230, rw = 230, Kinyarwanda = 230;
        public const int kir = 231, ky = 231, Kirghiz = 231;
        public const int kmb = 232, Kimbundu = 232;
        public const int kok = 233, Konkani = 233;
        public const int kom = 234, kv = 234, Komi = 234;
        public const int kon = 235, kg = 235, Kongo = 235;
        public const int kor = 236, ko = 236, Korean = 236;
        public const int kos = 237, Kosraean = 237;
        public const int kpe = 238, Kpelle = 238;
        public const int krc = 239, Karachay_Balkar = 239;
        public const int krl = 240, Karelian = 240;
        public const int kro = 241, Kru_languages = 241;
        public const int kru = 242, Kurukh = 242;
        public const int kua = 243, kj = 243, Kuanyama = 243;
        public const int kum = 244, Kumyk = 244;
        public const int kur = 245, ku = 245, Kurdish = 245;
        public const int kut = 246, Kutenai = 246;
        public const int lad = 247, Ladino = 247;
        public const int lah = 248, Lahnda = 248;
        public const int lam = 249, Lamba = 249;
        public const int lao = 250, lo = 250, Lao = 250;
        public const int lat = 251, la = 251, Latin = 251;
        public const int lav = 252, lv = 252, Latvian = 252;
        public const int lez = 253, Lezghian = 253;
        public const int lim = 254, li = 254, Limburgan = 254;
        public const int lin = 255, ln = 255, Lingala = 255;
        public const int lit = 256, lt = 256, Lithuanian = 256;
        public const int lol = 257, Mongo = 257;
        public const int loz = 258, Lozi = 258;
        public const int ltz = 259, lb = 259, Luxembourgish = 259;
        public const int lua = 260, Luba_Lulua = 260;
        public const int lub = 261, lu = 261, Luba_Katanga = 261;
        public const int lug = 262, lg = 262, Ganda = 262;
        public const int lui = 263, Luiseno = 263;
        public const int lun = 264, Lunda = 264;
        public const int luo = 265, Luo = 265;
        public const int lus = 266, Lushai = 266;
        public const int mkd = 267, mac = 267, mk = 267, Macedonian = 267;
        public const int mad = 268, Madurese = 268;
        public const int mag = 269, Magahi = 269;
        public const int mah = 270, mh = 270, Marshallese = 270;
        public const int mai = 271, Maithili = 271;
        public const int mak = 272, Makasar = 272;
        public const int mal = 273, ml = 273, Malayalam = 273;
        public const int man = 274, Mandingo = 274;
        public const int mri = 275, mao = 275, mi = 275, Maori = 275;
        public const int map = 276, Austronesian_languages = 276;
        public const int mar = 277, mr = 277, Marathi = 277;
        public const int mas = 278, Masai = 278;
        public const int msa = 279, may = 279, ms = 279, Malay = 279;
        public const int mdf = 280, Moksha = 280;
        public const int mdr = 281, Mandar = 281;
        public const int men = 282, Mende = 282;
        public const int mga = 283, Irish_Middle = 283;
        public const int mic = 284, Mi = 284;
        public const int min = 285, Minangkabau = 285;
        public const int mis = 286, Uncoded_languages = 286;
        public const int mkh = 287, Mon_Khmer_languages = 287;
        public const int mlg = 288, mg = 288, Malagasy = 288;
        public const int mlt = 289, mt = 289, Maltese = 289;
        public const int mnc = 290, Manchu = 290;
        public const int mni = 291, Manipuri = 291;
        public const int mno = 292, Manobo_languages = 292;
        public const int moh = 293, Mohawk = 293;
        public const int mon = 294, mn = 294, Mongolian = 294;
        public const int mos = 295, Mossi = 295;
        public const int mun = 296, Munda_languages = 296;
        public const int mus = 297, Creek = 297;
        public const int mwl = 298, Mirandese = 298;
        public const int mwr = 299, Marwari = 299;
        public const int myn = 300, Mayan_languages = 300;
        public const int myv = 301, Erzya = 301;
        public const int nah = 302, Nahuatl_languages = 302;
        public const int nai = 303, North_American_Indian_languages = 303;
        public const int nap = 304, Neapolitan = 304;
        public const int nau = 305, na = 305, Nauru = 305;
        public const int nav = 306, nv = 306, Navajo = 306;
        public const int nbl = 307, nr = 307, Ndebele_South = 307;
        public const int nde = 308, nd = 308, Ndebele_North = 308;
        public const int ndo = 309, ng = 309, Ndonga = 309;
        public const int nds = 310, Low_German = 310;
        public const int nep = 311, ne = 311, Nepali = 311;
        public const int @new = 312, Nepal_Bhasa = 312;
        public const int nia = 313, Nias = 313;
        public const int nic = 314, Niger_Kordofanian_languages = 314;
        public const int niu = 315, Niuean = 315;
        public const int nno = 316, nn = 316, Norwegian_Nynorsk = 316;
        public const int nob = 317, nb = 317, Bokmål_Norwegian = 317;
        public const int nog = 318, Nogai = 318;
        public const int non = 319, Norse_Old = 319;
        public const int nor = 320, no = 320, Norwegian = 320;
        public const int nqo = 321, N = 321;
        public const int nso = 322, Pedi = 322;
        public const int nub = 323, Nubian_languages = 323;
        public const int nwc = 324, Classical_Newari = 324;
        public const int nya = 325, ny = 325, Chichewa = 325;
        public const int nym = 326, Nyamwezi = 326;
        public const int nyn = 327, Nyankole = 327;
        public const int nyo = 328, Nyoro = 328;
        public const int nzi = 329, Nzima = 329;
        public const int oci = 330, oc = 330, Occitan = 330;
        public const int oji = 331, oj = 331, Ojibwa = 331;
        public const int ori = 332, or = 332, Oriya = 332;
        public const int orm = 333, om = 333, Oromo = 333;
        public const int osa = 334, Osage = 334;
        public const int oss = 335, os = 335, Ossetian = 335;
        public const int ota = 336, Turkish_Ottoman = 336;
        public const int oto = 337, Otomian_languages = 337;
        public const int paa = 338, Papuan_languages = 338;
        public const int pag = 339, Pangasinan = 339;
        public const int pal = 340, Pahlavi = 340;
        public const int pam = 341, Pampanga = 341;
        public const int pan = 342, pa = 342, Panjabi = 342;
        public const int pap = 343, Papiamento = 343;
        public const int pau = 344, Palauan = 344;
        public const int peo = 345, Persian_Old = 345;
        public const int phi = 346, Philippine_languages = 346;
        public const int phn = 347, Phoenician = 347;
        public const int pli = 348, pi = 348, Pali = 348;
        public const int pol = 349, pl = 349, Polish = 349;
        public const int pon = 350, Pohnpeian = 350;
        public const int por = 351, pt = 351, Portuguese = 351;
        public const int pra = 352, Prakrit_languages = 352;
        public const int pro = 353, Provençal_Old = 353;
        public const int pus = 354, ps = 354, Pushto = 354;
        public const int qtz = 355, Reserved_for_local_use = 355;
        public const int que = 356, qu = 356, Quechua = 356;
        public const int raj = 357, Rajasthani = 357;
        public const int rap = 358, Rapanui = 358;
        public const int rar = 359, Rarotongan = 359;
        public const int roa = 360, Romance_languages = 360;
        public const int roh = 361, rm = 361, Romansh = 361;
        public const int rom = 362, Romany = 362;
        public const int ron = 363, rum = 363, ro = 363, Romanian = 363;
        public const int run = 364, rn = 364, Rundi = 364;
        public const int rup = 365, Aromanian = 365;
        public const int rus = 366, ru = 366, Russian = 366;
        public const int sad = 367, Sandawe = 367;
        public const int sag = 368, sg = 368, Sango = 368;
        public const int sah = 369, Yakut = 369;
        public const int sai = 370, South_American_Indian_languages = 370;
        public const int sal = 371, Salishan_languages = 371;
        public const int sam = 372, Samaritan_Aramaic = 372;
        public const int san = 373, sa = 373, Sanskrit = 373;
        public const int sas = 374, Sasak = 374;
        public const int sat = 375, Santali = 375;
        public const int scn = 376, Sicilian = 376;
        public const int sco = 377, Scots = 377;
        public const int sel = 378, Selkup = 378;
        public const int sem = 379, Semitic_languages = 379;
        public const int sga = 380, Irish_Old = 380;
        public const int sgn = 381, Sign_Languages = 381;
        public const int shn = 382, Shan = 382;
        public const int sid = 383, Sidamo = 383;
        public const int sin = 384, si = 384, Sinhala = 384;
        public const int sio = 385, Siouan_languages = 385;
        public const int sit = 386, Sino_Tibetan_languages = 386;
        public const int sla = 387, Slavic_languages = 387;
        public const int slk = 388, slo = 388, sk = 388, Slovak = 388;
        public const int slv = 389, sl = 389, Slovenian = 389;
        public const int sma = 390, Southern_Sami = 390;
        public const int sme = 391, se = 391, Northern_Sami = 391;
        public const int smi = 392, Sami_languages = 392;
        public const int smj = 393, Lule_Sami = 393;
        public const int smn = 394, Inari_Sami = 394;
        public const int smo = 395, sm = 395, Samoan = 395;
        public const int sms = 396, Skolt_Sami = 396;
        public const int sna = 397, sn = 397, Shona = 397;
        public const int snd = 398, sd = 398, Sindhi = 398;
        public const int snk = 399, Soninke = 399;
        public const int sog = 400, Sogdian = 400;
        public const int som = 401, so = 401, Somali = 401;
        public const int son = 402, Songhai_languages = 402;
        public const int sot = 403, st = 403, Sotho_Southern = 403;
        public const int spa = 404, es = 404, Spanish = 404;
        public const int srd = 405, sc = 405, Sardinian = 405;
        public const int srn = 406, Sranan_Tongo = 406;
        public const int srp = 407, sr = 407, Serbian = 407;
        public const int srr = 408, Serer = 408;
        public const int ssa = 409, Nilo_Saharan_languages = 409;
        public const int ssw = 410, ss = 410, Swati = 410;
        public const int suk = 411, Sukuma = 411;
        public const int sun = 412, su = 412, Sundanese = 412;
        public const int sus = 413, Susu = 413;
        public const int sux = 414, Sumerian = 414;
        public const int swa = 415, sw = 415, Swahili = 415;
        public const int swe = 416, sv = 416, Swedish = 416;
        public const int syc = 417, Classical_Syriac = 417;
        public const int syr = 418, Syriac = 418;
        public const int tah = 419, ty = 419, Tahitian = 419;
        public const int tai = 420, Tai_languages = 420;
        public const int tam = 421, ta = 421, Tamil = 421;
        public const int tat = 422, tt = 422, Tatar = 422;
        public const int tel = 423, te = 423, Telugu = 423;
        public const int tem = 424, Timne = 424;
        public const int ter = 425, Tereno = 425;
        public const int tet = 426, Tetum = 426;
        public const int tgk = 427, tg = 427, Tajik = 427;
        public const int tgl = 428, tl = 428, Tagalog = 428;
        public const int tha = 429, th = 429, Thai = 429;
        public const int tig = 430, Tigre = 430;
        public const int tir = 431, ti = 431, Tigrinya = 431;
        public const int tiv = 432, Tiv = 432;
        public const int tkl = 433, Tokelau = 433;
        public const int tlh = 434, Klingon = 434;
        public const int tli = 435, Tlingit = 435;
        public const int tmh = 436, Tamashek = 436;
        public const int ton = 438, tog = 438, to = 438, Tonga = 438;
        public const int tpi = 439, Tok_Pisin = 439;
        public const int tsi = 440, Tsimshian = 440;
        public const int tsn = 441, tn = 441, Tswana = 441;
        public const int tso = 442, ts = 442, Tsonga = 442;
        public const int tuk = 443, tk = 443, Turkmen = 443;
        public const int tum = 444, Tumbuka = 444;
        public const int tup = 445, Tupi_languages = 445;
        public const int tur = 446, tr = 446, Turkish = 446;
        public const int tut = 447, Altaic_languages = 447;
        public const int tvl = 448, Tuvalu = 448;
        public const int twi = 449, tw = 449, Twi = 449;
        public const int tyv = 450, Tuvinian = 450;
        public const int udm = 451, Udmurt = 451;
        public const int uga = 452, Ugaritic = 452;
        public const int uig = 453, ug = 453, Uighur = 453;
        public const int ukr = 454, uk = 454, Ukrainian = 454;
        public const int umb = 455, Umbundu = 455;
        public const int urd = 456, ur = 456, Urdu = 456;
        public const int uzb = 457, uz = 457, Uzbek = 457;
        public const int vai = 458, Vai = 458;
        public const int ven = 459, ve = 459, Venda = 459;
        public const int vie = 460, vi = 460, Vietnamese = 460;
        public const int vol = 461, vo = 461, Volapuk = 461;
        public const int vot = 462, Votic = 462;
        public const int wak = 463, Wakashan_languages = 463;
        public const int wal = 464, Wolaitta = 464;
        public const int war = 465, Waray = 465;
        public const int was = 466, Washo = 466;
        public const int wen = 467, Sorbian_languages = 467;
        public const int wln = 468, wa = 468, Walloon = 468;
        public const int wol = 469, wo = 469, Wolof = 469;
        public const int xal = 470, Kalmyk = 470;
        public const int xho = 471, xh = 471, Xhosa = 471;
        public const int yao = 472, Yao = 472;
        public const int yap = 473, Yapese = 473;
        public const int yid = 474, yi = 474, Yiddish = 474;
        public const int yor = 475, yo = 475, Yoruba = 475;
        public const int ypk = 476, Yupik_languages = 476;
        public const int zap = 477, Zapotec = 477;
        public const int zbl = 478, Blissymbols = 478;
        public const int zen = 479, Zenaga = 479;
        public const int zgh = 480, Standard_Moroccan_Tamazight = 480;
        public const int zha = 481, za = 481, Zhuang = 481;
        public const int znd = 482, Zande_languages = 482;
        public const int zul = 483, zu = 483, Zulu = 483;
        public const int zun = 484, Zuni = 484;
        public const int zxx = 485, No_linguistic_content = 485;
        public const int zza = 486, Zaza = 486;

        #endregion
    }
}
