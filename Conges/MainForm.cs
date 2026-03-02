using System.Globalization;
using CalculateBestDaysOff.Domain;

namespace Conges;

public partial class MainForm : Form
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    // ── toolbar ──────────────────────────────────────────────────────────────
    private readonly NumericUpDown _nudYear    = new() { Minimum = 1900, Maximum = 2100, Width = 70 };
    private readonly NumericUpDown _nudMaxDays = new() { Minimum = 1,    Maximum = 60,   Width = 55, Value = 10 };
    private readonly NumericUpDown _nudWeeks   = new() { Minimum = 0,    Maximum = 12,   Width = 55, Value = 0 };
    private readonly NumericUpDown _nudTop     = new() { Minimum = 1,    Maximum = 200,  Width = 55, Value = 10 };
    private readonly Button        _btnCalc    = new() { Text = "Calculer ▶", AutoSize = true, FlatStyle = FlatStyle.Flat };

    // ── layout ───────────────────────────────────────────────────────────────
    private readonly DataGridView _grid      = new();
    private readonly Label        _lblStatus = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

    public MainForm()
    {
        InitializeComponent();
        SetupForm();
        _nudYear.Value = DateTime.Now.Year;
        RunCalculation();
    }

    private void SetupForm()
    {
        // form
        Text            = "Congés";
        MinimumSize     = new Size(1100, 620);
        Size            = new Size(1200, 700);
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = Color.FromArgb(30, 30, 30);
        ForeColor       = Color.WhiteSmoke;
        Font            = new Font("Segoe UI", 10f);

        // ── toolbar panel ─────────────────────────────────────────────────
        var toolbar = new FlowLayoutPanel
        {
            Dock        = DockStyle.Top,
            Height      = 46,
            BackColor   = Color.FromArgb(40, 40, 40),
            Padding     = new Padding(8, 6, 8, 6),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };

        toolbar.Controls.Add(MakeLabel("Année :"));
        StyleNud(_nudYear);
        toolbar.Controls.Add(_nudYear);

        toolbar.Controls.Add(MakeSep());
        toolbar.Controls.Add(MakeLabel("Max jours posés :"));
        StyleNud(_nudMaxDays);
        toolbar.Controls.Add(_nudMaxDays);

        toolbar.Controls.Add(MakeSep());
        toolbar.Controls.Add(MakeLabel("Semaines min :"));
        StyleNud(_nudWeeks);
        var weeksNote = MakeLabel("(0 = tous)");
        weeksNote.ForeColor = Color.Gray;
        toolbar.Controls.Add(_nudWeeks);
        toolbar.Controls.Add(weeksNote);

        toolbar.Controls.Add(MakeSep());
        toolbar.Controls.Add(MakeLabel("Top :"));
        StyleNud(_nudTop);
        toolbar.Controls.Add(_nudTop);

        toolbar.Controls.Add(MakeSep());
        StyleButton(_btnCalc);
        toolbar.Controls.Add(_btnCalc);

        // ── status bar ────────────────────────────────────────────────────
        var statusBar = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = Color.FromArgb(40, 40, 40) };
        _lblStatus.ForeColor = Color.Gray;
        _lblStatus.Font      = new Font("Segoe UI", 9f);
        statusBar.Controls.Add(_lblStatus);

        // ── main area ────────────────────────────────────────────────────
        SetupGrid();

        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(statusBar);

        _btnCalc.Click        += (_, _) => RunCalculation();
        _nudYear.ValueChanged += (_, _) => RunCalculation();
    }

    private void SetupGrid()
    {
        _grid.Dock                  = DockStyle.Fill;
        _grid.ReadOnly              = true;
        _grid.AllowUserToAddRows    = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible     = false;
        _grid.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
        _grid.BackgroundColor       = Color.FromArgb(30, 30, 30);
        _grid.GridColor             = Color.FromArgb(60, 60, 60);
        _grid.BorderStyle           = BorderStyle.None;
        _grid.CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.DefaultCellStyle      = new DataGridViewCellStyle
        {
            BackColor     = Color.FromArgb(38, 38, 38),
            ForeColor     = Color.WhiteSmoke,
            SelectionBackColor = Color.FromArgb(70, 100, 140),
            SelectionForeColor = Color.White,
            Font          = new Font("Segoe UI", 10f),
            Padding       = new Padding(4, 2, 4, 2),
        };
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor  = Color.FromArgb(50, 50, 50),
            ForeColor  = Color.Orange,
            Font       = new Font("Segoe UI", 10f, FontStyle.Bold),
            Padding    = new Padding(4, 4, 4, 4),
        };
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.ColumnHeadersHeight = 32;
        _grid.RowTemplate.Height  = 28;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rank",     HeaderText = "#",             Width = 36,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Start",    HeaderText = "Début",         Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "End",      HeaderText = "Fin",           Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total",    HeaderText = "Jours off",     Width = 80,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Taken",    HeaderText = "Posés",         Width = 60,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ratio",    HeaderText = "Ratio",         Width = 70,  DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "WorkDays", HeaderText = "Jours à poser", MinimumWidth = 120, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Holidays", HeaderText = "Fériés inclus", MinimumWidth = 120, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        _grid.CellFormatting += Grid_CellFormatting;
    }

    private void RunCalculation()
    {
        int year    = (int)_nudYear.Value;
        int maxDays = (int)_nudMaxDays.Value;
        int weeks   = (int)_nudWeeks.Value;
        int top     = (int)_nudTop.Value;

        if (weeks > 0)
            maxDays = Math.Max(maxDays, weeks * 5);

        int minDays = weeks > 0 ? weeks * 7 : 0;

        var results = VacationOptimizer.FindBest(year, maxDays)
            .Where(o => minDays == 0 || o.TotalDaysOff >= minDays)
            .Take(top)
            .ToList();

        // grid
        _grid.Rows.Clear();
        for (int i = 0; i < results.Count; i++)
        {
            var o = results[i];
            _grid.Rows.Add(
                i + 1,
                o.Start.ToString("ddd d MMM", Fr),
                o.End.ToString("ddd d MMM", Fr),
                o.TotalDaysOff,
                o.DaysTaken,
                $"{o.Ratio:F1}",
                string.Join(", ", o.WorkDaysTaken.Select(d => d.ToString("dd/MM", Fr))),
                string.Join(", ", o.HolidaysIncluded));
        }

        _lblStatus.Text = $"  {results.Count} résultat(s) — ratio = jours off consécutifs / jours posés";
    }

    private static void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var grid = (DataGridView)sender!;
        var row  = grid.Rows[e.RowIndex];

        // alternating row color
        row.DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
            ? Color.FromArgb(38, 38, 38)
            : Color.FromArgb(44, 44, 44);

        if (grid.Columns[e.ColumnIndex].Name == "Ratio" && e.Value is string ratioStr
            && double.TryParse(ratioStr, System.Globalization.NumberStyles.Any,
                               System.Globalization.CultureInfo.InvariantCulture, out double ratio))
        {
            e.CellStyle.ForeColor = ratio switch
            {
                >= 7 => Color.Gold,
                >= 5 => Color.LimeGreen,
                >= 3 => Color.MediumSpringGreen,
                >= 2 => Color.Khaki,
                _    => Color.WhiteSmoke,
            };
            e.CellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        }

        if (grid.Columns[e.ColumnIndex].Name == "Rank" && e.RowIndex < 3)
        {
            e.CellStyle.ForeColor = e.RowIndex switch
            {
                0 => Color.Gold,
                1 => Color.Silver,
                2 => Color.Peru,
                _ => Color.WhiteSmoke,
            };
            e.CellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        }
    }

    // ── helpers ───────────────────────────────────────────────────────────────
    private static Label MakeLabel(string text) => new()
    {
        Text      = text,
        AutoSize  = true,
        ForeColor = Color.Silver,
        Margin    = new Padding(6, 8, 2, 0),
    };

    private static Label MakeSep() => new()
    {
        Text    = "|",
        AutoSize = true,
        ForeColor = Color.FromArgb(70, 70, 70),
        Margin  = new Padding(6, 8, 6, 0),
    };

    private static void StyleNud(NumericUpDown nud)
    {
        nud.BackColor = Color.FromArgb(55, 55, 55);
        nud.ForeColor = Color.WhiteSmoke;
        nud.Margin    = new Padding(2, 5, 2, 0);
    }

    private static void StyleButton(Button btn)
    {
        btn.BackColor   = Color.FromArgb(200, 100, 20);
        btn.ForeColor   = Color.White;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font        = new Font("Segoe UI", 10f, FontStyle.Bold);
        btn.Margin      = new Padding(10, 4, 2, 0);
        btn.Cursor      = Cursors.Hand;
    }
}
