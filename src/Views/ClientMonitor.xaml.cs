//-------------------------------------------------------------------------------------------------
// <copyright file="ClientMonitor.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

using System.Drawing;
using System.Windows;
using System.Windows.Media;

namespace TestViewer.Views
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Forms.Integration;
    
    using MSTSCLib;

    /// <summary>
    /// Interaction logic for ClientMonitor.xaml
    /// </summary>
    public partial class ClientMonitor : UserControl
    {
        private readonly Dictionary<string, StackPanel> HostList = new Dictionary<string, StackPanel>();

        public ClientMonitor()
        {
            InitializeComponent();
        }

        public void RemoteMachineByName(string pcName)
        {
            if (!HostList.ContainsKey(pcName))
            {
                var sp = new StackPanel();
                var bt = new Button { Content = pcName };
                bt.Click += delegate
                {
                    // not full screen
                    bool meIsFullScreen = false;
                    // The StackPanel
                    UIElement meControl = null;
                    for (int i = 0; i < this.PCBox.Children.Count; i++)
                    {
                        if (bt.Parent.Equals(this.PCBox.Children[i]))
                        {
                            meControl = this.PCBox.Children[i];
                        }
                        else
                        {
                            if (this.PCBox.Children[i].Visibility == Visibility.Collapsed)
                            {
                                meIsFullScreen = true;
                            }    
                        }
                    }

                    var wfhControl = (meControl as StackPanel).Children[1] as WindowsFormsHost;
                    if(meIsFullScreen)
                    {
                        //wfhControl.Width = 750;
                        //wfhControl.Height = 450;
                        wfhControl.Width = this.ActualWidth/2;
                        wfhControl.Height = this.ActualHeight/2;
                    }
                    else
                    {
                        wfhControl.Width = this.ActualWidth;
                        wfhControl.Height = this.ActualHeight - 25;
                    }

                    for (int i = 0; i < this.PCBox.Children.Count; i++)
                    {
                        if (bt.Parent != this.PCBox.Children[i])
                        {
                            if (meIsFullScreen)
                            {
                                this.PCBox.Children[i].Visibility = Visibility.Visible;
                            }
                            else
                            {
                                this.PCBox.Children[i].Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                };

                sp.Children.Add(bt);
                var termServ = new AxMSTSCLib.AxMsRdpClient7();
                termServ.Size = new System.Drawing.Size(1280,768);
                //termServ.ClientSizeChanged += delegate { MessageBox.Show("Clientsize changed"); };

                
                var wfh = new WindowsFormsHost {Child = termServ};
                if (HostList.Count == 0)
                {
                    wfh.Width = this.ActualWidth;
                    wfh.Height = this.ActualHeight - 25;
                }
                else
                {
                    var wfhFirstChild = (PCBox.Children[0] as StackPanel).Children[1] as WindowsFormsHost;
                    //wfhFirstChild.Width = 750;
                    //wfhFirstChild.Height = 450;
                    wfhFirstChild.Width = this.ActualWidth / 2;
                    wfhFirstChild.Height = this.ActualHeight / 2;
                    //wfh.Width = 750;
                    //wfh.Height = 450;
                    wfh.Width = this.ActualWidth / 2;
                    wfh.Height = this.ActualHeight / 2;
                }

                sp.Children.Add(wfh);
                PCBox.Children.Add(sp);
                HostList.Add(pcName, sp);

                termServ.Server = pcName;
                termServ.UserName = @"fareast\v-bawei";
                ((IMsTscNonScriptable)termServ.GetOcx()).ClearTextPassword = "7ujm*IK<";
                termServ.AdvancedSettings.allowBackgroundInput = -1;
                termServ.Connect();
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (HostList.Count > 0)
            {
                if (HostList.Count == 1)
                {
                    UIElement meControl = this.PCBox.Children[0];
                    var wfh = (meControl as StackPanel).Children[1] as WindowsFormsHost;
                    wfh.Width = this.ActualWidth;
                    wfh.Height = this.ActualHeight - 25;
                }
                else
                {
                    // not full screen
                    bool anyOneIsFullScreen = false;
                    UIElement fullScreenControl = null;
                    // The StackPanel
                    for (int i = 0; i < this.PCBox.Children.Count; i++)
                    {
                        if (this.PCBox.Children[i].Visibility == Visibility.Visible)
                        {
                            fullScreenControl = this.PCBox.Children[i];
                        }
                        if (this.PCBox.Children[i].Visibility == Visibility.Collapsed)
                        {
                            anyOneIsFullScreen = true;
                        }
                    }

                    var wfhControl = fullScreenControl == null
                                         ? null
                                         : (fullScreenControl as StackPanel).Children[1] as WindowsFormsHost;
                    if (anyOneIsFullScreen)
                    {
                        wfhControl.Width = this.ActualWidth;
                        wfhControl.Height = this.ActualHeight - 25;
                        for (int i = 0; i < this.PCBox.Children.Count; i++)
                        {
                            if (!fullScreenControl.Equals(this.PCBox.Children[i]))
                            {
                                var eachWfhControl =
                                    (this.PCBox.Children[i] as StackPanel).Children[1] as WindowsFormsHost;
                                eachWfhControl.Width = this.ActualWidth/2;
                                eachWfhControl.Height = this.ActualHeight/2 - 50;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.PCBox.Children.Count; i++)
                        {
                            var eachWfhControl = (this.PCBox.Children[i] as StackPanel).Children[1] as WindowsFormsHost;
                            eachWfhControl.Width = this.ActualWidth / 2;
                            eachWfhControl.Height = this.ActualHeight / 2 - 50;    
                        }
                    }
                }
            }
        }
    }
}
