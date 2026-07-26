using Ffvi.SaveTool;

namespace Ffvi.SaveTool.Gui;

public sealed class SwitchFolderDialog : Form
{
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };

    public string? SelectedPath { get; private set; }

    public SwitchFolderDialog(string folder)
    {
        Text = "Choose a Switch save slot";
        Width = 1000;
        Height = 480;
        StartPosition = FormStartPosition.CenterParent;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Slot ID", DataPropertyName = nameof(Row.SlotId), FillWeight = 35 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Play time", DataPropertyName = nameof(Row.PlayTime), FillWeight = 55 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Gil", DataPropertyName = nameof(Row.Gil), FillWeight = 45 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Timestamp", DataPropertyName = nameof(Row.Timestamp), FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Filename", DataPropertyName = nameof(Row.FileName), FillWeight = 210 });
        _grid.DoubleClick += (_, _) => AcceptSelection();

        var open = new Button { Text = "Open selected", AutoSize = true };
        open.Click += (_, _) => AcceptSelection();
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(open);

        Controls.Add(_grid);
        Controls.Add(buttons);
        AcceptButton = open;
        CancelButton = cancel;

        var rows = new List<Row>();
        foreach (var path in Directory.EnumerateFiles(folder))
        {
            if (Path.GetFileName(path).StartsWith(".nx_save_meta", StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                var save = SaveFile.Load(path);
                if (!save.IsSlotFile()) continue;
                rows.Add(new Row(path, save.SlotId, FormatPlayTime(save.PlayTime), save.UserData.Gil, save.Timestamp ?? "", Path.GetFileName(path)));
            }
            catch
            {
                // Non-slot metadata/config files are expected in a JKSV folder.
            }
        }

        _grid.DataSource = rows.OrderBy(r => r.SlotId).ThenBy(r => r.FileName).ToList();
        if (rows.Count == 0)
            MessageBox.Show(this, "No readable character save slots were found in this folder.", "No slots found", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AcceptSelection()
    {
        if (_grid.CurrentRow?.DataBoundItem is not Row row) return;
        SelectedPath = row.Path;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static string FormatPlayTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}:{ts.Minutes:00}:{ts.Seconds:00}" : $"{ts.Minutes}:{ts.Seconds:00}";
    }

    private sealed record Row(string Path, int SlotId, string PlayTime, int Gil, string Timestamp, string FileName);
}
