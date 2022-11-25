using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RhythmThingEditor.RhythmSystem;
//using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;
using RhythmThingEditor.Models;

namespace RhythmThingEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string songFileAbsolutePath = "", saveTo = "";
        public MainWindow()
        {
            if(!EditorSettings.Load())
            {
                System.Windows.MessageBox.Show("Please input first time config.", "Init", MessageBoxButton.OK, MessageBoxImage.Information);
                Window window = new EditorConfigWindow();
                window.Show();
                
                this.Close();
            } else
            {
                InitializeComponent();

            }

        }

        private void btnPickFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog songFile = new OpenFileDialog()
            {
                Filter = "mp3 files|*.mp3",
                Title = "Select Song File"
            };

            songFileAbsolutePath = songFile.ShowDialog() == System.Windows.Forms.DialogResult.OK ? songFile.FileName : songFileAbsolutePath;
            songFileLabel.Text = System.IO.Path.GetFileName(songFileAbsolutePath); // System.IO.Path to distinguish between it and System.Windows.Shapes.Path
        }

        private void btnPickFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderSelection = new FolderBrowserDialog();
            // Using this requires System.Windows.Forms and modifying the csproj file - addition of <UseWindowsForms>true</UseWindowsForms>
            // There is an alternative workaround using OpenFileDialog(), which uses Microsoft.Win32 instead of System.Windows.Forms and that doesn't require modifying the csproj file
            // but it's "hacky" - I've included it in a region below for future reference

            saveTo = folderSelection.ShowDialog() == System.Windows.Forms.DialogResult.OK ? folderSelection.SelectedPath : saveTo;
            saveFolderLabel.Text = saveTo;

            #region Alternative using OpenFileDialog
            //OpenFileDialog folderSelection = new OpenFileDialog()
            //{
            //    ValidateNames = false,
            //    CheckFileExists = false,
            //    CheckPathExists = true,
            //    FileName = "Folder Selection."
            //};

            //saveTo = folderSelection.ShowDialog() == System.Windows.Forms.DialogResult.OK ? System.IO.Path.GetFullPath(folderSelection.FileName) : saveTo;
            //saveFolderLabel.Text = saveTo;
            #endregion
        }

        // This entire region is a workaround to manually recreate WinForms's NumericUpDown, since there isn't any equivalent in WPF afaik
        #region NumericUpDown
        private void NumericOnly(object sender, TextCompositionEventArgs e) => NumericTextBox.NumericOnly(sender, e);
        private void SetTextNumeric(object sender, RoutedEventArgs e) => NumericTextBox.SetTextNumeric(sender);
        private void VerifyNumeric(object sender, RoutedEventArgs e) => NumericTextBox.VerifyNumeric((System.Windows.Controls.TextBox)sender);

        private void btnIncrease_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox BPM = txtBPM, OFFSET = txtOffset;
            switch (((System.Windows.Controls.Button)sender).Tag)
            {
                case "bpm":
                    NumericTextBox.VerifyNumeric(BPM, 1);
                    break;
                case "offset":
                    NumericTextBox.VerifyNumeric(OFFSET, 1);
                    break;
            }
        }

        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox BPM = txtBPM, OFFSET = txtOffset;
            switch (((System.Windows.Controls.Button)sender).Tag)
            {
                case "bpm":
                    NumericTextBox.VerifyNumeric(BPM, -1);
                    break;
                case "offset":
                    NumericTextBox.VerifyNumeric(OFFSET, -1);
                    break;
            }
        }
        #endregion

        private void btnBegin_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(saveTo))
            {
                System.Windows.MessageBox.Show("No save location has been selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } else if(String.IsNullOrEmpty(songFileAbsolutePath))
            {
                System.Windows.MessageBox.Show("No song file has been selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string songFilePath = $@"{saveTo}\song.mp3", chartInfoPath = @$"{saveTo}\ChartInfo.json";

            if (File.Exists(songFilePath))
            {
                System.Windows.MessageBox.Show($"The file {songFilePath} already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (File.Exists(chartInfoPath))
            {
                System.Windows.MessageBox.Show($"The file {chartInfoPath} already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            File.Copy(songFileAbsolutePath, songFilePath);
            JsonChart myJson = new JsonChart(
                "song.mp3",
                float.Parse(txtBPM.Text),
                float.Parse(txtOffset.Text)
            );

            //
            Window window = new MainEditor(chartInfoPath, myJson);
            window.Show();
            this.Close();
        }

        private void btnExisting_Click(object sender, RoutedEventArgs e )
        {
            try
            {
                OpenFileDialog jsonFile = new OpenFileDialog()
                    {
                        Filter = "ChartInfo file|ChartInfo.json",
                        Title = "Select Song File",
                        FileName = "ChartInfo.json"
                    };
                string JsonPath = ""; 
                JsonPath = jsonFile.ShowDialog() == System.Windows.Forms.DialogResult.OK ? jsonFile.FileName : JsonPath;
                string jsonText = "";
                jsonText = File.ReadAllText(JsonPath);
                JsonChart fromJson = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonChart>(jsonText);
                Window editor = new MainEditor(JsonPath, fromJson);
                editor.Show();
                this.Close();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

    }
}
