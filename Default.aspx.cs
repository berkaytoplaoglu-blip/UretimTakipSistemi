using System;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (SessionHelper.IsAdmin)
            {
                Response.Redirect("~/Admin/Dashboard.aspx");
            }
            else if (SessionHelper.IsBoyahane)
            {
                Response.Redirect("~/Boyahane/BoyahaneUretim.aspx");
            }
            else if (SessionHelper.IsIcParca)
            {
                Response.Redirect("~/IcParca/IcParcaUretim.aspx");
            }
            else if (SessionHelper.IsEnjeksiyon)
            {
                Response.Redirect("~/Enjeksiyon/EnjeksiyonTerminal.aspx");
            }
            else
            {
                Response.Redirect("~/Login.aspx");
            }
        }
    }
}