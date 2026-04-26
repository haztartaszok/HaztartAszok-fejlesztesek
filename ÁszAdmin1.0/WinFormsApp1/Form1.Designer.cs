namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            titleLabel = new Label();
            fileGroupBox = new GroupBox();
            SablonButton = new Button();
            browseButton = new Button();
            filePathTextBox = new TextBox();
            filePathLabel = new Label();
            sheetComboBox = new ComboBox();
            sheetLabel = new Label();
            optionsGroupBox = new GroupBox();
            existingItemModeComboBox = new ComboBox();
            existingItemModeLabel = new Label();
            importTypeComboBox = new ComboBox();
            importTypeLabel = new Label();
            previewGroupBox = new GroupBox();
            previewDataGridView = new DataGridView();
            skuColumn = new DataGridViewTextBoxColumn();
            nameColumn = new DataGridViewTextBoxColumn();
            categoryColumn = new DataGridViewTextBoxColumn();
            priceColumn = new DataGridViewTextBoxColumn();
            stockColumn = new DataGridViewTextBoxColumn();
            stateColumn = new DataGridViewTextBoxColumn();
            bulkTitleLabel = new Label();
            priceGroupBox = new GroupBox();
            priceActionButton = new Button();
            priceValueTextBox = new TextBox();
            priceValueLabel = new Label();
            priceModeComboBox = new ComboBox();
            priceModeLabel = new Label();
            priceCategoryComboBox = new ComboBox();
            priceCategoryLabel = new Label();
            priceDescriptionLabel = new Label();
            statusGroupBox = new GroupBox();
            statusActionButton = new Button();
            affectedProductsValueLabel = new Label();
            affectedProductsLabel = new Label();
            statusValueComboBox = new ComboBox();
            statusValueLabel = new Label();
            statusCategoryComboBox = new ComboBox();
            statusFilterComboBox = new ComboBox();
            statusFilterLabel = new Label();
            statusDescriptionLabel = new Label();
            categoryGroupBox = new GroupBox();
            categoryActionButton = new Button();
            moveCountValueLabel = new Label();
            moveCountLabel = new Label();
            targetCategoryComboBox = new ComboBox();
            targetCategoryLabel = new Label();
            sourceCategoryComboBox = new ComboBox();
            sourceCategoryLabel = new Label();
            categoryDescriptionLabel = new Label();
            deleteGroupBox = new GroupBox();
            label1 = new Label();
            deleteActionButton = new Button();
            confirmDeleteCheckBox = new CheckBox();
            deleteWarningValueLabel = new Label();
            deleteWarningLabel = new Label();
            deleteConditionComboBox = new ComboBox();
            deleteConditionLabel = new Label();
            deleteDescriptionLabel = new Label();
            footerPanel = new Panel();
            historyButton = new Button();
            validateButton = new Button();
            importButton = new Button();
            fileGroupBox.SuspendLayout();
            optionsGroupBox.SuspendLayout();
            previewGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)previewDataGridView).BeginInit();
            priceGroupBox.SuspendLayout();
            statusGroupBox.SuspendLayout();
            categoryGroupBox.SuspendLayout();
            deleteGroupBox.SuspendLayout();
            footerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            titleLabel.Location = new Point(25, 22);
            titleLabel.Margin = new Padding(4, 0, 4, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(109, 38);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Import";
            // 
            // fileGroupBox
            // 
            fileGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            fileGroupBox.Controls.Add(SablonButton);
            fileGroupBox.Controls.Add(browseButton);
            fileGroupBox.Controls.Add(filePathTextBox);
            fileGroupBox.Controls.Add(filePathLabel);
            fileGroupBox.Location = new Point(25, 85);
            fileGroupBox.Margin = new Padding(4);
            fileGroupBox.Name = "fileGroupBox";
            fileGroupBox.Padding = new Padding(4);
            fileGroupBox.Size = new Size(1225, 109);
            fileGroupBox.TabIndex = 1;
            fileGroupBox.TabStop = false;
            fileGroupBox.Text = "Forrásfájl";
            // 
            // SablonButton
            // 
            SablonButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SablonButton.Location = new Point(820, 57);
            SablonButton.Margin = new Padding(4);
            SablonButton.Name = "SablonButton";
            SablonButton.Size = new Size(179, 35);
            SablonButton.TabIndex = 3;
            SablonButton.Text = "Sablon";
            SablonButton.UseVisualStyleBackColor = true;
            // 
            // browseButton
            // 
            browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseButton.Location = new Point(1016, 57);
            browseButton.Margin = new Padding(4);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(179, 38);
            browseButton.TabIndex = 2;
            browseButton.Text = "Tallózás";
            browseButton.UseVisualStyleBackColor = true;
            // 
            // filePathTextBox
            // 
            filePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            filePathTextBox.Location = new Point(8, 61);
            filePathTextBox.Margin = new Padding(4);
            filePathTextBox.Name = "filePathTextBox";
            filePathTextBox.PlaceholderText = "Valassz import fajlt...";
            filePathTextBox.Size = new Size(772, 31);
            filePathTextBox.TabIndex = 1;
            // 
            // filePathLabel
            // 
            filePathLabel.AutoSize = true;
            filePathLabel.Location = new Point(8, 32);
            filePathLabel.Margin = new Padding(4, 0, 4, 0);
            filePathLabel.Name = "filePathLabel";
            filePathLabel.Size = new Size(75, 25);
            filePathLabel.TabIndex = 0;
            filePathLabel.Text = "Fájl útja:";
            // 
            // sheetComboBox
            // 
            sheetComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sheetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sheetComboBox.FormattingEnabled = true;
            sheetComboBox.Items.AddRange(new object[] { "Munkalap 1", "Munkalap 2", "Munkalap 3" });
            sheetComboBox.Location = new Point(973, 25);
            sheetComboBox.Margin = new Padding(4);
            sheetComboBox.Name = "sheetComboBox";
            sheetComboBox.Size = new Size(222, 33);
            sheetComboBox.TabIndex = 4;
            // 
            // sheetLabel
            // 
            sheetLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sheetLabel.AutoSize = true;
            sheetLabel.Location = new Point(867, 28);
            sheetLabel.Margin = new Padding(4, 0, 4, 0);
            sheetLabel.Name = "sheetLabel";
            sheetLabel.Size = new Size(94, 25);
            sheetLabel.TabIndex = 3;
            sheetLabel.Text = "Munkalap:";
            // 
            // optionsGroupBox
            // 
            optionsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            optionsGroupBox.Controls.Add(existingItemModeComboBox);
            optionsGroupBox.Controls.Add(existingItemModeLabel);
            optionsGroupBox.Controls.Add(importTypeComboBox);
            optionsGroupBox.Controls.Add(importTypeLabel);
            optionsGroupBox.Location = new Point(25, 202);
            optionsGroupBox.Margin = new Padding(4);
            optionsGroupBox.Name = "optionsGroupBox";
            optionsGroupBox.Padding = new Padding(4);
            optionsGroupBox.Size = new Size(1225, 115);
            optionsGroupBox.TabIndex = 2;
            optionsGroupBox.TabStop = false;
            optionsGroupBox.Text = "Import beállítások";
            // 
            // existingItemModeComboBox
            // 
            existingItemModeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            existingItemModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            existingItemModeComboBox.FormattingEnabled = true;
            existingItemModeComboBox.Items.AddRange(new object[] { "Frissites SKU alapjan", "Kihagyas" });
            existingItemModeComboBox.Location = new Point(548, 55);
            existingItemModeComboBox.Margin = new Padding(4);
            existingItemModeComboBox.Name = "existingItemModeComboBox";
            existingItemModeComboBox.Size = new Size(646, 33);
            existingItemModeComboBox.TabIndex = 3;
            // 
            // existingItemModeLabel
            // 
            existingItemModeLabel.AutoSize = true;
            existingItemModeLabel.Location = new Point(548, 26);
            existingItemModeLabel.Margin = new Padding(4, 0, 4, 0);
            existingItemModeLabel.Name = "existingItemModeLabel";
            existingItemModeLabel.Size = new Size(212, 25);
            existingItemModeLabel.TabIndex = 2;
            existingItemModeLabel.Text = "Meglévő elemek kezelése";
            // 
            // importTypeComboBox
            // 
            importTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            importTypeComboBox.FormattingEnabled = true;
            importTypeComboBox.Items.AddRange(new object[] { "Termek import", "Kep import", "Kategoria import", "Tulajdonsag import", "Osszes importalasa" });
            importTypeComboBox.Location = new Point(30, 55);
            importTypeComboBox.Margin = new Padding(4);
            importTypeComboBox.Name = "importTypeComboBox";
            importTypeComboBox.Size = new Size(449, 33);
            importTypeComboBox.TabIndex = 1;
            // 
            // importTypeLabel
            // 
            importTypeLabel.AutoSize = true;
            importTypeLabel.Location = new Point(30, 26);
            importTypeLabel.Margin = new Padding(4, 0, 4, 0);
            importTypeLabel.Name = "importTypeLabel";
            importTypeLabel.Size = new Size(124, 25);
            importTypeLabel.TabIndex = 0;
            importTypeLabel.Text = "Import tipusa:";
            // 
            // previewGroupBox
            // 
            previewGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            previewGroupBox.Controls.Add(sheetComboBox);
            previewGroupBox.Controls.Add(sheetLabel);
            previewGroupBox.Controls.Add(previewDataGridView);
            previewGroupBox.Location = new Point(25, 325);
            previewGroupBox.Margin = new Padding(4);
            previewGroupBox.Name = "previewGroupBox";
            previewGroupBox.Padding = new Padding(4);
            previewGroupBox.Size = new Size(1225, 430);
            previewGroupBox.TabIndex = 3;
            previewGroupBox.TabStop = false;
            previewGroupBox.Text = "Előnézet";
            // 
            // previewDataGridView
            // 
            previewDataGridView.AllowUserToAddRows = false;
            previewDataGridView.AllowUserToDeleteRows = false;
            previewDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            previewDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            previewDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            previewDataGridView.Columns.AddRange(new DataGridViewColumn[] { skuColumn, nameColumn, categoryColumn, priceColumn, stockColumn, stateColumn });
            previewDataGridView.Location = new Point(30, 84);
            previewDataGridView.Margin = new Padding(4);
            previewDataGridView.Name = "previewDataGridView";
            previewDataGridView.ReadOnly = true;
            previewDataGridView.RowHeadersVisible = false;
            previewDataGridView.RowHeadersWidth = 51;
            previewDataGridView.Size = new Size(1165, 321);
            previewDataGridView.TabIndex = 0;
            // 
            // skuColumn
            // 
            skuColumn.HeaderText = "SKU";
            skuColumn.MinimumWidth = 6;
            skuColumn.Name = "skuColumn";
            skuColumn.ReadOnly = true;
            // 
            // nameColumn
            // 
            nameColumn.HeaderText = "Nev";
            nameColumn.MinimumWidth = 6;
            nameColumn.Name = "nameColumn";
            nameColumn.ReadOnly = true;
            // 
            // categoryColumn
            // 
            categoryColumn.HeaderText = "Kategoria";
            categoryColumn.MinimumWidth = 6;
            categoryColumn.Name = "categoryColumn";
            categoryColumn.ReadOnly = true;
            // 
            // priceColumn
            // 
            priceColumn.HeaderText = "Ar";
            priceColumn.MinimumWidth = 6;
            priceColumn.Name = "priceColumn";
            priceColumn.ReadOnly = true;
            // 
            // stockColumn
            // 
            stockColumn.HeaderText = "Keszlet";
            stockColumn.MinimumWidth = 6;
            stockColumn.Name = "stockColumn";
            stockColumn.ReadOnly = true;
            // 
            // stateColumn
            // 
            stateColumn.HeaderText = "Statusz";
            stateColumn.MinimumWidth = 6;
            stateColumn.Name = "stateColumn";
            stateColumn.ReadOnly = true;
            // 
            // bulkTitleLabel
            // 
            bulkTitleLabel.AutoSize = true;
            bulkTitleLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            bulkTitleLabel.Location = new Point(25, 759);
            bulkTitleLabel.Margin = new Padding(4, 0, 4, 0);
            bulkTitleLabel.Name = "bulkTitleLabel";
            bulkTitleLabel.Size = new Size(240, 32);
            bulkTitleLabel.TabIndex = 4;
            bulkTitleLabel.Text = "Tömeges műveletek";
            // 
            // priceGroupBox
            // 
            priceGroupBox.Controls.Add(priceActionButton);
            priceGroupBox.Controls.Add(priceValueTextBox);
            priceGroupBox.Controls.Add(priceValueLabel);
            priceGroupBox.Controls.Add(priceModeComboBox);
            priceGroupBox.Controls.Add(priceModeLabel);
            priceGroupBox.Controls.Add(priceCategoryComboBox);
            priceGroupBox.Controls.Add(priceCategoryLabel);
            priceGroupBox.Controls.Add(priceDescriptionLabel);
            priceGroupBox.Location = new Point(25, 810);
            priceGroupBox.Margin = new Padding(4);
            priceGroupBox.Name = "priceGroupBox";
            priceGroupBox.Padding = new Padding(4);
            priceGroupBox.Size = new Size(655, 380);
            priceGroupBox.TabIndex = 5;
            priceGroupBox.TabStop = false;
            priceGroupBox.Text = "Tömeges árfrissités";
            // 
            // priceActionButton
            // 
            priceActionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            priceActionButton.Location = new Point(30, 323);
            priceActionButton.Margin = new Padding(4);
            priceActionButton.Name = "priceActionButton";
            priceActionButton.Size = new Size(595, 42);
            priceActionButton.TabIndex = 7;
            priceActionButton.Text = "Frissités végrehajtása";
            priceActionButton.UseVisualStyleBackColor = true;
            // 
            // priceValueTextBox
            // 
            priceValueTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            priceValueTextBox.Location = new Point(30, 257);
            priceValueTextBox.Margin = new Padding(4);
            priceValueTextBox.Name = "priceValueTextBox";
            priceValueTextBox.PlaceholderText = "+10 vagy -15";
            priceValueTextBox.Size = new Size(594, 31);
            priceValueTextBox.TabIndex = 6;
            // 
            // priceValueLabel
            // 
            priceValueLabel.AutoSize = true;
            priceValueLabel.Location = new Point(30, 229);
            priceValueLabel.Margin = new Padding(4, 0, 4, 0);
            priceValueLabel.Name = "priceValueLabel";
            priceValueLabel.Size = new Size(51, 25);
            priceValueLabel.TabIndex = 5;
            priceValueLabel.Text = "Érték";
            // 
            // priceModeComboBox
            // 
            priceModeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            priceModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            priceModeComboBox.FormattingEnabled = true;
            priceModeComboBox.Items.AddRange(new object[] { "Szazalekos modositas", "Fix osszeg hozzaadasa", "Uj ar beallitasa" });
            priceModeComboBox.Location = new Point(30, 181);
            priceModeComboBox.Margin = new Padding(4);
            priceModeComboBox.Name = "priceModeComboBox";
            priceModeComboBox.Size = new Size(594, 33);
            priceModeComboBox.TabIndex = 4;
            // 
            // priceModeLabel
            // 
            priceModeLabel.AutoSize = true;
            priceModeLabel.Location = new Point(30, 153);
            priceModeLabel.Margin = new Padding(4, 0, 4, 0);
            priceModeLabel.Name = "priceModeLabel";
            priceModeLabel.Size = new Size(149, 25);
            priceModeLabel.TabIndex = 3;
            priceModeLabel.Text = "Módosítás tipusa";
            // 
            // priceCategoryComboBox
            // 
            priceCategoryComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            priceCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            priceCategoryComboBox.FormattingEnabled = true;
            priceCategoryComboBox.Items.AddRange(new object[] { "Osszes kategoria", "Elektronika", "Ruhazat", "Cipok" });
            priceCategoryComboBox.Location = new Point(30, 105);
            priceCategoryComboBox.Margin = new Padding(4);
            priceCategoryComboBox.Name = "priceCategoryComboBox";
            priceCategoryComboBox.Size = new Size(594, 33);
            priceCategoryComboBox.TabIndex = 2;
            // 
            // priceCategoryLabel
            // 
            priceCategoryLabel.AutoSize = true;
            priceCategoryLabel.Location = new Point(30, 77);
            priceCategoryLabel.Margin = new Padding(4, 0, 4, 0);
            priceCategoryLabel.Name = "priceCategoryLabel";
            priceCategoryLabel.Size = new Size(184, 25);
            priceCategoryLabel.TabIndex = 1;
            priceCategoryLabel.Text = "Kategória kiválasztása";
            // 
            // priceDescriptionLabel
            // 
            priceDescriptionLabel.AutoSize = true;
            priceDescriptionLabel.Location = new Point(30, 42);
            priceDescriptionLabel.Margin = new Padding(4, 0, 4, 0);
            priceDescriptionLabel.Name = "priceDescriptionLabel";
            priceDescriptionLabel.Size = new Size(383, 25);
            priceDescriptionLabel.TabIndex = 0;
            priceDescriptionLabel.Text = "Árak modosítása kategória vagy szűrés alapján";
            // 
            // statusGroupBox
            // 
            statusGroupBox.Controls.Add(statusActionButton);
            statusGroupBox.Controls.Add(affectedProductsValueLabel);
            statusGroupBox.Controls.Add(affectedProductsLabel);
            statusGroupBox.Controls.Add(statusValueComboBox);
            statusGroupBox.Controls.Add(statusValueLabel);
            statusGroupBox.Controls.Add(statusCategoryComboBox);
            statusGroupBox.Controls.Add(statusFilterComboBox);
            statusGroupBox.Controls.Add(statusFilterLabel);
            statusGroupBox.Controls.Add(statusDescriptionLabel);
            statusGroupBox.Location = new Point(695, 810);
            statusGroupBox.Margin = new Padding(4);
            statusGroupBox.Name = "statusGroupBox";
            statusGroupBox.Padding = new Padding(4);
            statusGroupBox.Size = new Size(655, 380);
            statusGroupBox.TabIndex = 6;
            statusGroupBox.TabStop = false;
            statusGroupBox.Text = "Aktiválás / Inaktiválás";
            // 
            // statusActionButton
            // 
            statusActionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            statusActionButton.Location = new Point(30, 323);
            statusActionButton.Margin = new Padding(4);
            statusActionButton.Name = "statusActionButton";
            statusActionButton.Size = new Size(595, 42);
            statusActionButton.TabIndex = 7;
            statusActionButton.Text = "Státusz módosítása";
            statusActionButton.UseVisualStyleBackColor = true;
            // 
            // affectedProductsValueLabel
            // 
            affectedProductsValueLabel.AutoSize = true;
            affectedProductsValueLabel.Location = new Point(208, 263);
            affectedProductsValueLabel.Margin = new Padding(4, 0, 4, 0);
            affectedProductsValueLabel.Name = "affectedProductsValueLabel";
            affectedProductsValueLabel.Size = new Size(22, 25);
            affectedProductsValueLabel.TabIndex = 6;
            affectedProductsValueLabel.Text = "0";
            // 
            // affectedProductsLabel
            // 
            affectedProductsLabel.AutoSize = true;
            affectedProductsLabel.Location = new Point(30, 263);
            affectedProductsLabel.Margin = new Padding(4, 0, 4, 0);
            affectedProductsLabel.Name = "affectedProductsLabel";
            affectedProductsLabel.Size = new Size(150, 25);
            affectedProductsLabel.TabIndex = 5;
            affectedProductsLabel.Text = "Érintett termékek:";
            // 
            // statusValueComboBox
            // 
            statusValueComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            statusValueComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            statusValueComboBox.FormattingEnabled = true;
            statusValueComboBox.Items.AddRange(new object[] { "Aktiv", "Inaktiv" });
            statusValueComboBox.Location = new Point(30, 211);
            statusValueComboBox.Margin = new Padding(4);
            statusValueComboBox.Name = "statusValueComboBox";
            statusValueComboBox.Size = new Size(594, 33);
            statusValueComboBox.TabIndex = 4;
            // 
            // statusValueLabel
            // 
            statusValueLabel.AutoSize = true;
            statusValueLabel.Location = new Point(30, 183);
            statusValueLabel.Margin = new Padding(4, 0, 4, 0);
            statusValueLabel.Name = "statusValueLabel";
            statusValueLabel.Size = new Size(88, 25);
            statusValueLabel.TabIndex = 3;
            statusValueLabel.Text = "Új státusz";
            // 
            // statusCategoryComboBox
            // 
            statusCategoryComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            statusCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            statusCategoryComboBox.FormattingEnabled = true;
            statusCategoryComboBox.Location = new Point(304, 135);
            statusCategoryComboBox.Margin = new Padding(4);
            statusCategoryComboBox.Name = "statusCategoryComboBox";
            statusCategoryComboBox.Size = new Size(320, 33);
            statusCategoryComboBox.TabIndex = 8;
            statusCategoryComboBox.Visible = false;
            // 
            // statusFilterComboBox
            // 
            statusFilterComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            statusFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            statusFilterComboBox.FormattingEnabled = true;
            statusFilterComboBox.Items.AddRange(new object[] { "Adott kategoria", "Osszes termek", "Nincs raktaron" });
            statusFilterComboBox.Location = new Point(30, 135);
            statusFilterComboBox.Margin = new Padding(4);
            statusFilterComboBox.Name = "statusFilterComboBox";
            statusFilterComboBox.Size = new Size(594, 33);
            statusFilterComboBox.TabIndex = 2;
            // 
            // statusFilterLabel
            // 
            statusFilterLabel.AutoSize = true;
            statusFilterLabel.Location = new Point(30, 107);
            statusFilterLabel.Margin = new Padding(4, 0, 4, 0);
            statusFilterLabel.Name = "statusFilterLabel";
            statusFilterLabel.Size = new Size(62, 25);
            statusFilterLabel.TabIndex = 1;
            statusFilterLabel.Text = "Szűrés";
            // 
            // statusDescriptionLabel
            // 
            statusDescriptionLabel.AutoSize = true;
            statusDescriptionLabel.Location = new Point(30, 42);
            statusDescriptionLabel.Margin = new Padding(4, 0, 4, 0);
            statusDescriptionLabel.Name = "statusDescriptionLabel";
            statusDescriptionLabel.Size = new Size(356, 25);
            statusDescriptionLabel.TabIndex = 0;
            statusDescriptionLabel.Text = "Termékek státuszának tömeges módosítása";
            // 
            // categoryGroupBox
            // 
            categoryGroupBox.Controls.Add(categoryActionButton);
            categoryGroupBox.Controls.Add(moveCountValueLabel);
            categoryGroupBox.Controls.Add(moveCountLabel);
            categoryGroupBox.Controls.Add(targetCategoryComboBox);
            categoryGroupBox.Controls.Add(targetCategoryLabel);
            categoryGroupBox.Controls.Add(sourceCategoryComboBox);
            categoryGroupBox.Controls.Add(sourceCategoryLabel);
            categoryGroupBox.Controls.Add(categoryDescriptionLabel);
            categoryGroupBox.Location = new Point(25, 1205);
            categoryGroupBox.Margin = new Padding(4);
            categoryGroupBox.Name = "categoryGroupBox";
            categoryGroupBox.Padding = new Padding(4);
            categoryGroupBox.Size = new Size(655, 380);
            categoryGroupBox.TabIndex = 7;
            categoryGroupBox.TabStop = false;
            categoryGroupBox.Text = "Kategoria hozzarendeles";
            // 
            // categoryActionButton
            // 
            categoryActionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            categoryActionButton.Location = new Point(30, 323);
            categoryActionButton.Margin = new Padding(4);
            categoryActionButton.Name = "categoryActionButton";
            categoryActionButton.Size = new Size(595, 42);
            categoryActionButton.TabIndex = 7;
            categoryActionButton.Text = "Áthelyezés";
            categoryActionButton.UseVisualStyleBackColor = true;
            // 
            // moveCountValueLabel
            // 
            moveCountValueLabel.AutoSize = true;
            moveCountValueLabel.Location = new Point(164, 263);
            moveCountValueLabel.Margin = new Padding(4, 0, 4, 0);
            moveCountValueLabel.Name = "moveCountValueLabel";
            moveCountValueLabel.Size = new Size(22, 25);
            moveCountValueLabel.TabIndex = 6;
            moveCountValueLabel.Text = "0";
            // 
            // moveCountLabel
            // 
            moveCountLabel.AutoSize = true;
            moveCountLabel.Location = new Point(30, 263);
            moveCountLabel.Margin = new Padding(4, 0, 4, 0);
            moveCountLabel.Name = "moveCountLabel";
            moveCountLabel.Size = new Size(120, 25);
            moveCountLabel.TabIndex = 5;
            moveCountLabel.Text = "Áthelyezendő";
            // 
            // targetCategoryComboBox
            // 
            targetCategoryComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            targetCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            targetCategoryComboBox.FormattingEnabled = true;
            targetCategoryComboBox.Items.AddRange(new object[] { "Valasszon...", "Elektronika", "Ruhazat", "Cipok" });
            targetCategoryComboBox.Location = new Point(30, 211);
            targetCategoryComboBox.Margin = new Padding(4);
            targetCategoryComboBox.Name = "targetCategoryComboBox";
            targetCategoryComboBox.Size = new Size(594, 33);
            targetCategoryComboBox.TabIndex = 4;
            // 
            // targetCategoryLabel
            // 
            targetCategoryLabel.AutoSize = true;
            targetCategoryLabel.Location = new Point(30, 183);
            targetCategoryLabel.Margin = new Padding(4, 0, 4, 0);
            targetCategoryLabel.Name = "targetCategoryLabel";
            targetCategoryLabel.Size = new Size(115, 25);
            targetCategoryLabel.TabIndex = 3;
            targetCategoryLabel.Text = "Cél kategória";
            // 
            // sourceCategoryComboBox
            // 
            sourceCategoryComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            sourceCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sourceCategoryComboBox.FormattingEnabled = true;
            sourceCategoryComboBox.Items.AddRange(new object[] { "Elektronika", "Ruhazat", "Cipok" });
            sourceCategoryComboBox.Location = new Point(30, 135);
            sourceCategoryComboBox.Margin = new Padding(4);
            sourceCategoryComboBox.Name = "sourceCategoryComboBox";
            sourceCategoryComboBox.Size = new Size(594, 33);
            sourceCategoryComboBox.TabIndex = 2;
            // 
            // sourceCategoryLabel
            // 
            sourceCategoryLabel.AutoSize = true;
            sourceCategoryLabel.Location = new Point(30, 107);
            sourceCategoryLabel.Margin = new Padding(4, 0, 4, 0);
            sourceCategoryLabel.Name = "sourceCategoryLabel";
            sourceCategoryLabel.Size = new Size(140, 25);
            sourceCategoryLabel.TabIndex = 1;
            sourceCategoryLabel.Text = "Forrás kategória";
            // 
            // categoryDescriptionLabel
            // 
            categoryDescriptionLabel.AutoSize = true;
            categoryDescriptionLabel.Location = new Point(30, 42);
            categoryDescriptionLabel.Margin = new Padding(4, 0, 4, 0);
            categoryDescriptionLabel.Name = "categoryDescriptionLabel";
            categoryDescriptionLabel.Size = new Size(400, 25);
            categoryDescriptionLabel.TabIndex = 0;
            categoryDescriptionLabel.Text = "Termékek tömeges áthelyezése kategóriák között";
            // 
            // deleteGroupBox
            // 
            deleteGroupBox.Controls.Add(label1);
            deleteGroupBox.Controls.Add(deleteActionButton);
            deleteGroupBox.Controls.Add(confirmDeleteCheckBox);
            deleteGroupBox.Controls.Add(deleteWarningValueLabel);
            deleteGroupBox.Controls.Add(deleteWarningLabel);
            deleteGroupBox.Controls.Add(deleteConditionComboBox);
            deleteGroupBox.Controls.Add(deleteConditionLabel);
            deleteGroupBox.Controls.Add(deleteDescriptionLabel);
            deleteGroupBox.Location = new Point(695, 1205);
            deleteGroupBox.Margin = new Padding(4);
            deleteGroupBox.Name = "deleteGroupBox";
            deleteGroupBox.Padding = new Padding(4);
            deleteGroupBox.Size = new Size(655, 380);
            deleteGroupBox.TabIndex = 8;
            deleteGroupBox.TabStop = false;
            deleteGroupBox.Text = "Tömeges törlés";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(183, 237);
            label1.Name = "label1";
            label1.Size = new Size(59, 25);
            label1.TabIndex = 7;
            label1.Text = "label1";
            // 
            // deleteActionButton
            // 
            deleteActionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            deleteActionButton.Location = new Point(30, 323);
            deleteActionButton.Margin = new Padding(4);
            deleteActionButton.Name = "deleteActionButton";
            deleteActionButton.Size = new Size(595, 42);
            deleteActionButton.TabIndex = 6;
            deleteActionButton.Text = "Törlés végrehajtása";
            deleteActionButton.UseVisualStyleBackColor = true;
            // 
            // confirmDeleteCheckBox
            // 
            confirmDeleteCheckBox.AutoSize = true;
            confirmDeleteCheckBox.Location = new Point(30, 276);
            confirmDeleteCheckBox.Margin = new Padding(4);
            confirmDeleteCheckBox.Name = "confirmDeleteCheckBox";
            confirmDeleteCheckBox.Size = new Size(212, 29);
            confirmDeleteCheckBox.TabIndex = 5;
            confirmDeleteCheckBox.Text = "Megerősítem a törlést";
            confirmDeleteCheckBox.UseVisualStyleBackColor = true;
            // 
            // deleteWarningValueLabel
            // 
            deleteWarningValueLabel.AutoSize = true;
            deleteWarningValueLabel.Location = new Point(30, 237);
            deleteWarningValueLabel.Margin = new Padding(4, 0, 4, 0);
            deleteWarningValueLabel.Name = "deleteWarningValueLabel";
            deleteWarningValueLabel.Size = new Size(145, 25);
            deleteWarningValueLabel.TabIndex = 4;
            deleteWarningValueLabel.Text = "Törlendő termék:";
            // 
            // deleteWarningLabel
            // 
            deleteWarningLabel.AutoSize = true;
            deleteWarningLabel.Location = new Point(30, 197);
            deleteWarningLabel.Margin = new Padding(4, 0, 4, 0);
            deleteWarningLabel.Name = "deleteWarningLabel";
            deleteWarningLabel.Size = new Size(349, 25);
            deleteWarningLabel.TabIndex = 3;
            deleteWarningLabel.Text = "Figyelem! Ez a művelet nem visszavonható";
            // 
            // deleteConditionComboBox
            // 
            deleteConditionComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            deleteConditionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            deleteConditionComboBox.FormattingEnabled = true;
            deleteConditionComboBox.Items.AddRange(new object[] { "Inaktiv termekek", "Nincs raktaron", "90+ napja nem frissitett" });
            deleteConditionComboBox.Location = new Point(30, 135);
            deleteConditionComboBox.Margin = new Padding(4);
            deleteConditionComboBox.Name = "deleteConditionComboBox";
            deleteConditionComboBox.Size = new Size(594, 33);
            deleteConditionComboBox.TabIndex = 2;
            // 
            // deleteConditionLabel
            // 
            deleteConditionLabel.AutoSize = true;
            deleteConditionLabel.Location = new Point(30, 107);
            deleteConditionLabel.Margin = new Padding(4, 0, 4, 0);
            deleteConditionLabel.Name = "deleteConditionLabel";
            deleteConditionLabel.Size = new Size(119, 25);
            deleteConditionLabel.TabIndex = 1;
            deleteConditionLabel.Text = "Törlési feltétel";
            // 
            // deleteDescriptionLabel
            // 
            deleteDescriptionLabel.AutoSize = true;
            deleteDescriptionLabel.Location = new Point(30, 42);
            deleteDescriptionLabel.Margin = new Padding(4, 0, 4, 0);
            deleteDescriptionLabel.Name = "deleteDescriptionLabel";
            deleteDescriptionLabel.Size = new Size(366, 25);
            deleteDescriptionLabel.TabIndex = 0;
            deleteDescriptionLabel.Text = "Termékek tömeges eltávolítása a rendszerből";
            // 
            // footerPanel
            // 
            footerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            footerPanel.Controls.Add(historyButton);
            footerPanel.Controls.Add(validateButton);
            footerPanel.Controls.Add(importButton);
            footerPanel.Location = new Point(25, 1585);
            footerPanel.Margin = new Padding(4);
            footerPanel.Name = "footerPanel";
            footerPanel.Size = new Size(1225, 55);
            footerPanel.TabIndex = 9;
            // 
            // historyButton
            // 
            historyButton.Location = new Point(0, 5);
            historyButton.Margin = new Padding(4);
            historyButton.Name = "historyButton";
            historyButton.Size = new Size(200, 42);
            historyButton.TabIndex = 0;
            historyButton.Text = "Import elozmenyek";
            historyButton.UseVisualStyleBackColor = true;
            // 
            // validateButton
            // 
            validateButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            validateButton.Location = new Point(828, 5);
            validateButton.Margin = new Padding(4);
            validateButton.Name = "validateButton";
            validateButton.Size = new Size(180, 42);
            validateButton.TabIndex = 1;
            validateButton.Text = "Ellenorzes";
            validateButton.UseVisualStyleBackColor = true;
            // 
            // importButton
            // 
            importButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            importButton.Location = new Point(1015, 5);
            importButton.Margin = new Padding(4);
            importButton.Name = "importButton";
            importButton.Size = new Size(180, 42);
            importButton.TabIndex = 2;
            importButton.Text = "Import inditasa";
            importButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1375, 1125);
            Controls.Add(footerPanel);
            Controls.Add(deleteGroupBox);
            Controls.Add(categoryGroupBox);
            Controls.Add(statusGroupBox);
            Controls.Add(priceGroupBox);
            Controls.Add(bulkTitleLabel);
            Controls.Add(previewGroupBox);
            Controls.Add(optionsGroupBox);
            Controls.Add(fileGroupBox);
            Controls.Add(titleLabel);
            Margin = new Padding(4);
            MinimumSize = new Size(840, 820);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AszAdmin 1.0";
            fileGroupBox.ResumeLayout(false);
            fileGroupBox.PerformLayout();
            optionsGroupBox.ResumeLayout(false);
            optionsGroupBox.PerformLayout();
            previewGroupBox.ResumeLayout(false);
            previewGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)previewDataGridView).EndInit();
            priceGroupBox.ResumeLayout(false);
            priceGroupBox.PerformLayout();
            statusGroupBox.ResumeLayout(false);
            statusGroupBox.PerformLayout();
            categoryGroupBox.ResumeLayout(false);
            categoryGroupBox.PerformLayout();
            deleteGroupBox.ResumeLayout(false);
            deleteGroupBox.PerformLayout();
            footerPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private GroupBox fileGroupBox;
        private ComboBox sheetComboBox;
        private Label sheetLabel;
        private Button browseButton;
        private TextBox filePathTextBox;
        private Label filePathLabel;
        private GroupBox optionsGroupBox;
        private ComboBox existingItemModeComboBox;
        private Label existingItemModeLabel;
        private ComboBox importTypeComboBox;
        private Label importTypeLabel;
        private GroupBox previewGroupBox;
        private DataGridView previewDataGridView;
        private DataGridViewTextBoxColumn skuColumn;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn categoryColumn;
        private DataGridViewTextBoxColumn priceColumn;
        private DataGridViewTextBoxColumn stockColumn;
        private DataGridViewTextBoxColumn stateColumn;
        private Label bulkTitleLabel;
        private GroupBox priceGroupBox;
        private Button priceActionButton;
        private TextBox priceValueTextBox;
        private Label priceValueLabel;
        private ComboBox priceModeComboBox;
        private Label priceModeLabel;
        private ComboBox priceCategoryComboBox;
        private Label priceCategoryLabel;
        private Label priceDescriptionLabel;
        private GroupBox statusGroupBox;
        private Button statusActionButton;
        private Label affectedProductsValueLabel;
        private Label affectedProductsLabel;
        private ComboBox statusValueComboBox;
        private Label statusValueLabel;
        private ComboBox statusCategoryComboBox;
        private ComboBox statusFilterComboBox;
        private Label statusFilterLabel;
        private Label statusDescriptionLabel;
        private GroupBox categoryGroupBox;
        private Button categoryActionButton;
        private Label moveCountValueLabel;
        private Label moveCountLabel;
        private ComboBox targetCategoryComboBox;
        private Label targetCategoryLabel;
        private ComboBox sourceCategoryComboBox;
        private Label sourceCategoryLabel;
        private Label categoryDescriptionLabel;
        private GroupBox deleteGroupBox;
        private Button deleteActionButton;
        private CheckBox confirmDeleteCheckBox;
        private Label deleteWarningValueLabel;
        private Label deleteWarningLabel;
        private ComboBox deleteConditionComboBox;
        private Label deleteConditionLabel;
        private Label deleteDescriptionLabel;
        private Panel footerPanel;
        private Button historyButton;
        private Button validateButton;
        private Button importButton;
        private Label label1;
        private Button SablonButton;
    }
}
