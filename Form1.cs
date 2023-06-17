using DiscordRPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections;
using System.Security.Policy;
using System.Xml.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Diagnostics;

namespace Bretxa_s_Discord_Rich_Presence
{
    public partial class BretxaRichPresence : Form
    {
        private List<string> names = new List<string>();
        private List<string> ids = new List<string>();
        private int x;
        private int y;
        private string appDataFolderPath;

        public BretxaRichPresence()
        {
            InitializeComponent();

            string appName = "BretxaRichPresenceDiscord";
            appDataFolderPath = GetAppDataFolderPath(appName);

            loadprofiles();

            clientid.Click += clientid_Click;
            details.Click += details_Click;
            state.Click += state_Click;
        }
        public class Asset
        {
            public string Id { get; set; }
            public int Type { get; set; }
            public string Name { get; set; }
        }

        DiscordRpcClient client;
        private int imagenum = 0;
        private bool clientidclick = true;
        private bool detailsclick = true;
        private bool stateclick = true;
        List<System.Windows.Forms.Button> buttons = new List<System.Windows.Forms.Button>();

        private void clientid_Click(object sender, EventArgs e)
        {
            if (clientidclick)
            {
                // Clear the text box on the first click
                clientid.Text = string.Empty;
                clientidclick = false;
            }
        }

        private void details_Click(object sender, EventArgs e)
        {
            if (detailsclick)
            {
                // Clear the text box on the first click
                details.Text = string.Empty;
                detailsclick = false;
            }
        }

        private void state_Click(object sender, EventArgs e)
        {
            if (stateclick)
            {
                // Clear the text box on the first click
                state.Text = string.Empty;
                stateclick = false;
            }
        }
        private DateTime startTime;
        private Timer presenceTimer;

        private void launch_Click(object sender, EventArgs e)
        {

            client = new DiscordRpcClient(clientid.Text);
            client.Initialize();

            if (details.Text == "Details")
            {
                details.Text = "";
            }
            if (state.Text == "State")
            {
                state.Text = "";
            }

            startTime = DateTime.UtcNow;

            if (checkBox1.Checked)
            {
                presenceTimer = new Timer();
                presenceTimer.Interval = 1000; // 1 second interval
                presenceTimer.Tick += PresenceTimer_Tick;
                presenceTimer.Start();
            }
            else
            {
                RichPresence presence = new RichPresence()
                {
                    Details = details.Text,
                    State = state.Text,
                    Assets = new Assets()
                    {
                        LargeImageKey = (string)comboBox1.SelectedItem
                    }
                };
                if (buttonbox1.Checked && buttonbox2.Checked)
                {
                    try
                    {
                        presence.Buttons = new DiscordRPC.Button[]
                        {
                            new DiscordRPC.Button() { Label = textBox1.Text, Url = textBox3.Text },
                            new DiscordRPC.Button() { Label = textBox2.Text, Url = textBox4.Text }
                        };

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid link, enter a valid one! \n\n" + ex);
                    }

                }
                else if (buttonbox1.Checked)
                {
                    try
                    {
                        presence.Buttons = new DiscordRPC.Button[]
                        {
                            new DiscordRPC.Button() { Label = textBox1.Text, Url = textBox3.Text }
                        };
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid link, enter a valid one! \n\n" + ex);
                    }
                }
                client.SetPresence(presence);
            }

        }
        private void PresenceTimer_Tick(object sender, EventArgs e)
        {
            UpdatePresence();
        }

        private void UpdatePresence()
        {
            TimeSpan elapsedTime = DateTime.UtcNow - startTime;
            state.Text = $"{elapsedTime.ToString(@"hh\:mm\:ss")} elapsed";
            state.Enabled = false;
            
            try
            {
                RichPresence presence = new RichPresence()
                {
                    Details = details.Text,
                    State = $"{elapsedTime.ToString(@"hh\:mm\:ss")} elapsed",
                    Assets = new Assets()
                    {
                        LargeImageKey = (string)comboBox1.SelectedItem
                    }
                };
                if (buttonbox1.Checked && buttonbox2.Checked)
                {
                    try
                    {
                        presence.Buttons = new DiscordRPC.Button[]
                        {
                            new DiscordRPC.Button() { Label = textBox1.Text, Url = textBox3.Text },
                            new DiscordRPC.Button() { Label = textBox2.Text, Url = textBox4.Text }
                        };

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid link, enter a valid one! \n\n" + ex);
                        if (client.IsInitialized == true)
                        {
                            client.Dispose();
                        }
                    }

                }
                else if (buttonbox1.Checked)
                {
                    try
                    {
                        presence.Buttons = new DiscordRPC.Button[]
                        {
                            new DiscordRPC.Button() { Label = textBox1.Text, Url = textBox3.Text }
                        };
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid link, enter a valid one! \n\n" + ex);
                        if (client.IsInitialized == true)
                        {
                            client.Dispose();
                        }
                    }
                }
                client.SetPresence(presence);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred(closed timer): " + ex.Message);
                presenceTimer.Stop();
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "https://discordapp.com/api/oauth2/applications/" + clientid.Text + "/assets";
                    HttpResponseMessage response = await client.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    string namePattern = "\"name\": \"(.*?)\"";
                    string idPattern = "\"id\": \"(.*?)\"";

                    MatchCollection nameMatches = Regex.Matches(responseBody, namePattern);
                    MatchCollection idMatches = Regex.Matches(responseBody, idPattern);

                    foreach (Match nameMatch in nameMatches)
                    {
                        string name = nameMatch.Groups[1].Value;
                        names.Add(name);
                    }

                    foreach (Match idMatch in idMatches)
                    {
                        string id = idMatch.Groups[1].Value;
                        ids.Add(id);
                    }

                    /*for (int i = 0; i < names.Count; i++)
                    {
                        Console.WriteLine($"Name: {names[i]}, ID: {ids[i]}");
                    }*/

                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(names.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }

        private void stop_Click(object sender, EventArgs e)
        {
            if (client.IsInitialized == true)
            {
                client.Dispose();
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int position = names.IndexOf((string)comboBox1.SelectedItem);
            string selectedId = ids[position];
            string imageUrl = "https://cdn.discordapp.com/app-assets/" + clientid.Text + "/" + selectedId + ".png";
            string appLocation = appDataFolderPath + "/previewimage" + imagenum + ".png";
            imagenum += 1;
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(new Uri(imageUrl), appLocation);
                    pictureBox5.Image = Image.FromFile(appLocation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }

        private void details_TextChanged(object sender, EventArgs e)
        {
            detailslabel.Text = details.Text;
        }

        private void state_TextChanged(object sender, EventArgs e)
        {
            statelabel.Text = state.Text;
        }

        private void save_Click(object sender, EventArgs e)
        {
            var jsonObject = new
            {
                profilenametext = profilename.Text,
                clientidtext = clientid.Text,
                detailstext = details.Text,
                statetext = state.Text,
                imageselected = (string)comboBox1.SelectedItem,
                chebox1 = buttonbox1.CheckState,
                buttontext1 = textBox1.Text,
                buttonurl1 = textBox3.Text,
                chebox2 = buttonbox2.CheckState,
                buttontext2 = textBox2.Text,
                buttonurl2 = textBox4.Text,
                timeelpased = checkBox1.CheckState,


            };

            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

            string filePath = appDataFolderPath + "/" + profilename.Text + ".json";

            File.WriteAllText(filePath, jsonString);

            loadprofiles();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            File.Delete(appDataFolderPath + "/" + profilename.Text + ".json");
            loadprofiles();
        }

        public void loadprofiles()
        {
            string[] jsonFiles = Directory.GetFiles(appDataFolderPath, "*.json");

            int jsonFileCount = jsonFiles.Length;

            x = 15;
            y = 190;

            foreach (System.Windows.Forms.Button button in buttons)
            {
                this.Controls.Remove(button);
                button.Dispose();
            }
            buttons.Clear();

            foreach (string filePath in jsonFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                System.Windows.Forms.Button button = new System.Windows.Forms.Button();

                button.Text = fileName;
                button.Location = new Point(x, y);
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Size = new Size(233, 36);
                button.Font = new Font(button.Font.FontFamily, 12f);
                buttons.Add(button);

                button.Click += (sender, e) =>
                {
                    try
                    {
                        string jsonString = File.ReadAllText(filePath);

                        JObject json = JObject.Parse(jsonString);

                        profilename.Text = fileName;
                        clientid.Text = (string)json["clientidtext"];
                        details.Text = (string)json["detailstext"];
                        state.Text = (string)json["statetext"];
                        comboBox1.SelectedItem = (string)json["imageselected"];
                        buttonbox1.Checked = (bool)json["chebox1"];
                        textBox1.Text = (string)json["buttontext1"];
                        textBox3.Text = (string)json["buttonurl1"];
                        buttonbox2.Checked = (bool)json["chebox2"];
                        textBox2.Text = (string)json["buttontext2"];
                        textBox4.Text = (string)json["buttonurl2"];
                        checkBox1.Checked = (bool)json["timeelpased"];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                    }
                };
                this.Controls.Add(button);

                y += 36;
            }
        }

        private void buttonbox1_CheckedChanged(object sender, EventArgs e)
        {
            if (buttonbox1.Checked)
            {
                textBox1.Enabled = true;
                textBox3.Enabled = true;
                buttonbox2.Enabled = true;
                ebutton1.Visible = true;
            }
            else
            {
                textBox1.Enabled = false;
                textBox3.Enabled = false;
                buttonbox2.Enabled = false;
                buttonbox2.Checked = false;
                textBox2.Enabled = false;
                textBox4.Enabled = false;
                ebutton1.Visible = false;
                ebutton2.Visible = false;

            }
        }

        private void buttonbox2_CheckedChanged(object sender, EventArgs e)
        {
            if (buttonbox2.Checked)
            {
                textBox2.Enabled = true;
                textBox4.Enabled = true;
                ebutton1.Visible = true;
                ebutton2.Visible = true;
            }
            else
            {
                textBox2.Enabled = false;
                textBox4.Enabled = false;
                ebutton2.Visible = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ebutton1.Text = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            ebutton2.Text = textBox2.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        public static string GetAppDataFolderPath(string appName)
        {
            string appDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

            if (!Directory.Exists(appDataFolderPath))
            {
                Directory.CreateDirectory(appDataFolderPath);
            }

            return appDataFolderPath;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                state.Enabled = false;
                state.Text = "00:00:00 elapsed";
            }
            else
            {
                state.Enabled = true;
                state.Text = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.com/developers/applications");
        }
    }
}
