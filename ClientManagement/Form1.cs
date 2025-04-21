using Guna.UI2.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using System.Media;
using System.Drawing;




namespace ClientManagement
{
    public partial class Form1 : Form
    {
        private string currentUserName;

        SoundPlayer player;
        public Form1(string userName)
        {
            InitializeComponent();
            currentUserName = userName;
            LoadData();
            bookingLoadData();
            WS_LoadData();
            player = new SoundPlayer("background.wav");
            player.PlayLooping();

            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            guna2DateTimePicker1.Value = DateTime.Now;
            guna2DateTimePicker2.Value = DateTime.Now;
        }

        static string connectionstring = "Data Source=BESHOY;Initial Catalog=ClientManagement;Integrated Security=True;";

        private void guna2Button1_Click_2(object sender, EventArgs e)
        {
            string Name = name.Text.Trim();
            string Phone = phone.Text.Trim();
            string Email = email.Text.Trim();

            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Phone) || string.IsNullOrEmpty(Email))
            {
                MessageBox.Show("يرجى ملء جميع الحقول!", "معلومات ناقصة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Email.Contains("@") || !Email.Contains("."))
            {
                MessageBox.Show("صيغة البريد الإلكتروني غير صالحة!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Clients WHERE Email = @Email OR Phone = @Phone";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", Email);
                        checkCmd.Parameters.AddWithValue("@Phone", Phone);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("البريد الإلكتروني أو الهاتف مسجل مسبقًا!", "إدخال مكرر", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    string query = "INSERT INTO Clients (Name, Phone, Email) VALUES (@Name, @Phone, @Email)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", Name);
                        cmd.Parameters.AddWithValue("@Phone", Phone);
                        cmd.Parameters.AddWithValue("@Email", Email);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("تم إضافة العميل بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadData();
                    name.Text = "";
                    phone.Text = "";
                    email.Text = "";
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("خطأ في قاعدة البيانات: " + ex.Message, "خطأ SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ غير متوقع: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadData()
        {
            greetingLabel.Text = $"مرحبًا بك، {currentUserName}";
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string selectQuery = "SELECT ClientID, Name, Phone, Email FROM Clients ORDER BY ClientID ASC";
                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في تحميل بيانات العملاء: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void guna2Button3_Click_2(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "هل أنت متأكد أنك تريد حذف جميع سجلات العملاء؟ لا يمكن التراجع عن هذا الإجراء.",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM Clients; DBCC CHECKIDENT ('Clients', RESEED, 0);";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("تم حذف جميع سجلات العملاء بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطأ أثناء حذف السجلات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void guna2Button2_Click_2(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string clientId = dataGridView1.SelectedRows[0].Cells["ClientID"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    "هل أنت متأكد أنك تريد حذف هذا العميل؟",
                    "تأكيد الحذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(connectionstring))
                    {
                        try
                        {
                            conn.Open();
                            string query = "DELETE FROM Clients WHERE clientId = @ClientID";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@ClientID", clientId);
                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("تم حذف العميل بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show("لم يتم العثور على العميل!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("خطأ أثناء الحذف: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("يرجى تحديد عميل لحذفه.", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
        }

        private void email_TextChanged(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void phone_TextChanged(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void name_TextChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool IsBookingOverlapping(int workspaceId, DateTime startTime, DateTime endTime)
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT COUNT(*) FROM Bookings 
                        WHERE WorkspaceID = @WorkspaceID 
                        AND (StartTime < @EndTime AND EndTime > @StartTime)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkspaceID", workspaceId);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء التحقق من الحجز: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(CL_ID.Text.Trim(), out int clientId) || !int.TryParse(WS_ID.Text.Trim(), out int workspaceId))
                {
                    MessageBox.Show("يرجى إدخال أرقام صحيحة لمعرف العميل ومعرف المساحة.", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime startTime = guna2DateTimePicker1.Value;
                double numberOfHours = (double)guna2NumericUpDown1.Value;

                if (numberOfHours <= 0)
                {
                    MessageBox.Show("يجب اختيار عدد ساعات أكبر من صفر.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime endTime = startTime.AddHours(numberOfHours);

                if (startTime >= endTime)
                {
                    MessageBox.Show("وقت البدء يجب أن يكون قبل وقت الانتهاء!", "خطأ في الوقت", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!DoesClientExist(clientId))
                {
                    MessageBox.Show("معرف العميل غير موجود. يرجى التحقق من الرقم.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!DoesWorkspaceExist(workspaceId))
                {
                    MessageBox.Show("معرف المساحة غير موجود. يرجى التحقق من الرقم.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (IsBookingOverlapping(workspaceId, startTime, endTime))
                {
                    MessageBox.Show("المساحة محجوزة بالفعل في هذا الوقت. يرجى اختيار وقت آخر.", "حجز متداخل", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(newPrice.Text.Trim(), out decimal hourlyRate) || hourlyRate <= 0)
                {
                    MessageBox.Show("يرجى إدخال سعر صحيح وأكبر من صفر.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal totalPrice = (decimal)numberOfHours * hourlyRate;
                decimal Price = (decimal)numberOfHours * hourlyRate;

                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    conn.Open();
                    try
                    {
                        string bookingQuery = @"INSERT INTO Bookings (ClientID, WorkspaceID, StartTime, EndTime, Price)
                                               VALUES (@ClientID, @WorkspaceID, @StartTime, @EndTime, @Price)";
                        using (SqlCommand cmd = new SqlCommand(bookingQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@ClientID", clientId);
                            cmd.Parameters.AddWithValue("@WorkspaceID", workspaceId);
                            cmd.Parameters.AddWithValue("@StartTime", startTime);
                            cmd.Parameters.AddWithValue("@EndTime", endTime);
                            cmd.Parameters.AddWithValue("@Price", Price);
                            cmd.ExecuteNonQuery();
                        }

                        string wsQuery = "UPDATE Workspaces SET Price = @Price WHERE WorkspaceID = @WorkspaceID";
                        using (SqlCommand cmd = new SqlCommand(wsQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@WorkspaceID", workspaceId);
                            cmd.Parameters.AddWithValue("@Price", hourlyRate);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show($"تم تأكيد الحجز! المبلغ الإجمالي: {totalPrice:F2} جنيه", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        bookingLoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطأ أثناء تأكيد الحجز: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ عام أثناء الحجز: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MarkAsNotified(int bookingId)
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                conn.Open();
                string updateQuery = "UPDATE Bookings SET Notified = 1 WHERE BookingID = @ID";
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                cmd.Parameters.AddWithValue("@ID", bookingId);
                cmd.ExecuteNonQuery();
            }
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                conn.Open();
                string query = "SELECT BookingID, ClientID, EndTime FROM Bookings WHERE EndTime <= GETDATE() AND Notified = 0";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int bookingId = reader.GetInt32(0);
                    int clientId = reader.GetInt32(1);
                    DateTime endTime = reader.GetDateTime(2);


                    MessageBox.Show($"انتهى حجز العميل رقم {clientId} في {endTime}", "تنبيه الحجز", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    MarkAsNotified(bookingId);
                }

                reader.Close();
            }
        }


        private bool DoesClientExist(int clientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Clients WHERE ClientID = @ClientID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", clientId);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء التحقق من العميل: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private bool DoesWorkspaceExist(int workspaceId)
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Workspaces WHERE WorkspaceID = @WorkspaceID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkspaceID", workspaceId);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء التحقق من المساحة: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private void bookingLoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string selectQuery = "SELECT BookingID, ClientID, WorkspaceID, StartTime, EndTime, Price FROM Bookings ORDER BY BookingID ASC";
                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView2.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في تحميل بيانات الحجوزات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string selectQuery = "SELECT BookingID, ClientID, WorkspaceID, StartTime, EndTime, Price FROM Bookings ORDER BY BookingID ASC";
                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView2.DataSource = dt;
                    MessageBox.Show("تم تحديث بيانات الحجوزات بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في تحميل بيانات الحجوزات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "هل أنت متأكد أنك تريد حذف جميع سجلات الحجوزات؟ لا يمكن التراجع عن هذا الإجراء.",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM Bookings; DBCC CHECKIDENT ('Bookings', RESEED, 0);";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("تم حذف جميع سجلات الحجوزات بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        bookingLoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطأ أثناء حذف الحجوزات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            player.Stop();
            Application.Exit();
        }

        private void CalculateBookingPrice()
        {
            decimal hourlyRate = 50;
            DateTime startTime = guna2DateTimePicker1.Value;
            DateTime endTime = guna2DateTimePicker2.Value;

            if (endTime <= startTime)
            {
                MessageBox.Show("يجب أن يكون وقت الانتهاء بعد وقت البدء!", "وقت غير صالح", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double totalHours = (endTime - startTime).TotalHours;
            decimal totalPrice = (decimal)totalHours * hourlyRate;

            MessageBox.Show($"التكلفة الإجمالية: {totalPrice:F2} جنيه", "سعر الحجز", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(CL_ID.Text.Trim(), out int clientId))
            {
                MessageBox.Show("يرجى إدخال معرف عميل صحيح.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM Bookings WHERE ClientID = @ClientID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", clientId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            MessageBox.Show("تم إلغاء الحجز بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBox.Show("لم يتم العثور على حجز لهذا العميل!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    bookingLoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء إلغاء الحجز: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void guna2NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void tabPage4_Click_1(object sender, EventArgs e)
        {
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void rememberMe_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void audioToggleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void guna2Button8_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newPrice.Text))
            {
                MessageBox.Show("يرجى إدخال السعر", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!decimal.TryParse(newPrice.Text.Trim(), out decimal hourlyRate) || hourlyRate <= 0)
                {
                    MessageBox.Show("يرجى إدخال سعر صحيح وأكبر من صفر.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Workspaces SET Price = @Price";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Price", hourlyRate);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("تم تعديل السعر بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء تعديل السعر: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2DateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {
        }

        private void audioToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (!audioToggle.Checked)
            {
                player.Stop();
                music_on.Image = Image.FromFile("Mute.png");

            }
            else
            {
                player.PlayLooping();
                music_on.Image = Image.FromFile("Audio.png");
            }
        }

        private void WS_LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string select2ndQuery = "SELECT *FROM Workspaces ORDER BY WorkspaceID ASC";
                    SqlDataAdapter adapter2 = new SqlDataAdapter(select2ndQuery, conn);
                    DataTable dt2 = new DataTable();
                    adapter2.Fill(dt2);
                    dataGridView3.DataSource = dt2;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في تحميل بيانات المساحات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void guna2Button9_Click_1(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string selectQuery = "SELECT * FROM Workspaces ORDER BY WorkspaceID ASC";
                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView3.DataSource = dt;
                    MessageBox.Show("تم تحديث بيانات المساحات بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في تحميل بيانات المساحات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {
            string status = sts.Text.Trim();
            string type = typ.Text.Trim();
            string priceText = price.Text.Trim();

            if (status == "" || type == "" || priceText == "")
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            decimal Price;
            if (!decimal.TryParse(priceText, out Price))
            {
                MessageBox.Show("Please enter a valid price.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Workspaces VALUES(@Status,@Type,@Price)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("Status", status);
                    cmd.Parameters.AddWithValue("Type", type);
                    cmd.Parameters.AddWithValue("Price", Price);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("تم إضافة مساحة عمل جديدة بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    WS_LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء إضافة مساحة عمل: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void pictureBox1_Click_2(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void guna2Button12_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
               "هل أنت متأكد أنك تريد حذف جميع المساحات؟ لا يمكن التراجع عن هذا الإجراء.",
               "تأكيد الحذف",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Warning
           );

            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM Workspaces; DBCC CHECKIDENT ('Workspaces', RESEED, 0);";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("تم حذف جميع المساحات بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        WS_LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطأ أثناء حذف المساحات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }

        private void guna2Button11_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count == 0)
            {
                MessageBox.Show("من فضلك اختر المساحة التي تريد حذفها.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int workspaceId = Convert.ToInt32(dataGridView3.SelectedRows[0].Cells["WorkspaceID"].Value);

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();

                    string checkBookingQuery = "SELECT COUNT(*) FROM Bookings WHERE WorkspaceID = @WorkspaceID";
                    using (SqlCommand checkCmd = new SqlCommand(checkBookingQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@WorkspaceID", workspaceId);
                        int bookingCount = (int)checkCmd.ExecuteScalar();

                        if (bookingCount > 0)
                        {
                            MessageBox.Show("لا يمكن حذف هذه المساحة لأنها محجوزة.", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string deleteQuery = "DELETE FROM Workspaces WHERE WorkspaceID = @WorkspaceID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@WorkspaceID", workspaceId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("تم حذف المساحة بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    WS_LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ أثناء الحذف: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void guna2Button13_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count == 0)
            {
                MessageBox.Show("من فضلك اختر صفاً واحداً على الأقل.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();

                    foreach (DataGridViewRow row in dataGridView3.SelectedRows)
                    {
                        int workspaceId = Convert.ToInt32(row.Cells["WorkspaceID"].Value);
                        string currentStatus = row.Cells["Status"].Value.ToString();

                        string newStatus = (currentStatus.ToLower() == "active") ? "Inactive" : "Active";

                        string query = "UPDATE Workspaces SET Status = @NewStatus WHERE WorkspaceID = @ID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                            cmd.Parameters.AddWithValue("@ID", workspaceId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("تم تحديث الحالات للصفوف المحددة.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    WS_LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ أثناء التعديل: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void music_on_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button14_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("هل تريد تسجيل الخروج بالفعل؟", "تسجيل خروج",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                login loginForm = new login();
                loginForm.Show();
                this.Hide();
            }
        }
        private void ApplyDarkMode(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is TabControl tabControl)
                {
                    tabControl.BackColor = Color.FromArgb(30, 30, 30);
                    tabControl.ForeColor = Color.White;

                    foreach (TabPage tab in tabControl.TabPages)
                    {
                        tab.BackColor = Color.FromArgb(30, 30, 30);
                        tab.ForeColor = Color.White;
                        ApplyDarkMode(tab.Controls);
                    }
                }
                else
                {
                    ctrl.ForeColor = Color.White;

                    if (!(ctrl is Button))
                        ctrl.BackColor = Color.FromArgb(45, 45, 45);

                    if (ctrl.HasChildren)
                        ApplyDarkMode(ctrl.Controls);
                }
            }
        }
        private void ApplyLightMode(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is TabControl tabControl)
                {
                    tabControl.BackColor = SystemColors.Control;
                    tabControl.ForeColor = SystemColors.ControlText;

                    foreach (TabPage tab in tabControl.TabPages)
                    {
                        tab.BackColor = SystemColors.Control;
                        tab.ForeColor = SystemColors.ControlText;
                        ApplyLightMode(tab.Controls);
                    }
                }
                else
                {
                    ctrl.ForeColor = SystemColors.ControlText;
                    ctrl.BackColor = SystemColors.Control;

                    if (ctrl.HasChildren)
                        ApplyLightMode(ctrl.Controls);
                }
            }
        }

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDarkMode.Checked)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                price_img.Image = Image.FromFile("Price Tag Euro oico.png");
                logout.Image = Image.FromFile("Logout_light.png");
                dark.Image = Image.FromFile("Sun.png");
                dark_mode.Text = "Light mode";
                user.Image = Image.FromFile("User.png");
                num.Image = Image.FromFile("Phone.png");
                mail.Image = Image.FromFile("Email.png");
                ApplyDarkMode(this.Controls);
            }
            else
            {
                this.BackColor = SystemColors.Control;
                price_img.Image = Image.FromFile("Price Tag Euro.png");
                logout.Image = Image.FromFile("Logout.png");
                dark.Image = Image.FromFile("Crescent Moon.png");
                dark_mode.Text = "Dark mode";
                user.Image = Image.FromFile("User2.png");
                num.Image = Image.FromFile("Phone2.png");
                mail.Image = Image.FromFile("Mail.png");
                ApplyLightMode(this.Controls);
            }
        }

        
    }
}

