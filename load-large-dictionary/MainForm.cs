using SQLite;
using System;
using System.Data;
using System.Drawing.Text;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace load_large_dictionary
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            if (!File.Exists(@"SI-utf8.db"))
            {
                createDatabaseOneTimeForDemo();
            }
            #region G L Y P H
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Fonts",
                "glyphs.ttf");
            privateFontCollection.AddFontFile(path);
            var fontFamily = privateFontCollection.Families[0];
            Glyphs = new Font(fontFamily, 14F);
            #endregion G L Y P H

            checkBoxDirection.Font = Glyphs;
            checkBoxDirection.Text = "\uE800";
            checkBoxDirection.CheckedChanged += onCheckedChanged;

            textBox1.KeyDown += onKeyDown;
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                e.SuppressKeyPress = e.Handled = true;
                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    List<Translation> translations;
                    using (var cnx = new SQLiteConnection(@"SI-utf8.db"))
                    {
                        if (checkBoxDirection.Checked)
                        {
                            translations =
                                cnx
                                .Query<Translation>(
                                    $"SELECT * FROM translations WHERE Sinhala LIKE '%{textBox1.Text}%' LIMIT 5");
                            textBox2.Text = string.Join(
                                Environment.NewLine,
                                translations.Select(_ => _.English));
                        }
                        else
                        {
                            translations =
                                cnx
                                .Query<Translation>(
                                    $"SELECT * FROM translations WHERE English LIKE '%{textBox1.Text}%' LIMIT 5");
                            textBox2.Text = string.Join(
                                Environment.NewLine,
                                translations.Select(_ => _.Sinhala));
                        }
                    }
                }
            }
        }

        private void onCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDirection.Checked)
            {
                label1.Text = "Sinhala";
                label2.Text = "English";
            }
            else
            {
                label1.Text = "English";
                label2.Text = "Sinhala";
            }
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                var swap = textBox1.Text;
                textBox1.Text = textBox2.Text;
                textBox2.Text = swap;
            }
        }

        public static Font Glyphs { get; private set; }
        PrivateFontCollection privateFontCollection = new PrivateFontCollection();


        /// <summary>
        ///  Creates the SQLite database that "usually" will
        ///  exist from the start in this scheme of things.
        /// </summary>
        private void createDatabaseOneTimeForDemo()
        {
            var translations = Enumerable
                .Range(1, 130000)
                .Select(_ => new Translation
                {
                    English = $"english{_}",
                    Sinhala = $"sinhala{_}A sinhala{_}B"
                });
            using (var cnx = new SQLiteConnection(@"SI-utf8.db"))
            {
                cnx.CreateTable<Translation>();
                cnx.InsertAll(translations);
                cnx.Insert(new Translation
                {
                    English= "Example",
                    Sinhala= "උදාහරණයක්",
                });
            }
        }
    }
    [Table("translations")]
    class Translation
    {
        [PrimaryKey]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public string English { get; set; }
        public string Sinhala  { get; set; }
     }
}
