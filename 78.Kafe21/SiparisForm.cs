﻿using Kafe21.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _78.Kafe21
{
    public partial class SiparisForm : Form
    {
        public event EventHandler<MasaTasindiEventArgs> MasaTasindi;

        KafeVeri db;
        Siparis siparis;
        BindingList<SiparisDetay> blSiparisDetaylar;
        public SiparisForm(KafeVeri kafeveri, Siparis siparis)
        {
            db = kafeveri;
            this.siparis = siparis; //class seviyesinde olusana local tanımlananı ata.
            InitializeComponent();
            dgvSiparisDetaylar.AutoGenerateColumns = false; // kendi columns eklemesin diye ama sadece bu yeterli değil. desing da dgv edit columns dan dataproproperyname lerini urun sınıfındaki ismlerle değiştiriyoruz.
            MasaNolariGuncelle();
            OdemeTutariGuncelle();
            UrunleriEkle();
            SiparisDetaylariYukle();
            tsslAcilisZamani.Text = "Açılış Zamanı: " + siparis.AcilisZamani.ToString();
        }

        private void SiparisDetaylariYukle()
        {
            blSiparisDetaylar = new BindingList<SiparisDetay>(siparis.SiparisDetaylar); //YÖNTEM-2
            dgvSiparisDetaylar.DataSource = blSiparisDetaylar;
        }

        private void UrunleriEkle()
        {
            cboUrun.DataSource = db.Urunler;
        }

        private void OdemeTutariGuncelle()
        {
            lblOdemeTutari.Text = siparis.ToplamTutarTL;
        }

        private void MasaNolariGuncelle()
        {
            Text = $"Masa {siparis.MasaNo:00} - Sipariş";
            lblMasaNo.Text = siparis.MasaNo.ToString("00");

            cboMasaNo.DataSource = Enumerable
                .Range(1, db.MasaAdet)
                .Where(x => !db.AktifSiparisler.Any(s => s.MasaNo ==x))
                .ToList();
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            Urun urun = (Urun)cboUrun.SelectedItem;
            int adet = (int)nudAdet.Value;

            blSiparisDetaylar.Add(new SiparisDetay()
            {
                UrunAd = urun.UrunAd,
                BirimFiyat = urun.BirimFiyat,
                Adet = adet
            });
            //dgvSiparisDetaylar.DataSource = null;
            //dgvSiparisDetaylar.DataSource = siparis.SiparisDetaylar; //YÖNTEM-1
            OdemeTutariGuncelle();

        }

        private void dgvSiparisDetaylar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && dgvSiparisDetaylar.SelectedRows.Count > 0)
            {
                DialogResult dr = MessageBox.Show("Seçili detay siparişten kaldırılacaktır. Onaylıyor musunuz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Yes)
                {
                    DataGridViewRow satir = dgvSiparisDetaylar.SelectedRows[0]; // 0 ilk seçilen demek.
                    SiparisDetay sd = (SiparisDetay)satir.DataBoundItem;
                    blSiparisDetaylar.Remove(sd);
                    OdemeTutariGuncelle();
                }
            }
        }

        private void btnAnasayfa_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;//her formun bir dialogresult u olur o yüzden yeni bir türl oluşturmaya gerek yok.
        }

        private void btnSiparisIptal_Click(object sender, EventArgs e)
        {
            SiparisKapat(SiparisDurum.Iptal);
        }

        private void btnOdemeAl_Click(object sender, EventArgs e)
        {
            SiparisKapat(SiparisDurum.Odendi);

        }
        private void SiparisKapat(SiparisDurum durum)
        {
            if (durum == SiparisDurum.Odendi)
                siparis.OdenenTutar = siparis.ToplamTutar();
            siparis.KapanisZamani = DateTime.Now;
            siparis.Durum = durum;
            db.AktifSiparisler.Remove(siparis);
            db.GecmisSiparisler.Add(siparis);
            DialogResult = DialogResult.OK; // bu formda işim bitti demek.
        }

        private void btnMasaTasi_Click(object sender, EventArgs e)
        {
            if (cboMasaNo.SelectedIndex > -1)
            {
                int eskiMasaNo = siparis.MasaNo;
                int yeniMasaNo = (int)cboMasaNo.SelectedItem;
                siparis.MasaNo = yeniMasaNo;
                MasaNolariGuncelle();

                if (MasaTasindi != null)
                {
                    MasaTasindi(this, new MasaTasindiEventArgs(eskiMasaNo,yeniMasaNo));
                }
            }
        }
    }
}
