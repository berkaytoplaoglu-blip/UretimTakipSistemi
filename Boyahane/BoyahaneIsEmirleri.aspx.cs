using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Boyahane
{
    public partial class BoyahaneIsEmirleri : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!SessionHelper.IsAdmin && !SessionHelper.IsBoyahane)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadIsEmirleri();
            }
        }

        private void LoadIsEmirleri()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    string query = @"
                        SELECT 
                            ie.IsEmriId,
                            ie.IsEmriNo,
                            ie.UrunAdi,
                            ie.UrunParcaKodu,
                            ie.Grami,
                            ie.KalipNo,
                            ISNULL(ie.BoyahaneDurum, 'YENİ') AS BoyahaneDurum,
                            ie.OlusturmaTarihi
                        FROM IsEmirleri ie
                        WHERE ie.Boyahane = 1 
                            AND ie.Durum = 'ACTIVE'
                        ORDER BY 
                            CASE ISNULL(ie.BoyahaneDurum, 'YENİ')
                                WHEN 'YENİ' THEN 1
                                WHEN 'AKTİF' THEN 2
                                WHEN 'TAMAMLANDI' THEN 3
                                ELSE 4
                            END,
                            ie.OlusturmaTarihi DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptIsEmirleri.DataSource = dt;
                            rptIsEmirleri.DataBind();
                            pnlBos.Visible = false;
                        }
                        else
                        {
                            rptIsEmirleri.DataSource = null;
                            rptIsEmirleri.DataBind();
                            pnlBos.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Hata: " + ex.Message, false);
            }
        }

        protected void rptIsEmirleri_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView row = (DataRowView)e.Item.DataItem;
                string durum = row["BoyahaneDurum"].ToString();

                // Durum badge oluştur
                Literal litDurumBadge = (Literal)e.Item.FindControl("litDurumBadge");
                litDurumBadge.Text = GetDurumBadgeHtml(durum);

                // Dropdown'a doğru değeri set et
                DropDownList ddlDurum = (DropDownList)e.Item.FindControl("ddlDurum");
                if (ddlDurum != null)
                {
                    ListItem item = ddlDurum.Items.FindByValue(durum);
                    if (item != null)
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        // Eğer değer yoksa YENİ seç
                        ddlDurum.SelectedValue = "YENİ";
                    }
                }
            }
        }

        protected void rptIsEmirleri_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Guncelle")
            {
                int isEmriId = Convert.ToInt32(e.CommandArgument);
                DropDownList ddlDurum = (DropDownList)e.Item.FindControl("ddlDurum");
                string yeniDurum = ddlDurum.SelectedValue;

                GuncelleDurum(isEmriId, yeniDurum);
            }
        }

        private void GuncelleDurum(int isEmriId, string yeniDurum)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    string query = @"
                        UPDATE IsEmirleri 
                        SET BoyahaneDurum = @BoyahaneDurum
                        WHERE IsEmriId = @IsEmriId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IsEmriId", isEmriId);
                        cmd.Parameters.AddWithValue("@BoyahaneDurum", yeniDurum);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        ShowMessage($"✅ İş emri durumu '{yeniDurum}' olarak güncellendi.", true);
                        LoadIsEmirleri();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Güncelleme hatası: " + ex.Message, false);
            }
        }

        private string GetDurumBadgeHtml(string durum)
        {
            string cssClass = "";
            string text = durum;

            switch (durum)
            {
                case "YENİ":
                    cssClass = "badge-yeni";
                    break;
                case "AKTİF":
                    cssClass = "badge-aktif";
                    break;
                case "TAMAMLANDI":
                    cssClass = "badge-tamamlandi";
                    break;
                default:
                    cssClass = "badge-bos";
                    text = "YENİ";
                    break;
            }

            return $"<span class='badge {cssClass}'>{text}</span>";
        }

        private void ShowMessage(string message, bool success)
        {
            litMesaj.Text = $"<div class='alert {(success ? "alert-success" : "alert-error")}'>{message}</div>";
            pnlMesaj.Visible = true;

            ScriptManager.RegisterStartupScript(this, GetType(), "hideMsg",
                "setTimeout(function(){ var panel = document.getElementById('" + pnlMesaj.ClientID + "'); if(panel) panel.style.display='none'; }, 5000);",
                true);
        }
    }
}