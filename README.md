This "might" be what's known as an [XY Problem](https://meta.stackexchange.com/a/66378') because your question asks how to **load a huge txt file** and one might ask: _Why would you want to **load** a txt file if it's so **huge**?_ 

Consider an alternative where the translation entries are stored on a local SQLite database.

[![screenshot][1]][1]

One example of a NuGet that makes this simple is `sqlite-net-pcl`). All you need is a class for an entry:

***
**Translation**

    [Table("translations")]
    class Translation
    {
        [PrimaryKey]
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public string English { get; set; }
        public string Sinhala  { get; set; }
    }

***
**Query**

When the enter key is pressed in the left-hand box, just look up the translation from English to Sinhala or the other way around depending on the direction.

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

***
**Demo: Create Database**

The _first_ time this runs, it creates a large database for demonstration purposes.

    using SQLite;

then

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // First time only
            if (!File.Exists(@"SI-utf8.db"))
            {
                createDatabaseOneTimeForDemo();
            }

            checkBoxDirection.Font = Glyphs;
            checkBoxDirection.Text = "<=>";
            checkBoxDirection.CheckedChanged += onCheckedChanged;

            textBox1.KeyDown += onKeyDown;
        }

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
    }


  [1]: https://i.stack.imgur.com/nyK9n.png