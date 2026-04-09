using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

// Разработка в ветке dev

namespace TeploMasterPlus_DB
{
    public partial class Form1 : Form
    {
        private string connStr = "Server=localhost;Database=TeploMasterPlus_DB;Trusted_Connection=True;TrustServerCertificate=True;";
        private DataGridView grid;
        private ComboBox cmb;
        private DateTimePicker dtp;
        private NumericUpDown num;
        private Button btnAdd, btnDel, btnClear;

        public Form1()
        {
            this.Text = "ООО ТепломастерПлюс";
            this.Size = new System.Drawing.Size(750, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);

            Label title = new Label();
            title.Text = "ООО ТЕПЛОМАСТЕРПЛЮС";
            title.Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold);
            title.ForeColor = System.Drawing.Color.Gold;
            title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            title.Dock = DockStyle.Top;
            title.Height = 50;
            this.Controls.Add(title);

            grid = new DataGridView();
            grid.Location = new System.Drawing.Point(10, 60);
            grid.Size = new System.Drawing.Size(710, 260);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ReadOnly = true;
            this.Controls.Add(grid);

            int y = 340;
            Label l1 = new Label(); l1.Text = "Сотрудник:"; l1.ForeColor = Color.White;
            l1.Location = new System.Drawing.Point(10, y); l1.Size = new System.Drawing.Size(100, 25);
            this.Controls.Add(l1);

            cmb = new ComboBox();
            cmb.Location = new System.Drawing.Point(120, y); cmb.Size = new System.Drawing.Size(200, 25);
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmb);

            y += 40;
            Label l2 = new Label(); l2.Text = "Дата:"; l2.ForeColor = Color.White;
            l2.Location = new System.Drawing.Point(10, y); l2.Size = new System.Drawing.Size(100, 25);
            this.Controls.Add(l2);

            dtp = new DateTimePicker();
            dtp.Location = new System.Drawing.Point(120, y); dtp.Size = new System.Drawing.Size(200, 25);
            this.Controls.Add(dtp);

            y += 40;
            Label l3 = new Label(); l3.Text = "Часы:"; l3.ForeColor = Color.White;
            l3.Location = new System.Drawing.Point(10, y); l3.Size = new System.Drawing.Size(100, 25);
            this.Controls.Add(l3);

            num = new NumericUpDown();
            num.Location = new System.Drawing.Point(120, y); num.Size = new System.Drawing.Size(100, 25);
            num.Minimum = 0; num.Maximum = 24;
            this.Controls.Add(num);

            y += 55;
            btnAdd = new Button();
            btnAdd.Text = "Добавить";
            btnAdd.Location = new System.Drawing.Point(10, y); btnAdd.Size = new System.Drawing.Size(100, 40);
            btnAdd.BackColor = System.Drawing.Color.DarkGreen;
            btnAdd.ForeColor = System.Drawing.Color.White;
            btnAdd.Click += AddClick;
            this.Controls.Add(btnAdd);

            btnDel = new Button();
            btnDel.Text = "Удалить";
            btnDel.Location = new System.Drawing.Point(120, y); btnDel.Size = new System.Drawing.Size(100, 40);
            btnDel.BackColor = System.Drawing.Color.DarkRed;
            btnDel.ForeColor = System.Drawing.Color.White;
            btnDel.Click += DelClick;
            this.Controls.Add(btnDel);

            btnClear = new Button();
            btnClear.Text = "Очистить всё";
            btnClear.Location = new System.Drawing.Point(230, y); btnClear.Size = new System.Drawing.Size(120, 40);
            btnClear.BackColor = System.Drawing.Color.DarkOrange;
            btnClear.ForeColor = System.Drawing.Color.White;
            btnClear.Click += (s, e) =>
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM ЗаписиОРаботе", conn);
                    int rows = cmd.ExecuteNonQuery();
                    MessageBox.Show($"Удалено записей: {rows}");
                    LoadData();
                }
            };
            this.Controls.Add(btnClear);

            LoadEmployees();

            // Показываем пустую таблицу
            DataTable emptyTable = new DataTable();
            emptyTable.Columns.Add("№", typeof(int));
            emptyTable.Columns.Add("Сотрудник", typeof(string));
            emptyTable.Columns.Add("Дата", typeof(string));
            emptyTable.Columns.Add("Часы", typeof(string));
            grid.DataSource = emptyTable;
        }

        private void LoadEmployees()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Номер, Фамилия + ' ' + Имя AS Name FROM Сотрудники WHERE Активен = 1", conn);
                SqlDataReader r = cmd.ExecuteReader();
                cmb.Items.Clear();
                while (r.Read())
                {
                    cmb.Items.Add(new { Id = r["Номер"], Name = r["Name"] });
                }
                r.Close();
            }
            cmb.DisplayMember = "Name";
            cmb.ValueMember = "Id";
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        private void LoadData()
        {
            string sql = @"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY ЗаписиОРаботе.Номер) AS [№],
                    Сотрудники.Фамилия + ' ' + Сотрудники.Имя AS [Сотрудник],
                    ЗаписиОРаботе.ДатаРаботы AS [Дата],
                    ЗаписиОРаботе.КоличествоЧасов AS [Часы],
                    ЗаписиОРаботе.Номер AS [RealId]
                FROM ЗаписиОРаботе
                JOIN Сотрудники ON ЗаписиОРаботе.НомерСотрудника = Сотрудники.Номер
                ORDER BY ЗаписиОРаботе.Номер";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.Fill(dt);
            }
            grid.DataSource = dt;
            if (grid.Columns["RealId"] != null)
                grid.Columns["RealId"].Visible = false;
        }

        private void AddClick(object sender, EventArgs e)
        {
            if (num.Value <= 0) { MessageBox.Show("Введите часы!"); return; }
            if (cmb.SelectedItem == null) { MessageBox.Show("Выберите сотрудника!"); return; }

            dynamic sel = cmb.SelectedItem;

            string sql = @"INSERT INTO ЗаписиОРаботе (НомерСотрудника, ДатаРаботы, КоличествоЧасов, НомерТипаРаботы, НомерОбъекта, Подтверждено) 
                           VALUES (@emp, @date, @hours, 1, 1, 0)";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@emp", sel.Id);
                cmd.Parameters.AddWithValue("@date", dtp.Value);
                cmd.Parameters.AddWithValue("@hours", (double)num.Value);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Добавлено!");
            LoadData();
            num.Value = 0;
        }

        private void DelClick(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) { MessageBox.Show("Выберите запись!"); return; }

            int realId = Convert.ToInt32(grid.SelectedRows[0].Cells["RealId"].Value);
            string employeeName = grid.SelectedRows[0].Cells["Сотрудник"].Value.ToString();

            if (MessageBox.Show($"Удалить запись сотрудника {employeeName}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"DELETE FROM ЗаписиОРаботе WHERE Номер = {realId}", conn);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Удалено!");
                LoadData();
            }
        }
    }
}