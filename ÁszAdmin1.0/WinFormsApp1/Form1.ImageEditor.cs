using System.ComponentModel;
using System.Text;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace WinFormsApp1
{
    public partial class Form1
    {
        private static readonly HttpClient ImageDownloadHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(45)
        };

        private static readonly string[] MainImageSizeFolders = ["medium", "small", string.Empty];
        private static readonly string[] AdditionalImageSizeFolders = ["medium", "small", string.Empty, "tiny"];
        private readonly Button imageEditorPageButton = new();
        private readonly Panel imageEditorPanel = new();
        private readonly GroupBox imageSearchGroupBox = new();
        private readonly Label imageSearchDescriptionLabel = new();
        private readonly Label imageSkuLabel = new();
        private readonly TextBox imageSkuTextBox = new();
        private readonly Button imageSearchButton = new();
        private readonly Button imageSearchClearButton = new();
        private readonly Label imageSearchResultsLabel = new();
        private readonly Button imageLoadSelectedProductButton = new();
        private readonly DataGridView imageSearchResultsGrid = new();
        private readonly BindingSource imageSearchResultsBindingSource = new();
        private readonly BindingList<ImageEditorSearchRow> imageSearchResultRows = [];
        private readonly GroupBox imageDetailsGroupBox = new();
        private readonly Label imageProductSummaryLabel = new();
        private readonly PictureBox imagePreviewBox = new();
        private readonly Label imagePreviewCaptionLabel = new();
        private readonly GroupBox imageActionsGroupBox = new();
        private readonly Label imageActionsSummaryLabel = new();
        private readonly Label imageSelectedImageLabel = new();
        private readonly Button imageSetMainButton = new();
        private readonly Button imageDeleteButton = new();
        private readonly Button imageUploadButton = new();
        private readonly Button imageRefreshButton = new();
        private readonly Label imageActionStatusLabel = new();
        private readonly GroupBox imageGalleryGroupBox = new();
        private readonly FlowLayoutPanel imageGalleryFlowPanel = new();
        private readonly Label imageGalleryEmptyLabel = new();
        private HotcakesProduct? imageEditorCurrentProduct;
        private readonly List<ImageEditorImageItem> imageEditorImages = [];
        private ImageEditorImageItem? imageEditorSelectedImage;

        private void ConfigureImageEditorPage()
        {
            imageEditorPanel.Visible = false;
            imageEditorPanel.BackColor = Color.Transparent;

            imageSearchGroupBox.Text = "Termék keresése";
            imageDetailsGroupBox.Text = "Termék és előnézet";
            imageActionsGroupBox.Text = "Képműveletek";
            imageGalleryGroupBox.Text = "Termékképek";

            imageSearchDescriptionLabel.AutoSize = true;
            imageSearchDescriptionLabel.Text = "A keresés SKU alapján szűri a termékeket, majd a találati listából kiválasztható, melyik termék képeit szeretnéd módosítani.";

            imageSkuLabel.AutoSize = true;
            imageSkuLabel.Text = "SKU szűrő";

            imageSkuTextBox.PlaceholderText = "Példa: ABC-001 vagy részlet";
            imageSkuTextBox.KeyDown += ImageSkuTextBox_KeyDown;

            imageSearchButton.Text = "Szűrés";
            imageSearchButton.Click += async (_, _) => await SearchImageEditorProductsAsync();

            imageSearchClearButton.Text = "Mező törlése";
            imageSearchClearButton.Click += ImageSearchClearButton_Click;

            imageSearchResultsLabel.AutoSize = true;
            imageSearchResultsLabel.Text = "Nincs találat.";

            imageLoadSelectedProductButton.Text = "Kiválasztott termék betöltése";
            imageLoadSelectedProductButton.Click += async (_, _) => await LoadSelectedImageEditorProductAsync();

            imageSearchResultsGrid.AllowUserToAddRows = false;
            imageSearchResultsGrid.AllowUserToDeleteRows = false;
            imageSearchResultsGrid.AllowUserToResizeRows = false;
            imageSearchResultsGrid.AutoGenerateColumns = false;
            imageSearchResultsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            imageSearchResultsGrid.MultiSelect = false;
            imageSearchResultsGrid.ReadOnly = true;
            imageSearchResultsGrid.RowHeadersVisible = false;
            imageSearchResultsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            imageSearchResultsGrid.DataSource = imageSearchResultsBindingSource;
            imageSearchResultsGrid.SelectionChanged += ImageSearchResultsGrid_SelectionChanged;
            imageSearchResultsGrid.CellDoubleClick += async (_, _) => await LoadSelectedImageEditorProductAsync();
            imageSearchResultsGrid.Columns.AddRange(
            [
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(ImageEditorSearchRow.Sku),
                    HeaderText = "SKU",
                    MinimumWidth = 140,
                    FillWeight = 24
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(ImageEditorSearchRow.ProductName),
                    HeaderText = "Termék neve",
                    MinimumWidth = 220,
                    FillWeight = 42
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(ImageEditorSearchRow.ProductTypeName),
                    HeaderText = "Product type",
                    MinimumWidth = 150,
                    FillWeight = 24
                }
            ]);

            imageProductSummaryLabel.AutoSize = false;
            imageProductSummaryLabel.Text = "Nincs kiválasztott termék.";

            imagePreviewBox.BackColor = SurfaceStrongColor;
            imagePreviewBox.BorderStyle = BorderStyle.FixedSingle;
            imagePreviewBox.SizeMode = PictureBoxSizeMode.Zoom;
            imagePreviewBox.TabStop = false;

            imagePreviewCaptionLabel.AutoSize = false;
            imagePreviewCaptionLabel.AutoEllipsis = true;
            imagePreviewCaptionLabel.Text = "Előnézet";

            imageActionsSummaryLabel.AutoSize = true;
            imageActionsSummaryLabel.Text = "A galériában kiválasztott képet főképpé lehet tenni, a további képeket törölni lehet, illetve új képeket is fel lehet tölteni. A főkép törlése itt nem engedélyezett.";

            imageSelectedImageLabel.AutoSize = false;
            imageSelectedImageLabel.Text = "Kiválasztott kép: nincs";

            imageSetMainButton.Text = "Kiválasztott kép beállítása főképként";
            imageSetMainButton.Click += async (_, _) => await SetSelectedImageAsMainAsync();

            imageDeleteButton.Text = "Kiválasztott kép törlése";
            imageDeleteButton.Click += async (_, _) => await DeleteSelectedImageAsync();

            imageUploadButton.Text = "Képek feltöltése";
            imageUploadButton.Click += async (_, _) => await UploadImagesFromDialogAsync();

            imageRefreshButton.Text = "Képek frissítése";
            imageRefreshButton.Click += async (_, _) => await RefreshImageEditorCurrentProductAsync();

            imageActionStatusLabel.AutoSize = true;
            imageActionStatusLabel.Text = "A feltöltés több fájlt is kezel. A már létező nevű képek kihagyásra kerülnek.";

            imageGalleryFlowPanel.AutoScroll = true;
            imageGalleryFlowPanel.WrapContents = true;
            imageGalleryFlowPanel.FlowDirection = FlowDirection.LeftToRight;
            imageGalleryFlowPanel.BackColor = Color.Transparent;
            imageGalleryFlowPanel.BorderStyle = BorderStyle.None;

            imageGalleryEmptyLabel.AutoSize = true;
            imageGalleryEmptyLabel.Text = "Ehhez a termékhez még nincs betöltött kép.";

            imageSearchGroupBox.Controls.Add(imageSearchDescriptionLabel);
            imageSearchGroupBox.Controls.Add(imageSkuLabel);
            imageSearchGroupBox.Controls.Add(imageSkuTextBox);
            imageSearchGroupBox.Controls.Add(imageSearchButton);
            imageSearchGroupBox.Controls.Add(imageSearchClearButton);
            imageSearchGroupBox.Controls.Add(imageSearchResultsLabel);
            imageSearchGroupBox.Controls.Add(imageLoadSelectedProductButton);
            imageSearchGroupBox.Controls.Add(imageSearchResultsGrid);

            imageDetailsGroupBox.Controls.Add(imageProductSummaryLabel);
            imageDetailsGroupBox.Controls.Add(imagePreviewBox);
            imageDetailsGroupBox.Controls.Add(imagePreviewCaptionLabel);

            imageActionsGroupBox.Controls.Add(imageActionsSummaryLabel);
            imageActionsGroupBox.Controls.Add(imageSelectedImageLabel);
            imageActionsGroupBox.Controls.Add(imageSetMainButton);
            imageActionsGroupBox.Controls.Add(imageDeleteButton);
            imageActionsGroupBox.Controls.Add(imageUploadButton);
            imageActionsGroupBox.Controls.Add(imageRefreshButton);
            imageActionsGroupBox.Controls.Add(imageActionStatusLabel);

            imageGalleryGroupBox.Controls.Add(imageGalleryFlowPanel);
            imageGalleryGroupBox.Controls.Add(imageGalleryEmptyLabel);

            imageEditorPanel.Controls.Add(imageSearchGroupBox);
            imageEditorPanel.Controls.Add(imageDetailsGroupBox);
            imageEditorPanel.Controls.Add(imageActionsGroupBox);
            imageEditorPanel.Controls.Add(imageGalleryGroupBox);
            Controls.Add(imageEditorPanel);
            imageEditorPanel.BringToFront();

            UpdateImageEditorHeaderState();
            ResetImageSearchResults();
            ClearImageEditorState(clearSku: false);
        }

        private void ApplyImageEditorTheme()
        {
            ConfigureSurfaceGroupBox(imageSearchGroupBox);
            ConfigureSurfaceGroupBox(imageDetailsGroupBox);
            ConfigureSurfaceGroupBox(imageActionsGroupBox);
            ConfigureSurfaceGroupBox(imageGalleryGroupBox);

            StyleTextBox(imageSkuTextBox);
            StylePrimaryButton(imageSearchButton);
            StyleSecondaryButton(imageSearchClearButton);
            StylePrimaryButton(imageLoadSelectedProductButton);
            StyleSecondaryButton(imageSetMainButton);
            StylePrimaryButton(imageDeleteButton);
            StyleSecondaryButton(imageUploadButton);
            StyleSecondaryButton(imageRefreshButton);

            foreach (Label label in new[]
            {
                imageSearchDescriptionLabel,
                imageSkuLabel,
                imageSearchResultsLabel,
                imageProductSummaryLabel,
                imagePreviewCaptionLabel,
                imageActionsSummaryLabel,
                imageSelectedImageLabel,
                imageActionStatusLabel,
                imageGalleryEmptyLabel
            })
            {
                label.ForeColor = InkColor;
            }

            imageActionStatusLabel.ForeColor = MutedInkColor;
            imagePreviewCaptionLabel.ForeColor = AccentBlueColor;

            imageSearchResultsGrid.BackgroundColor = SurfaceStrongColor;
            imageSearchResultsGrid.BorderStyle = BorderStyle.None;
            imageSearchResultsGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            imageSearchResultsGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            imageSearchResultsGrid.EnableHeadersVisualStyles = false;
            imageSearchResultsGrid.GridColor = BorderColor;
            imageSearchResultsGrid.DefaultCellStyle.BackColor = SurfaceColor;
            imageSearchResultsGrid.DefaultCellStyle.ForeColor = InkColor;
            imageSearchResultsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(227, 238, 243);
            imageSearchResultsGrid.DefaultCellStyle.SelectionForeColor = InkColor;
            imageSearchResultsGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 246, 231);
            imageSearchResultsGrid.ColumnHeadersDefaultCellStyle.BackColor = AccentBlueColor;
            imageSearchResultsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            imageSearchResultsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            imageSearchResultsGrid.ColumnHeadersHeight = 38;
            imageSearchResultsGrid.RowTemplate.Height = 34;
        }

        private int LayoutImageEditorSection(int contentWidth, int y)
        {
            UpdateImageEditorHeaderState();

            bool twoColumns = contentWidth >= 1180;
            int searchHeight = 340;
            int galleryHeight = 410;
            int topCardsHeight = twoColumns ? 400 : 720;
            int panelHeight = searchHeight + SectionSpacing + topCardsHeight + SectionSpacing + galleryHeight;

            imageEditorPanel.SetBounds(PageMargin, y, contentWidth, panelHeight);

            imageSearchGroupBox.SetBounds(0, 0, contentWidth, searchHeight);
            LayoutImageSearchGroup();

            int currentY = imageSearchGroupBox.Bottom + SectionSpacing;

            if (twoColumns)
            {
                int leftWidth = 380;
                int rightWidth = contentWidth - leftWidth - CardSpacing;
                imageDetailsGroupBox.SetBounds(0, currentY, leftWidth, topCardsHeight);
                imageActionsGroupBox.SetBounds(leftWidth + CardSpacing, currentY, rightWidth, topCardsHeight);
            }
            else
            {
                imageDetailsGroupBox.SetBounds(0, currentY, contentWidth, 320);
                imageActionsGroupBox.SetBounds(0, imageDetailsGroupBox.Bottom + CardSpacing, contentWidth, 384);
            }

            LayoutImageDetailsGroup();
            LayoutImageActionsGroup();

            imageGalleryGroupBox.SetBounds(0, imageActionsGroupBox.Bottom + SectionSpacing, contentWidth, galleryHeight);
            LayoutImageGalleryGroup();

            imageEditorPanel.Height = imageGalleryGroupBox.Bottom;
            return imageEditorPanel.Bottom + SectionSpacing;
        }

        private void LayoutImageSearchGroup()
        {
            const int left = 24;
            const int top = 36;
            const int gap = 16;
            const int rowGap = 8;
            const int buttonHeight = 40;
            const int searchButtonWidth = 160;
            const int clearButtonWidth = 150;
            const int loadButtonWidth = 250;

            int contentWidth = Math.Max(220, imageSearchGroupBox.ClientSize.Width - (left * 2));
            int currentY = top;

            imageSearchDescriptionLabel.MaximumSize = new Size(contentWidth, 0);
            imageSearchDescriptionLabel.Location = new Point(left, currentY);
            currentY = imageSearchDescriptionLabel.Bottom + gap;

            bool twoColumns = contentWidth >= 900;

            if (twoColumns)
            {
                int leftColumnWidth = (contentWidth - gap) / 2;
                int rightColumnWidth = contentWidth - leftColumnWidth - gap;
                int leftColumnX = left;
                int rightColumnX = left + leftColumnWidth + gap;

                imageSkuLabel.Location = new Point(leftColumnX, currentY);
                int controlTop = imageSkuLabel.Bottom + rowGap;
                imageSkuTextBox.SetBounds(leftColumnX, controlTop, leftColumnWidth, imageSkuTextBox.Height);

                int buttonTop = imageSkuTextBox.Bottom + 12;
                imageSearchButton.SetBounds(leftColumnX, buttonTop, searchButtonWidth, buttonHeight);
                imageSearchClearButton.SetBounds(imageSearchButton.Right + 12, buttonTop, clearButtonWidth, buttonHeight);

                imageSearchResultsLabel.Location = new Point(rightColumnX, currentY + 4);
                imageLoadSelectedProductButton.SetBounds(
                    rightColumnX + rightColumnWidth - loadButtonWidth,
                    currentY - 2,
                    loadButtonWidth,
                    buttonHeight);

                int resultsGridTop = imageSearchResultsLabel.Bottom + 10;
                int resultsGridHeight = imageSearchGroupBox.ClientSize.Height - resultsGridTop - 20;
                imageSearchResultsGrid.SetBounds(rightColumnX, resultsGridTop, rightColumnWidth, resultsGridHeight);
                return;
            }

            imageSkuLabel.Location = new Point(left, currentY);
            currentY = imageSkuLabel.Bottom + rowGap;

            int textBoxWidth = Math.Max(260, contentWidth - searchButtonWidth - clearButtonWidth - 24);
            imageSkuTextBox.SetBounds(left, currentY, textBoxWidth, imageSkuTextBox.Height);
            imageSearchButton.SetBounds(imageSkuTextBox.Right + 12, currentY - 1, searchButtonWidth, buttonHeight);
            imageSearchClearButton.SetBounds(imageSearchButton.Right + 12, currentY - 1, clearButtonWidth, buttonHeight);

            currentY = Math.Max(imageSkuTextBox.Bottom, imageSearchButton.Bottom) + 16;
            imageSearchResultsLabel.Location = new Point(left, currentY + 6);
            imageLoadSelectedProductButton.SetBounds(imageSearchGroupBox.ClientSize.Width - left - loadButtonWidth, currentY, loadButtonWidth, buttonHeight);

            int gridTop = imageLoadSelectedProductButton.Bottom + 12;
            int gridHeight = imageSearchGroupBox.ClientSize.Height - gridTop - 22;
            imageSearchResultsGrid.SetBounds(left, gridTop, contentWidth, gridHeight);
        }

        private void LayoutImageDetailsGroup()
        {
            const int left = 24;
            const int top = 36;
            const int gap = 16;

            int contentWidth = Math.Max(220, imageDetailsGroupBox.ClientSize.Width - (left * 2));

            imageProductSummaryLabel.SetBounds(left, top, contentWidth, 106);
            int previewTop = imageProductSummaryLabel.Bottom + gap;
            int previewHeight = Math.Max(150, imageDetailsGroupBox.ClientSize.Height - previewTop - 74);
            imagePreviewBox.SetBounds(left, previewTop, contentWidth, previewHeight);
            imagePreviewCaptionLabel.SetBounds(left, imagePreviewBox.Bottom + 10, contentWidth, 36);
        }

        private void LayoutImageActionsGroup()
        {
            const int left = 24;
            const int top = 36;
            const int gap = 14;
            const int buttonHeight = 42;

            int contentWidth = Math.Max(220, imageActionsGroupBox.ClientSize.Width - (left * 2));
            int currentY = top;

            imageActionsSummaryLabel.MaximumSize = new Size(contentWidth, 0);
            imageActionsSummaryLabel.Location = new Point(left, currentY);
            currentY = imageActionsSummaryLabel.Bottom + gap;

            Size selectedImageTextSize = TextRenderer.MeasureText(
                imageSelectedImageLabel.Text,
                imageSelectedImageLabel.Font,
                new Size(contentWidth, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.Left);
            int selectedImageLabelHeight = Math.Max(36, Math.Min(108, selectedImageTextSize.Height + 8));
            imageSelectedImageLabel.SetBounds(left, currentY, contentWidth, selectedImageLabelHeight);
            currentY = imageSelectedImageLabel.Bottom + gap;

            imageSetMainButton.SetBounds(left, currentY, contentWidth, buttonHeight);
            currentY = imageSetMainButton.Bottom + 10;

            bool canSplit = contentWidth >= 520;

            if (canSplit)
            {
                int halfWidth = (contentWidth - 12) / 2;
                imageDeleteButton.SetBounds(left, currentY, halfWidth, buttonHeight);
                imageUploadButton.SetBounds(imageDeleteButton.Right + 12, currentY, contentWidth - halfWidth - 12, buttonHeight);
                currentY = imageUploadButton.Bottom + 10;
                imageRefreshButton.SetBounds(left, currentY, contentWidth, buttonHeight);
            }
            else
            {
                imageDeleteButton.SetBounds(left, currentY, contentWidth, buttonHeight);
                currentY = imageDeleteButton.Bottom + 10;
                imageUploadButton.SetBounds(left, currentY, contentWidth, buttonHeight);
                currentY = imageUploadButton.Bottom + 10;
                imageRefreshButton.SetBounds(left, currentY, contentWidth, buttonHeight);
            }

            currentY = imageRefreshButton.Bottom + gap;
            imageActionStatusLabel.MaximumSize = new Size(contentWidth, 0);
            imageActionStatusLabel.Location = new Point(left, currentY);
        }

        private void LayoutImageGalleryGroup()
        {
            const int left = 24;
            const int top = 36;
            const int bottom = 18;

            int contentWidth = Math.Max(220, imageGalleryGroupBox.ClientSize.Width - (left * 2));
            int contentHeight = Math.Max(120, imageGalleryGroupBox.ClientSize.Height - top - bottom);

            imageGalleryFlowPanel.SetBounds(left, top, contentWidth, contentHeight);
            imageGalleryEmptyLabel.MaximumSize = new Size(contentWidth, 0);
            imageGalleryEmptyLabel.Location = new Point(left, top + 4);
        }

        private void UpdateImageEditorActionStates(bool isBusy)
        {
            bool hasSelectedSearchRow = GetSelectedImageEditorSearchRow() is not null;
            bool hasProduct = imageEditorCurrentProduct is not null;
            bool hasSelectedImage = imageEditorSelectedImage is not null;

            imageSkuTextBox.Enabled = !isBusy;
            imageSearchButton.Enabled = hotcakesReady && !isBusy;
            imageSearchClearButton.Enabled = !isBusy;
            imageLoadSelectedProductButton.Enabled = hotcakesReady && !isBusy && hasSelectedSearchRow;
            imageSetMainButton.Enabled = hotcakesReady && !isBusy && hasProduct && hasSelectedImage;
            imageDeleteButton.Enabled = hotcakesReady &&
                !isBusy &&
                hasProduct &&
                hasSelectedImage &&
                imageEditorSelectedImage is not null &&
                !imageEditorSelectedImage.IsCurrentMain;
            imageUploadButton.Enabled = hotcakesReady && !isBusy && hasProduct;
            imageRefreshButton.Enabled = hotcakesReady && !isBusy && hasProduct;
        }

        private void UpdateImageEditorHeaderState()
        {
            if (currentPage != FormPage.ImageEditor)
            {
                return;
            }

            titleLabel.Text = "Kép szerkesztő";
            subtitleLabel.Text = "SKU alapján szűrhető terméklista, képelőnézet, főkép-váltás, törlés és több fájlos feltöltés.";
            accentBadgeLabel.Text = "Kép mód";
        }

        private async Task SearchImageEditorProductsAsync()
        {
            if (!hotcakesReady)
            {
                AppDialog.ShowWarning(
                    this,
                    "Kép szerkesztő",
                    "A Hotcakes kapcsolat még nem áll készen a kép szerkesztő használatához.");
                return;
            }

            string skuFilter = imageSkuTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(skuFilter))
            {
                AppDialog.ShowInfo(
                    this,
                    "Kép szerkesztő",
                    "Adj meg egy SKU-t vagy részletet a szűréshez.");
                return;
            }

            try
            {
                UseWaitCursor = true;
                isImporting = true;
                UpdateActionStates();
                SetStatusMessage($"Kép szerkesztő: termékek szűrése ({skuFilter})...");

                await EnsureProductsLoadedAsync();

                List<HotcakesProduct> matches = loadedProductsBySku.Values
                    .Where(product => !string.IsNullOrWhiteSpace(product.Sku) &&
                                      product.Sku.Contains(skuFilter, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(product => product.Sku, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                PopulateImageEditorSearchResults(matches);

                if (matches.Count == 0)
                {
                    ClearImageEditorState(clearSku: false);
                    SetStatusMessage($"Kép szerkesztő: nincs találat a megadott SKU szűrőre ({skuFilter}).", true);
                }
                else
                {
                    SetStatusMessage($"Kép szerkesztő: {matches.Count} termék találat.");
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(
                    this,
                    "Kép szerkesztő",
                    $"A termékek szűrése nem sikerült.{Environment.NewLine}{Environment.NewLine}{ex.Message}");
                SetStatusMessage("A kép szerkesztő szűrése nem sikerült.", true);
            }
            finally
            {
                isImporting = false;
                UpdateActionStates();
                UseWaitCursor = false;
            }
        }

        private void PopulateImageEditorSearchResults(IReadOnlyList<HotcakesProduct> products)
        {
            imageSearchResultRows.Clear();

            foreach (HotcakesProduct product in products)
            {
                imageSearchResultRows.Add(new ImageEditorSearchRow
                {
                    Product = product,
                    Sku = product.Sku,
                    ProductName = product.ProductName,
                    ProductTypeName = ResolveProductTypeDisplayName(product.ProductTypeId)
                });
            }

            imageSearchResultsBindingSource.DataSource = imageSearchResultRows;
            imageSearchResultsLabel.Text = products.Count == 0
                ? "Nincs találat."
                : $"Találatok: {products.Count}";

            if (imageSearchResultsGrid.Rows.Count > 0)
            {
                imageSearchResultsGrid.ClearSelection();
                imageSearchResultsGrid.Rows[0].Selected = true;
                imageSearchResultsGrid.CurrentCell = imageSearchResultsGrid.Rows[0].Cells[0];
            }

            UpdateActionStates();
        }

        private void ResetImageSearchResults()
        {
            imageSearchResultRows.Clear();
            imageSearchResultsBindingSource.DataSource = imageSearchResultRows;
            imageSearchResultsLabel.Text = "Nincs találat.";
        }

        private async Task LoadSelectedImageEditorProductAsync()
        {
            ImageEditorSearchRow? selectedRow = GetSelectedImageEditorSearchRow();

            if (selectedRow?.Product is null)
            {
                return;
            }

            try
            {
                UseWaitCursor = true;
                isImporting = true;
                UpdateActionStates();

                imageEditorCurrentProduct = selectedRow.Product;
                imageSkuTextBox.Text = selectedRow.Product.Sku;
                await LoadImageEditorImagesAsync(selectedRow.Product);
                SetStatusMessage($"Kép szerkesztő: {selectedRow.Product.Sku} képei betöltve.");
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(
                    this,
                    "Kép szerkesztő",
                    $"A kiválasztott termék betöltése nem sikerült.{Environment.NewLine}{Environment.NewLine}{ex.Message}");
                SetStatusMessage("A kiválasztott termék betöltése nem sikerült.", true);
            }
            finally
            {
                isImporting = false;
                UpdateActionStates();
                UseWaitCursor = false;
            }
        }

        private ImageEditorSearchRow? GetSelectedImageEditorSearchRow()
        {
            if (imageSearchResultsGrid.SelectedRows.Count == 0)
            {
                return null;
            }

            return imageSearchResultsGrid.SelectedRows[0].DataBoundItem as ImageEditorSearchRow;
        }

        private async Task RefreshImageEditorCurrentProductAsync()
        {
            if (imageEditorCurrentProduct is null)
            {
                return;
            }

            await LoadImageEditorImagesAsync(imageEditorCurrentProduct);
            SetStatusMessage($"Kép szerkesztő: {imageEditorCurrentProduct.Sku} képei frissítve.");
        }

        private async Task LoadImageEditorImagesAsync(HotcakesProduct product)
        {
            imageEditorImages.Clear();
            imageEditorSelectedImage = null;

            string mainFileName = ResolveMainImageFileName(product);
            string mainAlternateText = ResolveMainAlternateText(product, mainFileName);

            if (!string.IsNullOrWhiteSpace(mainFileName))
            {
                imageEditorImages.Add(new ImageEditorImageItem
                {
                    FileName = mainFileName,
                    AlternateText = mainAlternateText,
                    IsCurrentMain = true,
                    ImageLocation = ResolveMainImageLocation(product.Bvin, mainFileName)
                });
            }

            IReadOnlyList<HotcakesProductImage> existingImages = string.IsNullOrWhiteSpace(product.Bvin)
                ? []
                : await hotcakesClient.GetProductImagesForProductAsync(product.Bvin);

            foreach (HotcakesProductImage existingImage in existingImages
                .OrderBy(image => image.SortOrder)
                .ThenBy(image => image.FileName, StringComparer.OrdinalIgnoreCase))
            {
                string normalizedFileName = Path.GetFileName(existingImage.FileName?.Trim() ?? string.Empty);

                if (string.IsNullOrWhiteSpace(normalizedFileName))
                {
                    continue;
                }

                ImageEditorImageItem? currentItem = imageEditorImages.FirstOrDefault(item =>
                    string.Equals(item.FileName, normalizedFileName, StringComparison.OrdinalIgnoreCase));

                if (currentItem is not null)
                {
                    currentItem.ProductImageBvin = existingImage.Bvin;
                    currentItem.SortOrder = existingImage.SortOrder;
                    currentItem.ImageLocation = ResolveAdditionalImageLocation(product.Bvin, normalizedFileName, existingImage.Bvin);

                    if (string.IsNullOrWhiteSpace(currentItem.AlternateText))
                    {
                        currentItem.AlternateText = existingImage.AlternateText;
                    }

                    continue;
                }

                imageEditorImages.Add(new ImageEditorImageItem
                {
                    FileName = normalizedFileName,
                    AlternateText = existingImage.AlternateText,
                    ProductImageBvin = existingImage.Bvin,
                    SortOrder = existingImage.SortOrder,
                    IsCurrentMain = false,
                    ImageLocation = ResolveAdditionalImageLocation(product.Bvin, normalizedFileName, existingImage.Bvin)
                });
            }

            imageEditorSelectedImage = imageEditorImages.FirstOrDefault(static image => image.IsCurrentMain) ?? imageEditorImages.FirstOrDefault();
            RebuildImageGallery();
            UpdateImageEditorProductSummary();
            UpdateImageEditorSelectionState();
            UpdateActionStates();
        }

        private void RebuildImageGallery()
        {
            foreach (Control control in imageGalleryFlowPanel.Controls)
            {
                control.Dispose();
            }

            imageGalleryFlowPanel.Controls.Clear();

            foreach (ImageEditorImageItem imageItem in imageEditorImages)
            {
                imageGalleryFlowPanel.Controls.Add(CreateImageEditorCard(imageItem));
            }

            imageGalleryEmptyLabel.Visible = imageEditorImages.Count == 0;
            imageGalleryFlowPanel.Visible = imageEditorImages.Count > 0;
        }

        private Control CreateImageEditorCard(ImageEditorImageItem imageItem)
        {
            Panel cardPanel = new()
            {
                Width = 156,
                Height = 188,
                Margin = new Padding(0, 0, 14, 14),
                Padding = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = imageItem
            };

            cardPanel.Paint += ImageCardPanel_Paint;

            PictureBox thumbnailBox = new()
            {
                Left = 10,
                Top = 10,
                Width = 136,
                Height = 102,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = SurfaceStrongColor,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Tag = imageItem
            };

            if (!string.IsNullOrWhiteSpace(imageItem.ImageLocation))
            {
                _ = LoadPictureBoxImageAsync(thumbnailBox, imageItem.ImageLocation);
            }

            Label nameLabel = new()
            {
                Left = 10,
                Top = thumbnailBox.Bottom + 10,
                Width = 136,
                Height = 38,
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                ForeColor = InkColor,
                Text = imageItem.FileName,
                Cursor = Cursors.Hand,
                Tag = imageItem
            };

            Label badgeLabel = new()
            {
                Left = 10,
                Top = nameLabel.Bottom + 6,
                Width = 136,
                Height = 18,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = imageItem.IsCurrentMain ? AccentCoralColor : MutedInkColor,
                Text = imageItem.IsCurrentMain ? "Főkép" : "További kép",
                Cursor = Cursors.Hand,
                Tag = imageItem
            };

            cardPanel.Controls.Add(thumbnailBox);
            cardPanel.Controls.Add(nameLabel);
            cardPanel.Controls.Add(badgeLabel);

            WireImageEditorSelection(cardPanel, imageItem);
            WireImageEditorSelection(thumbnailBox, imageItem);
            WireImageEditorSelection(nameLabel, imageItem);
            WireImageEditorSelection(badgeLabel, imageItem);

            imageItem.CardPanel = cardPanel;
            return cardPanel;
        }

        private void WireImageEditorSelection(Control control, ImageEditorImageItem imageItem)
        {
            control.Click += (_, _) => SelectImageEditorImage(imageItem);
        }

        private void SelectImageEditorImage(ImageEditorImageItem imageItem)
        {
            imageEditorSelectedImage = imageItem;
            UpdateImageEditorSelectionState();
            UpdateActionStates();
        }

        private void UpdateImageEditorSelectionState()
        {
            foreach (ImageEditorImageItem imageItem in imageEditorImages)
            {
                imageItem.CardPanel?.Invalidate();
            }

            if (imageEditorSelectedImage is null)
            {
                imageSelectedImageLabel.Text = "Kiválasztott kép: nincs";
                imagePreviewCaptionLabel.Text = "Előnézet";
                imagePreviewBox.ImageLocation = null;
                imagePreviewBox.Image = null;
                LayoutImageActionsGroup();
                return;
            }

            string selectionKind = imageEditorSelectedImage.IsCurrentMain ? "főkép" : "további kép";
            imageSelectedImageLabel.Text = $"Kiválasztott kép: {imageEditorSelectedImage.FileName} ({selectionKind})";
            imagePreviewCaptionLabel.Text = imageEditorSelectedImage.IsCurrentMain
                ? $"Jelenlegi főkép: {imageEditorSelectedImage.FileName}"
                : $"Előnézet: {imageEditorSelectedImage.FileName}";
            imagePreviewBox.ImageLocation = null;
            imagePreviewBox.Image = null;
            LayoutImageActionsGroup();

            if (!string.IsNullOrWhiteSpace(imageEditorSelectedImage.ImageLocation))
            {
                _ = LoadPictureBoxImageAsync(imagePreviewBox, imageEditorSelectedImage.ImageLocation);
            }
        }

        private void UpdateImageEditorProductSummary()
        {
            if (imageEditorCurrentProduct is null)
            {
                imageProductSummaryLabel.Text = "Nincs kiválasztott termék.";
                return;
            }

            string mainFileName = imageEditorImages.FirstOrDefault(static image => image.IsCurrentMain)?.FileName ?? "nincs";
            StringBuilder summaryBuilder = new();
            summaryBuilder.AppendLine($"SKU: {imageEditorCurrentProduct.Sku}");
            summaryBuilder.AppendLine($"Név: {imageEditorCurrentProduct.ProductName}");
            summaryBuilder.AppendLine($"Product type: {ResolveProductTypeDisplayName(imageEditorCurrentProduct.ProductTypeId)}");
            summaryBuilder.AppendLine($"Összes kép: {imageEditorImages.Count}");
            summaryBuilder.Append($"Főkép: {mainFileName}");
            imageProductSummaryLabel.Text = summaryBuilder.ToString();
        }

        private async Task SetSelectedImageAsMainAsync()
        {
            if (imageEditorCurrentProduct is null || imageEditorSelectedImage is null)
            {
                return;
            }

            if (imageEditorSelectedImage.IsCurrentMain)
            {
                SetStatusMessage("Kép szerkesztő: a kiválasztott kép már főképként van beállítva.");
                return;
            }

            bool confirmed = ShowLocalizedConfirmation(
                $"Valóban főképpé teszed ezt a képet?{Environment.NewLine}{Environment.NewLine}{imageEditorSelectedImage.FileName}",
                "Kép szerkesztő");

            if (!confirmed)
            {
                return;
            }

            try
            {
                ImageEditorImageItem selectedImage = imageEditorSelectedImage;
                HotcakesProduct currentProduct = imageEditorCurrentProduct;
                UseWaitCursor = true;
                isImporting = true;
                UpdateActionStates();
                SetStatusMessage($"Kép szerkesztő: főkép módosítása ({selectedImage.FileName})...");

                byte[] selectedImageContent = await LoadImageFileContentAsync(currentProduct.Bvin, selectedImage);

                if (!string.IsNullOrWhiteSpace(selectedImage.ProductImageBvin))
                {
                    await ReplaceSelectedAdditionalImageWithCurrentMainAsync(currentProduct, selectedImage);
                }
                else
                {
                    await PreserveCurrentMainImageAsAdditionalAsync(currentProduct, selectedImage.FileName);
                }

                bool mainImageUploaded = await hotcakesClient.UploadProductMainImageAsync(
                    currentProduct.Bvin,
                    selectedImage.FileName,
                    selectedImageContent);

                if (!mainImageUploaded)
                {
                    throw new InvalidOperationException("A kiválasztott kép főképként történő feltöltése sikertelen volt.");
                }

                HotcakesProduct updatedProduct = await SaveMainImageMetadataAsync(currentProduct, selectedImage.FileName);

                imageEditorCurrentProduct = updatedProduct;
                loadedProductsBySku[updatedProduct.Sku] = updatedProduct;

                await LoadImageEditorImagesAsync(updatedProduct);
                SetStatusMessage($"Kép szerkesztő: új főkép beállítva ({selectedImage.FileName}).");
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(
                    this,
                    "Kép szerkesztő",
                    $"A főkép beállítása nem sikerült.{Environment.NewLine}{Environment.NewLine}{ex.Message}");
                SetStatusMessage("A főkép beállítása nem sikerült.", true);
            }
            finally
            {
                isImporting = false;
                UpdateActionStates();
                UseWaitCursor = false;
            }
        }

        private async Task DeleteSelectedImageAsync()
        {
            if (imageEditorCurrentProduct is null || imageEditorSelectedImage is null)
            {
                return;
            }

            if (imageEditorSelectedImage.IsCurrentMain)
            {
                AppDialog.ShowWarning(
                    this,
                    "Kép szerkesztő",
                    "A főkép törlése az alkalmazásban nem engedélyezett. Előbb válassz másik főképet, vagy törölj további képet.");
                SetStatusMessage("A főkép törlése nem engedélyezett.", true);
                return;
            }

            string deleteLabel = imageEditorSelectedImage.IsCurrentMain ? "főképet" : "képet";
            bool confirmed = ShowLocalizedConfirmation(
                $"Valóban törölni szeretnéd a kiválasztott {deleteLabel}?{Environment.NewLine}{Environment.NewLine}{imageEditorSelectedImage.FileName}",
                "Kép szerkesztő");

            if (!confirmed)
            {
                return;
            }

            try
            {
                UseWaitCursor = true;
                isImporting = true;
                UpdateActionStates();
                SetStatusMessage($"Kép szerkesztő: kép törlése ({imageEditorSelectedImage.FileName})...");

                HotcakesProduct product = imageEditorCurrentProduct;

                if (!string.IsNullOrWhiteSpace(imageEditorSelectedImage.ProductImageBvin))
                {
                    bool deleted = await hotcakesClient.DeleteProductImageAsync(imageEditorSelectedImage.ProductImageBvin);

                    if (!deleted)
                    {
                        throw new InvalidOperationException("A Hotcakes nem tudta törölni a kiválasztott képet.");
                    }
                }

                if (imageEditorSelectedImage.IsCurrentMain)
                {
                    product = await ClearMainImageMetadataAsync(product, imageEditorSelectedImage.FileName);
                    loadedProductsBySku[product.Sku] = product;
                }

                imageEditorCurrentProduct = product;
                await LoadImageEditorImagesAsync(product);
                SetStatusMessage($"Kép szerkesztő: kép törölve ({imageEditorSelectedImage.FileName}).");
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(
                    this,
                    "Kép szerkesztő",
                    $"A kép törlése nem sikerült.{Environment.NewLine}{Environment.NewLine}{ex.Message}");
                SetStatusMessage("A kép törlése nem sikerült.", true);
            }
            finally
            {
                isImporting = false;
                UpdateActionStates();
                UseWaitCursor = false;
            }
        }

        private async Task PreserveCurrentMainImageAsAdditionalAsync(HotcakesProduct product, string newMainFileName)
        {
            ArgumentNullException.ThrowIfNull(product);

            string currentMainFileName = ResolveMainImageFileName(product);
            string normalizedCurrentMainFileName = Path.GetFileName(currentMainFileName?.Trim() ?? string.Empty);
            string normalizedNewMainFileName = Path.GetFileName(newMainFileName?.Trim() ?? string.Empty);

            if (string.IsNullOrWhiteSpace(normalizedCurrentMainFileName) ||
                string.Equals(normalizedCurrentMainFileName, normalizedNewMainFileName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            IReadOnlyList<HotcakesProductImage> existingImages = await hotcakesClient.GetProductImagesForProductAsync(product.Bvin);
            bool alreadyExistsAsAdditional = existingImages.Any(image =>
                string.Equals(
                    Path.GetFileName(image.FileName?.Trim() ?? string.Empty),
                    normalizedCurrentMainFileName,
                    StringComparison.OrdinalIgnoreCase));

            if (alreadyExistsAsAdditional)
            {
                return;
            }

            byte[] fileContent = await LoadMainImageFileContentAsync(product.Bvin, normalizedCurrentMainFileName);
            string alternateText = ResolveMainAlternateText(product, normalizedCurrentMainFileName);
            bool uploaded = await hotcakesClient.UploadProductAdditionalImageAsync(
                product.Bvin,
                normalizedCurrentMainFileName,
                fileContent,
                alternateText,
                product.StoreId);

            if (!uploaded)
            {
                throw new InvalidOperationException("A jelenlegi főkép nem menthető át további képként.");
            }
        }

        private async Task ReplaceSelectedAdditionalImageWithCurrentMainAsync(HotcakesProduct product, ImageEditorImageItem selectedImage)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentNullException.ThrowIfNull(selectedImage);

            if (string.IsNullOrWhiteSpace(selectedImage.ProductImageBvin))
            {
                return;
            }

            string currentMainFileName = ResolveMainImageFileName(product);
            string normalizedCurrentMainFileName = Path.GetFileName(currentMainFileName?.Trim() ?? string.Empty);
            string normalizedSelectedFileName = Path.GetFileName(selectedImage.FileName?.Trim() ?? string.Empty);

            if (string.IsNullOrWhiteSpace(normalizedCurrentMainFileName) ||
                string.Equals(normalizedCurrentMainFileName, normalizedSelectedFileName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            byte[] currentMainContent = await LoadMainImageFileContentAsync(product.Bvin, normalizedCurrentMainFileName);
            string alternateText = ResolveMainAlternateText(product, normalizedCurrentMainFileName);
            HotcakesProductImage updatedImage = await hotcakesClient.UpdateProductImageAsync(
                new HotcakesProductImage
                {
                    Bvin = selectedImage.ProductImageBvin,
                    ProductId = product.Bvin,
                    FileName = normalizedCurrentMainFileName,
                    Caption = string.Empty,
                    AlternateText = alternateText,
                    SortOrder = selectedImage.SortOrder,
                    StoreId = product.StoreId,
                    LastUpdatedUtc = DateTime.UtcNow
                });

            if (string.IsNullOrWhiteSpace(updatedImage.Bvin))
            {
                throw new InvalidOperationException("A korábbi főkép képrekordja nem frissíthető a helycseréhez.");
            }

            bool uploaded = await hotcakesClient.UploadProductAdditionalImageAsync(
                product.Bvin,
                normalizedCurrentMainFileName,
                currentMainContent,
                alternateText,
                product.StoreId,
                selectedImage.ProductImageBvin);

            if (!uploaded)
            {
                throw new InvalidOperationException("A korábbi főkép nem menthető át a kiválasztott további kép helyére.");
            }
        }

        private async Task<byte[]> LoadImageFileContentAsync(string productBvin, ImageEditorImageItem imageItem)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productBvin);
            ArgumentNullException.ThrowIfNull(imageItem);

            if (!string.IsNullOrWhiteSpace(imageItem.ProductImageBvin))
            {
                string? additionalLocalPath = TryResolveAdditionalImageFilePath(productBvin, imageItem.ProductImageBvin, imageItem.FileName);

                if (!string.IsNullOrWhiteSpace(additionalLocalPath) && File.Exists(additionalLocalPath))
                {
                    return await File.ReadAllBytesAsync(additionalLocalPath);
                }

                string additionalImageUrl = BuildAdditionalImageUrl(productBvin, imageItem.ProductImageBvin, imageItem.FileName, "medium");
                using HttpClient httpClient = new();
                return await httpClient.GetByteArrayAsync(additionalImageUrl);
            }

            return await LoadMainImageFileContentAsync(productBvin, imageItem.FileName);
        }

        private async Task LoadPictureBoxImageAsync(PictureBox pictureBox, string imageLocation)
        {
            if (pictureBox.IsDisposed || string.IsNullOrWhiteSpace(imageLocation))
            {
                return;
            }

            try
            {
                using Bitmap loadedImage = await DecodeImageForPictureBoxAsync(imageLocation);
                Bitmap displayImage = new(loadedImage);

                if (pictureBox.IsDisposed)
                {
                    displayImage.Dispose();
                    return;
                }

                void AssignImage()
                {
                    if (pictureBox.IsDisposed)
                    {
                        displayImage.Dispose();
                        return;
                    }

                    Image? previousImage = pictureBox.Image;
                    pictureBox.ImageLocation = null;
                    pictureBox.Image = displayImage;
                    previousImage?.Dispose();
                }

                if (pictureBox.InvokeRequired)
                {
                    pictureBox.BeginInvoke(AssignImage);
                }
                else
                {
                    AssignImage();
                }
            }
            catch
            {
                if (!pictureBox.IsDisposed)
                {
                    pictureBox.ImageLocation = imageLocation;
                }
            }
        }

        private static async Task<Bitmap> DecodeImageForPictureBoxAsync(string imageLocation)
        {
            byte[] imageBytes = File.Exists(imageLocation)
                ? await File.ReadAllBytesAsync(imageLocation)
                : await ImageDownloadHttpClient.GetByteArrayAsync(imageLocation);

            using var image = ImageSharpImage.Load<Rgba32>(imageBytes);
            using MemoryStream pngStream = new();
            image.Save(pngStream, new PngEncoder());
            pngStream.Position = 0;

            using Bitmap decodedBitmap = new(pngStream);
            return new Bitmap(decodedBitmap);
        }

        private async Task<byte[]> LoadMainImageFileContentAsync(string productBvin, string fileName)
        {
            string? localPath = TryResolveMainImageFilePath(productBvin, fileName);

            if (!string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath))
            {
                return await File.ReadAllBytesAsync(localPath);
            }

            string imageUrl = BuildMainImageUrl(productBvin, fileName, "medium");
            return await ImageDownloadHttpClient.GetByteArrayAsync(imageUrl);
        }

        private async Task<HotcakesProduct> ClearMainImageMetadataAsync(HotcakesProduct product, string fileName)
        {
            string normalizedFileName = Path.GetFileName(fileName?.Trim() ?? string.Empty);
            bool changed = false;

            if (string.Equals(Path.GetFileName(product.ImageFileSmall?.Trim() ?? string.Empty), normalizedFileName, StringComparison.OrdinalIgnoreCase))
            {
                product.ImageFileSmall = string.Empty;
                product.ImageFileSmallAlternateText = string.Empty;
                changed = true;
            }

            if (string.Equals(Path.GetFileName(product.ImageFileMedium?.Trim() ?? string.Empty), normalizedFileName, StringComparison.OrdinalIgnoreCase))
            {
                product.ImageFileMedium = string.Empty;
                product.ImageFileMediumAlternateText = string.Empty;
                changed = true;
            }

            if (!changed)
            {
                return product;
            }

            return await hotcakesClient.UpdateProductAsync(product);
        }

        private async Task UploadImagesFromDialogAsync()
        {
            if (imageEditorCurrentProduct is null)
            {
                return;
            }

            using OpenFileDialog dialog = new()
            {
                Title = "Termékképek kiválasztása",
                Filter = "Képfájlok|*.jpg;*.jpeg;*.png;*.webp;*.gif;*.bmp",
                Multiselect = true,
                CheckFileExists = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.FileNames.Length == 0)
            {
                return;
            }

            await UploadImagesAsync(dialog.FileNames);
        }

        private async Task UploadImagesAsync(IReadOnlyList<string> filePaths)
        {
            if (imageEditorCurrentProduct is null || filePaths.Count == 0)
            {
                return;
            }

            try
            {
                UseWaitCursor = true;
                isImporting = true;
                UpdateActionStates();

                HotcakesProduct product = imageEditorCurrentProduct;
                HashSet<string> knownFileNames = await LoadKnownImageFileNamesForProductAsync(product);
                bool hasMainImage = HasMainImage(product);
                int uploadedMainCount = 0;
                int uploadedAdditionalCount = 0;
                int skippedDuplicateCount = 0;
                List<string> failedFiles = [];

                for (int index = 0; index < filePaths.Count; index++)
                {
                    string filePath = filePaths[index];
                    string uploadFileName = Path.GetFileName(filePath);

                    if (string.IsNullOrWhiteSpace(uploadFileName))
                    {
                        continue;
                    }

                    SetStatusMessage($"Kép szerkesztő: feltöltés folyamatban... ({index + 1}/{filePaths.Count})");

                    if (knownFileNames.Contains(uploadFileName))
                    {
                        skippedDuplicateCount++;
                        continue;
                    }

                    try
                    {
                        byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                        string alternateText = string.IsNullOrWhiteSpace(product.ProductName)
                            ? uploadFileName
                            : product.ProductName.Trim();

                        bool uploadAsMainImage = !hasMainImage;
                        bool uploaded = uploadAsMainImage
                            ? await hotcakesClient.UploadProductMainImageAsync(product.Bvin, uploadFileName, fileContent)
                            : await hotcakesClient.UploadProductAdditionalImageAsync(product.Bvin, uploadFileName, fileContent, alternateText, product.StoreId);

                        if (!uploaded)
                        {
                            failedFiles.Add(uploadFileName);
                            continue;
                        }

                        if (uploadAsMainImage)
                        {
                            product = await SaveMainImageMetadataAsync(product, uploadFileName);
                            loadedProductsBySku[product.Sku] = product;
                            uploadedMainCount++;
                            hasMainImage = true;
                        }
                        else
                        {
                            uploadedAdditionalCount++;
                        }

                        knownFileNames.Add(uploadFileName);
                    }
                    catch
                    {
                        failedFiles.Add(uploadFileName);
                    }
                }

                imageEditorCurrentProduct = product;
                await LoadImageEditorImagesAsync(product);

                StringBuilder resultBuilder = new();
                resultBuilder.AppendLine($"Feltöltött képek: {uploadedMainCount + uploadedAdditionalCount}");
                resultBuilder.AppendLine($"Főkép: {uploadedMainCount}");
                resultBuilder.AppendLine($"További kép: {uploadedAdditionalCount}");
                resultBuilder.AppendLine($"Kihagyott duplikátum: {skippedDuplicateCount}");

                if (failedFiles.Count > 0)
                {
                    resultBuilder.AppendLine($"Sikertelen fájlok: {string.Join(", ", failedFiles.Take(5))}");
                }

                SetStatusMessage($"Kép szerkesztő: feltöltés kész. {uploadedMainCount + uploadedAdditionalCount} kép feldolgozva.");

                if (failedFiles.Count == 0)
                {
                    AppDialog.ShowInfo(this, "Kép szerkesztő", resultBuilder.ToString());
                }
                else
                {
                    AppDialog.ShowWarning(this, "Kép szerkesztő", resultBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(
                    this,
                    "Kép szerkesztő",
                    $"A képfeltöltés nem sikerült.{Environment.NewLine}{Environment.NewLine}{ex.Message}");
                SetStatusMessage("A képfeltöltés nem sikerült.", true);
            }
            finally
            {
                isImporting = false;
                UpdateActionStates();
                UseWaitCursor = false;
            }
        }

        private void ClearImageEditorState(bool clearSku)
        {
            if (clearSku)
            {
                imageSkuTextBox.Clear();
                ResetImageSearchResults();
            }

            imageEditorCurrentProduct = null;
            imageEditorSelectedImage = null;
            imageEditorImages.Clear();
            RebuildImageGallery();
            UpdateImageEditorProductSummary();
            UpdateImageEditorSelectionState();
            UpdateActionStates();
        }

        private void ImageSearchClearButton_Click(object? sender, EventArgs e)
        {
            ClearImageEditorState(clearSku: true);
            SetStatusMessage("Kép szerkesztő: a keresőmező törölve.");
        }

        private async void ImageSkuTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            await SearchImageEditorProductsAsync();
        }

        private void ImageSearchResultsGrid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateActionStates();
        }

        private void ImageCardPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel || panel.Tag is not ImageEditorImageItem imageItem)
            {
                return;
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color borderColor = ReferenceEquals(imageEditorSelectedImage, imageItem)
                ? AccentCoralColor
                : BorderColor;

            float borderWidth = ReferenceEquals(imageEditorSelectedImage, imageItem) ? 2F : 1F;
            using Pen borderPen = new(borderColor, borderWidth);
            Rectangle borderBounds = new(0, 0, panel.Width - 1, panel.Height - 1);
            e.Graphics.DrawRectangle(borderPen, borderBounds);
        }

        private bool ShowLocalizedConfirmation(string message, string title)
        {
            return AppDialog.ShowConfirmation(this, title, message);
        }

        private string ResolveMainImageLocation(string productBvin, string fileName)
        {
            string? localPath = TryResolveMainImageFilePath(productBvin, fileName);

            if (!string.IsNullOrWhiteSpace(localPath))
            {
                return localPath;
            }

            return BuildMainImageUrl(productBvin, fileName, "medium");
        }

        private string ResolveAdditionalImageLocation(string productBvin, string fileName, string imageBvin)
        {
            string? localPath = TryResolveAdditionalImageFilePath(productBvin, imageBvin, fileName);

            if (!string.IsNullOrWhiteSpace(localPath))
            {
                return localPath;
            }

            return BuildAdditionalImageUrl(productBvin, imageBvin, fileName, "medium");
        }

        private static string? TryResolveMainImageFilePath(string productBvin, string fileName)
        {
            if (string.IsNullOrWhiteSpace(productBvin) || string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            string normalizedProductBvin = productBvin.Trim();
            string normalizedFileName = Path.GetFileName(fileName.Trim());

            foreach (string candidateRoot in EnumerateProductImageRoots())
            {
                string productRoot = Path.Combine(candidateRoot, normalizedProductBvin);

                if (!Directory.Exists(productRoot))
                {
                    continue;
                }

                foreach (string sizeFolder in MainImageSizeFolders)
                {
                    string candidateDirectory = string.IsNullOrWhiteSpace(sizeFolder)
                        ? productRoot
                        : Path.Combine(productRoot, sizeFolder);

                    string? candidatePath = TryResolveImageFileInDirectory(candidateDirectory, normalizedFileName);

                    if (!string.IsNullOrWhiteSpace(candidatePath))
                    {
                        return candidatePath;
                    }
                }
            }

            return null;
        }

        private static string? TryResolveAdditionalImageFilePath(string productBvin, string imageBvin, string fileName)
        {
            if (string.IsNullOrWhiteSpace(productBvin) || string.IsNullOrWhiteSpace(imageBvin) || string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            string normalizedProductBvin = productBvin.Trim();
            string normalizedImageBvin = imageBvin.Trim();
            string normalizedFileName = Path.GetFileName(fileName.Trim());

            foreach (string candidateRoot in EnumerateProductImageRoots())
            {
                string productRoot = Path.Combine(candidateRoot, normalizedProductBvin, "additional", normalizedImageBvin);

                if (!Directory.Exists(productRoot))
                {
                    continue;
                }

                foreach (string sizeFolder in AdditionalImageSizeFolders)
                {
                    string candidateDirectory = string.IsNullOrWhiteSpace(sizeFolder)
                        ? productRoot
                        : Path.Combine(productRoot, sizeFolder);

                    string? candidatePath = TryResolveImageFileInDirectory(candidateDirectory, normalizedFileName);

                    if (!string.IsNullOrWhiteSpace(candidatePath))
                    {
                        return candidatePath;
                    }
                }
            }

            return null;
        }

        private static string? TryResolveImageFileInDirectory(string directoryPath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || string.IsNullOrWhiteSpace(fileName) || !Directory.Exists(directoryPath))
            {
                return null;
            }

            string exactPath = Path.Combine(directoryPath, fileName);

            if (File.Exists(exactPath))
            {
                return exactPath;
            }

            string normalizedFileName = NormalizeToken(fileName);

            if (string.IsNullOrWhiteSpace(normalizedFileName))
            {
                return null;
            }

            return Directory.EnumerateFiles(directoryPath)
                .FirstOrDefault(candidatePath =>
                    string.Equals(
                        NormalizeToken(Path.GetFileName(candidatePath)),
                        normalizedFileName,
                        StringComparison.Ordinal));
        }

        private static IEnumerable<string> EnumerateProductImageRoots()
        {
            HashSet<string> returnedRoots = new(StringComparer.OrdinalIgnoreCase);

            foreach (string candidateRoot in new[]
            {
                Environment.CurrentDirectory,
                AppContext.BaseDirectory,
                @"C:\inetpub\dnn"
            })
            {
                if (string.IsNullOrWhiteSpace(candidateRoot))
                {
                    continue;
                }

                string portalsRoot = Path.Combine(candidateRoot, "Portals");

                if (!Directory.Exists(portalsRoot))
                {
                    continue;
                }

                foreach (string portalDirectory in Directory.EnumerateDirectories(portalsRoot))
                {
                    string productRoot = Path.Combine(portalDirectory, "Hotcakes", "Data", "products");

                    if (Directory.Exists(productRoot) && returnedRoots.Add(productRoot))
                    {
                        yield return productRoot;
                    }
                }
            }
        }

        private static string BuildMainImageUrl(string productBvin, string fileName, string sizeFolder)
        {
            return BuildProductImageUrl($"Portals/0/Hotcakes/Data/products/{Uri.EscapeDataString(productBvin.Trim())}/{sizeFolder}/{Uri.EscapeDataString(fileName.Trim())}");
        }

        private static string BuildAdditionalImageUrl(string productBvin, string imageBvin, string fileName, string sizeFolder)
        {
            return BuildProductImageUrl($"Portals/0/Hotcakes/Data/products/{Uri.EscapeDataString(productBvin.Trim())}/additional/{Uri.EscapeDataString(imageBvin.Trim())}/{sizeFolder}/{Uri.EscapeDataString(fileName.Trim())}");
        }

        private static string BuildProductImageUrl(string relativePath)
        {
            string baseUrl = AppSettings.Current.Hotcakes.BaseUrl.Trim();

            if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
            {
                baseUrl += "/";
            }

            return new Uri(new Uri(baseUrl, UriKind.Absolute), relativePath).ToString();
        }

        private static string ResolveMainImageFileName(HotcakesProduct product)
        {
            if (!string.IsNullOrWhiteSpace(product.ImageFileMedium))
            {
                return Path.GetFileName(product.ImageFileMedium.Trim());
            }

            if (!string.IsNullOrWhiteSpace(product.ImageFileSmall))
            {
                return Path.GetFileName(product.ImageFileSmall.Trim());
            }

            return string.Empty;
        }

        private static string ResolveMainAlternateText(HotcakesProduct product, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(product.ImageFileMediumAlternateText))
            {
                return product.ImageFileMediumAlternateText.Trim();
            }

            if (!string.IsNullOrWhiteSpace(product.ImageFileSmallAlternateText))
            {
                return product.ImageFileSmallAlternateText.Trim();
            }

            if (!string.IsNullOrWhiteSpace(product.ProductName))
            {
                return product.ProductName.Trim();
            }

            return fileName;
        }

        private sealed class ImageEditorImageItem
        {
            public string FileName { get; set; } = string.Empty;

            public string AlternateText { get; set; } = string.Empty;

            public string ProductImageBvin { get; set; } = string.Empty;

            public int SortOrder { get; set; }

            public bool IsCurrentMain { get; set; }

            public string ImageLocation { get; set; } = string.Empty;

            public Panel? CardPanel { get; set; }
        }

        private sealed class ImageEditorSearchRow
        {
            public HotcakesProduct? Product { get; init; }

            public string Sku { get; init; } = string.Empty;

            public string ProductName { get; init; } = string.Empty;

            public string ProductTypeName { get; init; } = string.Empty;
        }
    }
}
