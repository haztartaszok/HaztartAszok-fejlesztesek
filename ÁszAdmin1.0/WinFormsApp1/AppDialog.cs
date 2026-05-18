using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    internal static class AppDialog
    {
        private static readonly Color SurfaceColor = Color.FromArgb(255, 251, 242);
        private static readonly Color InkColor = Color.FromArgb(52, 71, 78);
        private static readonly Color AccentCoralColor = Color.FromArgb(227, 106, 106);
        private static readonly Color AccentBlueColor = Color.FromArgb(90, 156, 181);
        private static readonly Color BorderColor = Color.FromArgb(218, 200, 170);

        public static void ShowInfo(IWin32Window? owner, string title, string message)
        {
            ShowMessage(owner, title, message, isConfirmation: false);
        }

        public static void ShowWarning(IWin32Window? owner, string title, string message)
        {
            ShowMessage(owner, title, message, isConfirmation: false);
        }

        public static void ShowError(IWin32Window? owner, string title, string message)
        {
            ShowMessage(owner, title, message, isConfirmation: false);
        }

        public static bool ShowConfirmation(IWin32Window? owner, string title, string message)
        {
            return ShowMessage(owner, title, message, isConfirmation: true) == DialogResult.Yes;
        }

        private static DialogResult ShowMessage(IWin32Window? owner, string title, string message, bool isConfirmation)
        {
            using Form dialog = new()
            {
                Text = title,
                StartPosition = owner is null ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                BackColor = SurfaceColor,
                ForeColor = InkColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoScaleMode = AutoScaleMode.None
            };

            if (owner is Form ownerForm && ownerForm.Icon is not null)
            {
                dialog.Icon = ownerForm.Icon;
            }

            const int horizontalPadding = 20;
            const int topPadding = 18;
            const int bottomPadding = 18;
            const int buttonWidth = 120;
            const int buttonHeight = 40;
            const int buttonGap = 12;
            const int maxTextWidth = 390;

            Size textSize = TextRenderer.MeasureText(
                message,
                dialog.Font,
                new Size(maxTextWidth, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.Left);

            int clientWidth = maxTextWidth + (horizontalPadding * 2);
            int buttonsTop = topPadding + textSize.Height + 22;
            int clientHeight = buttonsTop + buttonHeight + bottomPadding;
            dialog.ClientSize = new Size(clientWidth, Math.Max(clientHeight, 170));

            Label messageLabel = new()
            {
                AutoSize = false,
                Left = horizontalPadding,
                Top = topPadding,
                Width = dialog.ClientSize.Width - (horizontalPadding * 2),
                Height = textSize.Height + 6,
                Text = message,
                ForeColor = InkColor
            };

            Button primaryButton = new()
            {
                Text = isConfirmation ? "Igen" : "Rendben",
                DialogResult = isConfirmation ? DialogResult.Yes : DialogResult.OK
            };
            StylePrimaryButton(primaryButton);

            dialog.Controls.Add(messageLabel);
            dialog.Controls.Add(primaryButton);

            if (isConfirmation)
            {
                Button secondaryButton = new()
                {
                    Text = "Nem",
                    DialogResult = DialogResult.No
                };
                StyleSecondaryButton(secondaryButton);

                secondaryButton.SetBounds(
                    dialog.ClientSize.Width - horizontalPadding - buttonWidth,
                    buttonsTop,
                    buttonWidth,
                    buttonHeight);
                primaryButton.SetBounds(
                    secondaryButton.Left - buttonGap - buttonWidth,
                    buttonsTop,
                    buttonWidth,
                    buttonHeight);

                dialog.CancelButton = secondaryButton;
                dialog.Controls.Add(secondaryButton);
            }
            else
            {
                primaryButton.SetBounds(
                    dialog.ClientSize.Width - horizontalPadding - buttonWidth,
                    buttonsTop,
                    buttonWidth,
                    buttonHeight);
            }

            dialog.AcceptButton = primaryButton;

            return owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        }

        private static void StylePrimaryButton(Button button)
        {
            button.BackColor = AccentCoralColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        private static void StyleSecondaryButton(Button button)
        {
            button.BackColor = Color.White;
            button.ForeColor = AccentBlueColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = BorderColor;
            button.FlatAppearance.BorderSize = 1;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }
    }
}
