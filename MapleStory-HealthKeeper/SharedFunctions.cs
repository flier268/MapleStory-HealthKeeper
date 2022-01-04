using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using MapleStory_HealthKeeper.Model;
using Microsoft.Win32;

namespace MapleStory_HealthKeeper
{
    public class SharedFunctions
    {
        public static JsonSerializerOptions? StepJsonSerializerOptions { get; set; }

        public static void InitJsonSerializerOptions()
        {
            StepJsonSerializerOptions = new()
            {
                WriteIndented = true,
                IgnoreReadOnlyProperties = true,
                IgnoreReadOnlyFields = true,
                Encoder = JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.BasicLatin, System.Text.Unicode.UnicodeRanges.All)
            };
        }

        public static void TextBox_PreviewKeyDown(TextBox sender, ref System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
            int? scanCode = (int?)e.GetType().GetProperty("ScanCode", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(e);
            if (scanCode is null)
                throw new ArgumentException("This should get scancode");
            bool? isExtendedKey = (bool?)e.GetType().GetProperty("IsExtendedKey", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(e);
            if (isExtendedKey is null)
                throw new ArgumentException("This should get scancode");
            var key = e.Key switch
            {
                Key.System => (Keys)KeyInterop.VirtualKeyFromKey(e.SystemKey),
                Key.ImeProcessed => (Keys)KeyInterop.VirtualKeyFromKey(e.ImeProcessedKey),
                _ => (Keys)KeyInterop.VirtualKeyFromKey(e.Key),
            };
            if (key == Keys.Escape)
            {
                key = Keys.None;
                scanCode = 0;
                isExtendedKey = false;
            }
            BindingExpression bindingExpression = sender.GetBindingExpression(TextBox.TextProperty);
            var p = sender.DataContext.GetType().GetProperty(bindingExpression.ResolvedSourcePropertyName);
            FullKeyInfo fullKeyInfo = new()
            {
                Key = key,
                ScanCode = (int)scanCode,
                IsExtented = (bool)isExtendedKey
            };
            p?.SetValue(sender.DataContext, fullKeyInfo);
        }

        public static void NumberValidationTextBox(object sender, ref TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public static void NumberValidationTextBox_AllowNegativeValue(object sender, ref TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public static bool? LoadJsonFile<T>(string path, [NotNullWhen(true)] out T? output)
        {
            try
            {
                if (File.Exists(path))
                {
                    using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using StreamReader streamReader = new(fileStream, Encoding.UTF8);
                    output = JsonSerializer.Deserialize<T>(streamReader.ReadToEnd(), StepJsonSerializerOptions);
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            output = default;
            return false;
        }

        public static bool LoadJsonFileWithOpenFileDialog<T>([NotNullWhen(true)] out T? output, string filter = "json files (*.json)|*.json")
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = filter;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using FileStream fileStream = new(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using StreamReader streamReader = new(fileStream, Encoding.UTF8);

                    output = JsonSerializer.Deserialize<T>(streamReader.ReadToEnd(), StepJsonSerializerOptions);
                    return output is not null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "出錯了", 0);
                }
            }
            output = default;
            return false;
        }

        public static bool LoadJsonFileWithOpenFileDialog<T>(Window owner, [NotNullWhen(true)] out T? output, string filter = "json files (*.json)|*.json")
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = filter;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using FileStream fileStream = new(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using StreamReader streamReader = new(fileStream, Encoding.UTF8);

                    output = JsonSerializer.Deserialize<T>(streamReader.ReadToEnd(), StepJsonSerializerOptions);
                    return output is not null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, ex.Message, "出錯了", 0);
                }
            }
            output = default;
            return false;
        }

        public static bool SaveJsonFile<T>(T model, string filePath)
        {
            try
            {
                using StreamWriter streamWriter = new(filePath, false, Encoding.UTF8);
                var s = JsonSerializer.Serialize(model, StepJsonSerializerOptions);
                streamWriter.Write(s);
                streamWriter.Flush();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static void SaveJsonFileWithSaveFileDialog<T>(Window owner, T model, string filter = "json files (*.json)|*.json")
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = filter,
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using StreamWriter streamWriter = new(saveFileDialog.FileName, false, Encoding.UTF8);
                    var s = JsonSerializer.Serialize(model, StepJsonSerializerOptions);
                    streamWriter.Write(s);
                    streamWriter.Flush();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, ex.Message, "出錯了", 0);
                }
            }
        }

        public static void SaveJsonFileWithSaveFileDialog<T>(T model, string filter = "json files (*.json)|*.json")
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = filter,
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using StreamWriter streamWriter = new(saveFileDialog.FileName, false, Encoding.UTF8);
                    var s = JsonSerializer.Serialize(model, StepJsonSerializerOptions);
                    streamWriter.Write(s);
                    streamWriter.Flush();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "出錯了", 0);
                }
            }
        }
    }
}