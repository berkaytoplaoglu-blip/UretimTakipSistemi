using System.Web;

namespace UretimTakipSistemi.Helpers
{
    public static class SessionHelper
    {
        private const string SESSION_KULLANICI_ID = "KullaniciId";
        private const string SESSION_KULLANICI_ADI = "KullaniciAdi";
        private const string SESSION_AD_SOYAD = "AdSoyad";
        private const string SESSION_ROL = "Rol";

        public static bool IsLoggedIn
        {
            get
            {
                return HttpContext.Current?.Session?[SESSION_KULLANICI_ID] != null;
            }
        }

        public static int KullaniciId
        {
            get
            {
                if (HttpContext.Current?.Session?[SESSION_KULLANICI_ID] != null)
                    return (int)HttpContext.Current.Session[SESSION_KULLANICI_ID];
                return 0;
            }
            set
            {
                if (HttpContext.Current?.Session != null)
                    HttpContext.Current.Session[SESSION_KULLANICI_ID] = value;
            }
        }

        public static string KullaniciAdi
        {
            get
            {
                return HttpContext.Current?.Session?[SESSION_KULLANICI_ADI]?.ToString() ?? "";
            }
            set
            {
                if (HttpContext.Current?.Session != null)
                    HttpContext.Current.Session[SESSION_KULLANICI_ADI] = value;
            }
        }

        public static string AdSoyad
        {
            get
            {
                return HttpContext.Current?.Session?[SESSION_AD_SOYAD]?.ToString() ?? "";
            }
            set
            {
                if (HttpContext.Current?.Session != null)
                    HttpContext.Current.Session[SESSION_AD_SOYAD] = value;
            }
        }

        public static string Rol
        {
            get
            {
                return HttpContext.Current?.Session?[SESSION_ROL]?.ToString() ?? "";
            }
            set
            {
                if (HttpContext.Current?.Session != null)
                    HttpContext.Current.Session[SESSION_ROL] = value;
            }
        }

        public static bool IsAdmin => Rol == "Admin";
        public static bool IsBoyahane => Rol == "Boyahane";
        public static bool IsIcParca => Rol == "IcParca";
        public static bool IsEnjeksiyon => Rol == "Enjeksiyon";

        public static void Clear()
        {
            if (HttpContext.Current?.Session == null) return;
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }

        public static void SetUser(int kullaniciId, string kullaniciAdi, string adSoyad, string rol)
        {
            KullaniciId = kullaniciId;
            KullaniciAdi = kullaniciAdi;
            AdSoyad = adSoyad;
            Rol = rol;
        }
    }
}
