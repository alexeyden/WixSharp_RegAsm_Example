using System;
using System.Windows.Forms;

namespace RegAsm
{
    class Program
    {
        public static int Main(string[] args)
        {
            // install mode
            if (args.Length == 1)
            {
                var regasm = new Regasm(args[0]);
                try
                {
                    regasm.Install();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Assembly registering failed: {e}");
                    return -2;
                }
            }
            // uninstall mode
            else if (args.Length == 2 && args[0] == "/u")
            {
                var regasm = new Regasm(args[1]);
                try
                {
                    regasm.Uninstall();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Assembly unregistering failed: {e}");
                    return -3;
                }
            }
            else
            {
                MessageBox.Show("Invalid arguments.");
                return -1;
            }

            return 0;
        }
    }
}
