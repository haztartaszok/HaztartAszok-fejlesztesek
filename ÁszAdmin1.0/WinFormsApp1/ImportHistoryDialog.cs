using System.Text;

namespace WinFormsApp1
{
    internal sealed class ImportHistoryDialog : Form
    {
        private readonly IReadOnlyList<ImportHistoryEntry> historyEntries;
        private readonly Label summaryLabel = new();
        private readonly SplitContainer splitContainer = new();
        private readonly DataGridView historyGrid = new();
        private readonly TextBox detailsTextBox = new();
        private readonly Button closeButton = new();

        public ImportHistoryDialog(IReadOnlyList<ImportHistoryEntry> historyEntries)
        {
            this.historyEntries = historyEntries
                .OrderByDescending(entry => entry.ImportedAt)
                .ToList();

            InitializeDialog();
            PopulateEntries();
        }

        private void InitializeDialog()
        {
            Text = "Import elozmenyek";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(920, 580);
            Size = new Size(1180, 720);

            summaryLabel.Dock = DockStyle.Top;
            summaryLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            summaryLabel.Height = 44;
            summaryLabel.Padding = new Padding(14, 10, 14, 0);

            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.None;
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterDistance = 300;

            ConfigureHistoryGrid();

            detailsTextBox.Dock = DockStyle.Fill;
            detailsTextBox.Font = new Font("Consolas", 10F);
            detailsTextBox.Multiline = true;
            detailsTextBox.ReadOnly = true;
            detailsTextBox.ScrollBars = ScrollBars.Both;
            detailsTextBox.WordWrap = false;

            Panel buttonPanel = new()
            {
                Dock = DockStyle.Bottom,
                Height = 58
            };

            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Location = new Point(Width - 160, 10);
            closeButton.Size = new Size(120, 36);
            closeButton.Text = "Bezaras";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += (_, _) => Close();

            buttonPanel.Resize += (_, _) =>
            {
                closeButton.Location = new Point(buttonPanel.ClientSize.Width - closeButton.Width - 14, 10);
            };

            buttonPanel.Controls.Add(closeButton);
            splitContainer.Panel1.Controls.Add(historyGrid);
            splitContainer.Panel2.Controls.Add(detailsTextBox);

            Controls.Add(splitContainer);
            Controls.Add(buttonPanel);
            Controls.Add(summaryLabel);
        }

        private void ConfigureHistoryGrid()
        {
            historyGrid.AllowUserToAddRows = false;
            historyGrid.AllowUserToDeleteRows = false;
            historyGrid.AutoGenerateColumns = false;
            historyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            historyGrid.BackgroundColor = SystemColors.Window;
            historyGrid.Dock = DockStyle.Fill;
            historyGrid.MultiSelect = false;
            historyGrid.ReadOnly = true;
            historyGrid.RowHeadersVisible = false;
            historyGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            historyGrid.Columns.AddRange(
            [
                CreateTextColumn(nameof(ImportHistoryEntry.ImportedAtDisplay), "Datum / ido", 150, 18),
                CreateTextColumn(nameof(ImportHistoryEntry.ImportTypeLabel), "Import tipus", 150, 18),
                CreateTextColumn(nameof(ImportHistoryEntry.SourceFileName), "Forrasfajl", 240, 34),
                CreateTextColumn(nameof(ImportHistoryEntry.CreatedCount), "Uj", 60, 8),
                CreateTextColumn(nameof(ImportHistoryEntry.UpdatedCount), "Frissitett", 90, 11),
                CreateTextColumn(nameof(ImportHistoryEntry.CategoryLinkedCount), "Kategoriak", 90, 11),
                CreateTextColumn(nameof(ImportHistoryEntry.ImageUploadedCount), "Kepek", 70, 9),
                CreateTextColumn(nameof(ImportHistoryEntry.PropertyAppliedCount), "Tulajdonsagok", 120, 14),
                CreateTextColumn(nameof(ImportHistoryEntry.ErrorCount), "Hibak", 60, 7)
            ]);

            historyGrid.SelectionChanged += HistoryGrid_SelectionChanged;
        }

        private static DataGridViewTextBoxColumn CreateTextColumn(
            string propertyName,
            string headerText,
            int minimumWidth,
            float fillWeight)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName,
                FillWeight = fillWeight,
                HeaderText = headerText,
                MinimumWidth = minimumWidth,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic
            };
        }

        private void PopulateEntries()
        {
            summaryLabel.Text = historyEntries.Count == 0
                ? "Meg nincs rogzitett importelozmeny."
                : $"Osszes import: {historyEntries.Count}";

            historyGrid.DataSource = historyEntries.ToList();

            if (historyEntries.Count == 0)
            {
                detailsTextBox.Text = "Meg nincs rogzitett importelozmeny.";
                return;
            }

            if (historyGrid.Rows.Count > 0)
            {
                historyGrid.Rows[0].Selected = true;
                historyGrid.CurrentCell = historyGrid.Rows[0].Cells[0];
                UpdateDetails(historyEntries[0]);
            }
        }

        private void HistoryGrid_SelectionChanged(object? sender, EventArgs e)
        {
            if (historyGrid.CurrentRow?.DataBoundItem is ImportHistoryEntry selectedEntry)
            {
                UpdateDetails(selectedEntry);
            }
        }

        private void UpdateDetails(ImportHistoryEntry entry)
        {
            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine($"Datum / ido: {entry.ImportedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss zzz}");
            detailsBuilder.AppendLine($"Import tipus: {entry.ImportTypeLabel}");

            if (!string.IsNullOrWhiteSpace(entry.SourceFilePath))
            {
                detailsBuilder.AppendLine($"Forrasfajl: {entry.SourceFilePath}");
            }

            detailsBuilder.AppendLine($"Statusz: {entry.StatusMessage}");
            detailsBuilder.AppendLine();
            detailsBuilder.Append(entry.DetailsMessage);

            detailsTextBox.Text = detailsBuilder.ToString().TrimEnd();
            detailsTextBox.SelectionStart = 0;
            detailsTextBox.SelectionLength = 0;
        }
    }
}
