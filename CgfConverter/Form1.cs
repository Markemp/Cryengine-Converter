using System.Windows.Forms;
using System.IO;
using System;

namespace CgfConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool path_cheker = false;
        private string input_file;
        private string output_dir;
        private void InputButton_Click(object sender, System.EventArgs e)
        {
            const int min_path_length = 2; //I don't think that path to file can be shorter than 2 chars
            if (input_file_path.Text.Length > min_path_length)
            {
                var input_path = input_file_path.Text;
                System.Text.StringBuilder file_type_detector = new System.Text.StringBuilder(input_path);  //use a string builder for work with file path
                file_type_detector.Remove(0, input_path.Length - 3);   //for txt type need only 3 last symbols !!! THIS IS NOT RIGHT CODE FOR TYPE DETECTING
                if (file_type_detector.ToString() != "cgf" && file_type_detector.ToString() != "cga")
                {
                    MessageBox.Show(input_file_path + "not a Cryengine file.\n Please, choose correct file (for example .cgf)");
                }
                else
                {
                    try //is file exist and can be open
                    {
                        FileStream FS = File.Open(input_path, FileMode.Open);
                        FS.Close();
                        MessageBox.Show(input_path + " was uploaded");
                        if (output_path.Text.Length < min_path_length)
                        {
                            path_cheker = true;
                            input_file = input_file_path.Text;
                            output_dir = output_path.Text;
                        }
                        else
                        {
                            MessageBox.Show("Choose output path!");
                        }
                    }
                    catch (IOException)
                    {
                        MessageBox.Show(input_path + " doesn't exist or not a file!");
                    }

                }

            }
            else
            {
                openFileDialog1.Filter = "Cryengine files (*.cgf)|*.cgf (*.cga)|*.cga"; //user can choose only txt file
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel) //if openFileDialog can't be open
                {
                    System.Console.WriteLine("openFileMesage open error");
                }
                else
                {
                    var filename = openFileDialog1.FileName;
                    
                    try //is file exist and can be open
                    {
                        FileStream FS = File.Open(filename, FileMode.Open);
                        input_file_path.Text = filename;
                        FS.Close();
                        MessageBox.Show(filename + " was uploaded");
                        if (output_path.Text.Length < min_path_length)
                        {
                            path_cheker = true;
                            input_file = filename;
                            output_dir = output_path.Text;
                        }
                        else
                        {
                            MessageBox.Show("Choose output path!");
                        }
                    }
                    catch (IOException)
                    {
                        MessageBox.Show(filename + " doesn't exist!");
                    }
                }
            }
        }

        private void startConvert(string input, string output)
        {
            String[] args = new String[3];
            args[0] = input;
            args[1] = "-objectdir";
            args[2] = input;

            ArgsHandler argsHandler = new ArgsHandler();
            Int32 result = argsHandler.ProcessArgs(args);

            foreach (String inputFile in argsHandler.InputFiles)
            {
                try
                {
                    // Read CryEngine Files
                    CryEngine cryData = new CryEngine(inputFile, argsHandler.DataDir.FullName);

                    if (convert_to_obj.Checked == true)
                    {
                        Wavefront objFile = new Wavefront(argsHandler, cryData);

                        objFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                    }

                    if (convert_to_dae.Checked == true)
                    {
                        COLLADA daeFile = new COLLADA(argsHandler, cryData);

                        daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                    }

                }
                catch (Exception ex)
                {
                    Utils.Log(LogLevelEnum.Critical);
                    Utils.Log(LogLevelEnum.Critical, "********************************************************************************");
                    Utils.Log(LogLevelEnum.Critical, "There was an error rendering {0}", inputFile);
                    Utils.Log(LogLevelEnum.Critical);
                    Utils.Log(LogLevelEnum.Critical, ex.Message);
                    Utils.Log(LogLevelEnum.Critical);
                    Utils.Log(LogLevelEnum.Critical, ex.StackTrace);
                    Utils.Log(LogLevelEnum.Critical, "********************************************************************************");
                    Utils.Log(LogLevelEnum.Critical);
                }
            }
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (path_cheker)
            {
                startConvert(input_file, output_dir);
            }
            else
            {
                MessageBox.Show("Set input and output paths first!");
            }
        }
    }
}
