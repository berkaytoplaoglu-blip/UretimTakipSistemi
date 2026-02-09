using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            litKullanici.Text = $"👤 {SessionHelper.AdSoyad}";
            BuildMenu();
        }

        private void BuildMenu()
        {
            menuList.Controls.Clear();


            if (SessionHelper.IsAdmin)
            {
                AddMenuSection("📊 YÖNETİM");
                AddMenuItem("Dashboard", "~/Admin/Dashboard.aspx", "📊 Dashboard");
                AddMenuItem("Parça Listesi", "~/Admin/ParcaListesi.aspx", "📦 Parça Listesi");
                AddMenuItem("İş Emri Aç", "~/Admin/IsEmriAc.aspx", "➕ İş Emri Aç");
                AddMenuItem("İş Emirleri", "~/Admin/IsEmirleri.aspx", "📋 İş Emirleri");
                AddMenuItem("Kullanıcı Yönetimi", "~/Admin/KullaniciYonetimi.aspx", "👥 Kullanıcılar");

                AddMenuSection("📈 RAPORLAR");
                AddMenuItem("Temel Raporlar", "~/Admin/Raporlar.aspx", "📄 Temel Raporlar");
                AddMenuItem("Gelişmiş Analiz", "~/Admin/RaporlarGelismis.aspx", "📊 Gelişmiş Analiz");
                AddMenuItem("Fire Raporu", "~/Admin/FireRaporu.aspx", "🔥 Fire Raporu");

                AddMenuSection("🏭 ÜRETİM");
                AddMenuItem("Boyahane Üretim", "~/Boyahane/BoyahaneUretim.aspx", "🎨 Boyahane Üretim");
                AddMenuItem("Boyahane İş Emirleri", "~/Boyahane/BoyahaneIsEmirleri.aspx", "📋 Boyahane İş Takip");  // YENİ
                AddMenuItem("İç Parça Üretim", "~/IcParca/IcParcaUretim.aspx", "🔧 İç Parça Üretim");
                AddMenuItem("İç Parça İş Emirleri", "~/IcParca/IcParcaIsEmirleri.aspx", "📋 İç Parça İş Takip");  // YENİ
                AddMenuItem("Enjeksiyon Terminal", "~/Enjeksiyon/EnjeksiyonTerminal.aspx", "⚙️ Enjeksiyon Terminal");
                AddMenuItem("Enjeksiyon Manuel", "~/Enjeksiyon/ManuelGiris.aspx", "✍️ Enjeksiyon Manuel");
            }
            else if (SessionHelper.IsBoyahane)
            {
                AddMenuSection("🎨 BOYAHANE");
                AddMenuItem("Üretim Giriş", "~/Boyahane/BoyahaneUretim.aspx", "📝 Üretim");
                AddMenuItem("İş Emirleri", "~/Boyahane/BoyahaneIsEmirleri.aspx", "📋 İş Takip");  // YENİ
            }
            else if (SessionHelper.IsIcParca)
            {
                AddMenuSection("🔧 MONTAJ - İÇ PARÇA");
                AddMenuItem("Üretim Giriş", "~/IcParca/IcParcaUretim.aspx", "📝 Üretim");
                AddMenuItem("İş Emirleri", "~/IcParca/IcParcaIsEmirleri.aspx", "📋 İş Takip");  // YENİ
            }
            else if (SessionHelper.IsEnjeksiyon)
            {
                AddMenuSection("⚙️ ENJEKSİYON");
                AddMenuItem("Terminal", "~/Enjeksiyon/EnjeksiyonTerminal.aspx", "🖥️ Terminal");
                AddMenuItem("Manuel Giriş", "~/Enjeksiyon/ManuelGiris.aspx", "✍️ Manuel Giriş");
            }
        }

        private void AddMenuSection(string title)
        {
            HtmlGenericControl section = new HtmlGenericControl("div");
            section.Attributes["class"] = "menu-section";

            HtmlGenericControl sectionTitle = new HtmlGenericControl("div");
            sectionTitle.Attributes["class"] = "menu-section-title";
            sectionTitle.InnerText = title;

            section.Controls.Add(sectionTitle);
            menuList.Controls.Add(section);
        }

        private void AddMenuItem(string key, string url, string text)
        {
            HtmlAnchor anchor = new HtmlAnchor();
            anchor.HRef = ResolveUrl(url);
            anchor.InnerText = text;
            anchor.Attributes["class"] = "menu-item";

            string currentPage = Request.Url.AbsolutePath.ToLower();
            string targetPage = ResolveUrl(url).ToLower();

            if (currentPage.Contains(targetPage.Replace("~", "")))
            {
                anchor.Attributes["class"] = "menu-item active";
            }

            HtmlGenericControl lastSection = menuList.Controls[menuList.Controls.Count - 1] as HtmlGenericControl;
            if (lastSection != null)
            {
                lastSection.Controls.Add(anchor);
            }
        }

        protected void btnCikis_Click(object sender, EventArgs e)
        {
            SessionHelper.Clear();
            Response.Redirect("~/Login.aspx");
        }
    }
}