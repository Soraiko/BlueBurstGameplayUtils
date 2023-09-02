using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace BlueBurstGameplayUtils
{
    public partial class Form1 : Form
    {
        SrkSettings settings;
        string[] defaultsettings = new string[]
        {
            "camera_speed=90",
            "microsoft_controller_type=True",
            "controlling_pointer=648A00",
            "camera_yaw_offset=30",
            "offset_to_char_data_pointer=1C",
            "offset_to_char_yaw=60",
            "offset_to_animationstate=B4"
        };

        public Form1()
        {
            InitializeComponent();
            settings = new SrkSettings("settings.ini", defaultsettings, "=");
            microsoftController.Checked = bool.Parse(settings.GetProperty("microsoft_controller_type"));
            cameraSpeed.Value = int.Parse(settings.GetProperty("camera_speed"));
        }

        private void microsoftController_CheckedChanged(object sender, EventArgs e)
        {
            settings.SetProperty("microsoft_controller_type", microsoftController.Checked.ToString());
        }

        private void cameraSpeed_ValueChanged(object sender, EventArgs e)
        {
            settings.SetProperty("camera_speed", cameraSpeed.Value.ToString());
        }

        int gamepadIndex = -1;
        int oldGamepadIndex = -2;

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (gamepadIndex < 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    OpenTK.Input.GamePadState gamepadState = OpenTK.Input.GamePad.GetState(i);
                    if (gamepadState.IsConnected == false)
                        continue;

                    float x = 0;
                    float y = 0;
                    if (microsoftController.Checked)
                    {
                        x = gamepadState.ThumbSticks.Right.X;
                        y = gamepadState.ThumbSticks.Right.Y;
                    }
                    else
                    {
                        x = (gamepadState.Triggers.Left - 0.5f) * 2f;
                        y = -gamepadState.ThumbSticks.Right.X;
                    }

                    double hypo = Math.Pow(x * x + y * y, 0.5);

                    if (hypo > 0.4)
                    {
                        gamepadIndex = i;
                        button1.Text = "Game controller n°" + i + " linked.\nClick to relink...";
                        button1.Enabled = true;
                        break;
                    }
                }
            }

            if (PsobbAccess.SeekPSOBB())
            {

                OpenTK.Input.GamePadState gamepadState = OpenTK.Input.GamePad.GetState(gamepadIndex);

                float rx = 0;
                float ry = 0;
                float lx = 0;
                float ly = 0;
                float lt = 0;
                float rt = 0;

                bool a = false;


                lx = gamepadState.ThumbSticks.Left.X;
                ly = gamepadState.ThumbSticks.Left.Y;

                if (microsoftController.Checked)
                {
                    rx = gamepadState.ThumbSticks.Right.X;
                    ry = gamepadState.ThumbSticks.Right.Y;


                    lt = gamepadState.Triggers.Left;
                    rt = gamepadState.Triggers.Right;

                    a = gamepadState.Buttons.A == OpenTK.Input.ButtonState.Pressed;
                }
                else
                {
                    rx = (gamepadState.Triggers.Left - 0.5f) * 2f;
                    ry = -gamepadState.ThumbSticks.Right.X;

                    lt = (1 - gamepadState.ThumbSticks.Right.Y) / 2f;
                    rt = gamepadState.Triggers.Right;


                    a = gamepadState.Buttons.B == OpenTK.Input.ButtonState.Pressed;
                }
                int controlling_address = PsobbAccess.ReadIntRAM((int)(PsobbAccess.PSOBB.MainModule.BaseAddress + Convert.ToInt32(settings.GetProperty("controlling_pointer"), 16)));

                int camera_yaw_address = controlling_address + Convert.ToInt32(settings.GetProperty("camera_yaw_offset"), 16);
                int camera_yaw_integer = PsobbAccess.ReadIntRAM(camera_yaw_address);
                double angleCamera = IntToDoubleAngle(camera_yaw_integer);

                int char_data_address = PsobbAccess.ReadIntRAM(controlling_address + Convert.ToInt32(settings.GetProperty("offset_to_char_data_pointer"), 16));

                int char_yaw_address = char_data_address + Convert.ToInt32(settings.GetProperty("offset_to_char_yaw"), 16);
                int char_yaw_integer = PsobbAccess.ReadIntRAM(char_yaw_address);
                
                textBox1.Text = char_yaw_integer + "\r\n";




                double hypoGauche = Math.Pow(lx * lx + ly * ly, 0.5);


                double angleJoystick = Math.Atan2(ly / hypoGauche, lx / hypoGauche);

                if (hypoGauche>0.4)
                {
                    double anglePerso = IntToDoubleAngle(char_yaw_integer);

                    anglePerso = angleCamera + Math.PI / 2;
                    anglePerso += angleJoystick;


                    char_yaw_integer = DoubleAngleToInt(anglePerso);
                    PsobbAccess.WriteIntRAM(char_yaw_address, char_yaw_integer);
                    PsobbAccess.WriteIntRAM(0x0CDB6B8C, char_yaw_integer);
                }

                int char_animationstate_address = char_data_address + Convert.ToInt32(settings.GetProperty("offset_to_animationstate"), 16);
                int char_animationstate_integer = PsobbAccess.ReadShortRAM(char_animationstate_address);



            }
        }

        static int DoubleAngleToInt(double angle)
        {
            angle = Math.Atan2(Math.Sin(angle), Math.Cos(angle));
            return (int)(((angle + Math.PI) / (Math.PI * 2)) * 0xFFFF);
        }

        static double IntToDoubleAngle(int angle)
        {
            while (angle < 0) angle += 0xFFFF;
            while (angle > 0xFFFF) angle -= 0xFFFF;
            return -Math.PI + (angle / (double)0xFFFF) * Math.PI * 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gamepadIndex = -1;
            button1.Text = "Move your right Joystick...";
            button1.Enabled = false;
        }
    }
}
