using System.ComponentModel;
using System.Text;

namespace WinFormsApp1
{
    public partial class Form1
    {
        private readonly Button categoryManagerPageButton = new();
        private readonly Panel categoryManagerPanel = new();
        private readonly GroupBox categoryFilterGroupBox = new();
        private readonly Label categoryFilterDescriptionLabel = new();
        private readonly Label categorySkuFilterLabel = new();
        private readonly TextBox categorySkuFilterTextBox = new();
        private readonly Label categoryProductTypeFilterLabel = new();
        private readonly ComboBox categoryProductTypeFilterComboBox = new();
        private readonly Button categorySearchButton = new();
        private readonly Button categoryClearFiltersButton = new();
        private readonly GroupBox categoryResultsGroupBox = new();
        private readonly Label categoryResultsSummaryLabel = new();
        private readonly Button categorySelectAllButton = new();
        private readonly Button categoryClearSelectionButton = new();
        private readonly DataGridView categoryManagerGrid = new();
        private readonly BindingSource categoryManagerBindingSource = new();
        private readonly BindingList<CategoryManagerRow> categoryManagerRows = [];
        private readonly GroupBox categoryActionsGroupBox = new();
        private readonly Label categoryBulkCategoryLabel = new();
        private readonly ComboBox categoryBulkCategoryComboBox = new();
        private readonly Button categoryAddSelectedButton = new();
        private readonly Button categoryRemoveSelectedButton = new();
        private readonly Label categoryDetailsLabel = new();
        private readonly TextBox categoryDetailsTextBox = new();
        private readonly Label categoryActionStatusLabel = new();

        private void ConfigureCategoryManagerPage()
        {
            categoryManagerPanel.Visible = false;
            categoryManagerPanel.BackColor = Color.Transparent;

            categoryFilterGroupBox.Text = "Szűrés és keresés";
            categoryResultsGroupBox.Text = "Találatok";
            categoryActionsGroupBox.Text = "Kategória műveletek";

            categoryFilterDescriptionLabel.AutoSize = true;
            categoryFilterDescriptionLabel.Text = "Termékek szűrése SKU vagy Product type alapján, kategóriák áttekintésével.";

            categorySkuFilterLabel.AutoSize = true;
            categorySkuFilterLabel.Text = "SKU szűrő";

            categorySkuFilterTextBox.PlaceholderText = "Példa: ABC-001, ABC-002 vagy részlet";

            categoryProductTypeFilterLabel.AutoSize = true;
            categoryProductTypeFilterLabel.Text = "Product type";

            categoryProductTypeFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            categorySearchButton.Text = "Keresés";
            categorySearchButton.Click += async (_, _) => await SearchCategoryManagerProductsAsync();

            categoryClearFiltersButton.Text = "Szűrők törlése";
            categoryClearFiltersButton.Click += CategoryClearFiltersButton_Click;

            categoryResultsSummaryLabel.AutoSize = true;
            categoryResultsSummaryLabel.Text = "Nincs betöltött találat.";

            categorySelectAllButton.Text = "Összes kijelölése";
            categorySelectAllButton.Click += CategorySelectAllButton_Click;

            categoryClearSelectionButton.Text = "Kijelölés törlése";
            categoryClearSelectionButton.Click += CategoryClearSelectionButton_Click;

            categoryManagerGrid.AllowUserToAddRows = false;
            categoryManagerGrid.AllowUserToDeleteRows = false;
            categoryManagerGrid.AllowUserToResizeRows = false;
            categoryManagerGrid.AutoGenerateColumns = false;
            categoryManagerGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            categoryManagerGrid.MultiSelect = false;
            categoryManagerGrid.RowHeadersVisible = false;
            categoryManagerGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            categoryManagerGrid.DataSource = categoryManagerBindingSource;
            categoryManagerGrid.CurrentCellDirtyStateChanged += CategoryManagerGrid_CurrentCellDirtyStateChanged;
            categoryManagerGrid.CellValueChanged += CategoryManagerGrid_CellValueChanged;
            categoryManagerGrid.SelectionChanged += CategoryManagerGrid_SelectionChanged;
            categoryManagerGrid.Columns.AddRange(
            [
                new DataGridViewCheckBoxColumn
                {
                    DataPropertyName = nameof(CategoryManagerRow.IsSelected),
                    HeaderText = "",
                    Width = 42,
                    MinimumWidth = 42,
                    FillWeight = 10,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(CategoryManagerRow.Sku),
                    HeaderText = "SKU",
                    MinimumWidth = 110,
                    FillWeight = 18,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(CategoryManagerRow.ProductName),
                    HeaderText = "Termék neve",
                    MinimumWidth = 180,
                    FillWeight = 34,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(CategoryManagerRow.ProductTypeName),
                    HeaderText = "Product type",
                    MinimumWidth = 140,
                    FillWeight = 22,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(CategoryManagerRow.CategoriesDisplay),
                    HeaderText = "Kategóriák",
                    MinimumWidth = 220,
                    FillWeight = 40,
                    ReadOnly = true
                }
            ]);

            categoryBulkCategoryLabel.AutoSize = true;
            categoryBulkCategoryLabel.Text = "Szerkesztendő kategória";

            categoryBulkCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            categoryAddSelectedButton.Text = "Hozzáadás a kijelöltekhez";
            categoryAddSelectedButton.Click += async (_, _) => await ApplyCategoryToSelectedProductsAsync(addCategory: true);

            categoryRemoveSelectedButton.Text = "Eltávolítás a kijelöltektől";
            categoryRemoveSelectedButton.Click += async (_, _) => await ApplyCategoryToSelectedProductsAsync(addCategory: false);

            categoryDetailsLabel.AutoSize = true;
            categoryDetailsLabel.Text = "Aktuális termék kategóriái";

            categoryDetailsTextBox.Multiline = true;
            categoryDetailsTextBox.ReadOnly = true;
            categoryDetailsTextBox.ScrollBars = ScrollBars.Vertical;

            categoryActionStatusLabel.AutoSize = true;
            categoryActionStatusLabel.Text = "A műveletek a kijelölt sorokra futnak le.";

            categoryFilterGroupBox.Controls.Add(categoryFilterDescriptionLabel);
            categoryFilterGroupBox.Controls.Add(categorySkuFilterLabel);
            categoryFilterGroupBox.Controls.Add(categorySkuFilterTextBox);
            categoryFilterGroupBox.Controls.Add(categoryProductTypeFilterLabel);
            categoryFilterGroupBox.Controls.Add(categoryProductTypeFilterComboBox);
            categoryFilterGroupBox.Controls.Add(categorySearchButton);
            categoryFilterGroupBox.Controls.Add(categoryClearFiltersButton);

            categoryResultsGroupBox.Controls.Add(categoryResultsSummaryLabel);
            categoryResultsGroupBox.Controls.Add(categorySelectAllButton);
            categoryResultsGroupBox.Controls.Add(categoryClearSelectionButton);
            categoryResultsGroupBox.Controls.Add(categoryManagerGrid);

            categoryActionsGroupBox.Controls.Add(categoryBulkCategoryLabel);
            categoryActionsGroupBox.Controls.Add(categoryBulkCategoryComboBox);
            categoryActionsGroupBox.Controls.Add(categoryAddSelectedButton);
            categoryActionsGroupBox.Controls.Add(categoryRemoveSelectedButton);
            categoryActionsGroupBox.Controls.Add(categoryDetailsLabel);
            categoryActionsGroupBox.Controls.Add(categoryDetailsTextBox);
            categoryActionsGroupBox.Controls.Add(categoryActionStatusLabel);

            categoryManagerPanel.Controls.Add(categoryFilterGroupBox);
            categoryManagerPanel.Controls.Add(categoryResultsGroupBox);
            categoryManagerPanel.Controls.Add(categoryActionsGroupBox);
            Controls.Add(categoryManagerPanel);
            categoryManagerPanel.SendToBack();

            categoryManagerBindingSource.DataSource = categoryManagerRows;
            UpdateCategoryManagerHeaderState();
            UpdateCategoryManagerSummary();
        }

        private void ApplyCategoryManagerTheme()
        {
            ConfigureSurfaceGroupBox(categoryFilterGroupBox);
            ConfigureSurfaceGroupBox(categoryResultsGroupBox);
            ConfigureSurfaceGroupBox(categoryActionsGroupBox);

            StyleSecondaryButton(categorySearchButton);
            StyleSecondaryButton(categoryClearFiltersButton);
            StyleSecondaryButton(categorySelectAllButton);
            StyleSecondaryButton(categoryClearSelectionButton);
            StylePrimaryButton(categoryAddSelectedButton);
            StyleSecondaryButton(categoryRemoveSelectedButton);

            StyleTextBox(categorySkuFilterTextBox);
            StyleTextBox(categoryDetailsTextBox);
            StyleComboBox(categoryProductTypeFilterComboBox);
            StyleComboBox(categoryBulkCategoryComboBox);

            foreach (Label label in new[]
            {
                categoryFilterDescriptionLabel,
                categorySkuFilterLabel,
                categoryProductTypeFilterLabel,
                categoryResultsSummaryLabel,
                categoryBulkCategoryLabel,
                categoryDetailsLabel,
                categoryActionStatusLabel
            })
            {
                label.ForeColor = InkColor;
            }

            categoryActionStatusLabel.ForeColor = MutedInkColor;
            categoryDetailsTextBox.BackColor = SurfaceStrongColor;

            categoryManagerGrid.BackgroundColor = SurfaceStrongColor;
            categoryManagerGrid.BorderStyle = BorderStyle.None;
            categoryManagerGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            categoryManagerGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            categoryManagerGrid.EnableHeadersVisualStyles = false;
            categoryManagerGrid.GridColor = BorderColor;
            categoryManagerGrid.DefaultCellStyle.BackColor = SurfaceColor;
            categoryManagerGrid.DefaultCellStyle.ForeColor = InkColor;
            categoryManagerGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(227, 238, 243);
            categoryManagerGrid.DefaultCellStyle.SelectionForeColor = InkColor;
            categoryManagerGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 246, 231);
            categoryManagerGrid.ColumnHeadersDefaultCellStyle.BackColor = AccentBlueColor;
            categoryManagerGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            categoryManagerGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            categoryManagerGrid.ColumnHeadersHeight = 38;
            categoryManagerGrid.RowTemplate.Height = 34;
        }

        private int LayoutCategoryManagerSection(int contentWidth, int y)
        {
            UpdateCategoryManagerHeaderState();

            categoryManagerPanel.SetBounds(PageMargin, y, contentWidth, 760);

            int filterHeight = 220;
            categoryFilterGroupBox.SetBounds(0, 0, contentWidth, filterHeight);
            LayoutCategoryManagerFilterGroup();

            bool twoColumns = contentWidth >= 1180;
            int contentTop = categoryFilterGroupBox.Bottom + SectionSpacing;

            if (twoColumns)
            {
                int resultsWidth = (int)Math.Round(contentWidth * 0.62D);
                int actionsWidth = contentWidth - resultsWidth - CardSpacing;
                categoryResultsGroupBox.SetBounds(0, contentTop, resultsWidth, 520);
                categoryActionsGroupBox.SetBounds(resultsWidth + CardSpacing, contentTop, actionsWidth, 520);
            }
            else
            {
                categoryResultsGroupBox.SetBounds(0, contentTop, contentWidth, 430);
                categoryActionsGroupBox.SetBounds(0, categoryResultsGroupBox.Bottom + CardSpacing, contentWidth, 320);
            }

            LayoutCategoryManagerResultsGroup();
            LayoutCategoryManagerActionsGroup();

            categoryManagerPanel.Height = categoryActionsGroupBox.Bottom;
            return categoryManagerPanel.Bottom + SectionSpacing;
        }

        private void LayoutCategoryManagerFilterGroup()
        {
            const int left = 24;
            const int top = 36;
            const int gap = 16;
            const int rowGap = 8;
            const int buttonHeight = 40;
            const int buttonWidth = 170;

            int contentWidth = Math.Max(220, categoryFilterGroupBox.ClientSize.Width - (left * 2));
            int currentY = top;

            categoryFilterDescriptionLabel.MaximumSize = new Size(contentWidth, 0);
            categoryFilterDescriptionLabel.Location = new Point(left, currentY);
            currentY = categoryFilterDescriptionLabel.Bottom + gap;

            bool twoColumns = contentWidth >= 760;

            if (twoColumns)
            {
                int fieldWidth = (contentWidth - gap) / 2;

                categorySkuFilterLabel.Location = new Point(left, currentY);
                categoryProductTypeFilterLabel.Location = new Point(left + fieldWidth + gap, currentY);

                int controlTop = categorySkuFilterLabel.Bottom + rowGap;
                categorySkuFilterTextBox.SetBounds(left, controlTop, fieldWidth, categorySkuFilterTextBox.Height);
                categoryProductTypeFilterComboBox.SetBounds(left + fieldWidth + gap, controlTop, fieldWidth, categoryProductTypeFilterComboBox.Height);

                int buttonTop = Math.Max(categorySkuFilterTextBox.Bottom, categoryProductTypeFilterComboBox.Bottom) + 14;
                categoryClearFiltersButton.SetBounds(categoryFilterGroupBox.ClientSize.Width - left - buttonWidth, buttonTop, buttonWidth, buttonHeight);
                categorySearchButton.SetBounds(categoryClearFiltersButton.Left - 12 - buttonWidth, buttonTop, buttonWidth, buttonHeight);
                return;
            }

            categorySkuFilterLabel.Location = new Point(left, currentY);
            currentY = categorySkuFilterLabel.Bottom + rowGap;
            categorySkuFilterTextBox.SetBounds(left, currentY, contentWidth, categorySkuFilterTextBox.Height);

            currentY = categorySkuFilterTextBox.Bottom + gap;
            categoryProductTypeFilterLabel.Location = new Point(left, currentY);
            currentY = categoryProductTypeFilterLabel.Bottom + rowGap;
            categoryProductTypeFilterComboBox.SetBounds(left, currentY, contentWidth, categoryProductTypeFilterComboBox.Height);

            currentY = categoryProductTypeFilterComboBox.Bottom + 14;
            int splitWidth = (contentWidth - 12) / 2;
            categorySearchButton.SetBounds(left, currentY, splitWidth, buttonHeight);
            categoryClearFiltersButton.SetBounds(categorySearchButton.Right + 12, currentY, contentWidth - splitWidth - 12, buttonHeight);
        }

        private void LayoutCategoryManagerResultsGroup()
        {
            const int left = 24;
            const int top = 36;
            const int right = 24;
            const int buttonHeight = 38;
            const int buttonWidth = 150;

            int contentWidth = Math.Max(220, categoryResultsGroupBox.ClientSize.Width - left - right);

            categoryResultsSummaryLabel.Location = new Point(left, top + 6);
            categoryClearSelectionButton.SetBounds(categoryResultsGroupBox.ClientSize.Width - right - buttonWidth, top, buttonWidth, buttonHeight);
            categorySelectAllButton.SetBounds(categoryClearSelectionButton.Left - 10 - buttonWidth, top, buttonWidth, buttonHeight);

            int gridTop = categorySelectAllButton.Bottom + 14;
            categoryManagerGrid.SetBounds(left, gridTop, contentWidth, categoryResultsGroupBox.ClientSize.Height - gridTop - 18);
        }

        private void LayoutCategoryManagerActionsGroup()
        {
            const int left = 24;
            const int top = 36;
            const int right = 24;
            const int sectionGap = 14;
            const int rowGap = 8;
            const int buttonHeight = 40;

            int contentWidth = Math.Max(220, categoryActionsGroupBox.ClientSize.Width - left - right);
            int currentY = top;

            categoryBulkCategoryLabel.Location = new Point(left, currentY);
            currentY = categoryBulkCategoryLabel.Bottom + rowGap;
            categoryBulkCategoryComboBox.SetBounds(left, currentY, contentWidth, categoryBulkCategoryComboBox.Height);

            currentY = categoryBulkCategoryComboBox.Bottom + sectionGap;
            categoryAddSelectedButton.SetBounds(left, currentY, contentWidth, buttonHeight);

            currentY = categoryAddSelectedButton.Bottom + 10;
            categoryRemoveSelectedButton.SetBounds(left, currentY, contentWidth, buttonHeight);

            currentY = categoryRemoveSelectedButton.Bottom + sectionGap;
            categoryDetailsLabel.Location = new Point(left, currentY);

            currentY = categoryDetailsLabel.Bottom + rowGap;
            int statusHeight = categoryActionStatusLabel.PreferredHeight;
            int detailsHeight = Math.Max(120, categoryActionsGroupBox.ClientSize.Height - currentY - statusHeight - 20);
            categoryDetailsTextBox.SetBounds(left, currentY, contentWidth, detailsHeight);

            categoryActionStatusLabel.MaximumSize = new Size(contentWidth, 0);
            categoryActionStatusLabel.Location = new Point(left, categoryDetailsTextBox.Bottom + 8);
        }

        private void PopulateCategoryManagerSelectors(IReadOnlyList<CategoryComboItem> categoryItems)
        {
            List<object> bulkCategoryItems = [new CategoryComboItem("Válasszon kategóriát...", null, null)];
            bulkCategoryItems.AddRange(categoryItems.Cast<object>());
            ReplaceComboBoxItems(categoryBulkCategoryComboBox, bulkCategoryItems, 0);

            List<object> productTypeItems = [new CategoryManagerProductTypeItem("Összes product type", null)];
            productTypeItems.AddRange(loadedProductTypes
                .OrderBy(productType => productType.ProductTypeName, StringComparer.CurrentCultureIgnoreCase)
                .Select(productType => (object)new CategoryManagerProductTypeItem(productType.ProductTypeName, productType.Bvin)));
            ReplaceComboBoxItems(categoryProductTypeFilterComboBox, productTypeItems, 0);
        }

        private void UpdateCategoryManagerActionStates(bool isBusy)
        {
            if (categorySearchButton is null)
            {
                return;
            }

            bool hasSelection = categoryManagerRows.Any(static row => row.IsSelected);
            bool hasCategorySelection = categoryBulkCategoryComboBox.SelectedItem is CategoryComboItem selectedCategory &&
                                        !string.IsNullOrWhiteSpace(selectedCategory.Bvin);

            categorySearchButton.Enabled = hotcakesReady && !isBusy;
            categoryClearFiltersButton.Enabled = !isBusy;
            categorySelectAllButton.Enabled = !isBusy && categoryManagerRows.Count > 0;
            categoryClearSelectionButton.Enabled = !isBusy && hasSelection;
            categoryAddSelectedButton.Enabled = hotcakesReady && !isBusy && hasSelection && hasCategorySelection;
            categoryRemoveSelectedButton.Enabled = hotcakesReady && !isBusy && hasSelection && hasCategorySelection;
        }

        private void UpdateCategoryManagerHeaderState()
        {
            if (currentPage != FormPage.CategoryManager)
            {
                return;
            }

            titleLabel.Text = "Kategória kezelő";
            subtitleLabel.Text = "Szűrhető terméklista SKU és Product type alapján, kategóriák áttekintésével és tömeges szerkesztéssel.";
            accentBadgeLabel.Text = "Kategória mód";
        }

        private async Task SearchCategoryManagerProductsAsync()
        {
            if (!hotcakesReady)
            {
                MessageBox.Show(
                    this,
                    "A Hotcakes kapcsolat még nem áll készen a kategória kezelő használatához.",
                    "Kategória kezelő",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                UseWaitCursor = true;
                isImporting = true;
                UpdateActionStates();
                SetStatusMessage("Kategória kezelő: termékek és kategóriák betöltése...");

                await EnsureProductsLoadedAsync();

                List<HotcakesProduct> filteredProducts = GetCategoryManagerFilteredProducts();
                categoryManagerRows.Clear();

                for (int index = 0; index < filteredProducts.Count; index++)
                {
                    HotcakesProduct product = filteredProducts[index];
                    SetStatusMessage($"Kategória kezelő: kategóriák betöltése... ({index + 1}/{filteredProducts.Count})");

                    IReadOnlySet<string> categoryIds = string.IsNullOrWhiteSpace(product.Bvin)
                        ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        : await GetAssignedCategoryIdsForProductAsync(product.Bvin);

                    categoryManagerRows.Add(new CategoryManagerRow
                    {
                        IsSelected = false,
                        ProductBvin = product.Bvin,
                        Sku = product.Sku,
                        ProductName = product.ProductName,
                        ProductTypeId = product.ProductTypeId,
                        ProductTypeName = ResolveProductTypeDisplayName(product.ProductTypeId),
                        CategoryIds = new HashSet<string>(categoryIds, StringComparer.OrdinalIgnoreCase),
                        CategoriesDisplay = FormatCategoryDisplay(categoryIds)
                    });
                }

                categoryManagerBindingSource.ResetBindings(false);
                UpdateCategoryManagerSummary();
                UpdateCategoryManagerDetails();
                UpdateActionStates();
                SetStatusMessage($"Kategória kezelő: {categoryManagerRows.Count} termék betöltve.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"A kategória kezelő keresése nem sikerült.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Kategória kezelő",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                SetStatusMessage("A kategória kezelő keresése nem sikerült.", true);
            }
            finally
            {
                isImporting = false;
                UpdateActionStates();
                UseWaitCursor = false;
            }
        }

        private List<HotcakesProduct> GetCategoryManagerFilteredProducts()
        {
            List<HotcakesProduct> products = loadedProductsBySku.Values
                .Where(product => !string.IsNullOrWhiteSpace(product.Sku))
                .OrderBy(product => product.Sku, StringComparer.OrdinalIgnoreCase)
                .ToList();

            string[] skuTokens = categorySkuFilterTextBox.Text
                .Split([',', ';', '\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (skuTokens.Length == 1)
            {
                string skuToken = skuTokens[0];
                products = products
                    .Where(product => product.Sku.Contains(skuToken, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else if (skuTokens.Length > 1)
            {
                HashSet<string> skuSet = skuTokens.ToHashSet(StringComparer.OrdinalIgnoreCase);
                products = products
                    .Where(product => skuSet.Contains(product.Sku))
                    .ToList();
            }

            if (categoryProductTypeFilterComboBox.SelectedItem is CategoryManagerProductTypeItem selectedProductType &&
                !string.IsNullOrWhiteSpace(selectedProductType.Bvin))
            {
                products = products
                    .Where(product => string.Equals(product.ProductTypeId, selectedProductType.Bvin, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return products;
        }

        private async Task ApplyCategoryToSelectedProductsAsync(bool addCategory)
        {
            if (categoryBulkCategoryComboBox.SelectedItem is not CategoryComboItem selectedCategory ||
                string.IsNullOrWhiteSpace(selectedCategory.Bvin))
            {
                MessageBox.Show(
                    this,
                    "Válassz ki egy kategóriát a művelethez.",
                    "Kategória kezelő",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            List<CategoryManagerRow> selectedRows = categoryManagerRows
                .Where(static row => row.IsSelected)
                .ToList();

            if (selectedRows.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "Jelölj ki legalább egy terméket a táblázatban.",
                    "Kategória kezelő",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string actionLabel = addCategory ? "hozzáadás" : "eltávolítás";
            DialogResult confirmation = MessageBox.Show(
                this,
                $"Valóban lefuttatod a kategória {actionLabel} műveletet {selectedRows.Count} kijelölt terméken?{Environment.NewLine}{Environment.NewLine}Kategória: {selectedCategory.DisplayText}",
                "Kategória kezelő",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                UseWaitCursor = true;
                isImporting = true;
                UpdateActionStates();

                int changedCount = 0;
                int skippedCount = 0;
                List<string> errors = [];

                for (int index = 0; index < selectedRows.Count; index++)
                {
                    CategoryManagerRow row = selectedRows[index];
                    SetStatusMessage($"Kategória kezelő: {actionLabel} folyamatban... ({index + 1}/{selectedRows.Count})");

                    try
                    {
                        if (string.IsNullOrWhiteSpace(row.ProductBvin))
                        {
                            skippedCount++;
                            continue;
                        }

                        if (addCategory)
                        {
                            if (row.CategoryIds.Contains(selectedCategory.Bvin))
                            {
                                skippedCount++;
                                continue;
                            }

                            await hotcakesClient.CreateCategoryProductAssociationAsync(new HotcakesCategoryProductAssociation
                            {
                                CategoryId = selectedCategory.Bvin,
                                ProductId = row.ProductBvin
                            });

                            row.CategoryIds.Add(selectedCategory.Bvin);
                        }
                        else
                        {
                            if (!row.CategoryIds.Contains(selectedCategory.Bvin))
                            {
                                skippedCount++;
                                continue;
                            }

                            bool removed = await hotcakesClient.RemoveCategoryProductAssociationAsync(row.ProductBvin, selectedCategory.Bvin);

                            if (!removed)
                            {
                                throw new InvalidOperationException("A kategória kapcsolat eltávolítása nem járt sikerrel.");
                            }

                            row.CategoryIds.Remove(selectedCategory.Bvin);
                        }

                        loadedCategoryIdsByProductBvin[row.ProductBvin] = new HashSet<string>(row.CategoryIds, StringComparer.OrdinalIgnoreCase);
                        row.CategoriesDisplay = FormatCategoryDisplay(row.CategoryIds);
                        changedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{row.Sku}: {ex.Message}");
                    }
                }

                categoryManagerBindingSource.ResetBindings(false);
                UpdateCategoryManagerSummary();
                UpdateCategoryManagerDetails();
                UpdateActionStates();

                StringBuilder resultBuilder = new();
                resultBuilder.AppendLine($"Kategória {actionLabel} eredménye");
                resultBuilder.AppendLine($"Kijelölt termékek: {selectedRows.Count}");
                resultBuilder.AppendLine($"Módosított termékek: {changedCount}");
                resultBuilder.AppendLine($"Kihagyott termékek: {skippedCount}");
                resultBuilder.AppendLine($"Hibás termékek: {errors.Count}");

                if (errors.Count > 0)
                {
                    resultBuilder.AppendLine();
                    resultBuilder.AppendLine("Első hibák:");

                    foreach (string error in errors.Take(8))
                    {
                        resultBuilder.AppendLine($"- {error}");
                    }
                }

                categoryActionStatusLabel.Text = $"Legutóbbi művelet: {changedCount} módosítva, {skippedCount} kihagyva.";
                SetStatusMessage($"Kategória kezelő: {changedCount} termék frissítve.");

                MessageBox.Show(
                    this,
                    resultBuilder.ToString(),
                    "Kategória kezelő",
                    MessageBoxButtons.OK,
                    errors.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"A kategória művelet nem sikerült.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Kategória kezelő",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                SetStatusMessage("A kategória művelet nem sikerült.", true);
            }
            finally
            {
                isImporting = false;
                UpdateActionStates();
                UseWaitCursor = false;
            }
        }

        private void CategoryClearFiltersButton_Click(object? sender, EventArgs e)
        {
            categorySkuFilterTextBox.Clear();

            if (categoryProductTypeFilterComboBox.Items.Count > 0)
            {
                categoryProductTypeFilterComboBox.SelectedIndex = 0;
            }

            categoryActionStatusLabel.Text = "A műveletek a kijelölt sorokra futnak le.";
        }

        private void CategorySelectAllButton_Click(object? sender, EventArgs e)
        {
            foreach (CategoryManagerRow row in categoryManagerRows)
            {
                row.IsSelected = true;
            }

            categoryManagerBindingSource.ResetBindings(false);
            UpdateCategoryManagerSummary();
            UpdateActionStates();
        }

        private void CategoryClearSelectionButton_Click(object? sender, EventArgs e)
        {
            foreach (CategoryManagerRow row in categoryManagerRows)
            {
                row.IsSelected = false;
            }

            categoryManagerBindingSource.ResetBindings(false);
            UpdateCategoryManagerSummary();
            UpdateActionStates();
        }

        private void CategoryManagerGrid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (categoryManagerGrid.IsCurrentCellDirty)
            {
                categoryManagerGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void CategoryManagerGrid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            UpdateCategoryManagerSummary();
            UpdateActionStates();
        }

        private void CategoryManagerGrid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateCategoryManagerDetails();
        }

        private void UpdateCategoryManagerSummary()
        {
            int selectedCount = categoryManagerRows.Count(static row => row.IsSelected);
            categoryResultsSummaryLabel.Text = categoryManagerRows.Count == 0
                ? "Nincs betöltött találat."
                : $"Találatok: {categoryManagerRows.Count} termék, kijelölve: {selectedCount}.";
        }

        private void UpdateCategoryManagerDetails()
        {
            if (categoryManagerGrid.CurrentRow?.DataBoundItem is not CategoryManagerRow row)
            {
                categoryDetailsTextBox.Text = "Nincs kiválasztott termék.";
                return;
            }

            StringBuilder builder = new();
            builder.AppendLine($"SKU: {row.Sku}");
            builder.AppendLine($"Termék: {row.ProductName}");
            builder.AppendLine($"Product type: {row.ProductTypeName}");
            builder.AppendLine();
            builder.AppendLine("Kategóriák:");
            builder.AppendLine(string.IsNullOrWhiteSpace(row.CategoriesDisplay) ? "Nincs hozzárendelt kategória." : row.CategoriesDisplay);
            categoryDetailsTextBox.Text = builder.ToString().TrimEnd();
        }

        private string ResolveProductTypeDisplayName(string productTypeId)
        {
            if (string.IsNullOrWhiteSpace(productTypeId))
            {
                return "Nincs product type";
            }

            HotcakesProductTypeSnapshot? productType = loadedProductTypes
                .FirstOrDefault(candidate => string.Equals(candidate.Bvin, productTypeId, StringComparison.OrdinalIgnoreCase));

            return productType?.ProductTypeName ?? productTypeId;
        }

        private string FormatCategoryDisplay(IEnumerable<string> categoryIds)
        {
            HashSet<string> categoryIdSet = categoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            List<string> categoryNames = loadedCategories
                .Where(category => categoryIdSet.Contains(category.Bvin))
                .Select(category => category.Name)
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            return categoryNames.Count == 0
                ? "Nincs kategória"
                : string.Join(", ", categoryNames);
        }

        private sealed class CategoryManagerRow
        {
            public bool IsSelected { get; set; }

            public string ProductBvin { get; set; } = string.Empty;

            public string Sku { get; set; } = string.Empty;

            public string ProductName { get; set; } = string.Empty;

            public string ProductTypeId { get; set; } = string.Empty;

            public string ProductTypeName { get; set; } = string.Empty;

            public HashSet<string> CategoryIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

            public string CategoriesDisplay { get; set; } = string.Empty;
        }

        private sealed record CategoryManagerProductTypeItem(string DisplayText, string? Bvin)
        {
            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}
