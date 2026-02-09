namespace UretimTakipSistemi.Api.Models
{
    public class EnjeksiyonIsEmriToplamDto
    {
        public string IsEmriNo { get; set; }
        public int UretimToplam { get; set; }
        public int FireToplam { get; set; }
        public string LastFinishTime { get; set; }
        public bool IsRunning { get; set; }
        public int ActiveSessionCount { get; set; }
        public string IsEmriDurum { get; set; }
    }
}
