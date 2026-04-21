using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private const int PageMargin = 24;
        private const int SectionSpacing = 16;
        private const int CardSpacing = 16;
        private const int BulkCardHeight = 380;
        private const int MinimumContentWidth = 760;
        private readonly List<WorksheetPreview> loadedWorkbookSheets = [];
        private bool isUpdatingSheetSelection;

        public Form1()
        {
            InitializeComponent();
            InitializeSelections();

            browseButton.Click += BrowseButton_Click;
            SablonButton.Click += SablonButton_Click;
            sheetComboBox.SelectedIndexChanged += SheetComboBox_SelectedIndexChanged;
            Load += (_, _) => UpdateResponsiveLayout();
            Resize += (_, _) => UpdateResponsiveLayout();

            UpdateResponsiveLayout();
        }

        private void InitializeSelections()
        {
            foreach (var comboBox in new[]
            {
                importTypeComboBox,
                existingItemModeComboBox,
                priceCategoryComboBox,
                priceModeComboBox,
                statusFilterComboBox,
                statusValueComboBox,
                sourceCategoryComboBox,
                targetCategoryComboBox,
                deleteConditionComboBox
            })
            {
                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
            }

            filePathTextBox.ReadOnly = true;
            sheetComboBox.Items.Clear();
            sheetComboBox.Enabled = false;
            ClearPreviewGrid();
        }

        private void UpdateResponsiveLayout()
        {
            if (ClientSize.Width <= 0)
            {
                return;
            }

            SuspendLayout();
            fileGroupBox.SuspendLayout();
            optionsGroupBox.SuspendLayout();
            previewGroupBox.SuspendLayout();
            priceGroupBox.SuspendLayout();
            statusGroupBox.SuspendLayout();
            categoryGroupBox.SuspendLayout();
            deleteGroupBox.SuspendLayout();
            footerPanel.SuspendLayout();

            try
            {
                int contentWidth = Math.Max(MinimumContentWidth, ClientSize.Width - (PageMargin * 2));
                int currentY = 22;

                titleLabel.Location = new Point(PageMargin, currentY);
                currentY = titleLabel.Bottom + 20;

                currentY = LayoutFileSection(contentWidth, currentY);
                currentY = LayoutOptionsSection(contentWidth, currentY);
                currentY = LayoutPreviewSection(contentWidth, currentY);

                bulkTitleLabel.Location = new Point(PageMargin, currentY);
                currentY = bulkTitleLabel.Bottom + 16;

                currentY = LayoutBulkSections(contentWidth, currentY);
                currentY = LayoutFooter(contentWidth, currentY);

                AutoScrollMinSize = new Size(0, currentY + PageMargin);
            }
            finally
            {
                footerPanel.ResumeLayout();
                deleteGroupBox.ResumeLayout();
                categoryGroupBox.ResumeLayout();
                statusGroupBox.ResumeLayout();
                priceGroupBox.ResumeLayout();
                previewGroupBox.ResumeLayout();
                optionsGroupBox.ResumeLayout();
                fileGroupBox.ResumeLayout();
                ResumeLayout();
            }
        }

        private int LayoutFileSection(int contentWidth, int y)
        {
            const int innerPadding = 24;
            const int rowGap = 12;
            const int buttonWidth = 170;
            const int buttonHeight = 38;

            fileGroupBox.SetBounds(PageMargin, y, contentWidth, 112);

            int innerWidth = fileGroupBox.Width - (innerPadding * 2);
            filePathLabel.Location = new Point(innerPadding, 34);

            bool stackButtons = innerWidth < 720;

            if (stackButtons)
            {
                filePathTextBox.SetBounds(innerPadding, 60, innerWidth, filePathTextBox.Height);

                int buttonY = filePathTextBox.Bottom + rowGap;
                int splitWidth = (innerWidth - rowGap) / 2;

                SablonButton.SetBounds(innerPadding, buttonY, splitWidth, buttonHeight);
                browseButton.SetBounds(SablonButton.Right + rowGap, buttonY, innerWidth - splitWidth - rowGap, buttonHeight);

                fileGroupBox.Height = browseButton.Bottom + 18;
            }
            else
            {
                int textBoxWidth = innerWidth - (buttonWidth * 2) - (rowGap * 2);

                filePathTextBox.SetBounds(innerPadding, 60, textBoxWidth, filePathTextBox.Height);
                SablonButton.SetBounds(filePathTextBox.Right + rowGap, 58, buttonWidth, buttonHeight);
                browseButton.SetBounds(SablonButton.Right + rowGap, 58, buttonWidth, buttonHeight);

                fileGroupBox.Height = 112;
            }

            return fileGroupBox.Bottom + SectionSpacing;
        }

        private int LayoutOptionsSection(int contentWidth, int y)
        {
            const int innerPadding = 24;
            const int columnGap = 28;

            optionsGroupBox.SetBounds(PageMargin, y, contentWidth, 115);

            int innerWidth = optionsGroupBox.Width - (innerPadding * 2);
            bool stacked = innerWidth < 820;

            importTypeLabel.Location = new Point(innerPadding, 32);
            importTypeComboBox.Location = new Point(innerPadding, 58);

            if (stacked)
            {
                importTypeComboBox.Width = innerWidth;

                existingItemModeLabel.Location = new Point(innerPadding, importTypeComboBox.Bottom + 14);
                existingItemModeComboBox.SetBounds(innerPadding, existingItemModeLabel.Bottom + 6, innerWidth, existingItemModeComboBox.Height);

                optionsGroupBox.Height = existingItemModeComboBox.Bottom + 20;
            }
            else
            {
                int fieldWidth = (innerWidth - columnGap) / 2;
                int secondColumnX = innerPadding + fieldWidth + columnGap;

                importTypeComboBox.Width = fieldWidth;
                existingItemModeLabel.Location = new Point(secondColumnX, 32);
                existingItemModeComboBox.SetBounds(secondColumnX, 58, fieldWidth, existingItemModeComboBox.Height);

                optionsGroupBox.Height = 115;
            }

            return optionsGroupBox.Bottom + SectionSpacing;
        }

        private int LayoutPreviewSection(int contentWidth, int y)
        {
            const int innerPadding = 24;
            const int comboWidth = 260;

            previewGroupBox.SetBounds(PageMargin, y, contentWidth, 430);

            int innerWidth = previewGroupBox.Width - (innerPadding * 2);
            bool inlineSelector = innerWidth >= 620;
            int gridTop;

            if (inlineSelector)
            {
                sheetComboBox.SetBounds(previewGroupBox.Width - innerPadding - comboWidth, 36, comboWidth, sheetComboBox.Height);
                sheetLabel.Location = new Point(sheetComboBox.Left - sheetLabel.PreferredWidth - 10, 39);

                previewGroupBox.Height = 430;
                gridTop = 84;
            }
            else
            {
                sheetLabel.Location = new Point(innerPadding, 36);
                sheetComboBox.SetBounds(innerPadding, sheetLabel.Bottom + 8, Math.Min(innerWidth, 320), sheetComboBox.Height);

                previewGroupBox.Height = 468;
                gridTop = sheetComboBox.Bottom + 16;
            }

            previewDataGridView.SetBounds(innerPadding, gridTop, innerWidth, previewGroupBox.Height - gridTop - 20);

            return previewGroupBox.Bottom + SectionSpacing;
        }

        private int LayoutBulkSections(int contentWidth, int y)
        {
            bool twoColumns = contentWidth >= 980;

            if (twoColumns)
            {
                int cardWidth = (contentWidth - CardSpacing) / 2;
                int rightColumnX = PageMargin + cardWidth + CardSpacing;
                int secondRowY = y + BulkCardHeight + CardSpacing;

                priceGroupBox.SetBounds(PageMargin, y, cardWidth, BulkCardHeight);
                statusGroupBox.SetBounds(rightColumnX, y, cardWidth, BulkCardHeight);
                categoryGroupBox.SetBounds(PageMargin, secondRowY, cardWidth, BulkCardHeight);
                deleteGroupBox.SetBounds(rightColumnX, secondRowY, cardWidth, BulkCardHeight);

                return deleteGroupBox.Bottom + SectionSpacing;
            }

            priceGroupBox.SetBounds(PageMargin, y, contentWidth, BulkCardHeight);
            statusGroupBox.SetBounds(PageMargin, priceGroupBox.Bottom + CardSpacing, contentWidth, BulkCardHeight);
            categoryGroupBox.SetBounds(PageMargin, statusGroupBox.Bottom + CardSpacing, contentWidth, BulkCardHeight);
            deleteGroupBox.SetBounds(PageMargin, categoryGroupBox.Bottom + CardSpacing, contentWidth, BulkCardHeight);

            return deleteGroupBox.Bottom + SectionSpacing;
        }

        private int LayoutFooter(int contentWidth, int y)
        {
            const int buttonHeight = 42;
            const int buttonGap = 12;

            footerPanel.Location = new Point(PageMargin, y);
            footerPanel.Width = contentWidth;

            if (contentWidth >= 760)
            {
                const int historyWidth = 200;
                const int actionWidth = 180;

                footerPanel.Height = 52;
                historyButton.SetBounds(0, 5, historyWidth, buttonHeight);
                importButton.SetBounds(footerPanel.Width - actionWidth, 5, actionWidth, buttonHeight);
                validateButton.SetBounds(importButton.Left - buttonGap - actionWidth, 5, actionWidth, buttonHeight);
            }
            else
            {
                int fullWidth = footerPanel.Width;

                historyButton.SetBounds(0, 0, fullWidth, buttonHeight);
                validateButton.SetBounds(0, historyButton.Bottom + 8, fullWidth, buttonHeight);
                importButton.SetBounds(0, validateButton.Bottom + 8, fullWidth, buttonHeight);

                footerPanel.Height = importButton.Bottom;
            }

            return footerPanel.Bottom;
        }

        private void SablonButton_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog saveDialog = new()
            {
                AddExtension = true,
                DefaultExt = "xlsx",
                FileName = "ImportSablon.xlsx",
                Filter = "Excel munkafuzet (*.xlsx)|*.xlsx|Minden fajl (*.*)|*.*",
                OverwritePrompt = true,
                RestoreDirectory = true,
                Title = "Import sablon mentese"
            };

            if (saveDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                CreateImportTemplate(saveDialog.FileName);

                MessageBox.Show(
                    this,
                    "A sablon sikeresen elmentve.",
                    "Sablon mentes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"A sablon mentese nem sikerult.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Sablon mentes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog openDialog = new()
            {
                CheckFileExists = true,
                Filter = "Excel munkafuzet (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|Minden fajl (*.*)|*.*",
                Multiselect = false,
                RestoreDirectory = true,
                Title = "Import fajl kivalasztasa"
            };

            if (openDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                List<WorksheetPreview> workbookSheets = LoadWorkbookPreview(openDialog.FileName);

                loadedWorkbookSheets.Clear();
                loadedWorkbookSheets.AddRange(workbookSheets);
                filePathTextBox.Text = openDialog.FileName;

                PopulateSheetSelector();
            }
            catch (Exception ex)
            {
                loadedWorkbookSheets.Clear();
                filePathTextBox.Clear();
                PopulateSheetSelector();

                MessageBox.Show(
                    this,
                    $"A kivalasztott Excel fajl nem olvashato be.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Import fajl megnyitasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SheetComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (isUpdatingSheetSelection)
            {
                return;
            }

            ShowSelectedWorksheet();
        }

        private void PopulateSheetSelector()
        {
            isUpdatingSheetSelection = true;

            try
            {
                sheetComboBox.Items.Clear();

                foreach (WorksheetPreview sheet in loadedWorkbookSheets)
                {
                    sheetComboBox.Items.Add(sheet.Name);
                }

                sheetComboBox.Enabled = loadedWorkbookSheets.Count > 0;

                if (loadedWorkbookSheets.Count > 0)
                {
                    sheetComboBox.SelectedIndex = 0;
                }
                else
                {
                    ClearPreviewGrid();
                }
            }
            finally
            {
                isUpdatingSheetSelection = false;
            }

            if (loadedWorkbookSheets.Count > 0)
            {
                ShowSelectedWorksheet();
            }
        }

        private void ShowSelectedWorksheet()
        {
            if (sheetComboBox.SelectedIndex < 0 || sheetComboBox.SelectedIndex >= loadedWorkbookSheets.Count)
            {
                ClearPreviewGrid();
                return;
            }

            RenderWorksheet(loadedWorkbookSheets[sheetComboBox.SelectedIndex]);
        }

        private void RenderWorksheet(WorksheetPreview worksheet)
        {
            ClearPreviewGrid();

            if (worksheet.Rows.Count == 0)
            {
                return;
            }

            int columnCount = worksheet.Rows.Max(row => row.Length);

            if (columnCount == 0)
            {
                return;
            }

            bool hasHeaderRow = worksheet.Rows[0].Any(cell => !string.IsNullOrWhiteSpace(cell));
            string[] headerRow = hasHeaderRow ? worksheet.Rows[0] : CreateDefaultHeaders(columnCount);

            for (int i = 0; i < columnCount; i++)
            {
                string headerText = i < headerRow.Length && !string.IsNullOrWhiteSpace(headerRow[i])
                    ? headerRow[i]
                    : $"Oszlop {i + 1}";

                previewDataGridView.Columns.Add($"previewColumn{i}", headerText);
            }

            int startRowIndex = hasHeaderRow ? 1 : 0;

            for (int rowIndex = startRowIndex; rowIndex < worksheet.Rows.Count; rowIndex++)
            {
                string[] row = NormalizeRowLength(worksheet.Rows[rowIndex], columnCount);
                previewDataGridView.Rows.Add(row.Cast<object>().ToArray());
            }
        }

        private void ClearPreviewGrid()
        {
            previewDataGridView.DataSource = null;
            previewDataGridView.Rows.Clear();
            previewDataGridView.Columns.Clear();
        }

        private static string[] CreateDefaultHeaders(int columnCount)
        {
            return Enumerable.Range(1, columnCount)
                .Select(index => $"Oszlop {index}")
                .ToArray();
        }

        private static string[] NormalizeRowLength(string[] sourceRow, int columnCount)
        {
            string[] normalizedRow = new string[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                normalizedRow[i] = i < sourceRow.Length ? sourceRow[i] : string.Empty;
            }

            return normalizedRow;
        }

        private static List<WorksheetPreview> LoadWorkbookPreview(string filePath)
        {
            string extension = Path.GetExtension(filePath);

            if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, ".xlsm", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Jelenleg csak .xlsx es .xlsm fajlok tamogatottak.");
            }

            using ZipArchive archive = ZipFile.OpenRead(filePath);

            XDocument workbookDocument = LoadXmlEntry(archive, "xl/workbook.xml");
            Dictionary<string, string> worksheetTargets = LoadWorkbookRelationships(archive);
            IReadOnlyList<string> sharedStrings = LoadSharedStrings(archive);

            XNamespace mainNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XNamespace relationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

            List<WorksheetPreview> worksheets = [];

            foreach (XElement sheetElement in workbookDocument.Descendants(mainNamespace + "sheet"))
            {
                string sheetName = (string?)sheetElement.Attribute("name") ?? $"Munkalap {worksheets.Count + 1}";
                string? relationshipId = (string?)sheetElement.Attribute(relationshipNamespace + "id");

                if (string.IsNullOrWhiteSpace(relationshipId) || !worksheetTargets.TryGetValue(relationshipId, out string? worksheetPath))
                {
                    continue;
                }

                List<string[]> rows = LoadWorksheetRows(archive, worksheetPath, sharedStrings);
                worksheets.Add(new WorksheetPreview(sheetName, rows));
            }

            return worksheets;
        }

        private static Dictionary<string, string> LoadWorkbookRelationships(ZipArchive archive)
        {
            XDocument relationshipsDocument = LoadXmlEntry(archive, "xl/_rels/workbook.xml.rels");
            XNamespace relationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";

            return relationshipsDocument.Root?
                .Elements(relationshipNamespace + "Relationship")
                .Where(element => string.Equals((string?)element.Attribute("Type"), "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", StringComparison.Ordinal))
                .ToDictionary(
                    element => (string?)element.Attribute("Id") ?? string.Empty,
                    element => CombineArchivePath("xl", (string?)element.Attribute("Target") ?? string.Empty),
                    StringComparer.Ordinal)
                ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }

        private static IReadOnlyList<string> LoadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry? sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");

            if (sharedStringsEntry is null)
            {
                return [];
            }

            using Stream sharedStringsStream = sharedStringsEntry.Open();
            XDocument sharedStringsDocument = XDocument.Load(sharedStringsStream);
            XNamespace mainNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            return sharedStringsDocument
                .Descendants(mainNamespace + "si")
                .Select(element => string.Concat(element.Descendants(mainNamespace + "t").Select(text => text.Value)))
                .ToList();
        }

        private static List<string[]> LoadWorksheetRows(ZipArchive archive, string worksheetPath, IReadOnlyList<string> sharedStrings)
        {
            XDocument worksheetDocument = LoadXmlEntry(archive, worksheetPath);
            XNamespace mainNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            List<Dictionary<int, string>> rowMaps = [];
            int maxColumnIndex = 0;

            foreach (XElement rowElement in worksheetDocument.Descendants(mainNamespace + "row"))
            {
                int rowNumber = (int?)rowElement.Attribute("r") ?? (rowMaps.Count + 1);

                while (rowMaps.Count < rowNumber - 1)
                {
                    rowMaps.Add([]);
                }

                Dictionary<int, string> rowValues = [];
                int sequentialColumnIndex = 1;

                foreach (XElement cellElement in rowElement.Elements(mainNamespace + "c"))
                {
                    string? cellReference = (string?)cellElement.Attribute("r");
                    int columnIndex = !string.IsNullOrWhiteSpace(cellReference)
                        ? GetColumnIndexFromCellReference(cellReference)
                        : sequentialColumnIndex;

                    rowValues[columnIndex] = GetCellValue(cellElement, mainNamespace, sharedStrings);
                    sequentialColumnIndex = columnIndex + 1;
                    maxColumnIndex = Math.Max(maxColumnIndex, columnIndex);
                }

                rowMaps.Add(rowValues);
            }

            if (maxColumnIndex == 0)
            {
                return [];
            }

            List<string[]> rows = rowMaps
                .Select(map => Enumerable.Range(1, maxColumnIndex)
                    .Select(columnIndex => map.TryGetValue(columnIndex, out string? value) ? value : string.Empty)
                    .ToArray())
                .ToList();

            while (rows.Count > 0 && rows[^1].All(string.IsNullOrWhiteSpace))
            {
                rows.RemoveAt(rows.Count - 1);
            }

            return rows;
        }

        private static string GetCellValue(XElement cellElement, XNamespace mainNamespace, IReadOnlyList<string> sharedStrings)
        {
            string cellType = (string?)cellElement.Attribute("t") ?? string.Empty;

            return cellType switch
            {
                "s" => GetSharedStringValue(cellElement, mainNamespace, sharedStrings),
                "inlineStr" => string.Concat(cellElement.Descendants(mainNamespace + "t").Select(element => element.Value)),
                "b" => string.Equals(cellElement.Element(mainNamespace + "v")?.Value, "1", StringComparison.Ordinal) ? "TRUE" : "FALSE",
                _ => cellElement.Element(mainNamespace + "v")?.Value ?? string.Concat(cellElement.Descendants(mainNamespace + "t").Select(element => element.Value))
            };
        }

        private static string GetSharedStringValue(XElement cellElement, XNamespace mainNamespace, IReadOnlyList<string> sharedStrings)
        {
            string? rawValue = cellElement.Element(mainNamespace + "v")?.Value;

            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sharedStringIndex) &&
                sharedStringIndex >= 0 &&
                sharedStringIndex < sharedStrings.Count)
            {
                return sharedStrings[sharedStringIndex];
            }

            return rawValue ?? string.Empty;
        }

        private static XDocument LoadXmlEntry(ZipArchive archive, string entryPath)
        {
            ZipArchiveEntry? entry = archive.GetEntry(entryPath);

            if (entry is null)
            {
                throw new FileNotFoundException($"A(z) '{entryPath}' bejegyzes nem talalhato az Excel fajlban.");
            }

            using Stream entryStream = entry.Open();
            return XDocument.Load(entryStream);
        }

        private static string CombineArchivePath(string baseDirectory, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return baseDirectory;
            }

            if (relativePath.StartsWith("/", StringComparison.Ordinal))
            {
                return NormalizeArchivePath(relativePath);
            }

            return NormalizeArchivePath($"{baseDirectory}/{relativePath}");
        }

        private static string NormalizeArchivePath(string path)
        {
            List<string> parts = [];

            foreach (string segment in path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries))
            {
                if (segment == ".")
                {
                    continue;
                }

                if (segment == "..")
                {
                    if (parts.Count > 0)
                    {
                        parts.RemoveAt(parts.Count - 1);
                    }

                    continue;
                }

                parts.Add(segment);
            }

            return string.Join("/", parts);
        }

        private static int GetColumnIndexFromCellReference(string cellReference)
        {
            int columnIndex = 0;

            foreach (char character in cellReference)
            {
                if (!char.IsLetter(character))
                {
                    break;
                }

                columnIndex = (columnIndex * 26) + (char.ToUpperInvariant(character) - 'A' + 1);
            }

            return Math.Max(columnIndex, 1);
        }

        private static void CreateImportTemplate(string filePath)
        {
            TemplateSheet[] sheets =
            [
                new("Termekek", ["SKU", "Nev", "Ar", "Keszlet", "TermekTipus", "Leiras"]),
                new("Kategoriak", ["SKU", "KategoriaSlug"]),
                new("Kepek", ["SKU", "KepUtvonal", "KepNev"]),
                new("OpciokTulajdonsagok", ["SKU", "TulajdonsagNev", "TulajdonsagErtek"])
            ];

            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using ZipArchive archive = new(fileStream, ZipArchiveMode.Create);

            CreateArchiveEntry(archive, "[Content_Types].xml", BuildContentTypesXml(sheets.Length));
            CreateArchiveEntry(archive, "_rels/.rels", BuildRootRelationshipsXml());
            CreateArchiveEntry(archive, "docProps/app.xml", BuildAppXml(sheets));
            CreateArchiveEntry(archive, "docProps/core.xml", BuildCoreXml());
            CreateArchiveEntry(archive, "xl/workbook.xml", BuildWorkbookXml(sheets));
            CreateArchiveEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationshipsXml(sheets.Length));
            CreateArchiveEntry(archive, "xl/styles.xml", BuildStylesXml());

            for (int i = 0; i < sheets.Length; i++)
            {
                string worksheetPath = $"xl/worksheets/sheet{i + 1}.xml";
                string worksheetXml = BuildWorksheetXml(sheets[i], i == 0);
                CreateArchiveEntry(archive, worksheetPath, worksheetXml);
            }
        }

        private static void CreateArchiveEntry(ZipArchive archive, string entryName, string content)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

            using Stream entryStream = entry.Open();
            using StreamWriter writer = new(entryStream, new UTF8Encoding(false));

            writer.Write(content);
        }

        private static string BuildContentTypesXml(int sheetCount)
        {
            StringBuilder builder = new();
            builder.Append("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                """);

            for (int i = 1; i <= sheetCount; i++)
            {
                builder.Append($"""
                  
                  <Override PartName="/xl/worksheets/sheet{i}.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  """);
            }

            builder.Append("""
                  
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                  <Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
                  <Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>
                </Types>
                """);

            return builder.ToString();
        }

        private static string BuildRootRelationshipsXml()
        {
            return """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
                  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>
                </Relationships>
                """;
        }

        private static string BuildAppXml(TemplateSheet[] sheets)
        {
            StringBuilder titlesBuilder = new();

            foreach (TemplateSheet sheet in sheets)
            {
                titlesBuilder.AppendLine($"      <vt:lpstr>{EscapeXml(sheet.Name)}</vt:lpstr>");
            }

            return $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">
                  <Application>Microsoft Excel</Application>
                  <HeadingPairs>
                    <vt:vector size="2" baseType="variant">
                      <vt:variant><vt:lpstr>Worksheets</vt:lpstr></vt:variant>
                      <vt:variant><vt:i4>{sheets.Length}</vt:i4></vt:variant>
                    </vt:vector>
                  </HeadingPairs>
                  <TitlesOfParts>
                    <vt:vector size="{sheets.Length}" baseType="lpstr">
                {titlesBuilder.ToString().TrimEnd()}
                    </vt:vector>
                  </TitlesOfParts>
                </Properties>
                """;
        }

        private static string BuildCoreXml()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            return $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:dcterms="http://purl.org/dc/terms/" xmlns:dcmitype="http://purl.org/dc/dcmitype/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                  <dc:creator>AszAdmin 1.0</dc:creator>
                  <cp:lastModifiedBy>AszAdmin 1.0</cp:lastModifiedBy>
                  <dcterms:created xsi:type="dcterms:W3CDTF">{timestamp}</dcterms:created>
                  <dcterms:modified xsi:type="dcterms:W3CDTF">{timestamp}</dcterms:modified>
                </cp:coreProperties>
                """;
        }

        private static string BuildWorkbookXml(TemplateSheet[] sheets)
        {
            StringBuilder sheetsBuilder = new();

            for (int i = 0; i < sheets.Length; i++)
            {
                sheetsBuilder.AppendLine($"    <sheet name=\"{EscapeXml(sheets[i].Name)}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>");
            }

            return $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <bookViews>
                    <workbookView xWindow="0" yWindow="0" windowWidth="16000" windowHeight="9000"/>
                  </bookViews>
                  <sheets>
                {sheetsBuilder.ToString().TrimEnd()}
                  </sheets>
                </workbook>
                """;
        }

        private static string BuildWorkbookRelationshipsXml(int sheetCount)
        {
            StringBuilder builder = new();
            builder.Append("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                """);

            for (int i = 1; i <= sheetCount; i++)
            {
                builder.Append($"""
                  
                  <Relationship Id="rId{i}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet{i}.xml"/>
                  """);
            }

            builder.Append($"""
                  
                  <Relationship Id="rId{sheetCount + 1}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);

            return builder.ToString();
        }

        private static string BuildStylesXml()
        {
            return """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="2">
                    <font>
                      <sz val="11"/>
                      <name val="Calibri"/>
                      <family val="2"/>
                    </font>
                    <font>
                      <b/>
                      <sz val="11"/>
                      <name val="Calibri"/>
                      <family val="2"/>
                    </font>
                  </fonts>
                  <fills count="2">
                    <fill><patternFill patternType="none"/></fill>
                    <fill><patternFill patternType="gray125"/></fill>
                  </fills>
                  <borders count="1">
                    <border><left/><right/><top/><bottom/><diagonal/></border>
                  </borders>
                  <cellStyleXfs count="1">
                    <xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
                  </cellStyleXfs>
                  <cellXfs count="2">
                    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
                    <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/>
                  </cellXfs>
                  <cellStyles count="1">
                    <cellStyle name="Normal" xfId="0" builtinId="0"/>
                  </cellStyles>
                </styleSheet>
                """;
        }

        private static string BuildWorksheetXml(TemplateSheet sheet, bool isSelected)
        {
            string dimension = $"A1:{GetCellReference(sheet.Headers.Length, 1)}";
            string selectedAttribute = isSelected ? " tabSelected=\"1\"" : string.Empty;

            StringBuilder columnsBuilder = new();

            for (int i = 0; i < sheet.Headers.Length; i++)
            {
                double width = Math.Max(14, sheet.Headers[i].Length + 4);
                string widthValue = width.ToString(CultureInfo.InvariantCulture);
                columnsBuilder.AppendLine($"    <col min=\"{i + 1}\" max=\"{i + 1}\" width=\"{widthValue}\" customWidth=\"1\"/>");
            }

            StringBuilder cellsBuilder = new();

            for (int i = 0; i < sheet.Headers.Length; i++)
            {
                string cellReference = GetCellReference(i + 1, 1);
                string headerText = EscapeXml(sheet.Headers[i]);
                cellsBuilder.AppendLine($"      <c r=\"{cellReference}\" s=\"1\" t=\"inlineStr\"><is><t>{headerText}</t></is></c>");
            }

            return $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <dimension ref="{dimension}"/>
                  <sheetViews>
                    <sheetView workbookViewId="0"{selectedAttribute}/>
                  </sheetViews>
                  <sheetFormatPr defaultRowHeight="15"/>
                  <cols>
                {columnsBuilder.ToString().TrimEnd()}
                  </cols>
                  <sheetData>
                    <row r="1">
                {cellsBuilder.ToString().TrimEnd()}
                    </row>
                  </sheetData>
                  <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
                </worksheet>
                """;
        }

        private static string GetCellReference(int columnIndex, int rowIndex)
        {
            StringBuilder columnName = new();
            int currentIndex = columnIndex;

            while (currentIndex > 0)
            {
                currentIndex--;
                columnName.Insert(0, (char)('A' + (currentIndex % 26)));
                currentIndex /= 26;
            }

            return $"{columnName}{rowIndex}";
        }

        private static string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("'", "&apos;", StringComparison.Ordinal);
        }

        private sealed record TemplateSheet(string Name, string[] Headers);
        private sealed record WorksheetPreview(string Name, List<string[]> Rows);
    }
}
