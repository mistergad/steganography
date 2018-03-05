using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MTLab1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        byte[] imageBin;
        string fileName;
        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bmp файл|*.bmp";
            openFileDialog.FilterIndex = 0;
            DialogResult userClickedOK = openFileDialog.ShowDialog();
            if (userClickedOK == DialogResult.OK)
            {
                fileName = openFileDialog.FileName;
                if (File.Exists(fileName))
                {
                    image = new Bitmap(fileName);
                    pictureBox1.Image = image;
                    radioButton1.Checked = true;
                    textBox1.Clear();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
                LSBInsert();
            else
                KJBInsert();
        }

        private void LSBInsert()
        {
            if (textBox1.Text.Length * 8 > 255)
            {
                MessageBox.Show("Длина сообщения не может превышать 30 символов");
                return;
            }
            if (textBox1.Text.Length * 8 > (image.Width - 1) * (image.Height - 1))
            {
                MessageBox.Show("Размер изображения слишком мал для вставки текста");
                return;
            }
            byte[] imageBinary = File.ReadAllBytes(fileName);
            int startByte = Convert.ToInt32(imageBinary[10]);
            BitArray message = new BitArray(Encoding.UTF8.GetBytes(textBox1.Text));
            BitArray messageLength = new BitArray(new byte[] { Convert.ToByte(message.Length) });

            for (int i = startByte, k = 0; i < startByte + 4; i++, k += 2)
            {
                BitArray temp = new BitArray(new byte[] { imageBinary[i] });
                temp[0] = messageLength[k];
                temp[1] = messageLength[k + 1];
                temp.CopyTo(imageBinary, i);
            }

            for (int i = startByte + 4, k = 0; i < imageBinary.Length; i++, k += 2)
            {
                BitArray temp = new BitArray(new byte[] { imageBinary[i] });
                if (k < message.Length)
                    temp[0] = message[k];
                if (k + 1 < message.Length)
                    temp[1] = message[k + 1];
                temp.CopyTo(imageBinary, i);
            }
            imageBin = imageBinary;
        }

        private void KJBInsert()
        {
            if (textBox1.Text.Length * 8 > 255)
            {
                MessageBox.Show("Длина сообщения не может превышать 30 символов");
                return;
            }
            if (textBox1.Text.Length * 8 > (image.Width - 1) * (image.Height - 1))
            {
                MessageBox.Show("Размер изображения слишком мал для вставки текста");
                return;
            }

            BitArray message = new BitArray(Encoding.UTF8.GetBytes(textBox1.Text));
            BitArray messageLength = new BitArray(new byte[] { Convert.ToByte(message.Length) });

            int x, y, k;
            if (3 < image.Width && 3 < image.Height)
            {
                for (x = 3, y = 3, k = 0; k < 8; k++)
                {
                    int R = image.GetPixel(x, y).R;
                    int G = image.GetPixel(x, y).G;
                    int B = image.GetPixel(x, y).B;
                    int Y = (int)(0.3 * R + 0.59 * G + 0.11 * B);
                    int Bnew;
                    if (messageLength[k] == true)
                        Bnew = (int)(B + 0.25 * Y);
                    else
                        Bnew = (int)(B - 0.25 * Y);

                    if (Bnew > 255)
                        Bnew = 255;
                    else if (Bnew < 0)
                        Bnew = 0;

                    image.SetPixel(x, y, Color.FromArgb(R, G, Bnew));
                    x += 4;
                    if (x + 4 >= image.Width)
                    {
                        x = 3;
                        y += 4;
                    }
                }

                for (k = 0; k < message.Length; k++)
                {
                    int R = image.GetPixel(x, y).R;
                    int G = image.GetPixel(x, y).G;
                    int B = image.GetPixel(x, y).B;
                    int Y = (int)(0.3 * R + 0.59 * G + 0.11 * B);
                    int Bnew;
                    if (message[k] == true)
                        Bnew = (int)(B + 0.25 * Y);
                    else
                        Bnew = (int)(B - 0.25 * Y);

                    if (Bnew > 255)
                        Bnew = 255;
                    else if (Bnew < 0)
                        Bnew = 0;
                    image.SetPixel(x, y, Color.FromArgb(R, G, Bnew));
                    x += 4;
                    if (x + 4 >= image.Width)
                    {
                        x = 3;
                        y += 4;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
                LSBExtract();
            else
                KJBExtract();
        }

        private void LSBExtract()
        {
            byte[] imageBinary = File.ReadAllBytes(fileName);
            int startByte = Convert.ToInt32(imageBinary[10]);
            BitArray messageLengthBin = new BitArray(8);
            byte[] messageLengthByte = new byte[1];
            for (int i = startByte, k = 0; i < startByte + 4; i++, k += 2)
            {
                BitArray temp = new BitArray(new byte[] { imageBinary[i] });
                messageLengthBin[k] = temp[0];
                messageLengthBin[k + 1] = temp[1];
            }
            messageLengthBin.CopyTo(messageLengthByte, 0);
            int messageLength = Convert.ToInt32(messageLengthByte[0]);
            BitArray messageBits = new BitArray(messageLength);

            for (int i = startByte + 4, k = 0; k < messageLength; i++, k += 2)
            {
                BitArray temp = new BitArray(new byte[] { imageBinary[i] });
                messageBits[k] = temp[0];
                messageBits[k + 1] = temp[1];
            }
            byte[] messageByte = new byte[messageLength / 8];
            messageBits.CopyTo(messageByte, 0);
            textBox1.Text = Encoding.UTF8.GetString(messageByte);
        }

        private void KJBExtract()
        {
            BitArray messageLengthBin = new BitArray(8);
            byte[] messageLengthByte = new byte[1];
            int Bsum = 0;
            int x, y, k;
            if (3 < image.Width && 3 < image.Height)
            {
                for (x = 3, y = 3, k = 0; k < 8; k++)
                {
                    for (int i = 1; i <= 3; i++)
                        Bsum += image.GetPixel(x, y + i).B;
                    for (int i = 1; i <= 3; i++)
                        Bsum += image.GetPixel(x, y - i).B;
                    for (int i = 1; i <= 3; i++)
                        Bsum += image.GetPixel(x + i, y).B;
                    for (int i = 1; i <= 3; i++)
                        Bsum += image.GetPixel(x - i, y).B;
                    Bsum /= 12;

                    if (image.GetPixel(x, y).B > Bsum)
                        messageLengthBin[k] = true;
                    else
                        messageLengthBin[k] = false;

                    x += 4;
                    if (x + 4 >= image.Width)
                    {
                        x = 3;
                        y += 4;
                    }
                }
                messageLengthBin.CopyTo(messageLengthByte, 0);
                int messageLength = Convert.ToInt32(messageLengthByte[0]);
                BitArray messageBits = new BitArray(messageLength);
                Bsum = 0;
                for (k = 0; k < messageLength; k++)
                {
                    for (int i = 1; i <= 3; i++)
                        Bsum += image.GetPixel(x, y + i).B;
                    for (int i = 1; i <= 3; i++)
                        Bsum += image.GetPixel(x, y - i).B;
                    for (int i = 1; i <= 3; i++)
                        Bsum += image.GetPixel(x + i, y).B;
                    for (int i = 1; i <= 3; i++)
                        Bsum += image.GetPixel(x - i, y).B;
                    Bsum /= 12;

                    if (image.GetPixel(x, y).B > Bsum)
                        messageBits[k] = true;
                    else
                        messageBits[k] = false;

                    x += 4;
                    if (x + 4 >= image.Width)
                    {
                        x = 3;
                        y += 4;
                    }
                }
                byte[] messageByte = new byte[messageLength / 8];
                messageBits.CopyTo(messageByte, 0);
                textBox1.Text = Encoding.UTF8.GetString(messageByte);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bmp файл|*.bmp";
            saveFileDialog.FilterIndex = 0;
            DialogResult userClicked = saveFileDialog.ShowDialog();
            if (userClicked == DialogResult.OK)
            {
                string fileName2 = saveFileDialog.FileName;
                if (radioButton1.Checked == true)
                {
                    File.WriteAllBytes(fileName2, imageBin);
                }
                else
                {
                    image.Save(fileName2);
                }
            }
        }
    }
}
