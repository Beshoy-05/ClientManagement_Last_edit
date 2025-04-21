using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ClientManagement
{
    public partial class sign_up : Form
    {
        static string connectionstring = "Data Source=BESHOY;Initial Catalog=ClientManagement;Integrated Security=True;";

        public sign_up()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string Name = name.Text.Trim();
            string UserName = user_name.Text.Trim();
            string Role = role.Text.Trim();
            string Password = pass.Text.Trim();

      
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(Role) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

      
            string hashedPassword = ComputeSha256Hash(Password);

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                conn.Open();
                try
                {
                    
                    string checkQuery = "SELECT COUNT(*) FROM Employees WHERE Username=@Username";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@Username", UserName);
                    int userExists = (int)checkCmd.ExecuteScalar();

                    if (userExists > 0)
                    {
                        MessageBox.Show("Username already exists. Please choose another.");
                        return;
                    }

                 
                    string query = "INSERT INTO Employees (Name, Username, PasswordHash, Role) VALUES (@Name, @Username, @PasswordHash, @Role)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Username", UserName);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@Role", Role);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Sign-up Successful");
                    login loginForm = new login();
                    loginForm.Show();
                    this.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
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
            login loginForm = new login();
            loginForm.Show();
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
