using System.Windows.Forms;

namespace UPG_SP_2024
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int scenario = 0;
            string? file = null;
            MainForm mainForm = new MainForm();


            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (int.TryParse(arg, out int parsed))
                    {
                        scenario = parsed;
                    }

                    else if (File.Exists(arg))
                    {
                        file = arg;
                    }

                    else if (arg.StartsWith("-g"))
                    {
                        var gridVal = args[1].Substring(2);
                        string[] dim = gridVal.Split("x");

                        if (dim.Length == 2 && int.TryParse(dim[0], out int gridX) && int.TryParse(dim[1], out int gridY))
                        {
                            MainForm.GridSpacingX = gridX;
                            MainForm.GridSpacingY = gridY;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid format (or if you inserted file path, it does not exist), executing with scenario 0");
                    }
                }
            }

            if(file != null)
            {
                mainForm.RetrieveFileScenario(file);
            }
            else
            {
                mainForm.RetrieveScenario(scenario);
            }

            mainForm.RefreshChargeList();
            Application.Run(mainForm);
            

        }
    }
}