using System.Windows.Forms;
using System;

namespace CgfConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string input_dir;
        private string output_dir;

        private void InputButton_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
            {
                MessageBox.Show("Input path: " + folderBrowserDialog1.SelectedPath);
                input_file_path.Text = folderBrowserDialog1.SelectedPath;
                input_dir = folderBrowserDialog1.SelectedPath;
                Console.WriteLine(input_dir);
            }

        }
        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(output_dir) && !string.IsNullOrWhiteSpace(input_dir))
            {
                startConvert(input_dir, output_dir);
            }
            else
            {
                MessageBox.Show("Set input and output paths first!");
            }
        }

        private void outputButton_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
            {
                MessageBox.Show("Output path: " + folderBrowserDialog1.SelectedPath);
                output_path.Text = folderBrowserDialog1.SelectedPath;
                output_dir = folderBrowserDialog1.SelectedPath;
                Console.WriteLine(output_dir);
            }
        }

        private void startConvert(string input, string output)
        {
            String[] args = new String[5];
            if (cgf.Checked)
            {
                args[0] = input + @"\*.cgf";    //always convert all files from input dir
            }
            else if (cga.Checked)
            {
                args[0] = input + @"\*.cga";    //always convert all files from input dir
            }
            else if (chr.Checked)
            {
                args[0] = input + @"\*.chr";    //always convert all files from input dir
            }
            else if (skin.Checked)
            {
                args[0] = input + @"\*.skin";    //always convert all files from input dir
            }
            Console.WriteLine(args[0]);
            args[1] = "-objectdir";
            args[2] = input;    //in most cases .mtl files are near .cgf files
            args[3] = "-out";
            args[4] = output;

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

    }
}
