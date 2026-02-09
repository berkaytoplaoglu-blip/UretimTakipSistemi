using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Services;
using System.Web.UI;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Admin
{
    public partial class Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn || !SessionHelper.IsAdmin)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
        }

        [WebMethod(EnableSession = true)]
        public static string GetDashboardData()
        {
            try
            {
                StringBuilder json = new StringBuilder();
                json.Append("{");

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_DashboardKPI", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            json.AppendFormat("\"ToplamUretim\":{0},", reader["ToplamUretim"]);
                            json.AppendFormat("\"ToplamFire\":{0},", reader["ToplamFire"]);
                            json.AppendFormat("\"FireOrani\":{0},", reader["FireOrani"].ToString().Replace(",", "."));
                            json.AppendFormat("\"AktifMakine\":{0},", reader["AktifMakine"]);
                            json.AppendFormat("\"ToplamMakine\":{0},", reader["ToplamMakine"]);
                        }
                        reader.Close();
                    }

                    json.Append("\"Makineler\":[");
                    using (SqlCommand cmd = new SqlCommand("sp_DashboardMakineDurumlari", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlDataReader reader = cmd.ExecuteReader();

                        bool first = true;
                        while (reader.Read())
                        {
                            if (!first) json.Append(",");
                            first = false;

                            json.Append("{");
                            json.AppendFormat("\"MakineId\":{0},", reader["MakineId"]);
                            json.AppendFormat("\"MakineAdi\":\"{0}\",", reader["MakineAdi"]);
                            json.AppendFormat("\"Durum\":\"{0}\",", reader["Durum"]);
                            json.AppendFormat("\"Personel\":\"{0}\",", reader["Personel"] != DBNull.Value ? reader["Personel"] : "");
                            json.AppendFormat("\"IsEmriNo\":\"{0}\"", reader["IsEmriNo"] != DBNull.Value ? reader["IsEmriNo"] : "");
                            json.Append("}");
                        }
                        reader.Close();
                    }
                    json.Append("]");
                }

                json.Append("}");
                return json.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Dashboard Data Hatası: " + ex.Message);
                return "{}";
            }
        }
    }
}