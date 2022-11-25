using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RhythmThingEditor.RhythmSystem
{
    /// <summary>
    /// Interaction logic for EditorConfig.xaml
    /// </summary>
    public partial class EditorConfigWindow : Window
    {
        public EditorConfigWindow()
        {
            InitializeComponent();
        }
        private static string _gamePath = "";
        private void exeBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderSelection = new FolderBrowserDialog();
            // Using this requires System.Windows.Forms and modifying the csproj file - addition of <UseWindowsForms>true</UseWindowsForms>
            // There is an alternative workaround using OpenFileDialog(), which uses Microsoft.Win32 instead of System.Windows.Forms and that doesn't require modifying the csproj file
            // but it's "hacky" - I've included it in a region below for future reference
            OpenFileDialog exeFile = new OpenFileDialog()
            {
                Filter = "RhythmThing|RhythmThing.exe",
                Title = "Select Game exe"
            };

            _gamePath = exeFile.ShowDialog() == System.Windows.Forms.DialogResult.OK ? exeFile.FileName : _gamePath;
            exeLabel.Text = _gamePath;
            //TODO: ADD CHECKING TO MAKE SURE ITS ACTUALLY THE GAME EXE.
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //verify stuff is in
            if(_gamePath == "")
            {
                System.Windows.MessageBox.Show("Please point to game exe.", "error", MessageBoxButton.OK, MessageBoxImage.Error);

            } else
            {
                EditorConfig editorConfig = new EditorConfig();
                editorConfig.GameInstall = _gamePath;
                EditorSettings.Init(editorConfig);
                this.Close();
            }
        }
    }
}
