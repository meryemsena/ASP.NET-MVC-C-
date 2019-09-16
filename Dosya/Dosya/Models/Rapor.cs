using System;
using System.Collections.Generic;

namespace Dosya.Models
{
    public class Rapor
    {
        public DateTime hIslemTarih
        {
            get; set;
        }
        public long hKartNo
        {
            get; set;
        }
        public long mTCKimlikNo
        {
            get; set;
        }
        public decimal hIslemTutariYI
        {
            get; set;
        }
        public string hHareketTipi
        {
            get; set;
        }
        public string iIslemAdi
        {
            get; set;
        }
        public string hIslemAciklamasi
        {
            get; set;
        }
        public string hMerchName
        {
            get; set;
        }
    }
}