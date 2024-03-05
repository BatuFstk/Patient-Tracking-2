﻿using Google.Cloud.Firestore;
using Patient_Follow_Up.Class;
using Patient_Tracking.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;


namespace Patient_Tracking
{
    public partial class Anasayfa : Form
    {
        private UserData loggedInUser;
        public Anasayfa(UserData loggedInUser)
        {
            InitializeComponent();

            this.loggedInUser = loggedInUser;




            HastalariGoruntule.Columns.Add("Tc", "TC");
            HastalariGoruntule.Columns.Add("Name", "İsim");
            HastalariGoruntule.Columns.Add("Surname", "Soyisim");
            HastalariGoruntule.Columns.Add("Birth", "Doğum Tarihi");
            HastalariGoruntule.Columns.Add("Gender", "Cinsiyet");
            HastalariGoruntule.Columns.Add("Barcode", "Barkod");
            HastalariGoruntule.Columns.Add("EntryDate", "Giriş Tarihi");
            HastalariGoruntule.Columns.Add("AcceptDate", "Onay Tarihi");
            HastalariGoruntule.Columns.Add("ResultDate", "Sonuç Tarihi");


            HastalariGoruntule.AutoGenerateColumns = false;


            UpdateDataGridView();

            Welcomelabel.Text = "Hoşgeldiniz , Sayın Dr. " + loggedInUser.DoktorName + " " + loggedInUser.DoktorSurname;


        }
        
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private async void SilButton_Click(object sender, EventArgs e)
        {
            if (HastalariGoruntule.SelectedRows.Count > 0)
            {

                int selectedRowIndex = HastalariGoruntule.SelectedRows[0].Index;


                string tcToDelete = Convert.ToString(HastalariGoruntule.Rows[selectedRowIndex].Cells["Tc"].Value);


                DocumentReference docRefToDelete = FirestoreHelper.Database
                    .Collection("UserData").Document(loggedInUser.Username)
                    .Collection("Patients").Document(tcToDelete);

                try
                {

                    await docRefToDelete.DeleteAsync();


                    UpdateDataGridView();

                    MessageBox.Show("Hasta verisi silindi.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bir hata oluştu: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz hastayı seçin.");
            }


        }

        private async void EkleButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Hastatc.Text) || string.IsNullOrEmpty(Hastaisim.Text) || string.IsNullOrEmpty(Hastasoyisim.Text) || CinsiyetCombobox.SelectedIndex == -1 || string.IsNullOrEmpty(Hastabarkodno.Text))
            {
                MessageBox.Show("TC Kimlik No, İsim, Soyisim, Cinsiyet ve Barkod alanları eksiksiz doldurulmalıdır!", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Hastatc.Text.Length != 11)
            {
                MessageBox.Show("TC kimlik numarası 11 haneli olmalıdır.", "Geçersiz TC Kimlik No!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var patientData = new PatientData
            {
                Tc = Hastatc.Text,
                Name = Hastaisim.Text,
                Surname = Hastasoyisim.Text,

                Birth = DateTime.SpecifyKind(Hastadogumdatetime.Value, DateTimeKind.Utc),
                Gender = CinsiyetCombobox.SelectedIndex,
                Barcode = Hastabarkodno.Text,

                EntryDate = DateTime.SpecifyKind(hastagiristarihDatetime.Value, DateTimeKind.Utc),

                AcceptDate = DateTime.SpecifyKind(HastaOnayDateTime.Value, DateTimeKind.Utc),

                ResultDate = DateTime.SpecifyKind(hastasonuctarihidate.Value, DateTimeKind.Utc),
            };


            DocumentReference patientRef = FirestoreHelper.Database
                .Collection("UserData").Document(loggedInUser.Username)
                .Collection("Patients").Document(patientData.Tc);

            try
            {

                await patientRef.SetAsync(patientData);

                MessageBox.Show("Hasta verisi eklendi.");


                UpdateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }

        }
        public async void UpdateDataGridView()
        {

            var patientsQuery = await FirestoreHelper.Database
                .Collection("UserData").Document(loggedInUser.Username)
                .Collection("Patients").GetSnapshotAsync();


            HastalariGoruntule.DataSource = null;
            HastalariGoruntule.Rows.Clear();


            foreach (var patientDocument in patientsQuery.Documents)
            {
                var patientData = patientDocument.ConvertTo<PatientData>();
                HastalariGoruntule.Rows.Add(patientData.Tc, patientData.Name, patientData.Surname, patientData.Birth, patientData.Gender, patientData.Barcode, patientData.EntryDate, patientData.AcceptDate, patientData.ResultDate);
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private async void AllDeleteButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Tüm hasta verilerini silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);


            if (result == DialogResult.Yes)
            {

                CollectionReference patientsCollection = FirestoreHelper.Database
                    .Collection("UserData").Document(loggedInUser.Username)
                    .Collection("Patients");

                try
                {

                    var patientsQuery = await patientsCollection.GetSnapshotAsync();


                    if (patientsQuery.Count > 0)
                    {

                        foreach (var patientDocument in patientsQuery.Documents)
                        {
                            await patientDocument.Reference.DeleteAsync();
                        }


                        UpdateDataGridView();

                        MessageBox.Show("Tüm hasta verileri silindi.");
                    }
                    else
                    {
                        MessageBox.Show("Silinecek hasta verisi bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bir hata oluştu: " + ex.Message);
                }
            }
        }

        private async void Guncelle_Click(object sender, EventArgs e)
        {
            if (HastalariGoruntule.SelectedRows.Count > 0)
            {
                int selectedRowIndex = HastalariGoruntule.SelectedRows[0].Index;
                string tcToUpdate = Convert.ToString(HastalariGoruntule.Rows[selectedRowIndex].Cells["Tc"].Value);

                // İlgili hasta verisinin referansını al
                DocumentReference docRefToUpdate = FirestoreHelper.Database
                    .Collection("UserData").Document(loggedInUser.Username)
                    .Collection("Patients").Document(tcToUpdate);

                // Güncellenecek hasta verisinin mevcut değerlerini al
                var existingPatient = (await docRefToUpdate.GetSnapshotAsync()).ConvertTo<PatientData>();

                // Yeni değerleri kullanıcıdan al veya mevcut değerleri kullanmasını sağla
                var updatedPatient = new PatientData
                {
                    Tc = existingPatient.Tc,
                    Name = string.IsNullOrEmpty(Hastaisim.Text) ? existingPatient.Name : Hastaisim.Text,
                    Surname = string.IsNullOrEmpty(Hastasoyisim.Text) ? existingPatient.Surname : Hastasoyisim.Text,
                    Birth = DateTime.SpecifyKind(Hastadogumdatetime.Value, DateTimeKind.Utc),
                    Gender = CinsiyetCombobox.SelectedIndex == -1 ? existingPatient.Gender : CinsiyetCombobox.SelectedIndex,
                    Barcode = string.IsNullOrEmpty(Hastabarkodno.Text) ? existingPatient.Barcode : Hastabarkodno.Text,
                    EntryDate = DateTime.SpecifyKind(hastagiristarihDatetime.Value, DateTimeKind.Utc),
                    AcceptDate = DateTime.SpecifyKind(HastaOnayDateTime.Value, DateTimeKind.Utc),
                    ResultDate = DateTime.SpecifyKind(hastasonuctarihidate.Value, DateTimeKind.Utc),
                };

                try
                {
                    // Hasta verisini güncelle
                    await docRefToUpdate.SetAsync(updatedPatient);

                    // DataGridView'ı güncelle
                    UpdateDataGridView();

                    MessageBox.Show("Hasta verisi güncellendi.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bir hata oluştu: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz hastayı seçin.");
            }
        }
        private void Anasayfa_Load(object sender, EventArgs e)
        {

        }

        private void SifreDegistirme_Click(object sender, EventArgs e)
        {
            SifreChange sifreChangeForm = new SifreChange(loggedInUser);

            // Bu formu gizleyin
            

            // SifreChange formunu gösterin
            sifreChangeForm.Show();
        }

        private async void basincdeger_Click(object sender, EventArgs e)
        {
            // Bluetooth bağlantısı kontrolü yap
            List<BluetoothDeviceInfo> connectedDevices = GetConnectedBluetoothDevices();

            if (connectedDevices.Count > 0)
            {
                // Kullanıcının seçtiği hastanın TC kimlik numarasını al
                string selectedPatientTc = "";

                if (HastalariGoruntule.SelectedRows.Count > 0)
                {
                    int selectedRowIndex = HastalariGoruntule.SelectedRows[0].Index;
                    selectedPatientTc = Convert.ToString(HastalariGoruntule.Rows[selectedRowIndex].Cells["Tc"].Value);
                }
                else
                {
                    MessageBox.Show("Lütfen basınç değeri eklemek istediğiniz hastayı seçin.", "Hasta Seçimi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Random bir basınç değeri oluştur (150 ila 300 arasında)
                Random random = new Random();
                int pressureValue = random.Next(150, 301);

                // Veritabanına kaydetme işlemi
                await KaydetBasincDeger(selectedPatientTc, pressureValue);

                MessageBox.Show($"Basınç değeri {pressureValue} hastanın veritabanına kaydedildi.", "Basınç Değeri Kaydedildi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Bağlı Bluetooth cihaz bulunamadı.", "Bluetooth Bağlantısı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private List<BluetoothDeviceInfo> GetConnectedBluetoothDevices()
        {
            List<BluetoothDeviceInfo> connectedDevices = new List<BluetoothDeviceInfo>();

            try
            {
                BluetoothClient bluetoothClient = new BluetoothClient();

                // Bağlı cihazları al
                IReadOnlyCollection<BluetoothDeviceInfo> devices = bluetoothClient.DiscoverDevices();

                foreach (BluetoothDeviceInfo device in devices)
                {
                    // Cihaz bağlıysa ekle
                    if (device.Connected)
                    {
                        connectedDevices.Add(device);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bluetooth cihazları alınırken bir hata oluştu: " + ex.Message, "Bluetooth Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return connectedDevices;
        }

        private async Task KaydetBasincDeger(string tc, int pressureValue)
        {
            // Veritabanına basınç değerini kaydetme işlemi
            DocumentReference patientRef = FirestoreHelper.Database
                .Collection("UserData").Document(loggedInUser.Username)
                .Collection("Patients").Document(tc);

            try
            {
                // Mevcut hastanın verilerini al
                var snapshot = await patientRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    await patientRef.UpdateAsync("BasincDegeri", FieldValue.ArrayUnion(pressureValue));

                    MessageBox.Show($"Basınç değeri {pressureValue} hastanın veritabanına kaydedildi.", "Basınç Değeri Kaydedildi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Hasta bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Basınç değeri kaydedilirken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }









    }
}
