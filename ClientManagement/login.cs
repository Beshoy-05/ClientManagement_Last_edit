using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ClientManagement
{
    public partial class login : Form
    {
        static string connectionstring = "Data Source=BESHOY;Initial Catalog=ClientManagement;Integrated Security=True;";

        private readonly string userFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClientManagement", "last_user.txt"
        );

        public login()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string enteredUsername = user_name.Text;
            string enteredPassword = pass.Text;

            string hashedPassword = ComputeSha256Hash(enteredPassword);

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                conn.Open();
                try
                {
                    string query = "SELECT Name FROM Employees WHERE UserName=@Username AND PasswordHash=@PasswordHash";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", enteredUsername);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string realName = result.ToString();

                        Directory.CreateDirectory(Path.GetDirectoryName(userFilePath));

                        if (rememberMe.Checked)
                        {
                            File.WriteAllText(userFilePath, enteredUsername);
                        }
                        else
                        {
                            if (File.Exists(userFilePath))
                                File.Delete(userFilePath);
                        }

                        MessageBox.Show("تم تسجيل الدخول بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Form1 form1 = new Form1(realName);
                        form1.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ: " + ex.Message);
                }
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            sign_up signUpForm = new sign_up();
            signUpForm.Show();
            this.Hide();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void login_Load(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(userFilePath))
                {
                    user_name.Text = File.ReadAllText(userFilePath);
                    rememberMe.Checked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل بيانات المستخدم الأخيرة: " + ex.Message);
            }
        }
    }
}
