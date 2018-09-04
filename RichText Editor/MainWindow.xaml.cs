using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Threading;
using System.Windows.Shapes;


namespace WpfTutorialSamples.Rich_text_controls
{
    public partial class EpaperEditor : Window
    {

        public EpaperEditor()
        {
            InitializeComponent();
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 60, 72 };
        }

        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            cmbFontFamily.SelectedItem = temp;
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
            cmbFontSize.Text = temp.ToString();
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Rtf);
                fileStream.Close();
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Save(fileStream, DataFormats.Rtf);
                fileStream.Close();
            }
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            // Hide the caret for display
            Keyboard.ClearFocus();

            RenderTargetBitmap rtb = new RenderTargetBitmap(640, 384, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(rtbEditor);

            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);
            
            Bitmap bmp = new Bitmap(stream);

            BitmapImage bmpImage = new BitmapImage();
            bmpImage.StreamSource = stream;

            try
            {
                bmp.Save("display.bmp");
            }
            catch
            {
                MessageBox.Show("Bitmap speichern nicht möglich");
            }
            
            // read Bitmap and create Blackwhite Byte Array
            int i = 0;  // 0...7 to write a Byte
            int x = 0;  // lines
            int y = 0;  // columns
            int actByte = 0;   // 8 Pixel werden als Byte ins Array geschrieben
            string byteString = "";
            string line = "";
            string dateTime = DateTime.Now.ToString();

            // Output to file
         
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = "Display file (*.c)|*.c|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                StreamWriter cfile = new System.IO.StreamWriter(dlg.FileName);
                StreamWriter txtfile = new System.IO.StreamWriter(dlg.FileName + ".txt");

                txtfile.WriteLine("#" + dateTime + "#");
                cfile.WriteLine("#" + dateTime + "#");

                long size = 0;

                for (y = 0; y < bmp.Height; y++)
                {
                    for (x = 0; x < bmp.Width; x++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);

                        int r = c.R;
                        int g = c.G;
                        int b = c.B;
                        int avg = (r + g + b) / 3;
                        if (avg > 100)
                        {
                            actByte += (int)Math.Pow(2, 7 - i);  // i
                            i++;
                            line += "X";
                        }
                        else
                        {
                            i++;
                            line += "_";
                        }


                        if (i >= 8)
                        {
                            byteString = byteString + String.Format("0x{0:X2}, ", actByte);
                            i = 0;
                            actByte = 0;

                        }
                    }



                    cfile.WriteLine(byteString);
                    size++;
                    txtfile.WriteLine(line);
                    line = "";
                    byteString = "";
                }
                cfile.Close();
                txtfile.Close();

            }
        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem != null)
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
        }

        private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
            }
            catch
            {

            }
        }

        private void TextAlignLeft_Checked(object sender, EventArgs e)
        {
            if (rtbEditor != null)
            {
                FlowDocument myFlowDoc = (FlowDocument)rtbEditor.Document;
                BlockCollection MyBC = myFlowDoc.Blocks;
                var curCaret = rtbEditor.CaretPosition;
                var curBlock = rtbEditor.Document.Blocks.Where(x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                curBlock.TextAlignment = TextAlignment.Left;
            }
        }

        private void TextAlignCenter_Checked(object sender, RoutedEventArgs e)
        {
            if (flowDoc != null)
            {
                FlowDocument myFlowDoc = (FlowDocument)rtbEditor.Document;
                BlockCollection MyBC = myFlowDoc.Blocks;
                var curCaret = rtbEditor.CaretPosition;
                var curBlock = rtbEditor.Document.Blocks.Where(x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                curBlock.TextAlignment = TextAlignment.Center;
            }

        }

        private void TextAlignRight_Checked(object sender, RoutedEventArgs e)
        {
            if (rtbEditor != null)
            {
                FlowDocument myFlowDoc = (FlowDocument)rtbEditor.Document;
                BlockCollection MyBC = myFlowDoc.Blocks;
                var curCaret = rtbEditor.CaretPosition;
                var curBlock = rtbEditor.Document.Blocks.Where(x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                curBlock.TextAlignment = TextAlignment.Right;
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            FlowDocument myFlowDoc = (FlowDocument)rtbEditor.Document;
            topmargin.FontSize += 1;
        }

        private void 
            Down_Click(object sender, RoutedEventArgs e)
        {
            FlowDocument myFlowDoc = (FlowDocument)rtbEditor.Document;
            if (topmargin.FontSize > 1) topmargin.FontSize -= 1;
        }
    }
}
