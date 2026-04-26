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
        private const int NavigationBarHeight = 56;
        private const int NavigationButtonHeight = 40;
        private const int NavigationButtonGap = 10;
        private const int StatusLabelHeight = 36;
        private const string DefaultHotcakesCultureCode = "en-US";
        private readonly List<WorksheetPreview> loadedWorkbookSheets = [];
        private readonly List<HotcakesCategorySnapshot> loadedCategories = [];
        private readonly List<HotcakesProductTypeSnapshot> loadedProductTypes = [];
        private readonly List<HotcakesProductPropertySnapshot> loadedProductProperties = [];
        private readonly Dictionary<string, HotcakesProduct> loadedProductsBySku = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HotcakesProductTypeSnapshot> loadedProductTypesByBvinToken = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HotcakesProductTypeSnapshot> loadedProductTypesByNameToken = new(StringComparer.Ordinal);
        private readonly HashSet<string> ambiguousProductTypeNameTokens = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HotcakesProductPropertySnapshot> loadedProductPropertiesByNameToken = new(StringComparer.Ordinal);
        private readonly HashSet<string> ambiguousProductPropertyNameTokens = new(StringComparer.Ordinal);
        private readonly HotcakesApiClient hotcakesClient;
        private readonly ImportHistoryStore importHistoryStore = new();
        private readonly Panel navigationPanel = new();
        private readonly Button importPageButton = new();
        private readonly Button bulkOperationsPageButton = new();
        private readonly Label importStatusLabel = new();
        private bool isUpdatingSheetSelection;
        private bool hotcakesReady;
        private bool isInitializingHotcakes;
        private bool isLoadingHotcakesProducts;
        private bool isImporting;
        private FormPage currentPage = FormPage.Import;
        private ImportValidationResult? lastValidationResult;

        public Form1()
        {
            InitializeComponent();
            hotcakesClient = new HotcakesApiClient(AppSettings.Current.Hotcakes);
            ConfigureImportStatusLabel();
            ConfigureNavigationBar();
            InitializeSelections();

            browseButton.Click += BrowseButton_Click;
            SablonButton.Click += SablonButton_Click;
            sheetComboBox.SelectedIndexChanged += SheetComboBox_SelectedIndexChanged;
            importTypeComboBox.SelectedIndexChanged += ImportTypeComboBox_SelectedIndexChanged;
            statusFilterComboBox.SelectedIndexChanged += StatusFilterComboBox_SelectedIndexChanged;
            validateButton.Click += ValidateButton_Click;
            importButton.Click += ImportButton_Click;
            historyButton.Click += HistoryButton_Click;
            Load += (_, _) => UpdateResponsiveLayout();
            Load += async (_, _) => await InitializeHotcakesAsync();
            Resize += (_, _) => UpdateResponsiveLayout();
            FormClosed += (_, _) => hotcakesClient.Dispose();

            UpdateActionStates();
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
            UpdateStatusCategoryFilterUI();
            SetStatusMessage("Valassz import fajlt az indulashoz.");
        }

        private void ConfigureImportStatusLabel()
        {
            importStatusLabel.AutoEllipsis = true;
            importStatusLabel.ForeColor = Color.DimGray;
            importStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            importStatusLabel.Text = "Hotcakes kapcsolat elokeszitese...";
            footerPanel.Controls.Add(importStatusLabel);
        }

        private void ConfigureNavigationBar()
        {
            navigationPanel.BackColor = Color.Black;
            navigationPanel.Dock = DockStyle.Top;
            navigationPanel.Height = NavigationBarHeight;
            navigationPanel.TabStop = false;

            ConfigureNavigationButton(importPageButton, "Importalas");
            ConfigureNavigationButton(bulkOperationsPageButton, "Tomeges muveletek");

            importPageButton.Click += (_, _) => SetCurrentPage(FormPage.Import);
            bulkOperationsPageButton.Click += (_, _) => SetCurrentPage(FormPage.BulkOperations);

            navigationPanel.Controls.Add(importPageButton);
            navigationPanel.Controls.Add(bulkOperationsPageButton);
            Controls.Add(navigationPanel);
            navigationPanel.BringToFront();

            ApplyCurrentPageState();
            UpdateNavigationState();
        }

        private static void ConfigureNavigationButton(Button button, string text)
        {
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(56, 56, 56);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(36, 36, 36);
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Margin = Padding.Empty;
            button.Text = text;
            button.UseVisualStyleBackColor = false;
        }

        private void SetCurrentPage(FormPage page)
        {
            if (currentPage == page)
            {
                return;
            }

            currentPage = page;
            ApplyCurrentPageState();
            UpdateNavigationState();
            AutoScrollPosition = new Point(0, 0);
            UpdateResponsiveLayout();
        }

        private void ApplyCurrentPageState()
        {
            bool isImportPage = currentPage == FormPage.Import;

            titleLabel.Text = isImportPage ? "Importalas" : "Tomeges muveletek";
            fileGroupBox.Visible = isImportPage;
            optionsGroupBox.Visible = isImportPage;
            previewGroupBox.Visible = isImportPage;
            footerPanel.Visible = isImportPage;
            priceGroupBox.Visible = !isImportPage;
            statusGroupBox.Visible = !isImportPage;
            categoryGroupBox.Visible = false;
            deleteGroupBox.Visible = false;
            bulkTitleLabel.Visible = false;
        }

        private void UpdateNavigationState()
        {
            StyleNavigationButton(importPageButton, currentPage == FormPage.Import);
            StyleNavigationButton(bulkOperationsPageButton, currentPage == FormPage.BulkOperations);
        }

        private static void StyleNavigationButton(Button button, bool isActive)
        {
            button.BackColor = isActive ? Color.White : Color.Black;
            button.ForeColor = isActive ? Color.Black : Color.White;
        }

        private void SetStatusMessage(string message, bool isError = false)
        {
            importStatusLabel.Text = message;
            importStatusLabel.ForeColor = isError ? Color.Firebrick : Color.DimGray;
        }

        private async Task InitializeHotcakesAsync()
        {
            if (isInitializingHotcakes)
            {
                return;
            }

            isInitializingHotcakes = true;
            UpdateActionStates();
            SetStatusMessage("Hotcakes kategoriak, termektipusok es tulajdonsagok betoltese...");

            try
            {
                IReadOnlyList<HotcakesCategorySnapshot> categories = await hotcakesClient.GetCategoriesAsync();
                IReadOnlyList<HotcakesProductTypeSnapshot> productTypes = [];
                IReadOnlyList<HotcakesProductPropertySnapshot> productProperties = [];

                loadedCategories.Clear();
                loadedCategories.AddRange(categories
                    .OrderBy(category => category.Name, StringComparer.CurrentCultureIgnoreCase));

                try
                {
                    productTypes = await hotcakesClient.GetProductTypesAsync();
                }
                catch (Exception ex)
                {
                    productTypes = [];

                    MessageBox.Show(
                        this,
                        $"A Hotcakes termektipusok betoltese nem sikerult.{Environment.NewLine}{Environment.NewLine}{ex.Message}{Environment.NewLine}{Environment.NewLine}A tovabbi import akkor tud TermekTipus mezot kezelni, ha ez a lista betoltheto.",
                        "Hotcakes kapcsolat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                loadedProductTypes.Clear();
                loadedProductTypes.AddRange(productTypes
                    .OrderBy(productType => productType.ProductTypeName, StringComparer.CurrentCultureIgnoreCase));
                RebuildProductTypeLookups();

                try
                {
                    productProperties = await hotcakesClient.GetProductPropertiesAsync();
                }
                catch (Exception ex)
                {
                    productProperties = [];

                    MessageBox.Show(
                        this,
                        $"A Hotcakes termektulajdonsagok betoltese nem sikerult.{Environment.NewLine}{Environment.NewLine}{ex.Message}{Environment.NewLine}{Environment.NewLine}A tulajdonsagimport csak akkor tud biztonsagosan meglevo property-khez kapcsolodni, ha ez a lista betoltheto.",
                        "Hotcakes kapcsolat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                loadedProductProperties.Clear();
                loadedProductProperties.AddRange(productProperties
                    .OrderBy(productProperty => string.IsNullOrWhiteSpace(productProperty.DisplayName)
                        ? productProperty.PropertyName
                        : productProperty.DisplayName,
                        StringComparer.CurrentCultureIgnoreCase));
                RebuildProductPropertyLookups();

                PopulateCategorySelectors();
                hotcakesReady = true;
                SetStatusMessage($"{loadedCategories.Count} Hotcakes kategoria, {loadedProductTypes.Count} termektipus, {loadedProductProperties.Count} termektulajdonsag betoltve.");
            }
            catch (Exception ex)
            {
                hotcakesReady = false;
                SetStatusMessage("Hotcakes kapcsolat nem elerheto. Az Excel elonezet tovabbra is mukodik.", true);

                MessageBox.Show(
                    this,
                    $"A Hotcakes kategoriak betoltese nem sikerult.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Hotcakes kapcsolat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                isInitializingHotcakes = false;
                UpdateActionStates();
            }
        }

        private void PopulateCategorySelectors()
        {
            List<CategoryComboItem> categoryItems = loadedCategories
                .Select(category => new CategoryComboItem(category.Name, category.Bvin, category.RewriteUrl))
                .ToList();

            List<object> priceItems = [new CategoryComboItem("Osszes kategoria", null, null)];
            priceItems.AddRange(categoryItems);

            List<object> targetItems = [new CategoryComboItem("Valasszon...", null, null)];
            targetItems.AddRange(categoryItems);

            List<object> statusItems = [new CategoryComboItem("Valasszon kategoriat...", null, null)];
            statusItems.AddRange(categoryItems);

            ReplaceComboBoxItems(priceCategoryComboBox, priceItems, 0);
            ReplaceComboBoxItems(sourceCategoryComboBox, categoryItems.Cast<object>().ToList(), categoryItems.Count > 0 ? 0 : -1);
            ReplaceComboBoxItems(targetCategoryComboBox, targetItems, 0);
            ReplaceComboBoxItems(statusCategoryComboBox, statusItems, 0);
            UpdateStatusCategoryFilterUI();
        }

        private static void ReplaceComboBoxItems(ComboBox comboBox, List<object> items, int selectedIndex)
        {
            comboBox.BeginUpdate();

            try
            {
                comboBox.Items.Clear();
                comboBox.Items.AddRange(items.ToArray());

                comboBox.SelectedIndex = selectedIndex >= 0 && selectedIndex < comboBox.Items.Count
                    ? selectedIndex
                    : -1;
            }
            finally
            {
                comboBox.EndUpdate();
            }
        }

        private void RebuildProductTypeLookups()
        {
            loadedProductTypesByBvinToken.Clear();
            loadedProductTypesByNameToken.Clear();
            ambiguousProductTypeNameTokens.Clear();

            foreach (HotcakesProductTypeSnapshot productType in loadedProductTypes)
            {
                string normalizedBvin = NormalizeToken(productType.Bvin);

                if (!string.IsNullOrWhiteSpace(normalizedBvin))
                {
                    loadedProductTypesByBvinToken[normalizedBvin] = productType;
                }

                string normalizedName = NormalizeToken(productType.ProductTypeName);

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    continue;
                }

                if (ambiguousProductTypeNameTokens.Contains(normalizedName))
                {
                    continue;
                }

                if (loadedProductTypesByNameToken.TryGetValue(normalizedName, out HotcakesProductTypeSnapshot? existingProductType) &&
                    !string.Equals(existingProductType.Bvin, productType.Bvin, StringComparison.OrdinalIgnoreCase))
                {
                    loadedProductTypesByNameToken.Remove(normalizedName);
                    ambiguousProductTypeNameTokens.Add(normalizedName);
                    continue;
                }

                loadedProductTypesByNameToken[normalizedName] = productType;
            }
        }

        private void RebuildProductPropertyLookups()
        {
            loadedProductPropertiesByNameToken.Clear();
            ambiguousProductPropertyNameTokens.Clear();

            foreach (HotcakesProductPropertySnapshot productProperty in loadedProductProperties)
            {
                RegisterProductPropertyLookupToken(productProperty.PropertyName, productProperty);
                RegisterProductPropertyLookupToken(productProperty.DisplayName, productProperty);
            }
        }

        private void RegisterProductPropertyLookupToken(string rawValue, HotcakesProductPropertySnapshot productProperty)
        {
            string normalizedValue = NormalizeToken(rawValue);

            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                return;
            }

            if (ambiguousProductPropertyNameTokens.Contains(normalizedValue))
            {
                return;
            }

            if (loadedProductPropertiesByNameToken.TryGetValue(normalizedValue, out HotcakesProductPropertySnapshot? existingProperty) &&
                existingProperty.Id != productProperty.Id)
            {
                loadedProductPropertiesByNameToken.Remove(normalizedValue);
                ambiguousProductPropertyNameTokens.Add(normalizedValue);
                return;
            }

            loadedProductPropertiesByNameToken[normalizedValue] = productProperty;
        }

        private bool TryResolveProductType(
            string rawValue,
            out HotcakesProductTypeSnapshot? productType,
            out string? failureReason)
        {
            productType = null;
            failureReason = null;

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            string normalizedValue = NormalizeToken(rawValue);

            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                failureReason = "A megadott TermekTipus ertek nem tartalmaz feloldhato karaktereket.";
                return false;
            }

            if (loadedProductTypesByBvinToken.TryGetValue(normalizedValue, out HotcakesProductTypeSnapshot? productTypeByBvin))
            {
                productType = productTypeByBvin;
                return true;
            }

            if (ambiguousProductTypeNameTokens.Contains(normalizedValue))
            {
                failureReason = "A megadott TermekTipus tobb Hotcakes termektipusra is illeszkedik, ezert nem egyertelmu.";
                return false;
            }

            if (loadedProductTypesByNameToken.TryGetValue(normalizedValue, out HotcakesProductTypeSnapshot? productTypeByName))
            {
                productType = productTypeByName;
                return true;
            }

            failureReason = loadedProductTypes.Count == 0
                ? "A Hotcakes termektipus lista nem erheto el."
                : "Nincs ilyen nevvel vagy BVIN-nel Hotcakes termektipus.";

            return false;
        }

        private bool TryResolveExistingProductProperty(
            string rawValue,
            out HotcakesProductPropertySnapshot? productProperty,
            out string? failureReason)
        {
            productProperty = null;
            failureReason = null;

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                failureReason = "A megadott tulajdonsagnev ures.";
                return false;
            }

            string normalizedValue = NormalizeToken(rawValue);

            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                failureReason = "A megadott tulajdonsagnev nem tartalmaz feloldhato karaktereket.";
                return false;
            }

            if (ambiguousProductPropertyNameTokens.Contains(normalizedValue))
            {
                failureReason = $"A megadott tulajdonsagnev tobb Hotcakes property-re is illeszkedik: {rawValue}.";
                return false;
            }

            return loadedProductPropertiesByNameToken.TryGetValue(normalizedValue, out productProperty);
        }

        private string ResolveProductTypeIdOrThrow(string rawValue, int rowNumber)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return string.Empty;
            }

            if (TryResolveProductType(rawValue, out HotcakesProductTypeSnapshot? productType, out string? failureReason))
            {
                return productType?.Bvin ?? string.Empty;
            }

            throw new InvalidOperationException(
                $"A(z) {rowNumber}. sor TermekTipus mezoje nem oldhato fel: '{rawValue}'. {failureReason}");
        }

        private void UpdateActionStates()
        {
            bool isBusy = isInitializingHotcakes || isLoadingHotcakesProducts || isImporting;

            validateButton.Enabled = hotcakesReady && !isBusy && loadedWorkbookSheets.Count > 0;
            importButton.Enabled = hotcakesReady && !isBusy && lastValidationResult?.CanProceed == true;
        }

        private void UpdateResponsiveLayout()
        {
            if (ClientSize.Width <= 0)
            {
                return;
            }

            SuspendLayout();
            navigationPanel.SuspendLayout();
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
                LayoutNavigationBar();
                int contentWidth = Math.Max(MinimumContentWidth, ClientSize.Width - (PageMargin * 2));
                int currentY = navigationPanel.Bottom + 20;

                titleLabel.Location = new Point(PageMargin, currentY);
                currentY = titleLabel.Bottom + 20;

                if (currentPage == FormPage.Import)
                {
                    currentY = LayoutFileSection(contentWidth, currentY);
                    currentY = LayoutOptionsSection(contentWidth, currentY);
                    currentY = LayoutPreviewSection(contentWidth, currentY);
                    currentY = LayoutFooter(contentWidth, currentY);
                }
                else
                {
                    currentY = LayoutBulkSections(contentWidth, currentY);
                }

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
                navigationPanel.ResumeLayout();
                ResumeLayout();
            }
        }

        private void LayoutNavigationBar()
        {
            const int topPadding = 8;
            const int importButtonWidth = 150;
            const int bulkButtonWidth = 230;

            navigationPanel.Height = NavigationBarHeight;
            importPageButton.SetBounds(PageMargin, topPadding, importButtonWidth, NavigationButtonHeight);
            bulkOperationsPageButton.SetBounds(
                importPageButton.Right + NavigationButtonGap,
                topPadding,
                bulkButtonWidth,
                NavigationButtonHeight);
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

                priceGroupBox.SetBounds(PageMargin, y, cardWidth, BulkCardHeight);
                statusGroupBox.SetBounds(rightColumnX, y, cardWidth, BulkCardHeight);
                LayoutStatusFilterControls();

                return Math.Max(priceGroupBox.Bottom, statusGroupBox.Bottom) + SectionSpacing;
            }

            priceGroupBox.SetBounds(PageMargin, y, contentWidth, BulkCardHeight);
            statusGroupBox.SetBounds(PageMargin, priceGroupBox.Bottom + CardSpacing, contentWidth, BulkCardHeight);
            LayoutStatusFilterControls();

            return statusGroupBox.Bottom + SectionSpacing;
        }

        private void StatusFilterComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateStatusCategoryFilterUI();
        }

        private void UpdateStatusCategoryFilterUI()
        {
            bool showCategorySelector = IsStatusCategoryFilterSelected();
            bool hasRealCategories = loadedCategories.Count > 0;

            statusCategoryComboBox.Visible = showCategorySelector;
            statusCategoryComboBox.Enabled = showCategorySelector && hasRealCategories;
            LayoutStatusFilterControls();
        }

        private bool IsStatusCategoryFilterSelected()
        {
            string selectedFilter = NormalizeToken(statusFilterComboBox.SelectedItem?.ToString() ?? string.Empty);
            return selectedFilter.Contains("ADOTTKATEGORIA", StringComparison.Ordinal);
        }

        private void LayoutStatusFilterControls()
        {
            const int left = 30;
            const int right = 30;
            const int top = 135;
            const int gap = 12;

            int contentWidth = Math.Max(220, statusGroupBox.ClientSize.Width - left - right);

            if (statusCategoryComboBox.Visible)
            {
                int filterWidth = Math.Max(190, (contentWidth - gap) / 2);
                int categoryWidth = Math.Max(190, contentWidth - filterWidth - gap);

                statusFilterComboBox.SetBounds(left, top, filterWidth, statusFilterComboBox.Height);
                statusCategoryComboBox.SetBounds(statusFilterComboBox.Right + gap, top, categoryWidth, statusCategoryComboBox.Height);
                return;
            }

            statusFilterComboBox.SetBounds(left, top, contentWidth, statusFilterComboBox.Height);
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

                int statusLeft = historyButton.Right + buttonGap;
                int statusWidth = Math.Max(0, validateButton.Left - buttonGap - statusLeft);
                importStatusLabel.SetBounds(statusLeft, 8, statusWidth, StatusLabelHeight);
            }
            else
            {
                int fullWidth = footerPanel.Width;

                historyButton.SetBounds(0, 0, fullWidth, buttonHeight);
                validateButton.SetBounds(0, historyButton.Bottom + 8, fullWidth, buttonHeight);
                importButton.SetBounds(0, validateButton.Bottom + 8, fullWidth, buttonHeight);
                importStatusLabel.SetBounds(0, importButton.Bottom + 8, fullWidth, StatusLabelHeight);

                footerPanel.Height = importStatusLabel.Bottom;
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
                lastValidationResult = null;
                UpdateActionStates();

                if (hotcakesReady)
                {
                    SetStatusMessage($"{loadedWorkbookSheets.Count} munkalap betoltve. Futtasd az ellenorzest a kovetkezo lepeshez.");
                }
            }
            catch (Exception ex)
            {
                loadedWorkbookSheets.Clear();
                filePathTextBox.Clear();
                PopulateSheetSelector();
                lastValidationResult = null;
                UpdateActionStates();

                if (hotcakesReady)
                {
                    SetStatusMessage("Az Excel fajl beolvasasa nem sikerult.", true);
                }

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

            lastValidationResult = null;
            UpdateActionStates();

            if (hotcakesReady && sheetComboBox.SelectedItem is not null)
            {
                SetStatusMessage($"Kivalasztott munkalap: {sheetComboBox.SelectedItem}.");
            }

            ShowSelectedWorksheet();
        }

        private void ImportTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            TrySelectSuggestedSheet();
            lastValidationResult = null;
            UpdateActionStates();

            if (hotcakesReady && loadedWorkbookSheets.Count > 0)
            {
                SetStatusMessage("Az import tipus frissult. Ellenorizd a kijelolt munkalapot.");
            }
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
                TrySelectSuggestedSheet();
                ShowSelectedWorksheet();
            }

            UpdateActionStates();
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

        private void TrySelectSuggestedSheet()
        {
            string? suggestedSheetName = GetSuggestedSheetName();

            if (string.IsNullOrWhiteSpace(suggestedSheetName))
            {
                return;
            }

            int suggestedIndex = loadedWorkbookSheets.FindIndex(
                sheet => string.Equals(NormalizeToken(sheet.Name), NormalizeToken(suggestedSheetName), StringComparison.Ordinal));

            if (suggestedIndex < 0 || suggestedIndex == sheetComboBox.SelectedIndex)
            {
                return;
            }

            isUpdatingSheetSelection = true;

            try
            {
                sheetComboBox.SelectedIndex = suggestedIndex;
            }
            finally
            {
                isUpdatingSheetSelection = false;
            }

            ShowSelectedWorksheet();
        }

        private string? GetSuggestedSheetName()
        {
            string selectedImportType = NormalizeToken(importTypeComboBox.SelectedItem?.ToString() ?? string.Empty);

            if (selectedImportType.Contains("KEPIMPORT", StringComparison.Ordinal))
            {
                return "Kepek";
            }

            if (selectedImportType.Contains("KATEGORIAIMPORT", StringComparison.Ordinal))
            {
                return "Kategoriak";
            }

            if (selectedImportType.Contains("TULAJDONSAGIMPORT", StringComparison.Ordinal))
            {
                return "OpciokTulajdonsagok";
            }

            if (selectedImportType.Contains("TERMEKIMPORT", StringComparison.Ordinal) ||
                selectedImportType.Contains("OSSZESIMPORTALASA", StringComparison.Ordinal))
            {
                return "Termekek";
            }

            return null;
        }

        private async void ValidateButton_Click(object? sender, EventArgs e)
        {
            try
            {
                UseWaitCursor = true;
                await EnsureProductsLoadedAsync();

                ImportValidationResult validationResult = ValidateCurrentImport();
                lastValidationResult = validationResult;
                UpdateActionStates();
                SetStatusMessage(validationResult.StatusMessage, !validationResult.CanProceed);

                MessageBox.Show(
                    this,
                    validationResult.DetailsMessage,
                    "Import ellenorzes",
                    MessageBoxButtons.OK,
                    validationResult.CanProceed ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                lastValidationResult = null;
                UpdateActionStates();
                SetStatusMessage("Az import ellenorzese nem sikerult.", true);

                MessageBox.Show(
                    this,
                    $"Az import ellenorzese nem sikerult.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Import ellenorzes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async void ImportButton_Click(object? sender, EventArgs e)
        {
            if (lastValidationResult is null)
            {
                MessageBox.Show(
                    this,
                    "Import inditas elott futtasd le az ellenorzest.",
                    "Import inditasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!lastValidationResult.CanProceed)
            {
                MessageBox.Show(
                    this,
                    "Az import inditasa elott javitsd a validacios hibakat, majd futtasd ujra az ellenorzest.",
                    "Import inditasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmationResult = MessageBox.Show(
                this,
                lastValidationResult.ConfirmationMessage,
                "Import inditasa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmationResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                UseWaitCursor = true;
                isImporting = true;
                UpdateActionStates();

                ImportExecutionResult importResult = await RunValidatedImportAsync();
                string? historySaveWarning = null;

                try
                {
                    SaveImportHistory(importResult);
                }
                catch (Exception ex)
                {
                    historySaveWarning = ex.Message;
                }

                try
                {
                    lastValidationResult = ValidateCurrentImport();
                }
                catch
                {
                    lastValidationResult = null;
                }

                UpdateActionStates();
                SetStatusMessage(importResult.StatusMessage, importResult.ErrorCount > 0);

                string detailsMessage = importResult.DetailsMessage;

                if (!string.IsNullOrWhiteSpace(historySaveWarning))
                {
                    detailsMessage +=
                        $"{Environment.NewLine}{Environment.NewLine}Figyelem: az importelozmeny mentese nem sikerult.{Environment.NewLine}{historySaveWarning}";
                }

                MessageBox.Show(
                    this,
                    detailsMessage,
                    "Import eredmeny",
                    MessageBoxButtons.OK,
                    importResult.ErrorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SetStatusMessage("Az import futtatasa nem sikerult.", true);

                MessageBox.Show(
                    this,
                    $"Az import futtatasa nem sikerult.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Import inditasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isImporting = false;
                UpdateActionStates();
                UseWaitCursor = false;
            }
        }

        private void HistoryButton_Click(object? sender, EventArgs e)
        {
            try
            {
                IReadOnlyList<ImportHistoryEntry> historyEntries = importHistoryStore.LoadAll();

                using ImportHistoryDialog historyDialog = new(historyEntries);
                historyDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Az importelozmenyek megnyitasa nem sikerult.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Import elozmenyek",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private ImportScope GetSelectedImportScope()
        {
            string selectedImportType = NormalizeToken(importTypeComboBox.SelectedItem?.ToString() ?? string.Empty);

            if (selectedImportType.Contains("KEPIMPORT", StringComparison.Ordinal))
            {
                return ImportScope.Images;
            }

            if (selectedImportType.Contains("KATEGORIAIMPORT", StringComparison.Ordinal))
            {
                return ImportScope.Categories;
            }

            if (selectedImportType.Contains("TULAJDONSAGIMPORT", StringComparison.Ordinal))
            {
                return ImportScope.Properties;
            }

            if (selectedImportType.Contains("OSSZESIMPORTALASA", StringComparison.Ordinal))
            {
                return ImportScope.All;
            }

            return ImportScope.Products;
        }

        private void SaveImportHistory(ImportExecutionResult importResult)
        {
            ArgumentNullException.ThrowIfNull(importResult);

            string importTypeLabel = importTypeComboBox.SelectedItem?.ToString()?.Trim() ?? "Ismeretlen import";
            string sourceFilePath = filePathTextBox.Text.Trim();

            importHistoryStore.Append(new ImportHistoryEntry
            {
                ImportedAt = DateTimeOffset.Now,
                ImportTypeLabel = importTypeLabel,
                SourceFilePath = sourceFilePath,
                CreatedCount = importResult.CreatedCount,
                UpdatedCount = importResult.UpdatedCount,
                SkippedExistingCount = importResult.SkippedExistingCount,
                ProductTypeAppliedCount = importResult.ProductTypeAppliedCount,
                CategoryLinkedCount = importResult.CategoryLinkedCount,
                CategoryAlreadyLinkedCount = importResult.CategoryAlreadyLinkedCount,
                ImageUploadedCount = importResult.ImageUploadedCount,
                MainImageUploadedCount = importResult.MainImageUploadedCount,
                AdditionalImageUploadedCount = importResult.AdditionalImageUploadedCount,
                PropertyAppliedCount = importResult.PropertyAppliedCount,
                ErrorCount = importResult.ErrorCount,
                StatusMessage = importResult.StatusMessage,
                DetailsMessage = importResult.DetailsMessage
            });
        }

        private bool IncludesProducts(ImportScope importScope)
        {
            return importScope is ImportScope.Products or ImportScope.All;
        }

        private bool IncludesCategories(ImportScope importScope)
        {
            return importScope is ImportScope.Products or ImportScope.Categories or ImportScope.All;
        }

        private bool IncludesImages(ImportScope importScope)
        {
            return importScope is ImportScope.Images or ImportScope.All;
        }

        private bool IncludesProperties(ImportScope importScope)
        {
            return importScope is ImportScope.Properties or ImportScope.All;
        }

        private async Task EnsureProductsLoadedAsync()
        {
            if (loadedProductsBySku.Count > 0)
            {
                return;
            }

            isLoadingHotcakesProducts = true;
            UpdateActionStates();
            SetStatusMessage("Hotcakes termekek betoltese...");

            try
            {
                IReadOnlyList<HotcakesProduct> products = await hotcakesClient.GetAllProductsAsync();
                int duplicateSkuCount = 0;

                loadedProductsBySku.Clear();

                foreach (HotcakesProduct product in products)
                {
                    string sku = product.Sku.Trim();

                    if (string.IsNullOrWhiteSpace(sku))
                    {
                        continue;
                    }

                    if (!loadedProductsBySku.TryAdd(sku, product))
                    {
                        duplicateSkuCount++;
                    }
                }

                string duplicateSuffix = duplicateSkuCount > 0
                    ? $" ({duplicateSkuCount} duplikalt API SKU kihagyva)"
                    : string.Empty;

                SetStatusMessage($"{loadedProductsBySku.Count} Hotcakes termek betoltve{duplicateSuffix}.");
            }
            finally
            {
                isLoadingHotcakesProducts = false;
                UpdateActionStates();
            }
        }

        private ImportValidationResult ValidateCurrentImport()
        {
            ImportScope importScope = GetSelectedImportScope();
            bool includesProducts = IncludesProducts(importScope);
            bool includesCategories = IncludesCategories(importScope);
            bool includesImages = IncludesImages(importScope);
            bool includesProperties = IncludesProperties(importScope);

            WorksheetTable? productTable = includesProducts
                ? BuildWorksheetTable(GetProductWorksheetForValidation())
                : null;
            WorksheetTable? categoryTable = includesCategories
                ? importScope == ImportScope.Categories
                    ? GetRequiredNamedWorksheetTable("Kategoriak")
                    : TryBuildWorksheetTable("Kategoriak")
                : null;
            WorksheetTable? imageTable = includesImages
                ? importScope == ImportScope.Images
                    ? GetRequiredNamedWorksheetTable("Kepek")
                    : TryBuildWorksheetTable("Kepek")
                : null;
            WorksheetTable? propertyTable = includesProperties
                ? importScope == ImportScope.Properties
                    ? GetRequiredNamedWorksheetTable("OpciokTulajdonsagok")
                    : TryBuildWorksheetTable("OpciokTulajdonsagok")
                : null;

            int productRowCount = 0;
            int existingProductCount = 0;
            int newProductCount = 0;
            bool productCanProceed = true;
            int productIssueCount = 0;
            StringBuilder detailsBuilder = new();
            HashSet<string> importedProductSkus = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> importedProductTypeValuesBySku = new(StringComparer.OrdinalIgnoreCase);

            if (productTable is not null)
            {
                int skuColumnIndex = GetRequiredColumnIndex(productTable, "SKU");
                int nameColumnIndex = GetOptionalColumnIndex(productTable, "Nev", "Name");
                int priceColumnIndex = GetOptionalColumnIndex(productTable, "Ar", "Price");
                int stockColumnIndex = GetOptionalColumnIndex(productTable, "Keszlet", "Inventory", "Stock");
                int productTypeColumnIndex = GetOptionalColumnIndex(productTable, "TermekTipus", "ProductType", "ProductTypeName");

                HashSet<string> seenSkus = new(StringComparer.OrdinalIgnoreCase);
                HashSet<string> duplicateSkus = new(StringComparer.OrdinalIgnoreCase);
                int missingSkuCount = 0;
                int missingNameForNewProductCount = 0;
                int invalidPriceCount = 0;
                int invalidStockCount = 0;
                int unresolvedProductTypeCount = 0;

                List<string> missingNameSkus = [];
                List<string> invalidPriceRows = [];
                List<string> invalidStockRows = [];
                List<string> unresolvedProductTypeRows = [];

                foreach ((int rowNumber, string[] rowValues) in productTable.Rows)
                {
                    string sku = GetCellValue(rowValues, skuColumnIndex);

                    if (string.IsNullOrWhiteSpace(sku))
                    {
                        missingSkuCount++;
                        continue;
                    }

                    importedProductSkus.Add(sku);

                    if (!seenSkus.Add(sku))
                    {
                        duplicateSkus.Add(sku);
                        continue;
                    }

                    string rawPrice = GetCellValue(rowValues, priceColumnIndex);

                    if (!string.IsNullOrWhiteSpace(rawPrice) && !TryParseImportDecimal(rawPrice, out _))
                    {
                        invalidPriceCount++;
                        invalidPriceRows.Add($"{sku} (sor {rowNumber})");
                    }

                    string rawStock = GetCellValue(rowValues, stockColumnIndex);

                    if (!string.IsNullOrWhiteSpace(rawStock) && !TryParseImportInt(rawStock, out _))
                    {
                        invalidStockCount++;
                        invalidStockRows.Add($"{sku} (sor {rowNumber})");
                    }

                    string rawProductType = GetCellValue(rowValues, productTypeColumnIndex);
                    importedProductTypeValuesBySku[sku] = rawProductType;

                    if (!string.IsNullOrWhiteSpace(rawProductType) &&
                        !TryResolveProductType(rawProductType, out _, out string? productTypeFailureReason))
                    {
                        unresolvedProductTypeCount++;
                        unresolvedProductTypeRows.Add($"{sku} (sor {rowNumber}) -> {rawProductType} ({productTypeFailureReason})");
                    }

                    if (loadedProductsBySku.ContainsKey(sku))
                    {
                        existingProductCount++;
                    }
                    else
                    {
                        newProductCount++;

                        if (nameColumnIndex < 0 || string.IsNullOrWhiteSpace(GetCellValue(rowValues, nameColumnIndex)))
                        {
                            missingNameForNewProductCount++;
                            missingNameSkus.Add(sku);
                        }
                    }
                }

                productRowCount = productTable.Rows.Count;
                productIssueCount = missingSkuCount +
                                    duplicateSkus.Count +
                                    missingNameForNewProductCount +
                                    invalidPriceCount +
                                    invalidStockCount +
                                    unresolvedProductTypeCount;
                productCanProceed = productTable.Rows.Count > 0 && productIssueCount == 0;

                detailsBuilder.AppendLine($"Termek munkalap: {productTable.Name}");
                detailsBuilder.AppendLine($"Adatsorok: {productTable.Rows.Count}");
                detailsBuilder.AppendLine($"Frissitheto termekek: {existingProductCount}");
                detailsBuilder.AppendLine($"Uj termekek: {newProductCount}");
                detailsBuilder.AppendLine($"Ures SKU sorok: {missingSkuCount}");
                detailsBuilder.AppendLine($"Duplikalt SKU-k: {duplicateSkus.Count}");
                detailsBuilder.AppendLine($"Uj termeknel hianyzo Nev mezok: {missingNameForNewProductCount}");
                detailsBuilder.AppendLine($"Hibas Ar mezok: {invalidPriceCount}");
                detailsBuilder.AppendLine($"Hibas Keszlet mezok: {invalidStockCount}");
                detailsBuilder.AppendLine($"Nem feloldhato TermekTipus ertekek: {unresolvedProductTypeCount}");

                if (duplicateSkus.Count > 0)
                {
                    detailsBuilder.AppendLine($"Pelda duplikalt SKU-k: {string.Join(", ", duplicateSkus.Take(5))}");
                }

                if (missingNameSkus.Count > 0)
                {
                    detailsBuilder.AppendLine($"Nev nelkuli uj SKU-k: {string.Join(", ", missingNameSkus.Take(5))}");
                }

                if (invalidPriceRows.Count > 0)
                {
                    detailsBuilder.AppendLine($"Hibas Ar mezok: {string.Join(", ", invalidPriceRows.Take(5))}");
                }

                if (invalidStockRows.Count > 0)
                {
                    detailsBuilder.AppendLine($"Hibas Keszlet mezok: {string.Join(", ", invalidStockRows.Take(5))}");
                }

                if (unresolvedProductTypeRows.Count > 0)
                {
                    detailsBuilder.AppendLine($"Nem feloldhato TermekTipus sorok: {string.Join(", ", unresolvedProductTypeRows.Take(5))}");
                }

            }

            CategorySheetValidationResult categoryValidation = includesCategories
                ? ValidateCategorySheet(categoryTable, importedProductSkus)
                : new CategorySheetValidationResult(0, 0, 0, 0, true, string.Empty);
            ImageSheetValidationResult imageValidation = includesImages
                ? ValidateImageSheet(imageTable, importedProductSkus)
                : new ImageSheetValidationResult(0, 0, 0, 0, true, string.Empty);
            PropertySheetValidationResult propertyValidation = includesProperties
                ? ValidatePropertySheet(propertyTable, importedProductSkus, importedProductTypeValuesBySku)
                : new PropertySheetValidationResult(0, 0, 0, 0, true, string.Empty);

            if (!string.IsNullOrWhiteSpace(categoryValidation.DetailsMessage))
            {
                if (detailsBuilder.Length > 0)
                {
                    detailsBuilder.AppendLine();
                }

                detailsBuilder.Append(categoryValidation.DetailsMessage);
            }

            if (!string.IsNullOrWhiteSpace(imageValidation.DetailsMessage))
            {
                if (detailsBuilder.Length > 0)
                {
                    detailsBuilder.AppendLine();
                }

                detailsBuilder.Append(imageValidation.DetailsMessage);
            }

            if (!string.IsNullOrWhiteSpace(propertyValidation.DetailsMessage))
            {
                if (detailsBuilder.Length > 0)
                {
                    detailsBuilder.AppendLine();
                }

                detailsBuilder.Append(propertyValidation.DetailsMessage);
            }

            int includedRowCount = productRowCount +
                                   categoryValidation.RowCount +
                                   imageValidation.RowCount +
                                   propertyValidation.RowCount;
            int totalIssueCount = productIssueCount +
                                  categoryValidation.UnknownCategorySlugCount +
                                  categoryValidation.UnknownCategorySkuCount +
                                  categoryValidation.IncompleteRowCount +
                                  imageValidation.MissingFileCount +
                                  imageValidation.UnknownSkuCount +
                                  imageValidation.IncompleteRowCount +
                                  propertyValidation.UnknownSkuCount +
                                  propertyValidation.IncompleteRowCount +
                                  propertyValidation.InvalidPropertyCount;

            bool canProceed = includedRowCount > 0 &&
                              productCanProceed &&
                              categoryValidation.CanProceed &&
                              imageValidation.CanProceed &&
                              propertyValidation.CanProceed;

            if (detailsBuilder.Length > 0)
            {
                detailsBuilder.AppendLine();
            }

            detailsBuilder.AppendLine(canProceed
                ? "Az import keszen all a tenyleges feltoltes futtatasara."
                : "Az import inditasa elott javitsd a fenti eltereseket.");

            List<string> statusParts = [];

            if (includesProducts)
            {
                statusParts.Add($"{productRowCount} termeksor: {existingProductCount} frissitheto, {newProductCount} uj");
            }

            if (includesCategories && categoryValidation.RowCount > 0)
            {
                statusParts.Add($"{categoryValidation.RowCount} kategoriakapcsolat");
            }

            if (includesImages && imageValidation.RowCount > 0)
            {
                statusParts.Add($"{imageValidation.RowCount} kepsor");
            }

            if (includesProperties && propertyValidation.RowCount > 0)
            {
                statusParts.Add($"{propertyValidation.RowCount} tulajdonsagsor");
            }

            string statusMessage = statusParts.Count == 0
                ? "Nem talalhato importalhato adatsor."
                : $"{string.Join(", ", statusParts)} ellenorizve.";

            if (totalIssueCount > 0)
            {
                statusMessage += $" {totalIssueCount} validacios problemaval.";
            }
            else
            {
                statusMessage += " Az import indithato.";
            }

            return new ImportValidationResult(
                canProceed,
                productRowCount,
                existingProductCount,
                newProductCount,
                categoryValidation.RowCount,
                imageValidation.RowCount,
                propertyValidation.RowCount,
                totalIssueCount,
                statusMessage,
                detailsBuilder.ToString(),
                BuildConfirmationMessage(
                    includesProducts,
                    includesCategories,
                    includesImages,
                    includesProperties,
                    productRowCount,
                    existingProductCount,
                    newProductCount,
                    categoryValidation.RowCount,
                    imageValidation.RowCount,
                    propertyValidation.RowCount));
        }

        private CategorySheetValidationResult ValidateCategorySheet(
            WorksheetTable? categoryTable,
            IReadOnlySet<string> importedProductSkus)
        {
            if (categoryTable is null)
            {
                return new CategorySheetValidationResult(0, 0, 0, 0, true, string.Empty);
            }

            int categorySkuColumnIndex = GetRequiredColumnIndex(categoryTable, "SKU");
            int categorySlugColumnIndex = GetRequiredColumnIndex(categoryTable, "KategoriaSlug", "CategorySlug", "RewriteUrl");
            HashSet<string> knownCategorySlugs = loadedCategories
                .Select(category => NormalizeToken(category.RewriteUrl))
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .ToHashSet(StringComparer.Ordinal);

            int rowCount = 0;
            int unknownCategorySkuCount = 0;
            int incompleteCategoryRowCount = 0;
            HashSet<string> unknownCategorySlugs = new(StringComparer.OrdinalIgnoreCase);
            List<string> unknownCategorySkus = [];

            foreach ((int rowNumber, string[] rowValues) in categoryTable.Rows)
            {
                string sku = GetCellValue(rowValues, categorySkuColumnIndex);
                string categorySlug = GetCellValue(rowValues, categorySlugColumnIndex);

                bool hasSku = !string.IsNullOrWhiteSpace(sku);
                bool hasCategorySlug = !string.IsNullOrWhiteSpace(categorySlug);

                if (!hasSku && !hasCategorySlug)
                {
                    continue;
                }

                if (!hasSku || !hasCategorySlug)
                {
                    incompleteCategoryRowCount++;
                    continue;
                }

                rowCount++;

                if (!knownCategorySlugs.Contains(NormalizeToken(categorySlug)))
                {
                    unknownCategorySlugs.Add(categorySlug);
                }

                if (!loadedProductsBySku.ContainsKey(sku) && !importedProductSkus.Contains(sku))
                {
                    unknownCategorySkuCount++;
                    unknownCategorySkus.Add($"{sku} (sor {rowNumber})");
                }
            }

            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine($"Kategoria munkalap: {categoryTable.Name}");
            detailsBuilder.AppendLine($"Kategoriarendeles sorok: {rowCount}");
            detailsBuilder.AppendLine($"Ismeretlen KategoriaSlug ertekek: {unknownCategorySlugs.Count}");
            detailsBuilder.AppendLine($"Nem feloldhato kategoriak SKU alapjan: {unknownCategorySkuCount}");
            detailsBuilder.AppendLine($"Hianyos kategoriarow-k: {incompleteCategoryRowCount}");

            if (unknownCategorySlugs.Count > 0)
            {
                detailsBuilder.AppendLine($"Ismeretlen kategoriak: {string.Join(", ", unknownCategorySlugs.Take(5))}");
            }

            if (unknownCategorySkus.Count > 0)
            {
                detailsBuilder.AppendLine($"Nem feloldhato kategoriarow-k: {string.Join(", ", unknownCategorySkus.Take(5))}");
            }

            bool canProceed = unknownCategorySlugs.Count == 0 &&
                              unknownCategorySkuCount == 0 &&
                              incompleteCategoryRowCount == 0;

            return new CategorySheetValidationResult(
                rowCount,
                unknownCategorySlugs.Count,
                unknownCategorySkuCount,
                incompleteCategoryRowCount,
                canProceed,
                detailsBuilder.ToString());
        }

        private ImageSheetValidationResult ValidateImageSheet(
            WorksheetTable? imageTable,
            IReadOnlySet<string> importedProductSkus)
        {
            if (imageTable is null)
            {
                return new ImageSheetValidationResult(0, 0, 0, 0, true, string.Empty);
            }

            int skuColumnIndex = GetRequiredColumnIndex(imageTable, "SKU");
            int imagePathColumnIndex = GetOptionalColumnIndex(imageTable, "KepUtvonal", "ImagePath", "ImageFolder");
            int imageNameColumnIndex = GetOptionalColumnIndex(imageTable, "KepNev", "ImageName", "FileName");
            string workbookFilePath = GetCurrentWorkbookFilePath();

            int rowCount = 0;
            int missingFileCount = 0;
            int unknownSkuCount = 0;
            int incompleteRowCount = 0;

            List<string> missingFileRows = [];
            List<string> unknownSkuRows = [];
            Dictionary<string, int> uploadableImageRowCountsBySku = new(StringComparer.OrdinalIgnoreCase);

            foreach ((int rowNumber, string[] rowValues) in imageTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string imagePath = GetCellValue(rowValues, imagePathColumnIndex);
                string imageName = GetCellValue(rowValues, imageNameColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) && string.IsNullOrWhiteSpace(imagePath) && string.IsNullOrWhiteSpace(imageName))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || (string.IsNullOrWhiteSpace(imagePath) && string.IsNullOrWhiteSpace(imageName)))
                {
                    incompleteRowCount++;
                    continue;
                }

                rowCount++;

                if (!loadedProductsBySku.ContainsKey(sku) && !importedProductSkus.Contains(sku))
                {
                    unknownSkuCount++;
                    unknownSkuRows.Add($"{sku} (sor {rowNumber})");
                    continue;
                }

                if (!TryResolveImageFilePath(workbookFilePath, imagePath, imageName, out string? resolvedPath))
                {
                    missingFileCount++;
                    missingFileRows.Add($"{sku} (sor {rowNumber}) -> {BuildImageReference(imagePath, imageName)}");
                    continue;
                }

                if (!File.Exists(resolvedPath))
                {
                    missingFileCount++;
                    missingFileRows.Add($"{sku} (sor {rowNumber}) -> {resolvedPath}");
                    continue;
                }

                uploadableImageRowCountsBySku[sku] = uploadableImageRowCountsBySku.TryGetValue(sku, out int existingCount)
                    ? existingCount + 1
                    : 1;
            }

            int uploadableMainImageCount = uploadableImageRowCountsBySku.Count;
            int uploadableAdditionalImageCount = uploadableImageRowCountsBySku.Values.Sum(static count => Math.Max(0, count - 1));
            int multiImageSkuCount = uploadableImageRowCountsBySku.Values.Count(static count => count > 1);
            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine($"Kepek munkalap: {imageTable.Name}");
            detailsBuilder.AppendLine($"Kepsorok: {rowCount}");
            detailsBuilder.AppendLine($"Hianyzo vagy nem feloldhato kepfajlok: {missingFileCount}");
            detailsBuilder.AppendLine($"Nem feloldhato SKU-k: {unknownSkuCount}");
            detailsBuilder.AppendLine($"Hianyos kepsorok: {incompleteRowCount}");
            detailsBuilder.AppendLine($"Tobb kepet kapo SKU-k: {multiImageSkuCount}");
            detailsBuilder.AppendLine($"Feltoltheto kepek bontasa: {uploadableMainImageCount} fokep, {uploadableAdditionalImageCount} tovabbi kep");

            if (missingFileRows.Count > 0)
            {
                detailsBuilder.AppendLine($"Pelda hianyzo kepfajlok: {string.Join(", ", missingFileRows.Take(5))}");
            }

            if (unknownSkuRows.Count > 0)
            {
                detailsBuilder.AppendLine($"Nem feloldhato kep SKU-k: {string.Join(", ", unknownSkuRows.Take(5))}");
            }

            if (rowCount > 0)
            {
                detailsBuilder.AppendLine("SKU-nkent az elso sikeresen feloldott kep fokepkent, a tobbi tovabbi kepkent kerul feltoltesre.");
            }

            bool canProceed = missingFileCount == 0 &&
                              unknownSkuCount == 0 &&
                              incompleteRowCount == 0;

            return new ImageSheetValidationResult(
                rowCount,
                missingFileCount,
                unknownSkuCount,
                incompleteRowCount,
                canProceed,
                detailsBuilder.ToString());
        }

        private PropertySheetValidationResult ValidatePropertySheet(
            WorksheetTable? propertyTable,
            IReadOnlySet<string> importedProductSkus,
            IReadOnlyDictionary<string, string> importedProductTypeValuesBySku)
        {
            if (propertyTable is null)
            {
                return new PropertySheetValidationResult(0, 0, 0, 0, true, string.Empty);
            }

            int skuColumnIndex = GetRequiredColumnIndex(propertyTable, "SKU");
            int propertyNameColumnIndex = GetRequiredColumnIndex(propertyTable, "TulajdonsagNev", "PropertyName", "Key");
            int propertyValueColumnIndex = GetOptionalColumnIndex(propertyTable, "TulajdonsagErtek", "PropertyValue", "Value");

            int rowCount = 0;
            int unknownSkuCount = 0;
            int incompleteRowCount = 0;
            int invalidPropertyCount = 0;
            List<string> unknownSkuRows = [];
            List<string> invalidPropertyRows = [];
            HashSet<string> createablePropertyTokens = new(StringComparer.Ordinal);

            foreach ((int rowNumber, string[] rowValues) in propertyTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string propertyName = GetCellValue(rowValues, propertyNameColumnIndex);
                string propertyValue = GetCellValue(rowValues, propertyValueColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) &&
                    string.IsNullOrWhiteSpace(propertyName) &&
                    string.IsNullOrWhiteSpace(propertyValue))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(propertyName))
                {
                    incompleteRowCount++;
                    continue;
                }

                rowCount++;

                if (!loadedProductsBySku.ContainsKey(sku) && !importedProductSkus.Contains(sku))
                {
                    unknownSkuCount++;
                    unknownSkuRows.Add($"{sku} (sor {rowNumber})");
                    continue;
                }

                bool hasProductType = loadedProductsBySku.TryGetValue(sku, out HotcakesProduct? existingProduct) &&
                                      !string.IsNullOrWhiteSpace(existingProduct.ProductTypeId);

                if (!hasProductType &&
                    importedProductTypeValuesBySku.TryGetValue(sku, out string? importedProductTypeValue) &&
                    !string.IsNullOrWhiteSpace(importedProductTypeValue))
                {
                    hasProductType = true;
                }

                if (!hasProductType)
                {
                    invalidPropertyCount++;
                    invalidPropertyRows.Add($"{sku} (sor {rowNumber}) -> a termekhez nincs feloldhato TermekTipus.");
                    continue;
                }

                bool hasExistingProperty = TryResolveExistingProductProperty(
                    propertyName,
                    out _,
                    out string? propertyFailureReason);

                if (!string.IsNullOrWhiteSpace(propertyFailureReason))
                {
                    invalidPropertyCount++;
                    invalidPropertyRows.Add($"{sku} (sor {rowNumber}) -> {propertyName} ({propertyFailureReason})");
                    continue;
                }

                if (!hasExistingProperty)
                {
                    createablePropertyTokens.Add(NormalizeToken(propertyName));
                }
            }

            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine($"Tulajdonsag munkalap: {propertyTable.Name}");
            detailsBuilder.AppendLine($"Tulajdonsagsorok: {rowCount}");
            detailsBuilder.AppendLine($"Nem feloldhato SKU-k: {unknownSkuCount}");
            detailsBuilder.AppendLine($"Hianyos tulajdonsagsorok: {incompleteRowCount}");
            detailsBuilder.AppendLine($"Nem importalhato tulajdonsagsorok: {invalidPropertyCount}");
            detailsBuilder.AppendLine($"Ujonnan letrehozhato Hotcakes property-k: {createablePropertyTokens.Count}");

            if (unknownSkuRows.Count > 0)
            {
                detailsBuilder.AppendLine($"Nem feloldhato tulajdonsag SKU-k: {string.Join(", ", unknownSkuRows.Take(5))}");
            }

            if (invalidPropertyRows.Count > 0)
            {
                detailsBuilder.AppendLine($"Nem importalhato tulajdonsagsorok: {string.Join(", ", invalidPropertyRows.Take(5))}");
            }

            bool canProceed = unknownSkuCount == 0 &&
                              incompleteRowCount == 0 &&
                              invalidPropertyCount == 0;

            return new PropertySheetValidationResult(
                rowCount,
                unknownSkuCount,
                incompleteRowCount,
                invalidPropertyCount,
                canProceed,
                detailsBuilder.ToString());
        }

        private WorksheetTable GetRequiredNamedWorksheetTable(string sheetName)
        {
            return TryBuildWorksheetTable(sheetName)
                ?? throw new InvalidOperationException($"A(z) '{sheetName}' munkalap nem talalhato az importfajlban.");
        }

        private static string BuildConfirmationMessage(
            bool includesProducts,
            bool includesCategories,
            bool includesImages,
            bool includesProperties,
            int productRowCount,
            int existingProductCount,
            int newProductCount,
            int categoryRowCount,
            int imageRowCount,
            int propertyRowCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("Valoban elinditod az importot?");
            builder.AppendLine();

            if (includesProducts)
            {
                builder.AppendLine($"Termeksorok: {productRowCount}");
                builder.AppendLine($"Uj termekek: {newProductCount}");
                builder.AppendLine($"Meglevo termekek: {existingProductCount}");
            }

            if (includesCategories)
            {
                builder.AppendLine($"Kategoriakapcsolatok: {categoryRowCount}");
            }

            if (includesImages)
            {
                builder.AppendLine($"Kepsorok: {imageRowCount}");
            }

            if (includesProperties)
            {
                builder.AppendLine($"Tulajdonsagsorok: {propertyRowCount}");
            }

            return builder.ToString().TrimEnd();
        }

        private async Task<ImportExecutionResult> RunValidatedImportAsync()
        {
            await EnsureProductsLoadedAsync();

            ImportScope importScope = GetSelectedImportScope();
            bool includesProducts = IncludesProducts(importScope);
            bool includesCategories = IncludesCategories(importScope);
            bool includesImages = IncludesImages(importScope);
            bool includesProperties = IncludesProperties(importScope);

            WorksheetTable? productTable = includesProducts
                ? BuildWorksheetTable(GetProductWorksheetForValidation())
                : null;
            WorksheetTable? categoryTable = includesCategories
                ? importScope == ImportScope.Categories
                    ? GetRequiredNamedWorksheetTable("Kategoriak")
                    : TryBuildWorksheetTable("Kategoriak")
                : null;
            WorksheetTable? imageTable = includesImages
                ? importScope == ImportScope.Images
                    ? GetRequiredNamedWorksheetTable("Kepek")
                    : TryBuildWorksheetTable("Kepek")
                : null;
            WorksheetTable? propertyTable = includesProperties
                ? importScope == ImportScope.Properties
                    ? GetRequiredNamedWorksheetTable("OpciokTulajdonsagok")
                    : TryBuildWorksheetTable("OpciokTulajdonsagok")
                : null;

            List<ProductImportRow> productRows = productTable is null ? [] : ParseProductImportRows(productTable);
            List<CategoryImportRow> categoryRows = ParseCategoryImportRows(categoryTable);
            List<ImageImportRow> imageRows = ParseImageImportRows(imageTable);
            List<PropertyImportRow> propertyRows = ParsePropertyImportRows(propertyTable);

            ExistingProductImportMode existingProductMode = GetExistingProductImportMode();
            Dictionary<string, HotcakesProduct> resolvedProductsBySku = new(StringComparer.OrdinalIgnoreCase);
            List<string> errors = [];

            int createdCount = 0;
            int updatedCount = 0;
            int skippedExistingCount = 0;
            int categoryLinkedCount = 0;
            int categoryAlreadyLinkedCount = 0;
            int imageUploadedCount = 0;
            int mainImageUploadedCount = 0;
            int additionalImageUploadedCount = 0;
            int productTypeAppliedCount = 0;
            int propertyAppliedCount = 0;

            for (int index = 0; index < productRows.Count; index++)
            {
                ProductImportRow row = productRows[index];
                SetStatusMessage($"Termek import folyamatban... ({index + 1}/{productRows.Count})");

                try
                {
                    string resolvedProductTypeId = ResolveProductTypeIdOrThrow(row.ProductTypeName, row.RowNumber);

                    if (loadedProductsBySku.TryGetValue(row.Sku, out HotcakesProduct? existingProduct))
                    {
                        if (existingProductMode == ExistingProductImportMode.SkipExisting)
                        {
                            skippedExistingCount++;
                            resolvedProductsBySku[row.Sku] = existingProduct;
                            continue;
                        }

                        HotcakesProduct? currentProduct = await hotcakesClient.GetProductBySkuAsync(row.Sku);

                        if (currentProduct is null)
                        {
                            errors.Add($"A(z) {row.RowNumber}. sor termeke idokozben nem talalhato SKU alapjan: {row.Sku}.");
                            continue;
                        }

                        ApplyImportedProductValues(currentProduct, row, isNewProduct: false, resolvedProductTypeId);
                        HotcakesProduct savedProduct = await hotcakesClient.UpdateProductAsync(currentProduct);

                        if (row.Stock.HasValue)
                        {
                            await UpsertInventoryAsync(savedProduct, row.Stock.Value);
                        }

                        loadedProductsBySku[row.Sku] = savedProduct;
                        resolvedProductsBySku[row.Sku] = savedProduct;

                        if (!string.IsNullOrWhiteSpace(resolvedProductTypeId))
                        {
                            productTypeAppliedCount++;
                        }

                        updatedCount++;
                    }
                    else
                    {
                        HotcakesProduct newProduct = BuildImportedProduct(row, resolvedProductTypeId);
                        HotcakesProduct savedProduct = await hotcakesClient.CreateProductAsync(newProduct);

                        if (row.Stock.HasValue)
                        {
                            await UpsertInventoryAsync(savedProduct, row.Stock.Value);
                        }

                        loadedProductsBySku[row.Sku] = savedProduct;
                        resolvedProductsBySku[row.Sku] = savedProduct;

                        if (!string.IsNullOrWhiteSpace(resolvedProductTypeId))
                        {
                            productTypeAppliedCount++;
                        }

                        createdCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"A(z) {row.RowNumber}. sor termek importja nem sikerult ({row.Sku}): {ex.Message}");
                }
            }

            if (categoryRows.Count > 0)
            {
                (categoryLinkedCount, categoryAlreadyLinkedCount) = await ImportCategoryRowsAsync(categoryRows, resolvedProductsBySku, errors);
            }

            if (imageRows.Count > 0)
            {
                (imageUploadedCount, mainImageUploadedCount, additionalImageUploadedCount) =
                    await ImportImageRowsAsync(imageRows, resolvedProductsBySku, errors);
            }

            if (propertyRows.Count > 0)
            {
                propertyAppliedCount = await ImportPropertyRowsAsync(propertyRows, resolvedProductsBySku, errors);
            }

            StringBuilder detailsBuilder = new();
            detailsBuilder.AppendLine("Import eredmeny");
            detailsBuilder.AppendLine($"Letrehozott termekek: {createdCount}");
            detailsBuilder.AppendLine($"Frissitett termekek: {updatedCount}");
            detailsBuilder.AppendLine($"Kihagyott meglevo termekek: {skippedExistingCount}");
            detailsBuilder.AppendLine($"Beallitott termektipusok: {productTypeAppliedCount}");
            detailsBuilder.AppendLine($"Letrehozott kategoriakapcsolatok: {categoryLinkedCount}");
            detailsBuilder.AppendLine($"Mar letezo vagy duplikalt kategoriakapcsolatok: {categoryAlreadyLinkedCount}");
            detailsBuilder.AppendLine($"Feltoltott kepek: {imageUploadedCount} ({mainImageUploadedCount} fokep, {additionalImageUploadedCount} tovabbi kep)");
            detailsBuilder.AppendLine($"Beallitott tulajdonsagok: {propertyAppliedCount}");
            detailsBuilder.AppendLine($"Import hibak: {errors.Count}");

            if (errors.Count > 0)
            {
                detailsBuilder.AppendLine();
                detailsBuilder.AppendLine("Elso hibak:");

                foreach (string error in errors.Take(10))
                {
                    detailsBuilder.AppendLine($"- {error}");
                }
            }

            List<string> statusParts = [];

            if (includesProducts)
            {
                statusParts.Add($"{createdCount} uj");
                statusParts.Add($"{updatedCount} frissitett");
            }

            if (includesCategories)
            {
                statusParts.Add($"{categoryLinkedCount} kategoriakapcsolat");
            }

            if (includesImages)
            {
                statusParts.Add($"{imageUploadedCount} kep");
            }

            if (includesProperties)
            {
                statusParts.Add($"{propertyAppliedCount} tulajdonsag");
            }

            string statusMessage = statusParts.Count == 0
                ? "Import lefutott."
                : $"Import lefutott: {string.Join(", ", statusParts)} feldolgozva.";

            if (errors.Count > 0)
            {
                statusMessage += $" {errors.Count} hibaval.";
            }

            return new ImportExecutionResult(
                createdCount,
                updatedCount,
                skippedExistingCount,
                productTypeAppliedCount,
                categoryLinkedCount,
                categoryAlreadyLinkedCount,
                imageUploadedCount,
                mainImageUploadedCount,
                additionalImageUploadedCount,
                propertyAppliedCount,
                errors.Count,
                statusMessage,
                detailsBuilder.ToString());
        }

        private async Task<(int LinkedCount, int AlreadyLinkedCount)> ImportCategoryRowsAsync(
            IReadOnlyList<CategoryImportRow> categoryRows,
            IReadOnlyDictionary<string, HotcakesProduct> importedProductsBySku,
            List<string> errors)
        {
            if (categoryRows.Count == 0)
            {
                return (0, 0);
            }

            Dictionary<string, HotcakesCategorySnapshot> categoriesBySlug = loadedCategories
                .Where(category => !string.IsNullOrWhiteSpace(category.RewriteUrl))
                .ToDictionary(
                    category => NormalizeToken(category.RewriteUrl),
                    category => category,
                    StringComparer.Ordinal);

            Dictionary<string, HashSet<string>> assignedCategoryIdsByProduct = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> processedPairs = new(StringComparer.OrdinalIgnoreCase);

            int linkedCount = 0;
            int alreadyLinkedCount = 0;

            for (int index = 0; index < categoryRows.Count; index++)
            {
                CategoryImportRow row = categoryRows[index];
                SetStatusMessage($"Kategoriak kapcsolasa... ({index + 1}/{categoryRows.Count})");

                string normalizedSku = NormalizeToken(row.Sku);
                string normalizedCategorySlug = NormalizeToken(row.CategorySlug);
                string pairKey = $"{normalizedSku}|{normalizedCategorySlug}";

                if (!processedPairs.Add(pairKey))
                {
                    alreadyLinkedCount++;
                    continue;
                }

                try
                {
                    if (!importedProductsBySku.TryGetValue(row.Sku, out HotcakesProduct? product) &&
                        !loadedProductsBySku.TryGetValue(row.Sku, out product))
                    {
                        errors.Add($"A(z) {row.RowNumber}. kategoriarow nem talal termeket ehhez az SKU-hoz: {row.Sku}.");
                        continue;
                    }

                    if (!categoriesBySlug.TryGetValue(normalizedCategorySlug, out HotcakesCategorySnapshot? category))
                    {
                        errors.Add($"A(z) {row.RowNumber}. kategoriarow ismeretlen KategoriaSlug erteket tartalmaz: {row.CategorySlug}.");
                        continue;
                    }

                    if (!assignedCategoryIdsByProduct.TryGetValue(product.Bvin, out HashSet<string>? assignedCategoryIds))
                    {
                        IReadOnlyList<HotcakesCategorySnapshot> assignedCategories = await hotcakesClient.GetCategoriesForProductAsync(product.Bvin);
                        assignedCategoryIds = assignedCategories
                            .Select(assignedCategory => assignedCategory.Bvin)
                            .Where(bvin => !string.IsNullOrWhiteSpace(bvin))
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        assignedCategoryIdsByProduct[product.Bvin] = assignedCategoryIds;
                    }

                    if (assignedCategoryIds.Contains(category.Bvin))
                    {
                        alreadyLinkedCount++;
                        continue;
                    }

                    await hotcakesClient.CreateCategoryProductAssociationAsync(new HotcakesCategoryProductAssociation
                    {
                        CategoryId = category.Bvin,
                        ProductId = product.Bvin
                    });

                    assignedCategoryIds.Add(category.Bvin);
                    linkedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"A(z) {row.RowNumber}. kategoriarow importja nem sikerult ({row.Sku} / {row.CategorySlug}): {ex.Message}");
                }
            }

            return (linkedCount, alreadyLinkedCount);
        }

        private async Task<(int UploadedCount, int MainImageCount, int AdditionalImageCount)> ImportImageRowsAsync(
            IReadOnlyList<ImageImportRow> imageRows,
            IDictionary<string, HotcakesProduct> resolvedProductsBySku,
            List<string> errors)
        {
            if (imageRows.Count == 0)
            {
                return (0, 0, 0);
            }

            string workbookFilePath = GetCurrentWorkbookFilePath();
            int uploadedCount = 0;
            int mainImageCount = 0;
            int additionalImageCount = 0;
            Dictionary<string, int> successfulImageUploadCountsBySku = new(StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < imageRows.Count; index++)
            {
                ImageImportRow row = imageRows[index];
                bool uploadAsMainImage = !successfulImageUploadCountsBySku.ContainsKey(row.Sku);
                string uploadTypeLabel = uploadAsMainImage ? "fokep" : "tovabbi kep";
                SetStatusMessage($"Kepek feltoltese... ({index + 1}/{imageRows.Count}) - {uploadTypeLabel}");

                try
                {
                    HotcakesProduct? product = await ResolveProductBySkuAsync(row.Sku, resolvedProductsBySku);

                    if (product is null)
                    {
                        errors.Add($"A(z) {row.RowNumber}. kepsor nem talal termeket ehhez az SKU-hoz: {row.Sku}.");
                        continue;
                    }

                    if (!TryResolveImageFilePath(workbookFilePath, row.ImagePath, row.ImageName, out string? resolvedPath))
                    {
                        errors.Add($"A(z) {row.RowNumber}. kepsor kepfajlja nem talalhato: {BuildImageReference(row.ImagePath, row.ImageName)}.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(resolvedPath))
                    {
                        errors.Add($"A(z) {row.RowNumber}. kepsor kepfajlja ures feloldasi utvonalat adott vissza ({row.Sku}).");
                        continue;
                    }

                    byte[] fileContent = await File.ReadAllBytesAsync(resolvedPath);
                    string uploadFileName = Path.GetFileName(resolvedPath);
                    bool uploaded = uploadAsMainImage
                        ? await hotcakesClient.UploadProductMainImageAsync(product.Bvin, uploadFileName, fileContent)
                        : await hotcakesClient.UploadProductAdditionalImageAsync(product.Bvin, uploadFileName, fileContent);

                    if (!uploaded)
                    {
                        errors.Add($"A(z) {row.RowNumber}. kepsor {uploadTypeLabel} feltoltese sikertelen volt ({row.Sku}): {uploadFileName}.");
                        continue;
                    }

                    if (uploadAsMainImage)
                    {
                        product = await SaveMainImageMetadataAsync(product, uploadFileName);
                        loadedProductsBySku[row.Sku] = product;
                        resolvedProductsBySku[row.Sku] = product;
                    }

                    successfulImageUploadCountsBySku[row.Sku] = successfulImageUploadCountsBySku.TryGetValue(row.Sku, out int currentCount)
                        ? currentCount + 1
                        : 1;

                    uploadedCount++;

                    if (uploadAsMainImage)
                    {
                        mainImageCount++;
                    }
                    else
                    {
                        additionalImageCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"A(z) {row.RowNumber}. kepsor importja nem sikerult ({row.Sku}): {ex.Message}");
                }
            }

            return (uploadedCount, mainImageCount, additionalImageCount);
        }

        private async Task<HotcakesProduct> SaveMainImageMetadataAsync(HotcakesProduct product, string fileName)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

            string normalizedFileName = fileName.Trim();
            string alternateText = string.IsNullOrWhiteSpace(product.ProductName)
                ? normalizedFileName
                : product.ProductName.Trim();

            bool requiresUpdate =
                !string.Equals(product.ImageFileSmall, normalizedFileName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(product.ImageFileMedium, normalizedFileName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(product.ImageFileSmallAlternateText, alternateText, StringComparison.Ordinal) ||
                !string.Equals(product.ImageFileMediumAlternateText, alternateText, StringComparison.Ordinal);

            if (!requiresUpdate)
            {
                return product;
            }

            product.ImageFileSmall = normalizedFileName;
            product.ImageFileMedium = normalizedFileName;
            product.ImageFileSmallAlternateText = alternateText;
            product.ImageFileMediumAlternateText = alternateText;

            return await hotcakesClient.UpdateProductAsync(product);
        }

        private async Task<int> ImportPropertyRowsAsync(
            IReadOnlyList<PropertyImportRow> propertyRows,
            IDictionary<string, HotcakesProduct> resolvedProductsBySku,
            List<string> errors)
        {
            if (propertyRows.Count == 0)
            {
                return 0;
            }

            int appliedCount = 0;
            Dictionary<string, HashSet<long>> assignedPropertyIdsByProductType = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> nextSortOrdersByProductType = new(StringComparer.OrdinalIgnoreCase);

            foreach (IGrouping<string, PropertyImportRow> group in propertyRows
                         .GroupBy(static row => row.Sku, StringComparer.OrdinalIgnoreCase))
            {
                PropertyImportRow firstRow = group.First();
                SetStatusMessage($"Tulajdonsagok beallitasa... ({appliedCount + 1}/{propertyRows.Count})");

                try
                {
                    HotcakesProduct? product = await ResolveProductBySkuAsync(group.Key, resolvedProductsBySku);

                    if (product is null)
                    {
                        errors.Add($"A(z) {firstRow.RowNumber}. tulajdonsagsor nem talal termeket ehhez az SKU-hoz: {group.Key}.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(product.ProductTypeId))
                    {
                        errors.Add($"A(z) {firstRow.RowNumber}. tulajdonsagsorhoz tartozo termeknek nincs Hotcakes ProductType-ja: {group.Key}.");
                        continue;
                    }

                    if (!assignedPropertyIdsByProductType.TryGetValue(product.ProductTypeId, out HashSet<long>? assignedPropertyIds))
                    {
                        IReadOnlyList<HotcakesProductPropertySnapshot> assignedProperties =
                            await hotcakesClient.GetProductPropertiesForProductAsync(product.Bvin);

                        assignedPropertyIds = assignedProperties
                            .Select(assignedProperty => assignedProperty.Id)
                            .Where(static propertyId => propertyId > 0)
                            .ToHashSet();

                        assignedPropertyIdsByProductType[product.ProductTypeId] = assignedPropertyIds;
                        nextSortOrdersByProductType[product.ProductTypeId] = Math.Max(1, assignedPropertyIds.Count + 1);
                    }

                    foreach (PropertyImportRow row in group)
                    {
                        HotcakesProductPropertySnapshot productProperty =
                            await GetOrCreateProductPropertyAsync(row.PropertyName);

                        if (!assignedPropertyIds.Contains(productProperty.Id))
                        {
                            int sortOrder = nextSortOrdersByProductType[product.ProductTypeId];
                            bool linked = await hotcakesClient.AddPropertyToProductTypeAsync(
                                product.ProductTypeId,
                                productProperty.Id,
                                sortOrder);

                            if (!linked)
                            {
                                throw new InvalidOperationException(
                                    $"A(z) '{productProperty.DisplayName}' tulajdonsag nem rendelheto a termek termektipusahoz.");
                            }

                            assignedPropertyIds.Add(productProperty.Id);
                            nextSortOrdersByProductType[product.ProductTypeId] = sortOrder + 1;
                        }

                        bool saved = await hotcakesClient.SetProductPropertyValueAsync(
                            productProperty.Id,
                            product.Bvin,
                            row.PropertyValue);

                        if (!saved)
                        {
                            throw new InvalidOperationException(
                                $"A(z) '{productProperty.DisplayName}' tulajdonsagertek nem mentheto a Hotcakes-ben.");
                        }

                        appliedCount++;
                    }

                    loadedProductsBySku[group.Key] = product;
                    resolvedProductsBySku[group.Key] = product;
                }
                catch (Exception ex)
                {
                    errors.Add($"A(z) {firstRow.RowNumber}. tulajdonsag importja nem sikerult ({group.Key}): {ex.Message}");
                }
            }

            return appliedCount;
        }

        private async Task<HotcakesProductPropertySnapshot> GetOrCreateProductPropertyAsync(string propertyName)
        {
            bool hasExistingProperty = TryResolveExistingProductProperty(
                propertyName,
                out HotcakesProductPropertySnapshot? existingProperty,
                out string? failureReason);

            if (!string.IsNullOrWhiteSpace(failureReason))
            {
                throw new InvalidOperationException(failureReason);
            }

            if (hasExistingProperty && existingProperty is not null)
            {
                return existingProperty;
            }

            string trimmedPropertyName = propertyName.Trim();
            HotcakesProductPropertySnapshot createdProperty = await hotcakesClient.CreateProductPropertyAsync(new HotcakesProductPropertySnapshot
            {
                PropertyName = trimmedPropertyName,
                DisplayName = trimmedPropertyName,
                DisplayOnSite = true,
                DisplayToDropShipper = false,
                TypeCode = HotcakesProductPropertyTypes.TextField,
                DefaultValue = string.Empty,
                CultureCode = DefaultHotcakesCultureCode
            });

            if (createdProperty.Id <= 0)
            {
                throw new InvalidOperationException($"A(z) '{trimmedPropertyName}' Hotcakes tulajdonsag letrehozasa nem adott vissza ervenyes azonosito.");
            }

            loadedProductProperties.Add(createdProperty);
            RebuildProductPropertyLookups();

            return createdProperty;
        }

        private async Task<HotcakesProduct?> ResolveProductBySkuAsync(
            string sku,
            IDictionary<string, HotcakesProduct> resolvedProductsBySku)
        {
            if (resolvedProductsBySku.TryGetValue(sku, out HotcakesProduct? resolvedProduct))
            {
                return resolvedProduct;
            }

            if (loadedProductsBySku.TryGetValue(sku, out HotcakesProduct? cachedProduct))
            {
                HotcakesProduct? currentProduct = await hotcakesClient.GetProductBySkuAsync(sku) ?? cachedProduct;
                loadedProductsBySku[sku] = currentProduct;
                resolvedProductsBySku[sku] = currentProduct;
                return currentProduct;
            }

            HotcakesProduct? fetchedProduct = await hotcakesClient.GetProductBySkuAsync(sku);

            if (fetchedProduct is not null)
            {
                loadedProductsBySku[sku] = fetchedProduct;
                resolvedProductsBySku[sku] = fetchedProduct;
            }

            return fetchedProduct;
        }

        private static bool TryResolveImageFilePath(
            string workbookFilePath,
            string imagePath,
            string imageName,
            out string? resolvedPath)
        {
            string workbookDirectory = Path.GetDirectoryName(workbookFilePath) ?? AppContext.BaseDirectory;
            List<string> candidates = [];

            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                candidates.Add(imagePath);

                if (!Path.IsPathRooted(imagePath))
                {
                    candidates.Add(Path.Combine(workbookDirectory, imagePath));
                }
            }

            if (!string.IsNullOrWhiteSpace(imageName))
            {
                candidates.Add(imageName);

                if (!Path.IsPathRooted(imageName))
                {
                    candidates.Add(Path.Combine(workbookDirectory, imageName));
                }
            }

            if (!string.IsNullOrWhiteSpace(imagePath) && !string.IsNullOrWhiteSpace(imageName))
            {
                candidates.Add(Path.Combine(imagePath, imageName));

                if (!Path.IsPathRooted(imagePath))
                {
                    candidates.Add(Path.Combine(workbookDirectory, imagePath, imageName));
                }
            }

            foreach (string candidate in candidates.Where(candidate => !string.IsNullOrWhiteSpace(candidate)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                string normalizedCandidate = Path.IsPathRooted(candidate)
                    ? Path.GetFullPath(candidate)
                    : Path.GetFullPath(candidate);

                if (File.Exists(normalizedCandidate))
                {
                    resolvedPath = normalizedCandidate;
                    return true;
                }

                if (Directory.Exists(normalizedCandidate) && !string.IsNullOrWhiteSpace(imageName))
                {
                    string combined = Path.Combine(normalizedCandidate, imageName);

                    if (File.Exists(combined))
                    {
                        resolvedPath = Path.GetFullPath(combined);
                        return true;
                    }
                }
            }

            resolvedPath = null;
            return false;
        }

        private static string BuildImageReference(string imagePath, string imageName)
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && !string.IsNullOrWhiteSpace(imageName))
            {
                return $"{imagePath}\\{imageName}";
            }

            return !string.IsNullOrWhiteSpace(imagePath) ? imagePath : imageName;
        }

        private static HotcakesProduct BuildImportedProduct(ProductImportRow row, string productTypeId)
        {
            HotcakesProduct product = new()
            {
                Sku = row.Sku,
                ProductName = row.Name,
                ProductTypeId = productTypeId,
                ListPrice = row.Price ?? 0m,
                SitePrice = row.Price ?? 0m,
                LongDescription = row.Description,
                IsSearchable = true,
                IsAvailableForSale = true,
                AllowReviews = true,
                Status = HotcakesProductStatuses.Active,
                TaxExempt = false,
                InventoryMode = row.Stock.HasValue
                    ? HotcakesInventoryModes.WhenOutOfStockShow
                    : HotcakesInventoryModes.AlwayInStock
            };

            return product;
        }

        private static void ApplyImportedProductValues(
            HotcakesProduct product,
            ProductImportRow row,
            bool isNewProduct,
            string productTypeId)
        {
            if (isNewProduct || !string.IsNullOrWhiteSpace(row.Name))
            {
                product.ProductName = row.Name;
            }

            if (!string.IsNullOrWhiteSpace(productTypeId))
            {
                product.ProductTypeId = productTypeId;
            }

            if (row.Price.HasValue)
            {
                product.ListPrice = row.Price.Value;
                product.SitePrice = row.Price.Value;
            }

            if (!string.IsNullOrWhiteSpace(row.Description))
            {
                product.LongDescription = row.Description;
            }

            if (row.Stock.HasValue &&
                (product.InventoryMode == HotcakesInventoryModes.NotSet ||
                 product.InventoryMode == HotcakesInventoryModes.Unknown ||
                 product.InventoryMode == HotcakesInventoryModes.AlwayInStock))
            {
                product.InventoryMode = HotcakesInventoryModes.WhenOutOfStockShow;
            }

            if (isNewProduct)
            {
                product.AllowReviews ??= true;
                product.IsSearchable = true;
                product.IsAvailableForSale = true;
                product.Status = HotcakesProductStatuses.Active;
            }
        }

        private async Task UpsertInventoryAsync(HotcakesProduct product, int quantityOnHand)
        {
            HotcakesProductInventory inventory = new()
            {
                ProductBvin = product.Bvin,
                VariantId = string.Empty,
                QuantityOnHand = quantityOnHand,
                QuantityReserved = 0,
                LowStockPoint = 0,
                OutOfStockPoint = 0,
                LastUpdated = DateTime.UtcNow
            };

            await hotcakesClient.UpsertProductInventoryAsync(inventory);
        }

        private static List<ProductImportRow> ParseProductImportRows(WorksheetTable productTable)
        {
            int skuColumnIndex = GetRequiredColumnIndex(productTable, "SKU");
            int nameColumnIndex = GetOptionalColumnIndex(productTable, "Nev", "Name");
            int priceColumnIndex = GetOptionalColumnIndex(productTable, "Ar", "Price");
            int stockColumnIndex = GetOptionalColumnIndex(productTable, "Keszlet", "Inventory", "Stock");
            int productTypeColumnIndex = GetOptionalColumnIndex(productTable, "TermekTipus", "ProductType", "ProductTypeName");
            int descriptionColumnIndex = GetOptionalColumnIndex(productTable, "Leiras", "Description", "LongDescription");

            List<ProductImportRow> rows = [];

            foreach ((int rowNumber, string[] rowValues) in productTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);

                if (string.IsNullOrWhiteSpace(sku))
                {
                    continue;
                }

                string rawPrice = GetCellValue(rowValues, priceColumnIndex);
                decimal? price = null;

                if (!string.IsNullOrWhiteSpace(rawPrice))
                {
                    if (!TryParseImportDecimal(rawPrice, out decimal parsedPrice))
                    {
                        throw new InvalidOperationException($"A(z) {rowNumber}. sor Ar mezoje nem ervenyes: {rawPrice}");
                    }

                    price = parsedPrice;
                }

                string rawStock = GetCellValue(rowValues, stockColumnIndex);
                int? stock = null;

                if (!string.IsNullOrWhiteSpace(rawStock))
                {
                    if (!TryParseImportInt(rawStock, out int parsedStock))
                    {
                        throw new InvalidOperationException($"A(z) {rowNumber}. sor Keszlet mezoje nem ervenyes: {rawStock}");
                    }

                    stock = parsedStock;
                }

                rows.Add(new ProductImportRow(
                    rowNumber,
                    sku,
                    GetCellValue(rowValues, nameColumnIndex),
                    price,
                    stock,
                    GetCellValue(rowValues, productTypeColumnIndex),
                    GetCellValue(rowValues, descriptionColumnIndex)));
            }

            return rows;
        }

        private static List<CategoryImportRow> ParseCategoryImportRows(WorksheetTable? categoryTable)
        {
            if (categoryTable is null)
            {
                return [];
            }

            int skuColumnIndex = GetRequiredColumnIndex(categoryTable, "SKU");
            int categorySlugColumnIndex = GetRequiredColumnIndex(categoryTable, "KategoriaSlug", "CategorySlug", "RewriteUrl");

            List<CategoryImportRow> rows = [];

            foreach ((int rowNumber, string[] rowValues) in categoryTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string categorySlug = GetCellValue(rowValues, categorySlugColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) && string.IsNullOrWhiteSpace(categorySlug))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(categorySlug))
                {
                    throw new InvalidOperationException($"A(z) {rowNumber}. kategoriarow csak reszben van kitoltve.");
                }

                rows.Add(new CategoryImportRow(rowNumber, sku, categorySlug));
            }

            return rows;
        }

        private static List<ImageImportRow> ParseImageImportRows(WorksheetTable? imageTable)
        {
            if (imageTable is null)
            {
                return [];
            }

            int skuColumnIndex = GetRequiredColumnIndex(imageTable, "SKU");
            int imagePathColumnIndex = GetOptionalColumnIndex(imageTable, "KepUtvonal", "ImagePath", "ImageFolder");
            int imageNameColumnIndex = GetOptionalColumnIndex(imageTable, "KepNev", "ImageName", "FileName");

            List<ImageImportRow> rows = [];

            foreach ((int rowNumber, string[] rowValues) in imageTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string imagePath = GetCellValue(rowValues, imagePathColumnIndex);
                string imageName = GetCellValue(rowValues, imageNameColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) &&
                    string.IsNullOrWhiteSpace(imagePath) &&
                    string.IsNullOrWhiteSpace(imageName))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || (string.IsNullOrWhiteSpace(imagePath) && string.IsNullOrWhiteSpace(imageName)))
                {
                    throw new InvalidOperationException($"A(z) {rowNumber}. kepsor csak reszben van kitoltve.");
                }

                rows.Add(new ImageImportRow(rowNumber, sku, imagePath, imageName));
            }

            return rows;
        }

        private static List<PropertyImportRow> ParsePropertyImportRows(WorksheetTable? propertyTable)
        {
            if (propertyTable is null)
            {
                return [];
            }

            int skuColumnIndex = GetRequiredColumnIndex(propertyTable, "SKU");
            int propertyNameColumnIndex = GetRequiredColumnIndex(propertyTable, "TulajdonsagNev", "PropertyName", "Key");
            int propertyValueColumnIndex = GetOptionalColumnIndex(propertyTable, "TulajdonsagErtek", "PropertyValue", "Value");

            List<PropertyImportRow> rows = [];

            foreach ((int rowNumber, string[] rowValues) in propertyTable.Rows)
            {
                string sku = GetCellValue(rowValues, skuColumnIndex);
                string propertyName = GetCellValue(rowValues, propertyNameColumnIndex);
                string propertyValue = GetCellValue(rowValues, propertyValueColumnIndex);

                if (string.IsNullOrWhiteSpace(sku) &&
                    string.IsNullOrWhiteSpace(propertyName) &&
                    string.IsNullOrWhiteSpace(propertyValue))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new InvalidOperationException($"A(z) {rowNumber}. tulajdonsagsor csak reszben van kitoltve.");
                }

                rows.Add(new PropertyImportRow(rowNumber, sku, propertyName, propertyValue));
            }

            return rows;
        }

        private string GetCurrentWorkbookFilePath()
        {
            string workbookFilePath = filePathTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(workbookFilePath))
            {
                throw new InvalidOperationException("Nincs kivalasztott importfajl.");
            }

            return workbookFilePath;
        }

        private ExistingProductImportMode GetExistingProductImportMode()
        {
            string selectedMode = NormalizeToken(existingItemModeComboBox.SelectedItem?.ToString() ?? string.Empty);

            if (selectedMode.Contains("KIHAGYAS", StringComparison.Ordinal))
            {
                return ExistingProductImportMode.SkipExisting;
            }

            return ExistingProductImportMode.UpdateBySku;
        }

        private static bool TryParseImportDecimal(string rawValue, out decimal value)
        {
            if (decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            return decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
        }

        private static bool TryParseImportInt(string rawValue, out int value)
        {
            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
            {
                return true;
            }

            if (TryParseImportDecimal(rawValue, out decimal decimalValue) &&
                decimal.Truncate(decimalValue) == decimalValue &&
                decimalValue >= int.MinValue &&
                decimalValue <= int.MaxValue)
            {
                value = (int)decimalValue;
                return true;
            }

            value = 0;
            return false;
        }

        private WorksheetPreview GetProductWorksheetForValidation()
        {
            WorksheetPreview? namedWorksheet = FindWorksheetByName("Termekek");

            if (namedWorksheet is not null)
            {
                return namedWorksheet;
            }

            if (sheetComboBox.SelectedIndex >= 0 && sheetComboBox.SelectedIndex < loadedWorkbookSheets.Count)
            {
                return loadedWorkbookSheets[sheetComboBox.SelectedIndex];
            }

            throw new InvalidOperationException("Nincs kijelolt vagy felismerheto termek munkalap.");
        }

        private WorksheetTable? TryBuildWorksheetTable(string sheetName)
        {
            WorksheetPreview? worksheet = FindWorksheetByName(sheetName);
            return worksheet is null ? null : BuildWorksheetTable(worksheet);
        }

        private WorksheetPreview? FindWorksheetByName(string sheetName)
        {
            string normalizedSheetName = NormalizeToken(sheetName);

            return loadedWorkbookSheets.FirstOrDefault(
                sheet => string.Equals(NormalizeToken(sheet.Name), normalizedSheetName, StringComparison.Ordinal));
        }

        private static WorksheetTable BuildWorksheetTable(WorksheetPreview worksheet)
        {
            if (worksheet.Rows.Count == 0)
            {
                throw new InvalidOperationException($"A '{worksheet.Name}' munkalap ures.");
            }

            string[] headers = NormalizeRowLength(worksheet.Rows[0], worksheet.Rows[0].Length);

            if (!headers.Any(header => !string.IsNullOrWhiteSpace(header)))
            {
                throw new InvalidOperationException($"A '{worksheet.Name}' munkalapon nem talalhato fejlec sor.");
            }

            Dictionary<string, int> headerIndexes = new(StringComparer.Ordinal);

            for (int i = 0; i < headers.Length; i++)
            {
                string normalizedHeader = NormalizeToken(headers[i]);

                if (!string.IsNullOrWhiteSpace(normalizedHeader) && !headerIndexes.ContainsKey(normalizedHeader))
                {
                    headerIndexes[normalizedHeader] = i;
                }
            }

            List<(int RowNumber, string[] Values)> rows = [];

            for (int rowIndex = 1; rowIndex < worksheet.Rows.Count; rowIndex++)
            {
                string[] rowValues = NormalizeRowLength(worksheet.Rows[rowIndex], headers.Length);

                if (rowValues.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                rows.Add((rowIndex + 1, rowValues));
            }

            return new WorksheetTable(worksheet.Name, headerIndexes, rows);
        }

        private static int GetRequiredColumnIndex(WorksheetTable worksheet, params string[] aliases)
        {
            int index = GetOptionalColumnIndex(worksheet, aliases);

            if (index >= 0)
            {
                return index;
            }

            throw new InvalidOperationException(
                $"A '{worksheet.Name}' munkalaprol hianyzik a(z) {string.Join(" / ", aliases)} oszlop.");
        }

        private static int GetOptionalColumnIndex(WorksheetTable worksheet, params string[] aliases)
        {
            foreach (string alias in aliases)
            {
                if (worksheet.HeaderIndexes.TryGetValue(NormalizeToken(alias), out int index))
                {
                    return index;
                }
            }

            return -1;
        }

        private static string GetCellValue(string[] rowValues, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= rowValues.Length)
            {
                return string.Empty;
            }

            return rowValues[columnIndex].Trim();
        }

        private static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string decomposed = value.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new();

            foreach (char character in decomposed)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            return builder.ToString();
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
        private sealed record CategoryComboItem(string DisplayText, string? Bvin, string? Slug)
        {
            public override string ToString()
            {
                return DisplayText;
            }
        }

        private sealed record WorksheetTable(
            string Name,
            Dictionary<string, int> HeaderIndexes,
            List<(int RowNumber, string[] Values)> Rows);

        private sealed record ImportValidationResult(
            bool CanProceed,
            int ProductRowCount,
            int ExistingProductCount,
            int NewProductCount,
            int CategoryAssignmentCount,
            int ImageRowCount,
            int PropertyRowCount,
            int ErrorCount,
            string StatusMessage,
            string DetailsMessage,
            string ConfirmationMessage);

        private sealed record ProductImportRow(
            int RowNumber,
            string Sku,
            string Name,
            decimal? Price,
            int? Stock,
            string ProductTypeName,
            string Description);

        private sealed record CategoryImportRow(
            int RowNumber,
            string Sku,
            string CategorySlug);

        private sealed record ImageImportRow(
            int RowNumber,
            string Sku,
            string ImagePath,
            string ImageName);

        private sealed record PropertyImportRow(
            int RowNumber,
            string Sku,
            string PropertyName,
            string PropertyValue);

        private sealed record ImportExecutionResult(
            int CreatedCount,
            int UpdatedCount,
            int SkippedExistingCount,
            int ProductTypeAppliedCount,
            int CategoryLinkedCount,
            int CategoryAlreadyLinkedCount,
            int ImageUploadedCount,
            int MainImageUploadedCount,
            int AdditionalImageUploadedCount,
            int PropertyAppliedCount,
            int ErrorCount,
            string StatusMessage,
            string DetailsMessage);

        private sealed record CategorySheetValidationResult(
            int RowCount,
            int UnknownCategorySlugCount,
            int UnknownCategorySkuCount,
            int IncompleteRowCount,
            bool CanProceed,
            string DetailsMessage);

        private sealed record ImageSheetValidationResult(
            int RowCount,
            int MissingFileCount,
            int UnknownSkuCount,
            int IncompleteRowCount,
            bool CanProceed,
            string DetailsMessage);

        private sealed record PropertySheetValidationResult(
            int RowCount,
            int UnknownSkuCount,
            int IncompleteRowCount,
            int InvalidPropertyCount,
            bool CanProceed,
            string DetailsMessage);

        private enum ExistingProductImportMode
        {
            UpdateBySku,
            SkipExisting
        }

        private enum ImportScope
        {
            Products,
            Images,
            Categories,
            Properties,
            All
        }

        private enum FormPage
        {
            Import,
            BulkOperations
        }

        private sealed record WorksheetPreview(string Name, List<string[]> Rows);
    }
}
