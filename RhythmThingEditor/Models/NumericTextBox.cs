using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace RhythmThingEditor.Models
{
    static class NumericTextBox 
        // This class is designed to work only in WPF. It includes static functions to functionally recreate the NumericUpDown object from Windows Forms
    {
        public static void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            const int TYPE_INDEX = 2;
            System.Windows.Controls.TextBox myBox = (System.Windows.Controls.TextBox)sender;
            string myText = myBox.Text + e.Text, type = myBox.Tag.ToString().Split(",")[TYPE_INDEX];

            if ((type == "double" && double.TryParse(myText, out _)) || (type == "int" && int.TryParse(myText, out _)))
                return;

            e.Handled = true;
        }
        public static void SetTextNumeric(object sender) => ((System.Windows.Controls.TextBox)sender).Text = ((System.Windows.Controls.TextBox)sender).Tag.ToString().Split(",")[0];

        public static void VerifyNumeric(System.Windows.Controls.TextBox myBox, double changeBy = 0)
        {
            myBox.Text = (double.Parse(myBox.Text) + changeBy).ToString();
            string myText = myBox.Text;
            double MIN = double.Parse(myBox.Tag.ToString().Split(",")[0]),
                   MAX = double.Parse(myBox.Tag.ToString().Split(",")[1]);

            if (double.Parse(myText) < MIN)
                myBox.Text = MIN.ToString();
            else if (double.Parse(myText) > MAX)
                myBox.Text = MAX.ToString();
        }
    }
}
