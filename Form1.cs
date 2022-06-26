using System;
using System.Net.Mail;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Threading;

namespace PSW_lab4
{
    public partial class Form1 : Form
    {
        private Thread th;

        public Form1()
        {
            InitializeComponent();
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void textBox_rp_password1_TextChanged(object sender, EventArgs e)
        {
            check_textBox_rp();
        }

        private void textBox_rp_password2_TextChanged(object sender, EventArgs e)
        {
            check_textBox_rp();
        }

        private void check_textBox_rp()
        {
            bool state = (textBox_rp_password1.Text != textBox_rp_password2.Text);

            label_rp_password_match.Visible = state;

        }

        private void checkBox_rp_password_CheckedChanged(object sender, EventArgs e)
        {
            textBox_rp_password1.UseSystemPasswordChar = !(textBox_rp_password1.UseSystemPasswordChar);                //weryfikacja hasła 
            textBox_rp_password2.UseSystemPasswordChar = !(textBox_rp_password2.UseSystemPasswordChar);
        }
	
        private void textBox_rp_email_TextChanged(object sender, EventArgs e)										//sprawdzanie adresu email
        {
            if (textBox_rp_email.Text.Length > 0)
            {
                label_rp_email_valid.Visible = isNotValidEmail(textBox_rp_email.Text);
            }
        }

        private bool isNotValidEmail(string emailaddress)
        {
            if (textBox_rp_email.Text.Length > 4)
            {
                try
                {
                    MailAddress m = new MailAddress(emailaddress);

                    return false;
                }
                catch (Exception)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private bool isUserInDatabase(string userName)                                         //połączenie z bazą danych
        {
            string server = "localhost";
            string database = "pswlab4";
            string user = "root";
            string password = "1234";

            MySqlConnection connection = null;
            MySqlDataReader reader = null;
            
            
            string connectionString = String.Format("server={0}; userid={1}; password={2}; database={3}", server, user, password, database);

            string cmdText = String.Format("SELECT users.login FROM users");
            List<string> logins = new List<string>();

            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                MySqlCommand command = new MySqlCommand(cmdText, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    logins.Add(reader.GetString(0));
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (connection != null)
                {
                    connection.Close();
                }
            }
            foreach (string login in logins)
            { 
                if (login == userName)
                    return true;
            }
            return false;
        }
  
        private bool isNotSQLInjection(TextBox box)
        {
            string[] chars = {";","'","\"" };
            foreach (string s in chars)
            {
                if (box.Text.Contains(s))
                    return false;
            }
            return true;
        }

        private void button_rp_login_Click(object sender, EventArgs e)
        {
            if (textBox_rp_login.Text.Length > 2)
            {
                string newLogin = textBox_rp_login.Text;
                bool userInDatabase = isUserInDatabase(newLogin);
                if (userInDatabase)
                {
                    textBox_rp_login.BackColor = Color.Red;								//ustawienie koloru czerwonego lub zielonego w zależności od dostępności danej nazwy użytkownika bo użyciu przycisku tego umożliwijącego
                }
                else
                {
                    textBox_rp_login.BackColor = Color.Green;
                }
            }
        }

        private void button_rp_register_Click(object sender, EventArgs e)
        {
            string tbvEmail = textBox_rp_email.Text;      
            string tbvName = textBox_rp_name.Text;
            string tbvSurname = textBox_rp_surname.Text;
            string tbvLogin = textBox_rp_login.Text;
            string tbvPassword = textBox_rp_password1.Text;

            bool email = (!isNotValidEmail(textBox_rp_email.Text));

            bool login = (!isUserInDatabase(textBox_rp_login.Text) 
                && textBox_rp_login.Text.Length > 2 
                && isNotSQLInjection(textBox_rp_login));

            bool password = (tbvPassword == textBox_rp_password2.Text
                && tbvPassword.Length > 2
                && isNotSQLInjection(textBox_rp_password1));

            bool name = (tbvLogin.Length > 2
                && isNotSQLInjection(textBox_rp_name));

            bool surname = (tbvSurname.Length > 2
                && isNotSQLInjection(textBox_rp_surname));

            Console.WriteLine("{0},{1},{2},{3},{4},", email, login, password, name, surname);

            if(email && login && password && name && surname)
            {
                UserDao user = new UserDao(
                    name: tbvName,
                    surname: tbvSurname,
                    login: tbvLogin,
                    password: tbvPassword,
                    email: tbvEmail);
                user.AddToDataBase();
                if (isUserInDatabase(tbvLogin))
                {
                    MailSender mailSender = new MailSender();
                    mailSender.SendEmail(tbvEmail, tbvLogin, tbvPassword);
                }
            }

        }

        private void button_lp_login_Click(object sender, EventArgs e)
        {
            bool toPassingThrough = true;

            String loginInput = lp_name.Text;


            if (loginInput.Equals(""))
            {
                toPassingThrough = false;
                MessageBox.Show("Wprowadź nazwę użytkownika!", "Logowanie");
            }

            String passwordInput = lp_password.Text;

            if (passwordInput.Equals(""))
            {
                toPassingThrough = false;
                MessageBox.Show("Wprowadź hasło!", "Logowanie");
            }

            if (!toPassingThrough) return;

            check_database_for_user(loginInput,passwordInput);

        }
        private void check_database_for_user( String login, String password)
        {
            String loginDB;
            String passwordDB;
            String permissionDB;

            string cmdText = String.Format("SELECT login,password,permissions From psw_lab4.users WHERE login LIKE '{0}';", login);                                                

            string server = "localhost";
            string database = "pswlab4";
            string user = "root";
            string passwd = "1234";


            string connectionString = String.Format("server={0}; userid={1}; password={2}; database={3}", server, user, passwd, database);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(cmdText, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
                connection.Clone();
                MySqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow);

                bool readerErr = !reader.Read();

                if (readerErr)
                {
                    MessageBox.Show("Twoje dane są błędne lub dany użytkownik nie istnieje !", "Logowanie");                        
                    return;
                }

                loginDB = reader["login"].ToString();
                passwordDB = reader["password"].ToString();
                permissionDB = reader["permissions"].ToString();
                if (passwordDB != password)
                {
                    MessageBox.Show("Nieprawidłowe hasło!", "Logowanie");
                    return;
                }
            }
            open_new_window(permissionDB);
            
        }

        private void open_new_window(string permissionDB)
        {
            if (permissionDB == "ADMIN")
            {
                this.Hide();
                new FormAdmin().ShowDialog();
                this.Close();
                return;
            }

            this.Hide();
            new FormUser().ShowDialog();
            this.Close();
        }

        private void lp_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            lp_password.UseSystemPasswordChar = !lp_password.UseSystemPasswordChar;
        }
    }
}


    
